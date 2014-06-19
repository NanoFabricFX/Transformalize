#region License

// /*
// Transformalize - Replicate, Transform, and Denormalize Your Data...
// Copyright (C) 2013 Dale Newman
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transformalize.Libs.Elasticsearch.Net.Extensions;
using Transformalize.Libs.Ninject.Syntax;
using Transformalize.Libs.NLog;
using Transformalize.Libs.Ninject;
using Transformalize.Libs.Rhino.Etl;
using Transformalize.Libs.Rhino.Etl.Operations;
using Transformalize.Main.Providers;
using Transformalize.Operations.Validate;
using Transformalize.Runner;
using Encoding = Transformalize.Libs.RazorEngine.Encoding;

namespace Transformalize.Main {

    public class Process {

        private readonly Logger _log = LogManager.GetLogger("tfl");
        private const StringComparison IC = StringComparison.OrdinalIgnoreCase;
        private List<IOperation> _transformOperations = new List<IOperation>();
        private IParameters _parameters = new Parameters.Parameters();
        private bool _enabled = true;
        private Dictionary<string, AbstractConnection> _connections = new Dictionary<string, AbstractConnection>();

        // fields (for now)
        public Fields CalculatedFields = new Fields();
        public List<Entity> Entities = new List<Entity>();
        public IKernel Kernal = new StandardKernel(new NinjectBindings());
        public Dictionary<string, Map> MapEndsWith = new Dictionary<string, Map>();
        public Dictionary<string, Map> MapEquals = new Dictionary<string, Map>();
        public Dictionary<string, Map> MapStartsWith = new Dictionary<string, Map>();
        public Entity MasterEntity;
        public string Name;
        public Options Options = new Options();
        public Dictionary<string, string> Providers = new Dictionary<string, string>();
        public List<Relationship> Relationships = new List<Relationship>();
        public Dictionary<string, Script> Scripts = new Dictionary<string, Script>();
        public Dictionary<string, SearchType> SearchTypes = new Dictionary<string, SearchType>();
        public Encoding TemplateContentType = Encoding.Raw;
        public Dictionary<string, Template> Templates = new Dictionary<string, Template>();
        public AbstractConnection OutputConnection;
        private PipelineThreading _pipelineThreading = PipelineThreading.MultiThreaded;
        private string _star = Common.DefaultValue;

        // properties
        public string TimeZone { get; set; }
        public bool IsFirstRun { get; set; }
        public long Anything { get; set; }
        public bool StarEnabled { get; set; }

        public string Star {
            get { return _star.Equals(Common.DefaultValue) ? Name + "Star" : _star; }
            set { _star = value; }
        }

        public Dictionary<string, AbstractConnection> Connections {
            get { return _connections; }
            set { _connections = value; }
        }

        public PipelineThreading PipelineThreading {
            get { return _pipelineThreading; }
            set { _pipelineThreading = value; }
        }

        public bool Enabled {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public List<IOperation> TransformOperations {
            get { return _transformOperations; }
            set { _transformOperations = value; }
        }

        public IParameters Parameters {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public IEnumerable<TemplateAction> Actions { get; set; }

        //constructor
        public Process(string name = "") {
            Name = name;
        }

        //methods
        public bool IsReady() {
            if (Enabled || Options.Force) {
                if (Connections.All(cn => cn.Value.IsReady())) {
                    Setup();
                    return true;
                }
                _log.Warn("Process is not ready.");
                return false;
            }
            _log.Error("Process is disabled. Data is not being updated.");
            return false;
        }

        private IProcessRunner GetRunner() {
            switch (Options.Mode.ToLower()) {
                case "init":
                    return new InitializeRunner();
                case "metadata":
                    return new MetadataRunner();
                case "delete":
                    return new DeleteRunner();
                default:
                    return new ProcessRunner();
            }
        }

        public void ExecuteScaler() {
            using (var runner = GetRunner()) {
                runner.Run(this);
            }
        }

        public IDictionary<string, IEnumerable<Row>> Execute() {
            using (var runner = GetRunner()) {
                return runner.Run(this);
            }
        }

        public IEnumerable<Row> ExecuteSingle() {
            using (var runner = GetRunner()) {
                return runner.Run(this)[Entities[0].Alias];
            }
        }
        public Fields OutputFields() {
            var fields = new Fields();
            foreach (var entity in Entities) {
                fields.Add(entity.Fields);
                fields.Add(entity.CalculatedFields);
            }
            fields.Add(CalculatedFields);
            return fields.WithOutput();
        }

        public Fields SearchFields() {
            var fields = new Fields();
            foreach (var pair in new StarFields(this).TypedFields()) {
                fields.Add(pair.Value.WithSearchType());
            }
            return fields;
        }

        public Entity this[string entity] {
            get {
                return Entities.Find(e => e.Alias.Equals(entity, IC) || e.Name.Equals(entity, IC));
            }
        }

        public int GetNextBatchId() {
            if ((new[] { "init", "metadata" }).Any(m => m.Equals(Options.Mode, IC)) || !OutputConnection.IsDatabase)
                return 1;
            return OutputConnection.NextBatchId(Name);
        }

        public Field GetField(string alias, string entity, bool issueWarning = true) {

            foreach (var fields in Entities.Where(e => e.Alias == entity || entity == string.Empty).Select(e => e.Fields).Where(fields => fields.Find(alias).Any())) {
                return fields.Find(alias).First();
            }

            foreach (var fields in Entities.Where(e => e.Alias == entity || entity == string.Empty).Select(e => e.CalculatedFields).Where(fields => fields.Find(alias).Any())) {
                return fields.Find(alias).First();
            }

            if (CalculatedFields.Find(alias).Any()) {
                return CalculatedFields.Find(alias).First();
            }

            if (!IsValidationResultField(alias, entity) && issueWarning) {
                _log.Warn("Can't find field with alias: {0}.", alias);
            }

            return null;
        }

        private bool IsValidationResultField(string alias, string entity) {
            return Entities
                    .Where(e => e.Alias == entity || entity == string.Empty)
                    .Any(e => e.OperationsAfterAggregation.OfType<ValidationOperation>().Any(operation => operation.ResultKey.Equals(alias)));
        }

        public bool TryGetField(string alias, string entity, out Field field, bool issueWarning = true) {
            field = GetField(alias, entity, issueWarning);
            return field != null;
        }

        public void CreateOutput(Entity entity) {
            OutputConnection.Create(this, entity);
        }

        public bool UpdatedAnything() {
            return Anything > 0;
        }

        public void PerformActions(Func<TemplateAction, bool> filter) {
            if (Actions.Any(filter)) {
                foreach (var action in Actions.Where(filter)) {
                    if (action.Conditional) {
                        if (UpdatedAnything()) {
                            action.Handle(FindFile(action));
                        }
                    } else {
                        action.Handle(FindFile(action));
                    }
                }
            }
        }

        private static string FindFile(TemplateAction action) {
            var file = action.File;
            if (!string.IsNullOrEmpty(file))
                return file;

            if (action.Connection != null && (action.Connection.Type == ProviderType.File || action.Connection.Type == ProviderType.Html)) {
                file = action.Connection.File;
            }
            return file;
        }

        public void Setup() {
            IsFirstRun = false;
            Anything = 0;
            var batchId = GetNextBatchId();
            foreach (var entity in Entities) {
                entity.Inserts = 0;
                entity.Updates = 0;
                entity.Deletes = 0;
                entity.Sampled = false;
                entity.HasRows = false;
                entity.HasRange = false;
                entity.TflBatchId = batchId;
                batchId++;
            }
        }

        public string ViewSql() {
            const string fieldSpacer = ",\r\n    ";
            var builder = new StringBuilder();
            var master = MasterEntity;
            var l = OutputConnection.L;
            var r = OutputConnection.R;
            var schema = master.SchemaPrefix(l, r);

            builder.AppendLine("SELECT");
            builder.Append("    ");
            builder.Append(new FieldSqlWriter(master.OutputFields()).Alias(l, r).Prepend("m.").Write(fieldSpacer));

            foreach (var rel in Relationships) {
                var joinFields = rel.Fields();
                foreach (var field in rel.RightEntity.OutputFields()) {
                    if (!joinFields.HaveField(field.Alias)) {
                        builder.Append(fieldSpacer);
                        builder.Append(new FieldSqlWriter(new Fields(field)).Alias(l, r).Prepend("r" + rel.RightEntity.Index + ".").Write(fieldSpacer));
                    }
                }
            }

            builder.AppendLine();
            builder.Append("FROM ");
            builder.Append(schema);
            builder.Append(l);
            builder.Append(master.OutputName());
            builder.Append(r);
            builder.Append(" m");

            foreach (var rel in Relationships) {
                builder.AppendLine();
                builder.Append("LEFT OUTER JOIN ");
                if (rel.RightEntity.IsMaster()) {
                    builder.Append("m");
                } else {
                    builder.Append(rel.RightEntity.SchemaPrefix(l, r));
                    builder.Append(l);
                    builder.Append(rel.RightEntity.OutputName());
                    builder.Append(r);
                }
                builder.Append(" ");
                builder.Append("r");
                builder.Append(rel.RightEntity.Index);
                builder.Append(" ON (");
                foreach (var j in rel.Join) {
                    if (rel.LeftEntity.IsMaster()) {
                        builder.Append("m");
                    } else {
                        builder.Append("r");
                        builder.Append(rel.LeftEntity.Index);
                    }
                    builder.Append(".");
                    builder.Append(l);
                    builder.Append(j.LeftField.Alias);
                    builder.Append(r);
                    builder.Append(" = ");
                    builder.Append("r");
                    builder.Append(rel.RightEntity.Index);
                    builder.Append(".");
                    builder.Append(l);
                    builder.Append(j.RightField.Alias);
                    builder.Append(r);
                }
                builder.Append(")");
            }

            builder.Append(";");
            return builder.ToString();

        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transformalize.Transforms;

namespace Transformalize.Model {

    public class Field {

        private FieldType _fieldType;
        private string _name;
        private string _sqlDataType;

        public StringBuilder StringBuilder;

        public string Type { get; private set; }
        public string SimpleType { get; private set; }
        public string Quote { get; private set; }
        public string Alias { get; set; }
        public string Schema { get; set; }
        public string Entity { get; set; }
        public string Parent { get; set; }
        public bool Input { get; set; }
        public int Length { get; private set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool Clustered { get; set; }
        public bool NotNull { get; set; }
        public bool Identity { get; set; }
        public KeyValuePair<string, string> References { get; set; }
        public ITransform[] Transforms { get; set; }
        public bool Output { get; private set; }
        public bool UseStringBuilder { get; private set; }
        public Type SystemType { get; private set; }
        public Dictionary<string, Field> InnerXml { get; set; }
        public string XPath { get; set; }
        public int Index { get; set; }
        public bool Unicode { get; set; }
        public bool VariableLength { get; set; }

        public string SqlDataType {
            get { return _sqlDataType ?? (_sqlDataType = new SqlServerDataTypeService().GetDataType(this)); }
        }

        public object Default;

        /// <summary>
        /// FieldType can affect Output
        /// </summary>
        public FieldType FieldType {
            get { return _fieldType; }
            set {
                _fieldType = value;
                if (MustBeOutput()) {
                    Output = true;
                }
            }
        }

        /// <summary>
        /// Alias follows name if no alias provided
        /// </summary>
        public string Name {
            get { return _name; }
            set {
                _name = value;
                if (string.IsNullOrEmpty(Alias)) {
                    Alias = Name;
                }
            }
        }

        public bool MustBeOutput() {
            return FieldType == FieldType.MasterKey || FieldType == FieldType.ForeignKey || FieldType == FieldType.PrimaryKey;
        }

        public Field(FieldType fieldType) : this("System.String", 64, fieldType, true, null) { }

        public Field(string typeName, int length, FieldType fieldType, bool output, object @default) {
            Initialize(typeName, length, fieldType, output, @default);
        }

        private void Initialize(string typeName, int length, FieldType fieldType, bool output, object @default) {
            Input = true;
            Unicode = true;
            VariableLength = true;
            Type = typeName;
            Length = length;
            SimpleType = Type.ToLower().Replace("system.", string.Empty);
            Quote = (new[] { "string", "char", "datetime", "guid" }).Any(t => t.Equals(SimpleType)) ? "'" : string.Empty;
            UseStringBuilder = (new[] { "string", "char" }).Any(t => t.Equals(SimpleType));
            FieldType = fieldType;
            Output = output || MustBeOutput();
            SystemType = System.Type.GetType(typeName);
            StringBuilder = UseStringBuilder ? new StringBuilder(length, 8000) : null;
            InnerXml = new Dictionary<string, Field>();
            Default = ConvertDefault(@default ?? (object)string.Empty);
        }

        private FieldSqlWriter _sqlWriter;
        public FieldSqlWriter SqlWriter {
            get { return _sqlWriter ?? (_sqlWriter = new FieldSqlWriter(this)); }
            set { _sqlWriter = value; }
        }

        public string AsJoin(string left, string right) {
            return string.Format("{0}.[{1}] = {2}.[{1}]", left, Name, right);
        }

        private object ConvertDefault(object @default) {
            var value = @default.ToString();
            switch (SimpleType) {
                case "datetime":
                    if (value == string.Empty)
                        value = "9999-12-31";
                    return Convert.ToDateTime(value);
                case "boolean":
                    if (value == string.Empty)
                        value = "false";
                    if (value == "0")
                        value = "false";
                    if (value == "1")
                        value = "true";
                    return Convert.ToBoolean(value);
                case "decimal":
                    if (value == string.Empty)
                        value = "0.0";
                    return Convert.ToDecimal(value);
                case "double":
                    if (value == string.Empty)
                        value = "0.0";
                    return Convert.ToDouble(value);
                case "single":
                    if (value == string.Empty)
                        value = "0.0";
                    return Convert.ToSingle(value);
                case "int64":
                    if (value == string.Empty)
                        value = "0";
                    return Convert.ToInt64(value);
                case "int32":
                    if (value == string.Empty)
                        value = "0";
                    return Convert.ToInt32(value);
                case "int16":
                    if (value == string.Empty)
                        value = "0";
                    return Convert.ToInt16(value);
                case "byte":
                    if (value == string.Empty)
                        value = "0";
                    return Convert.ToByte(value);
                case "guid":
                    if (value == string.Empty)
                        value = "00000000-0000-0000-0000-000000000000";
                    return value.ToLower() == "new" ? Guid.NewGuid() : Guid.Parse(value);
                case "char":
                    if (value == string.Empty)
                        value = " ";
                    return Convert.ToChar(value);
                default:
                    return value;
            }

        }
    }
}
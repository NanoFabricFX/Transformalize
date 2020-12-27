#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2019 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Linq;
using Autofac;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Ioc.Autofac;

namespace Transformalize.Command {

    public class RunTimeSchemaReader : IRunTimeSchemaReader {
        private readonly string _placeHolderStyle;
        public Process Process { get; set; }
        public Schema Read(Process process) {
            Process = process;
            return Read();
        }

        private readonly IContext _host;

        public RunTimeSchemaReader(IContext host, string placeHolderStyle) {
            _placeHolderStyle = placeHolderStyle;
            _host = host;
        }

        public RunTimeSchemaReader(Process process, IContext host) {
            Process = process;
            _host = host;
        }

        public Schema Read(Entity entity) {
            if (Process == null) {
                _host.Error("RunTimeSchemaReader executed without a Process");
                return new Schema();
            }

            using (var scope = DefaultContainer.Create(Process, _host.Logger, _placeHolderStyle)) {
                var reader = scope.ResolveNamed<ISchemaReader>(Process.Connections.First(c => c.Provider != Constants.DefaultSetting).Key);
                return reader.Read(entity);
            }
        }

        public Schema Read() {

            if (Process == null) {
                _host.Error("RunTimeSchemaReader executed without a Process");
                return new Schema();
            }

            using (var scope = DefaultContainer.Create(Process, _host.Logger, _placeHolderStyle)) {
                var reader = scope.ResolveNamed<ISchemaReader>(Process.Connections.First().Key);
                return Process.Entities.Count == 1 ? reader.Read(Process.Entities.First()) : reader.Read();
            }

        }
    }
}
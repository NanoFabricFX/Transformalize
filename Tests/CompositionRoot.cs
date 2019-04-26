﻿#region license
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.Trace;
using Process = Transformalize.Configuration.Process;

namespace Tests {

    public class CompositionRoot {

        public Process Process { get; set; }

        public IProcessController Compose(string cfg, LogLevel logLevel = LogLevel.Info, Dictionary<string, string> parameters = null, string placeHolderStyle = "@()") {

            var logger = new TraceLogger(logLevel);
            var container = new ConfigurationContainer().CreateScope(cfg, logger, parameters, placeHolderStyle);

            Process = parameters == null ? container.Resolve<Process>(new NamedParameter("cfg", cfg)) : container.Resolve<Process>(new NamedParameter("cfg", cfg), new NamedParameter("parameters", parameters));

            if (Process.Errors().Any()) {
                foreach (var error in Process.Errors()) {
                    Trace.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            if (Process.Warnings().Any()) {
                foreach (var warning in Process.Warnings()) {
                    Trace.WriteLine(warning);
                }
            }

            return new Container().CreateScope(Process, logger).Resolve<IProcessController>(new NamedParameter("cfg", cfg));
        }

    }

}

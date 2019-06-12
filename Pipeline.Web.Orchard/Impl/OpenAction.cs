#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
// Copyright 2013 Dale Newman
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
using System.Web;
using Transformalize;
using Transformalize.Contracts;
using Action = Transformalize.Configuration.Action;

namespace Pipeline.Web.Orchard.Impl {
    public class OpenAction : IAction {
        private readonly Action _action;

        public OpenAction(Action action) {
            _action = action;
        }

        public ActionResponse Execute() {
            var response = new ActionResponse();
            try {
                var urlOrFile = string.IsNullOrEmpty(_action.File) ? _action.Url : _action.File;
                HttpContext.Current.Response.Redirect(urlOrFile, true);
            } catch (Exception ex) {
                response.Code = 500;
                response.Message = string.Format("Error opening {0}. {1}", _action.File, ex.Message);
            }
            return response;
        }
    }
}

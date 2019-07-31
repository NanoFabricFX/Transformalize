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
using System;
using System.Collections.Generic;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms {

   public class SplitTransform : StringTransform {

      private readonly Field _input;
      private readonly char[] _separator;

      public SplitTransform(IContext context = null) : base(context, "object") {

         if (IsMissingContext()) {
            return;
         }

         Context.Operation.ProducesArray = true;

         if (IsMissing(Context.Operation.Separator)) {
            return;
         }

         _input = SingleInput();
         _separator = Context.Operation.Separator.ToCharArray();

      }

      public override IRow Operate(IRow row) {
         row[Context.Field] = GetString(row, _input).Split(_separator, StringSplitOptions.None);

         return row;
      }

      public override IEnumerable<OperationSignature> GetSignatures() {
         yield return new OperationSignature("split") {
            Parameters = new List<OperationParameter>(1) { new OperationParameter("separator", ",") }
         };
      }
   }
}
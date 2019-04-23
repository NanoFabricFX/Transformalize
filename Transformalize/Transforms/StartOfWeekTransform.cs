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


   public class StartOfWeekTransform : BaseTransform {

      private readonly Field _input;
      private readonly DayOfWeek _dayOfWeek;

      public StartOfWeekTransform(IContext context = null) : base(context, "datetime") {

         if (IsMissingContext()) {
            return;
         }

         if (string.IsNullOrEmpty(Context.Operation.DayOfWeek)) {
            Run = false;
            Context.Error("The firstdayofweek transform needs a the day or week parameter (e.g. Sunday, Monday, Tuesday, etc.)");
            return;
         }

         if (!Received().StartsWith("date")) {
            Run = false;
            Context.Error("The firstdayofweek transform requires a date(time) input.");
            return;
         }

         _input = SingleInput();
         try {
            _dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), Context.Operation.DayOfWeek, true);
         } catch (Exception ex) {
            Run = false;
            Context.Error(ex, ex.Message);
         }

      }

      public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek) {
         int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
         return dt.AddDays(-1 * diff).Date;
      }

      public override IRow Operate(IRow row) {
         var input = (DateTime)row[_input];
         row[Context.Field] = StartOfWeek(input, _dayOfWeek);
         return row;
      }

      public override IEnumerable<OperationSignature> GetSignatures() {
         yield return new OperationSignature("startofweek") { Parameters = new List<OperationParameter>(1) { new OperationParameter("dayofweek") } };
      }
   }


}
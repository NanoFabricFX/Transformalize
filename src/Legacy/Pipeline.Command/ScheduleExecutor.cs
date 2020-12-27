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
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Quartz;
using Transformalize.Contracts;

namespace Transformalize.Command {

   [DisallowConcurrentExecution]
   public class ScheduleExecutor : BaseExecutor, IJob, IDisposable {
      public string Schedule { get; private set; } = string.Empty;

      /// <summary>
      /// Called by Scheduled Run
      /// </summary>
      public ScheduleExecutor(IPipelineLogger logger, Options options) : base(logger, options, true) { }

      public new void Execute(string cfg, Dictionary<string, string> parameters) {

         if (ProcessFactory.TryCreate(Options, parameters, out var process)) {
            process.Mode = Options.Mode;

            /* if an internal schedule with mode is running this, 
               then mode should be updated if it's on any URL in TFL actions */
            if (!string.IsNullOrEmpty(Schedule)) {
               var schedule = process.Schedule.FirstOrDefault(s => s.Name == Schedule);
               if (schedule != null) {
                  var parameter = process.Parameters.FirstOrDefault(x => x.Name.Equals("Mode", StringComparison.OrdinalIgnoreCase));
                  if (parameter != null) {
                     foreach (var action in process.Actions.Where(a => a.Type == "tfl" && a.Url != string.Empty)) {
                        action.Url = action.Url.SetQueryParam(parameter.Name, schedule.Mode).ToString();
                     }
                  }
               }
            }

            Execute(process);
         }
      }

      /// <summary>
      /// This is the method Quartz.NET will use
      /// </summary>
      /// <param name="context"></param>
      public Task Execute(IJobExecutionContext context) {
         Options.Mode = context.MergedJobDataMap.Get("Mode") as string;
         Schedule = context.MergedJobDataMap.Get("Schedule") as string;

         Execute(context.MergedJobDataMap.Get("Cfg") as string, new Dictionary<string, string>());
         return Task.CompletedTask;
      }

      public void Dispose() {
         // shouldn't be anything to dispose
      }
   }
}
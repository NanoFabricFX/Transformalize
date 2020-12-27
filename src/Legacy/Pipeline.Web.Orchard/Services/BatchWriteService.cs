using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Pipeline.Web.Orchard.Models;
using Pipeline.Web.Orchard.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Impl;

namespace Pipeline.Web.Orchard.Services {

   public class BatchWriteService : IBatchWriteService {

      private const string BatchWriteIndicator = "BatchWrite";
      private readonly IOrchardServices _orchardServices;
      private readonly IProcessService _processService;

      public Localizer T { get; set; }
      public ILogger Logger { get; set; }

      public BatchWriteService(IOrchardServices services, IProcessService processService) {
         _orchardServices = services;
         _processService = processService;
         T = NullLocalizer.Instance;
         Logger = NullLogger.Instance;
      }

      public int Write(HttpRequestBase request, Process process, IDictionary<string, string> parameters) {

         var rows = new List<CfgRow>();

         var batchWrite = process.Actions.FirstOrDefault(a => a.Description.Equals(BatchWriteIndicator, StringComparison.OrdinalIgnoreCase));

         if (!(batchWrite != null && batchWrite.Id > 0)) {
            const string message = "Could not find BatchWrite action.  You need to have an action with description 'BatchWrite' that is responsible for writing your batch somewhere.";
            Logger.Error(message);
            _orchardServices.Notifier.Error(T(message));
            return 0;
         }

         var part = _orchardServices.ContentManager.Get(batchWrite.Id).As<PipelineConfigurationPart>();
         var writer = _processService.Resolve(part);
         writer.Load(part.Configuration, parameters);

         if (writer.Errors().Any()) {
            foreach (var error in writer.Errors()) {
               _orchardServices.Notifier.Add(NotifyType.Error, T(error));
               Logger.Error(error);
            }
            return 0;
         }

         try {

            // populate rows with a search or from the request
            var batchId = parameters["BatchId"];
            if (parameters.ContainsKey("count") && parameters["count"] == "All") {  // with a search
               process.Entities.First().Page = 0;
               process.Output().Provider = "internal";
               foreach (var field in process.Entities.First().GetAllFields()) {
                  field.Output = field.Alias == "BatchValue";
               }
               _orchardServices.WorkContext.Resolve<IRunTimeExecute>().Execute(process);

               foreach (var batchRow in process.Entities.First().Rows) {
                  var row = new CfgRow(new[] { "BatchId", "BatchValue" });
                  row["BatchId"] = batchId;
                  row["BatchValue"] = batchRow["BatchValue"];
                  rows.Add(row);
               }

            } else {
               var values = request.Form.GetValues("row") ?? request.QueryString.GetValues("row"); // from the request
               if (values == null) {
                  _orchardServices.Notifier.Warning(T("No rows submitted to bulk action."));
               } else {
                  foreach (var value in values) {
                     var row = new CfgRow(new[] { "BatchId", "BatchValue" });
                     row["BatchId"] = batchId;
                     row["BatchValue"] = value;
                     rows.Add(row);
                  }
               }
            }

            if (rows.Count > 0) {
               writer.Entities.First().Rows.AddRange(rows);
               _orchardServices.WorkContext.Resolve<IRunTimeExecute>().Execute(writer);
            }

            return rows.Count;
         } catch (Exception ex) {
            _orchardServices.Notifier.Error(T(ex.Message));
            Logger.Error(ex, ex.Message);
            return 0;
         }
      }
   }
}
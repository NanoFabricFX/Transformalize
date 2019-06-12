#region license
// Transformalize
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Linq;
using Autofac;
using Orchard.FileSystems.AppData;
using Orchard.Templates.Services;
using Orchard.UI.Notify;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Nulls;
using Pipeline.Web.Orchard.Modules;
using Transformalize.Providers.Solr.Autofac;
using Transformalize.Providers.Elasticsearch.Autofac;
using Transformalize.Providers.Clevest.Autofac;
using Transformalize.Providers.Excel.Autofac;
using Transformalize.Providers.GeoJson.Autofac;
using Transformalize.Providers.Json.Autofac;

namespace Pipeline.Web.Orchard.Impl {

   public class RunTimeDataReader : IRunTimeRun {
      private readonly IPipelineLogger _logger;
      private readonly IAppDataFolder _appDataFolder;
      private readonly ITemplateProcessor _templateProcessor;
      private readonly INotifier _notifier;

      public RunTimeDataReader(
          IPipelineLogger logger,
          IAppDataFolder appDataFolder,
          ITemplateProcessor templateProcessor,
          INotifier notifier) {
         _logger = logger;
         _appDataFolder = appDataFolder;
         _templateProcessor = templateProcessor;
         _notifier = notifier;
      }

      public IEnumerable<IRow> Run(Process process) {

         var nested = new ContainerBuilder();

         // Register Orchard CMS Stuff
         nested.RegisterInstance(_appDataFolder).As<IAppDataFolder>();
         nested.RegisterInstance(_templateProcessor).As<ITemplateProcessor>();
         nested.RegisterInstance(_notifier).As<INotifier>();

         var entity = process.Entities.First();
         nested.RegisterInstance(_logger).As<IPipelineLogger>();

         nested.RegisterCallback(new ContextModule(process).Configure);

         // providers
         nested.RegisterCallback(new AdoModule(process).Configure);
         nested.RegisterCallback(new SolrModule(process).Configure);
         nested.RegisterCallback(new ElasticsearchModule(process).Configure);
         nested.RegisterCallback(new InternalModule(process).Configure);
         nested.RegisterCallback(new FileModule(process).Configure);
         nested.RegisterCallback(new ExcelModule(process).Configure);
         nested.RegisterCallback(new WebModule(process).Configure);
         nested.RegisterCallback(new GeoJsonModule(process).Configure);
         nested.RegisterCallback(new KmlModule(process).Configure);
         nested.RegisterCallback(new ClevestModule(process).Configure);
         nested.RegisterCallback(new JsonModule(process).Configure);

         nested.RegisterCallback(new TransformModule().Configure);
         nested.RegisterCallback(new AdoTransformModule(process).Configure);
         nested.RegisterCallback(new ValidateModule().Configure);
         nested.RegisterCallback(new MapModule(process).Configure);
         nested.RegisterCallback(new TemplateModule(process, _templateProcessor).Configure);

         nested.RegisterType<NullOutputController>().Named<IOutputController>(entity.Key);
         nested.RegisterType<NullWriter>().Named<IWrite>(entity.Key);
         nested.RegisterType<NullUpdater>().Named<IUpdate>(entity.Key);
         nested.RegisterType<NullDeleteHandler>().Named<IEntityDeleteHandler>(entity.Key);
         nested.RegisterCallback(new EntityPipelineModule(process).Configure);

         using (var scope = nested.Build().BeginLifetimeScope()) {
            return scope.ResolveNamed<IPipeline>(entity.Key).Read();
         }
      }
   }
}
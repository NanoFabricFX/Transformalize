﻿#region license
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

using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Pipeline.Web.Orchard.Models;

namespace Pipeline.Web.Orchard.Drivers {

    public class ConfigurationPartDriver : ContentPartDriver<PipelineConfigurationPart> {

        protected override string Prefix {
            get { return Common.PipelineConfigurationName; }
        }

        //IMPORT, EXPORT
        protected override void Importing(PipelineConfigurationPart part, ImportContentContext context) {
            part.EditorMode = context.Attribute(part.PartDefinition.Name, "EditorMode");
            part.Configuration = context.Attribute(part.PartDefinition.Name, "Configuration");
            part.StartAddress = context.Attribute(part.PartDefinition.Name, "StartAddress");
            part.EndAddress = context.Attribute(part.PartDefinition.Name, "EndAddress");
            part.Runnable = Convert.ToBoolean(context.Attribute(part.PartDefinition.Name, "Runnable"));
            part.NeedsInputFile = Convert.ToBoolean(context.Attribute(part.PartDefinition.Name, "NeedsInputFile"));
            part.Modes = context.Attribute(part.PartDefinition.Name, "Modes");
            part.PlaceHolderStyle = context.Attribute(part.PartDefinition.Name, "PlaceHolderStyle");
            part.ClientSideSorting = Convert.ToBoolean(context.Attribute(part.PartDefinition.Name, "ClientSideSorting"));
            part.MapCircleRadius = Convert.ToInt32(context.Attribute(part.PartDefinition.Name, "MapCircleRadius"));
            part.MapCircleOpacity = Convert.ToDouble(context.Attribute(part.PartDefinition.Name, "MapCircleOpacity"));
            part.PageSizes = context.Attribute(part.PartDefinition.Name, "PageSizes");
            part.MapSizes = context.Attribute(part.PartDefinition.Name, "MapSizes");
            part.EnableInlineParameters = Convert.ToBoolean(context.Attribute(part.PartDefinition.Name, "EnableInlineParameters"));
            part.Migrated = true;
        }

        protected override void Exporting(PipelineConfigurationPart part, ExportContentContext context) {
            if (part.Migrated) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("EditorMode", part.EditorMode);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Configuration", part.Configuration);
                context.Element(part.PartDefinition.Name).SetAttributeValue("StartAddress", part.StartAddress);
                context.Element(part.PartDefinition.Name).SetAttributeValue("EndAddress", part.EndAddress);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Runnable", part.Runnable);
                context.Element(part.PartDefinition.Name).SetAttributeValue("NeedsInputFile", part.NeedsInputFile);
                context.Element(part.PartDefinition.Name).SetAttributeValue("ClientSideSorting", part.ClientSideSorting);
                context.Element(part.PartDefinition.Name).SetAttributeValue("MapCircleRadius", part.MapCircleRadius);
                context.Element(part.PartDefinition.Name).SetAttributeValue("MapCircleOpacity", part.MapCircleOpacity);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Modes", part.Modes);
                context.Element(part.PartDefinition.Name).SetAttributeValue("PlaceHolderStyle", part.PlaceHolderStyle);
                context.Element(part.PartDefinition.Name).SetAttributeValue("PageSizes", part.PageSizes);
                context.Element(part.PartDefinition.Name).SetAttributeValue("MapSizes", part.MapSizes);
                context.Element(part.PartDefinition.Name).SetAttributeValue("EnableInlineParameters", part.EnableInlineParameters);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Migrated", true);
                
            } else {
                context.Element(part.PartDefinition.Name).SetAttributeValue("EditorMode", part.Record.EditorMode);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Configuration", part.Record.Configuration);
                context.Element(part.PartDefinition.Name).SetAttributeValue("StartAddress", part.Record.StartAddress);
                context.Element(part.PartDefinition.Name).SetAttributeValue("EndAddress", part.Record.EndAddress);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Runnable", part.Record.Runnable);
                context.Element(part.PartDefinition.Name).SetAttributeValue("NeedsInputFile", part.Record.NeedsInputFile);
                context.Element(part.PartDefinition.Name).SetAttributeValue("ClientSideSorting", part.Record.ClientSideSorting);

                context.Element(part.PartDefinition.Name).SetAttributeValue("MapCircleRadius", part.Record.MapCircleRadius);
                context.Element(part.PartDefinition.Name).SetAttributeValue("MapCircleOpacity", part.Record.MapCircleOpacity);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Modes", part.Record.Modes);
                context.Element(part.PartDefinition.Name).SetAttributeValue("PlaceHolderStyle", part.Record.PlaceHolderStyle);
                context.Element(part.PartDefinition.Name).SetAttributeValue("PageSizes", part.Record.PageSizes);
                context.Element(part.PartDefinition.Name).SetAttributeValue("MapSizes", part.Record.MapSizes);
                context.Element(part.PartDefinition.Name).SetAttributeValue("EnableInlineParameters", part.Record.EnableInlineParameters);
                context.Element(part.PartDefinition.Name).SetAttributeValue("Migrated", false);
            }
        }

        //GET EDITOR
        protected override DriverResult Editor(PipelineConfigurationPart part, dynamic shapeHelper) {
            return ContentShape("Parts_" + Prefix + "_Edit", () => shapeHelper
                .EditorTemplate(TemplateName: "Parts/" + Prefix, Model: part, Prefix: Prefix));
        }

        //POST EDITOR
        protected override DriverResult Editor(PipelineConfigurationPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}

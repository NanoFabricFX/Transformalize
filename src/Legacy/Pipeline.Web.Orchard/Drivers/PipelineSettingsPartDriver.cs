﻿using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Pipeline.Web.Orchard.Models;

namespace Pipeline.Web.Orchard.Drivers {

    public class PipelineSettingsPartDriver : ContentPartDriver<PipelineSettingsPart> {

        public PipelineSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override void Importing(PipelineSettingsPart part, ImportContentContext context) {
            part.Record.EditorTheme = context.Attribute(part.PartDefinition.Name, "EditorTheme");
            part.Record.MapBoxToken = context.Attribute(part.PartDefinition.Name, "MapBoxToken");
            part.Record.StartingLatitude = double.Parse(context.Attribute(part.PartDefinition.Name, "StartingLatitude"));
            part.Record.StartingLongitude = double.Parse(context.Attribute(part.PartDefinition.Name, "StartingLongitude"));
        }

        protected override void Exporting(PipelineSettingsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("EditorTheme", part.Record.EditorTheme);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MapBoxToken", part.Record.MapBoxToken);
            context.Element(part.PartDefinition.Name).SetAttributeValue("StartingLatitude", part.Record.StartingLatitude);
            context.Element(part.PartDefinition.Name).SetAttributeValue("StartingLongitude", part.Record.StartingLongitude);
        }

        protected override string Prefix { get { return Common.PipelineSettingsName; } }

        // GET EDITOR
        protected override DriverResult Editor(PipelineSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_" + Prefix + "_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/" + Prefix, Model: part, Prefix: Prefix))
                    .OnGroup(Common.ModuleGroupName);
        }

        // POST EDITOR
        protected override DriverResult Editor(PipelineSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_" + Prefix + "_Edit", () => {
                updater.TryUpdateModel(part, Prefix, null, null);
                return shapeHelper.EditorTemplate(TemplateName: "Parts/" + Prefix, Model: part, Prefix: Prefix);
            }).OnGroup(Common.ModuleGroupName);
        }
    }
}
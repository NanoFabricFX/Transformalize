﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentPermissions.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.UI.Notify;
using Pipeline.Web.Orchard.Models;
using Pipeline.Web.Orchard.Services.Contracts;

namespace Pipeline.Web.Orchard.Services {

   public class FileService : IFileService {

      private readonly IOrchardServices _orchardServices;
      private readonly IAppDataFolder _appDataFolder;
      private readonly ITagService _tagService;

      public FileService(
          IOrchardServices orchardServices,
          IAppDataFolder appDataFolder,
          ITagService tagService) {
         _orchardServices = orchardServices;
         _appDataFolder = appDataFolder;
         _tagService = tagService;
         Logger = NullLogger.Instance;
         T = NullLocalizer.Instance;
      }

      public Localizer T { get; set; }
      public ILogger Logger { get; set; }

      public PipelineFilePart Upload(HttpPostedFileBase input, string role, string tag, int number) {

         var part = _orchardServices.ContentManager.New<PipelineFilePart>(Common.PipelineFileName);
         var permissions = part.As<ContentPermissionsPart>();
         permissions.Enabled = true;

         permissions.ViewContent = "Administrator";
         permissions.EditContent = "Administrator";
         permissions.DeleteContent = "Administrator";

         permissions.ViewOwnContent = "Authenticated";
         permissions.EditOwnContent = "Authenticated";
         permissions.DeleteOwnContent = "Authenticated";

         if (role != "Private") {
            permissions.ViewContent += "," + role;
         }

         part.OriginalName = input.FileName;
         part.As<TitlePart>().Title = input.FileName;

         if (tag != string.Empty) {
            var tags = tag.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(t => !t.Equals(Common.AllTag, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (tags.Any()) {
               _tagService.UpdateTagsForContentItem(part.ContentItem, tags);
            }
         }

         var exportFile = Common.GetSafeFileName(_orchardServices.WorkContext.CurrentUser.UserName, Path.GetFileName(input.FileName), number);

         var path = Common.GetAppFolder();
         if (!_appDataFolder.DirectoryExists(path)) {
            _appDataFolder.CreateDirectory(path);
         }

         part.FullPath = _appDataFolder.MapPath(_appDataFolder.Combine(path, exportFile));
         part.Direction = "In";
         input.SaveAs(part.FullPath);
         _orchardServices.ContentManager.Create(part);

         _orchardServices.Notifier.Information(T("{0} uploaded successfully.", Path.GetFileName(input.FileName)));

         return part;
      }

      public PipelineFilePart Create(string fileName) {
         var part = _orchardServices.ContentManager.New<PipelineFilePart>(Common.PipelineFileName);
         part.FullPath = fileName;
         part.Direction = "Out";
         _orchardServices.ContentManager.Create(part);
         return part;
      }

      public PipelineFilePart Create(string name, string extension) {

         var file = Common.GetSafeFileName(_orchardServices.WorkContext.CurrentUser.UserName, name, extension);

         var fileFolder = Common.GetAppFolder();
         if (!_appDataFolder.DirectoryExists(fileFolder)) {
            _appDataFolder.CreateDirectory(fileFolder);
         }

         if (!_appDataFolder.DirectoryExists(fileFolder)) {
            _appDataFolder.CreateDirectory(fileFolder);
         }

         var path = _appDataFolder.Combine(fileFolder, file);
         _appDataFolder.CreateFile(path, string.Empty);

         var part = _orchardServices.ContentManager.New<PipelineFilePart>(Common.PipelineFileName);
         part.FullPath = _appDataFolder.MapPath(path);
         part.Direction = "Out";
         _orchardServices.ContentManager.Create(part);

         return part;
      }

      /// <summary>
      /// Apply security after the list() method
      /// </summary>
      /// <returns></returns>
      public IEnumerable<PipelineFilePart> List(string tag, int top = 15) {

         if (string.IsNullOrEmpty(tag) || tag.Equals(Common.AllTag, StringComparison.OrdinalIgnoreCase)) {
            return _orchardServices.ContentManager.Query<PipelineFilePart, PipelineFilePartRecord>(VersionOptions.Published)
                .Join<CommonPartRecord>()
                .OrderByDescending(cpr => cpr.CreatedUtc)
                .Slice(0, top);
         }

         return _orchardServices.ContentManager.Query<PipelineFilePart, PipelineFilePartRecord>(VersionOptions.Published)
             .Join<TagsPartRecord>()
             .Where(tpr => tpr.Tags.Any(t => t.TagRecord.TagName.Equals(tag, StringComparison.OrdinalIgnoreCase)))
             .Join<CommonPartRecord>()
             .OrderByDescending(cpr => cpr.CreatedUtc)
             .Slice(0, top);

      }

      public PipelineFilePart Get(int id) {
         return _orchardServices.ContentManager.Get(id).As<PipelineFilePart>();
      }

      public void Delete(PipelineFilePart part) {
         _orchardServices.ContentManager.Remove(part.ContentItem);
         if (string.IsNullOrEmpty(part.FullPath)) {
            _orchardServices.Notifier.Add(NotifyType.Warning, T("The file path associated with this content item is empty."));
         } else {
            var fileInfo = new FileInfo(part.FullPath);
            if (fileInfo.Exists) {
               fileInfo.Delete();
               _orchardServices.Notifier.Add(NotifyType.Information, T("The file {0} is no more.", part.FileName()));
            } else {
               _orchardServices.Notifier.Add(NotifyType.Warning, T("The file associated with this content item no longer exists."));
            }
         }

      }
   }
}
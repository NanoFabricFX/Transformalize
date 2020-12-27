﻿using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Pipeline.Web.Orchard.Models {
    public class PipelineSettingsPart : ContentPart<PipelineSettingsPartRecord> {

        private readonly string[] _themes = { "3024-day", "3024-night", "ambiance-mobile", "ambiance", "base16-dark", "base16-light", "blackboard", "cobalt", "eclipse", "elegant", "erlang-dark", "lesser-dark", "mbo", "mdn-like", "midnight", "monokai", "neat", "neo", "night", "paraiso-dark", "paraiso-light", "pastel-on-dark", "rubyblue", "solarized", "the-matrix", "tomorrow-night-eighties", "twilight", "vibrant-ink", "xq-dark", "xq-light" };

        public PipelineSettingsPart() {
            EditorThemes = new List<SelectListItem>();
            foreach (var theme in _themes) {
                EditorThemes.Add(new SelectListItem { Selected = false, Text = theme, Value = theme });
            }
        }

        public List<SelectListItem> EditorThemes { get; set; }

        public string EditorTheme {
            get { return string.IsNullOrEmpty(Record.EditorTheme) ? "cobalt" : Record.EditorTheme; }
            set { Record.EditorTheme = value; }
        }

        public string MapBoxToken {
            get { return Record.MapBoxToken; }
            set { Record.MapBoxToken = value; }
        }

        public double StartingLatitude {
            get { return Record.StartingLatitude; }
            set { Record.StartingLatitude = value; }
        }

        public double StartingLongitude {
            get { return Record.StartingLongitude; }
            set { Record.StartingLongitude = value; }
        }

        public bool IsValid() {
            return true;
        }
    }
}
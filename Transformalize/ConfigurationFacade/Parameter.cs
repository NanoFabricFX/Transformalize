﻿#region license
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
using Cfg.Net;
using System.Collections.Generic;
using System.Linq;

namespace Transformalize.ConfigurationFacade {

    public class Parameter : CfgNode {

        [Cfg]
        public string Entity { get; set; }

        [Cfg]
        public string Field { get; set; }

        [Cfg]
        public string Name { get; set; }

        [Cfg]
        public string InvalidCharacters { get; set; }

        [Cfg]
        public string Value { get; set; }

        [Cfg]
        public string Scope { get; set; }

        [Cfg]
        public string Input { get; set; }

        [Cfg]
        public string Prompt { get; set; }

        [Cfg]
        public string Map { get; set; }

        [Cfg]
        public string T { get; set; }

        [Cfg]
        public List<Operation> Transforms { get; set; }

        [Cfg]
        public string Type { get; set; }

        [Cfg]
        public string Label { get; set; }

        [Cfg]
        public string Format { get; set; }

        [Cfg]
        public string Width { get; set; }

        [Cfg]
        public string Multiple { get; set; }

        [Cfg]
        public string Required { get; set; }

        [Cfg]
        public string Sticky { get; set; }

        public Configuration.Parameter ToParameter() {
            var parameter = new Configuration.Parameter {
                Entity = this.Entity,
                Field = this.Field,
                InvalidCharacters = this.InvalidCharacters,
                Name = this.Name,
                Value = this.Value,
                Scope = this.Scope,
                Map = this.Map,
                Type = this.Type,
                Label = this.Label,
                Format = this.Format
            };

            int.TryParse(this.Width, out var width);
            parameter.Width = width;

            bool.TryParse(this.Multiple, out var multiple);
            parameter.Multiple = multiple;

            bool.TryParse(this.Required, out var required);
            parameter.Required = required;

            bool.TryParse(this.Sticky, out var sticky);
            parameter.Sticky = sticky;

            bool.TryParse(this.Input, out var input);
            parameter.Input = input;

            bool.TryParse(this.Prompt, out var prompt);
            parameter.Prompt = prompt;

            parameter.Transforms = this.Transforms.Select(o => o.ToOperation()).ToList();

            return parameter;
        }


    }

}
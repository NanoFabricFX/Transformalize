﻿#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
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
using Humanizer;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Transforms;

namespace Transformalize.Transform.Humanizer {
    public class FromMetricTransform : BaseTransform {
        private readonly Func<IRow, object> _transform;
        private readonly Field _input;
        private readonly HashSet<string> _warnings = new HashSet<string>();

        public FromMetricTransform(IContext context) : base(context, context.Field.Type) {

            _input = SingleInput();

            switch (_input.Type) {
                case "string":
                    _transform = (row) => {
                        var input = (string)row[_input];
                        if (!IsInvalidMetricNumeral(input))
                            return Context.Field.Convert(input.FromMetric());

                        var warning = $"The value {input} is an invalid metric numeral.";
                        if (_warnings.Add(warning)) {
                            context.Warn(warning);
                        }
                        var numbers = GetNumbers(input);
                        return Context.Field.Convert(numbers.Length > 0 ? numbers : "0");
                    };
                    break;
                default:
                    _transform = (row) => row[_input];
                    break;
            }
        }

        public override IRow Transform(IRow row) {
            row[Context.Field] = _transform(row);
            Increment();
            return row;
        }

        public override void Dispose() {
            _warnings.Clear();
        }

        private static string GetNumbers(string input) {
            return new string(input.Where(char.IsDigit).ToArray());
        }

        // The following code is from Humanizer
        // Humanizer is by Alois de Gouvello https://github.com/aloisdg
        // The MIT License (MIT)
        // Copyright (c) 2015 Alois de Gouvello

        private static readonly List<char>[] Symbols = {
            new List<char> { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' },
            new List<char> { 'm', 'μ', 'n', 'p', 'f', 'a', 'z', 'y' }
        };


        private static readonly Dictionary<char, string> Names = new Dictionary<char, string> {
            {'Y', "yotta" }, {'Z', "zetta" }, {'E', "exa" }, {'P', "peta" }, {'T', "tera" }, {'G', "giga" }, {'M', "mega" }, {'k', "kilo" },
            {'m', "milli" }, {'μ', "micro" }, {'n', "nano" }, {'p', "pico" }, {'f', "femto" }, {'a', "atto" }, {'z', "zepto" }, {'y', "yocto" }
        };

        private static string ReplaceNameBySymbol(string input) {
            return Names.Aggregate(input, (current, name) =>
                current.Replace(name.Value, name.Key.ToString()));
        }

        private static bool IsInvalidMetricNumeral(string input) {
            input = input.Trim();
            input = ReplaceNameBySymbol(input);

            double number;
            var index = input.Length - 1;
            var last = input[index];
            var isSymbol = Symbols[0].Contains(last) || Symbols[1].Contains(last);
            return !double.TryParse(isSymbol ? input.Remove(index) : input, out number);
        }

    }
}
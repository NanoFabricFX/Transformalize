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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Excel;
using Transformalize.Contracts;
using Transformalize.Extensions;

namespace Transformalize.Provider.Excel {
    public class ExcelLineReader {
        private readonly IConnectionContext _context;
        private readonly int _lines;
        private readonly FileInfo _fileInfo;

        public ExcelLineReader(IConnectionContext context, int lines = 100) {
            _fileInfo = new FileInfo(context.Connection.File);
            _context = context;
            _lines = lines;
        }

        public IEnumerable<object[]> Read() {

            // todo: fix duplication here

            using (var fileStream = File.Open(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                var isBinary = _fileInfo.Extension.ToLower() == ".xls";
                using (var reader = isBinary ? ExcelReaderFactory.CreateBinaryReader(fileStream) : ExcelReaderFactory.CreateOpenXmlReader(fileStream)) {

                    if (reader == null) {
                        yield break;
                    }

                    var emptyDetector = new StringBuilder();

                    bool foundData = false;

                    while (reader.Read()) {
                        emptyDetector.Clear();
                        var row = new List<object>();
                        for (var i = 0; i < reader.FieldCount; i++) {
                            if (i == _lines)
                                break;
                            var value = reader.GetValue(i);
                            row.Add(value);
                            emptyDetector.Append(value);
                        }
                        emptyDetector.Trim(" ");
                        if (!emptyDetector.ToString().Equals(string.Empty)) {
                            foundData = true;
                            yield return row.ToArray();
                        }
                    }

                    // second attempt, use data set
                    if (!foundData) {
                        var dataSet = reader.AsDataSet();
                        using (var r = dataSet.CreateDataReader()) {
                            while (r.Read()) {
                                emptyDetector.Clear();
                                var row = new List<object>();
                                for (var i = 0; i < r.FieldCount; i++) {
                                    if (i == _lines)
                                        break;
                                    var value = r.GetValue(i);
                                    row.Add(value);
                                    emptyDetector.Append(value);
                                }
                                emptyDetector.Trim(" ");
                                if (!emptyDetector.ToString().Equals(string.Empty)) {
                                    yield return row.ToArray();
                                }

                            }
                        }
                    }
                }
            }
        }

    }
}

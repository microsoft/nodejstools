/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Parsing;

namespace AnalysisTests {
    class AnalysisFile {
        public readonly string Filename, Content;

        public AnalysisFile(string filename, string content = "") {
            Filename = filename;
            Content = content;
        }

        public static AnalysisFile PackageJson(string path, string mainFile) {
            return new AnalysisFile(path, mainFile);
        }
    }

    static class Analysis {
        public static JsAnalyzer Analyze(params AnalysisFile[] files) {
            return Analyze(null, files);
        }

        public static JsAnalyzer Analyze(AnalysisLimits limits, params AnalysisFile[] files) {
            Dictionary<string, IJsProjectEntry> entries = new Dictionary<string, IJsProjectEntry>();
            var analyzer = new JsAnalyzer(limits);

            foreach (var file in files) {
                if (Path.GetFileName(file.Filename).Equals("package.json", StringComparison.OrdinalIgnoreCase)) {
                    analyzer.AddPackageJson(file.Filename, file.Content);
                } else {
                    var projEntry = analyzer.AddModule(file.Filename);
                    entries[file.Filename] = projEntry;
                }
            }

            foreach (var file in files) {
                if (!Path.GetFileName(file.Filename).Equals("package.json", StringComparison.OrdinalIgnoreCase)) {
                    var source = Analysis.GetSourceUnit(file.Content);
                    Analysis.Prepare(
                        entries[file.Filename],
                        source
                    );
                }
            }

            foreach (var file in files) {
                
                if (!Path.GetFileName(file.Filename).Equals("package.json", StringComparison.OrdinalIgnoreCase)) {
                    ((IGroupableAnalysisProjectEntry)entries[file.Filename]).Analyze(CancellationToken.None, true);
                }
            }

            foreach (var file in files) {
                IJsProjectEntry projEntry;
                if (entries.TryGetValue(file.Filename, out projEntry)) {
                    projEntry.AnalysisGroup.AnalyzeQueuedEntries(CancellationToken.None);
                    break;
                }
            }

            return analyzer;
        }

        public static void Prepare(IJsProjectEntry entry, string code) {
            Prepare(entry, GetSourceUnit(code));
        }

        public static void Prepare(IJsProjectEntry entry, TextReader sourceUnit) {
            var parser = new JSParser(sourceUnit.ReadToEnd());
            var ast = parser.Parse(new CodeSettings());
            entry.UpdateTree(ast, null);
        }

        public static TextReader GetSourceUnit(string text) {
            return new StringReader(text);
        }

#if DEBUG
        public static string DumpAnalysis(string directory) {
            var analyzer = Analysis.Analyze(directory);

            return DumpAnalysis(analyzer);
        }

        public static string DumpAnalysis(JsAnalyzer analyzer) {
            var entries = analyzer.AllModules.ToArray();
            Array.Sort(entries, (x, y) => String.Compare(x.FilePath, y.FilePath));
            StringBuilder analysis = new StringBuilder();
            foreach (var entry in entries) {
                if (entry.Analysis != null) {
                    analysis.AppendLine(entry.Analysis.Dump());
                }
            }

            return analysis.ToString();
        }
#endif

        public static JsAnalyzer Analyze(string directory, AnalysisLimits limits = null) {
            List<AnalysisFile> files = new List<AnalysisFile>();
            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)) {
                if (String.Equals(Path.GetExtension(file), ".js", StringComparison.OrdinalIgnoreCase)) {
                    files.Add(new AnalysisFile(file, File.ReadAllText(file)));
                } else if (String.Equals(Path.GetFileName(file), "package.json", StringComparison.OrdinalIgnoreCase)) {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Dictionary<string, object> json;
                    try {
                        json = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(file));
                    } catch {
                        continue;
                    }

                    object mainFile;
                    if (json != null && json.TryGetValue("main", out mainFile) && mainFile is string) {
                        files.Add(AnalysisFile.PackageJson(file, (string)mainFile));
                    }
                }
            }

            return Analyze(limits, files.ToArray());
        }
    }
}

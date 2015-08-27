//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
            return Analyze(null, null, files);
        }

        public static JsAnalyzer Analyze(AnalysisLimits limits, params AnalysisFile[] files) {
            return Analyze(limits, null, files);
        }

        public static JsAnalyzer Analyze(AnalysisLimits limits, Action parseCallback, params AnalysisFile[] files) {
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

            if (parseCallback != null) {
                parseCallback();
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

        public static JsAnalyzer Analyze(string directory, AnalysisLimits limits = null, Action parseCallback = null) {
            List<AnalysisFile> files = new List<AnalysisFile>();
            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)) {
                if (String.Equals(Path.GetExtension(file), ".js", StringComparison.OrdinalIgnoreCase)) {
                    var relativeFile = file.Substring(directory.Length);
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

            return Analyze(limits, parseCallback, files.ToArray());
        }
    }
}

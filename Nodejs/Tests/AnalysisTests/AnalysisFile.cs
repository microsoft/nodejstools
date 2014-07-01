using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Analysis;

namespace AnalysisTests {
    class AnalysisFile {
        public readonly string Filename, Content;

        public AnalysisFile(string filename, string content) {
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
                    var source = AnalysisTests.GetSourceUnit(file.Content);
                    AnalysisTests.Prepare(
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
    }
}

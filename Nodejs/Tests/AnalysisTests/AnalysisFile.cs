using System;
using System.Collections.Generic;
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
    }

    static class Analysis {
        public static Dictionary<string, IJsProjectEntry> Analyze(params AnalysisFile[] files) {
            Dictionary<string, IJsProjectEntry> entries = new Dictionary<string, IJsProjectEntry>();
            var analyzer = new JsAnalyzer();

            foreach (var file in files) {
                var projEntry = analyzer.AddModule(file.Filename);
                entries[file.Filename] = projEntry;
            }

            foreach (var file in files) {
                var source = AnalysisTests.GetSourceUnit(file.Content);
                AnalysisTests.Prepare(
                    entries[file.Filename],
                    source
                );
            }

            foreach (var file in files) {
                entries[file.Filename].Analyze(CancellationToken.None);
            }

            return entries;
        }
    }
}

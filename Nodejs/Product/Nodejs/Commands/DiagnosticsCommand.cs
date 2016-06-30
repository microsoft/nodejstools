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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.Logging;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Commands {
    internal sealed class DiagnosticsCommand : Command {
        private static readonly string[] interestingDteProperties = new[] {
            "StartupFile",
            "WorkingDirectory",
            "PublishUrl",
            "SearchPath",
            "CommandLineArguments"
        };

        public DiagnosticsCommand(IServiceProvider serviceProvider) { }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidDiagnostics; }
        }

        public override void DoCommand(object sender, EventArgs args) {
            var dlg = new DiagnosticsForm("Gathering data...");

            ThreadPool.QueueUserWorkItem(x => {
                var data = GetData();
                try {
                    dlg.BeginInvoke((Action)(() => {
                        dlg.TextBox.Text = data;
                        dlg.TextBox.SelectAll();
                    }));
                } catch (InvalidOperationException) {
                    // Window has been closed already
                }
            });
            dlg.ShowDialog();
        }

        private string GetData() {
            var res = new StringBuilder();
            res.AppendLine("Use Ctrl-C to copy contents");
            res.AppendLine();
            res.AppendLine(GetSolutionInfo());
            res.AppendLine(GetEventsAndStatsInfo());
            res.AppendLine(GetLoadedAssemblyInfo());
            res.AppendLine(GetAnalysisLevelInfo());
            return res.ToString();
        }

        private static string GetSolutionInfo() {
            var res = new StringBuilder();

            var dte = (EnvDTE.DTE)NodejsPackage.GetGlobalService(typeof(EnvDTE.DTE));
            res.AppendLine("Projects:");

            foreach (EnvDTE.Project project in dte.Solution.Projects) {
                res.AppendLine(Indent(4, GetProjectInfo(project)));
            }

            return res.ToString();
        }

        private static string GetProjectInfo(EnvDTE.Project project) {
            var res = new StringBuilder();
            string name;
            try {
                // Some projects will throw rather than give us a unique
                // name. They are not ours, so we will ignore them.
                name = project.UniqueName;
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }
                bool isNodejsProject = false;
                try {
                    isNodejsProject = Utilities.GuidEquals(Guids.NodejsProjectFactoryString, project.Kind);
                } catch (Exception ex2) {
                    if (ex2.IsCriticalException()) {
                        throw;
                    }
                }
                if (isNodejsProject) {
                    // Actually, it was one of our projects, so we do care
                    // about the exception. We'll add it to the output,
                    // rather than crashing.
                    res.AppendLine("Project: " + ex.Message);
                    res.AppendLine(Indent(8, "Kind: Node.js"));
                }
                return res.ToString();
            }
            res.AppendLine("Project: " + name);
            res.AppendLine(Indent(4, GetProjectPropertiesInfo(project)));
            return res.ToString();
        }

        private static string GetProjectPropertiesInfo(EnvDTE.Project project) {
            var res = new StringBuilder();
            if (Utilities.GuidEquals(Guids.NodejsBaseProjectFactoryString, project.Kind)) {
                res.AppendLine("Kind: Node.js");
                foreach (var prop in interestingDteProperties) {
                    res.AppendLine(prop + ": " + GetProjectProperty(project, prop));
                }
                var njsProj = project.GetNodejsProject();
                if (njsProj != null) {
                    res.AppendLine(GetNodeJsProjectProperties(project));
                }
            } else {
                res.AppendLine("Kind: " + project.Kind);
            }
            return res.ToString();
        }

        private static string GetNodeJsProjectProperties(Project.NodejsProjectNode project) {
            var res = new StringBuilder();

            var jsAnalyzer = project.Analyzer;
            if (jsAnalyzer != null) {
                res.AppendLine("Analysis Log: ");
                using (var writer = new StringWriter(res)) {
                    jsAnalyzer.DumpLog(writer);
                }
            }

            return res.ToString();
        }
        private static string GetProjectProperty(EnvDTE.Project project, string name) {
            try {
                var item = project.Properties.Item(name);
                if (item != null && item.Value != null) {
                    return item.Value.ToString();
                }
            } catch {
                // noop
            }
            return "<undefined>";
        }

        private static string GetEventsAndStatsInfo() {
            var res = new StringBuilder();
            res.AppendLine("Logged events/stats:");

            try {
                var inMemLogger = NodejsPackage.Instance.GetComponentModel().GetService<InMemoryLogger>();
                res.AppendLine(inMemLogger.ToString());
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }
                res.AppendLine(Indent(4, "Failed to access event log."));
                res.AppendLine(ex.ToString());
            }
            return res.ToString();
        }

        private static string GetLoadedAssemblyInfo() {
            var res = new StringBuilder();
            res.AppendLine("Loaded assemblies:");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(assem => assem.FullName)) {
                var assemFileVersion = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

                res.AppendLine(Indent(4, string.Format("{0}, FileVersion={1}",
                    assembly.FullName,
                    assemFileVersion == null ? "(null)" : assemFileVersion.Version)));
            }
            return res.ToString();
        }

        private static string GetAnalysisLevelInfo() {
            var res = new StringBuilder();
            res.AppendLine(String.Format("Analysis Level: {0}", NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLevel.ToString()));
            res.AppendLine();
            if (NodejsPackage.Instance._analyzer != null) {
                var jsAnalyzer = NodejsPackage.Instance._analyzer;
                res.AppendLine("Default Analysis Log: ");

                using (var writer = new StringWriter(res)) {
                    jsAnalyzer.DumpLog(writer);
                }
            }

            res.AppendLine(string.Format("IntelliSense Completion Only Tab or Enter to Commit: {0}", NodejsPackage.Instance.IntellisenseOptionsPage.OnlyTabOrEnterToCommit));
            return res.ToString();
        }

        private static string Indent(int count, string text) {
            var indent = new string(' ', count);
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var indentedText = lines.Select(line =>
                string.IsNullOrWhiteSpace(line) ? line : indent + line);
            return string.Join(Environment.NewLine, indentedText);
        }
    }
}

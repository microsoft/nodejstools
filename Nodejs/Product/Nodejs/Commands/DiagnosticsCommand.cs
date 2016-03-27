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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Logging;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Commands {
    /// <summary>
    /// Provides the command for starting a file or the start item of a project in the REPL window.
    /// </summary>
    internal sealed class DiagnosticsCommand : Command {
        private readonly IServiceProvider _serviceProvider;

        public DiagnosticsCommand(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
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

            StringBuilder res = new StringBuilder();
            res.AppendLine("Use Ctrl-C to copy contents");
            res.AppendLine();

            var dte = (EnvDTE.DTE)NodejsPackage.GetGlobalService(typeof(EnvDTE.DTE));
            res.AppendLine("Projects: ");

            var projects = dte.Solution.Projects;
            var interestingDteProperties = new[] { "StartupFile", "WorkingDirectory", "PublishUrl", "SearchPath", "CommandLineArguments" };
            //var interestingProjectProperties = new[] { "AnalysisLevel" };

            foreach (EnvDTE.Project project in projects) {
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
                        res.AppendLine("    Project: " + ex.Message);
                        res.AppendLine("        Kind: Node.js");
                    }
                    continue;
                }
                res.AppendLine("    Project: " + name);

                if (Utilities.GuidEquals(Guids.NodejsBaseProjectFactoryString, project.Kind)) {
                    res.AppendLine("        Kind: Node.js");

                    foreach (var prop in interestingDteProperties) {
                        res.AppendLine("        " + prop + ": " + GetProjectProperty(project, prop));
                    }
                    var njsProj = project.GetNodejsProject();
                    if (njsProj != null) {
                        var jsAnalyzer = njsProj.Analyzer;
                        res.AppendLine("Analysis Log: ");
                        
                        using (StringWriter writer = new StringWriter(res)) {
                            jsAnalyzer.DumpLog(writer);
                        }

                        //foreach (var prop in interestingProjectProperties) {
                        //    var propValue = njsProj.GetProjectProperty(prop);
                        //    if (propValue != null) {
                        //        res.AppendLine("        " + prop + ": " + propValue);
                        //    }
                        //}
                    }
                } else {
                    res.AppendLine("        Kind: " + project.Kind);
                }

                res.AppendLine();


            }

            try {
                res.AppendLine("Logged events/stats:");
                var inMemLogger = NodejsPackage.Instance.GetComponentModel().GetService<InMemoryLogger>();
                res.AppendLine(inMemLogger.ToString());
                res.AppendLine();
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }
                res.AppendLine("  Failed to access event log.");
                res.AppendLine(ex.ToString());
                res.AppendLine();
            }

            res.AppendLine("Loaded assemblies:");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(assem => assem.FullName)) {
                var assemFileVersion = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

                res.AppendLine(string.Format("  {0}, FileVersion={1}",
                    assembly.FullName,
                    assemFileVersion == null ? "(null)" : assemFileVersion.Version
                ));
            }
            res.AppendLine();
            

            res.AppendLine(String.Format("Analysis Level: {0}", NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLevel.ToString()));
            res.AppendLine();
            if(NodejsPackage.Instance._analyzer != null) {
                var jsAnalyzer = NodejsPackage.Instance._analyzer;
                res.AppendLine("Default Analysis Log: ");

                using (StringWriter writer = new StringWriter(res)) {
                    jsAnalyzer.DumpLog(writer);
                }
            }

            res.AppendLine(String.Format("IntelliSense Completion Only Tab or Enter to Commit: {0}", NodejsPackage.Instance.IntellisenseOptionsPage.OnlyTabOrEnterToCommit));
            res.AppendLine();

            return res.ToString();
        }

        private static string GetProjectProperty(EnvDTE.Project project, string name) {
            try {
                return project.Properties.Item(name).Value.ToString();
            } catch {
                return "<undefined>";
            }
        }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidDiagnostics; }
        }
    }
}

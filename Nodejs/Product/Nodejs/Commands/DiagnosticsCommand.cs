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
                        var jsAnalyzer = njsProj.Analyzer.Project;
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
                var inMemLogger = NodejsPackage.ComponentModel.GetService<InMemoryLogger>();
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
                var jsAnalyzer = NodejsPackage.Instance._analyzer.Project;
                res.AppendLine("Default Analysis Log: ");

                using (StringWriter writer = new StringWriter(res)) {
                    jsAnalyzer.DumpLog(writer);
                }
            }

			res.AppendLine(String.Format("Intellisense Completion Committed By: {0}", NodejsPackage.Instance.IntellisenseOptionsPage.CompletionCommittedBy));
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

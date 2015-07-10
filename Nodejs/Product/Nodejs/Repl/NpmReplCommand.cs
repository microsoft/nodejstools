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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using SR = Microsoft.NodejsTools.Project.SR;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.NodejsTools.Repl {
#if INTERACTIVE_WINDOW
    using IReplCommand = IInteractiveWindowCommand;
    using IReplWindow = IInteractiveWindow;    
#endif

    [Export(typeof(IReplCommand))]
    class NpmReplCommand : IReplCommand {
        #region IReplCommand Members

        public async Task<ExecutionResult> Execute(IReplWindow window, string arguments) {
            string projectPath = string.Empty;
            string npmArguments = arguments.Trim(' ', '\t');

            // Parse project name/directory in square brackets
            if (npmArguments.StartsWith("[")) {
                var match = Regex.Match(npmArguments, @"(?:[[]\s*\""?\s*)(.*?)(?:\s*\""?\s*[]]\s*)");
                projectPath = match.Groups[1].Value;
                npmArguments = npmArguments.Substring(match.Length);
            }

            // Include spaces on either side of npm arguments so that we can more simply detect arguments
            // at beginning and end of string (e.g. '--global')
            npmArguments = string.Format(" {0} ", npmArguments);

            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            IEnumerable<IVsProject> loadedProjects = solution.EnumerateLoadedProjects(onlyNodeProjects: true);

            var projectNameToDirectoryDictionary = new Dictionary<string, Tuple<string, IVsHierarchy>>(StringComparer.OrdinalIgnoreCase);
            foreach (IVsProject project in loadedProjects) {
                var hierarchy = (IVsHierarchy)project;
                object extObject;

                var projectResult = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject);
                if (!ErrorHandler.Succeeded(projectResult)) {
                    continue;
                }

                EnvDTE.Project dteProject = extObject as EnvDTE.Project;
                if (dteProject == null) {
                    continue;
                }

                EnvDTE.Properties properties = dteProject.Properties;
                if (dteProject.Properties == null) {
                    continue;
                }

                string projectName = dteProject.Name;
                EnvDTE.Property projectHome = properties.Item("ProjectHome");
                if (projectHome == null || projectName == null) {
                    continue;
                }

                var projectDirectory = projectHome.Value as string;
                if (projectDirectory != null) {
                    projectNameToDirectoryDictionary.Add(projectName, Tuple.Create(projectDirectory, hierarchy));
                }
            }

            Tuple<string, IVsHierarchy> projectInfo;
            if (string.IsNullOrEmpty(projectPath) && projectNameToDirectoryDictionary.Count == 1) {
                projectInfo = projectNameToDirectoryDictionary.Values.First();
            } else {
                projectNameToDirectoryDictionary.TryGetValue(projectPath, out projectInfo);
            }

            NodejsProjectNode nodejsProject = null;
            if (projectInfo != null) {
                projectPath = projectInfo.Item1;
                if (projectInfo.Item2 != null) {
                    nodejsProject = projectInfo.Item2.GetProject().GetNodejsProject();
                }
            }

            bool isGlobalCommand = false;
            if (string.IsNullOrWhiteSpace(npmArguments) ||
                npmArguments.Contains(" -g ") || npmArguments.Contains(" --global ")) {
                projectPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                isGlobalCommand = true;
            }

            // In case someone copies filename
            string projectDirectoryPath = File.Exists(projectPath) ? Path.GetDirectoryName(projectPath) : projectPath;
            
            if (!isGlobalCommand && !(Directory.Exists(projectDirectoryPath) && File.Exists(Path.Combine(projectDirectoryPath, "package.json")))) {
                window.WriteError("Please specify a valid Node.js project or project directory. If your solution contains multiple projects, specify a target project using .npm [ProjectName or ProjectDir] <npm arguments> For example: .npm [MyApp] list");
                return ExecutionResult.Failure;
            }

            string npmPath;
            try {
                npmPath = NpmHelpers.GetPathToNpm(
                    nodejsProject != null ? nodejsProject.GetProjectProperty(NodejsConstants.NodeExePath) : null);
            } catch (NpmNotFoundException) {
                Nodejs.ShowNodejsNotInstalled();
                return ExecutionResult.Failure;
            }

            var npmReplRedirector = new NpmReplRedirector(window);
               
            await ExecuteNpmCommandAsync(
                npmReplRedirector,
                npmPath,
                projectDirectoryPath,
                new[] { npmArguments },
                null);

            if (npmReplRedirector.HasErrors) {
                window.WriteError(SR.GetString(SR.NpmReplCommandCompletedWithErrors, arguments));
            } else {
                window.WriteLine(SR.GetString(SR.NpmSuccessfullyCompleted, arguments));
            }

            if (nodejsProject != null) {
                await nodejsProject.CheckForLongPaths(npmArguments);
            }

            return ExecutionResult.Success;
        }

        public string Description {
            get { return "Executes npm command. If solution contains multiple projects, specify target project using .npm [ProjectName] <npm arguments>"; }
        }

        public string Command {
            get { return "npm"; }
        }

        public object ButtonContent {
            get { return null; }
        }

        // TODO: This is duplicated from Npm project
        // We should consider using InternalsVisibleTo to avoid code duplication
        internal static async Task<IEnumerable<string>> ExecuteNpmCommandAsync(
            Redirector redirector, 
            string pathToNpm,
            string executionDirectory,
            string[] arguments,
            ManualResetEvent cancellationResetEvent) {

            IEnumerable<string> standardOutputLines = null;

            using (var process = ProcessOutput.Run(
                pathToNpm,
                arguments,
                executionDirectory,
                null,
                false,
                redirector,
                quoteArgs: false,
                outputEncoding: Encoding.UTF8 // npm uses UTF-8 regardless of locale if its output is redirected
                )) {
                var whnd = process.WaitHandle;
                if (whnd == null) {
                    // Process failed to start, and any exception message has
                    // already been sent through the redirector
                    if (redirector != null) {
                        redirector.WriteErrorLine("Error - cannot start npm");
                    }
                } else {
                    var handles = cancellationResetEvent != null ? new[] { whnd, cancellationResetEvent } : new [] { whnd };
                    var i = await Task.Run(() => WaitHandle.WaitAny(handles));
                    if (i == 0) {
                        Debug.Assert(process.ExitCode.HasValue, "npm process has not really exited");
                        process.Wait();
                        if (process.StandardOutputLines != null) {
                            standardOutputLines = process.StandardOutputLines.ToList();                            
                        }
                    } else {
                        process.Kill();
                        if (redirector != null) {
                            redirector.WriteErrorLine(string.Format(
                            "\r\n===={0}====\r\n\r\n",
                            "npm command cancelled"));
                        }
                        
                        if (cancellationResetEvent != null) {
                            cancellationResetEvent.Reset();
                        }

                        throw new OperationCanceledException();
                    }
                }
            }
            return standardOutputLines;
        }

        #endregion

        internal class NpmReplRedirector : Redirector {
            
            internal const string ErrorAnsiColor = "\x1b[31;1m";
            internal const string WarnAnsiColor = "\x1b[33;22m";
            internal const string NormalAnsiColor = "\x1b[39;49m";

            private const string ErrorText = "npm ERR!";
            private const string WarningText = "npm WARN";

            private IReplWindow _window;

            public NpmReplRedirector(IReplWindow window) {
                _window = window;
                HasErrors = false;
            }
            public bool HasErrors { get; set; }

            public override void WriteLine(string decodedString) {
                var substring = string.Empty;
                string outputString = string.Empty;

                if (decodedString.StartsWith(ErrorText)) {
                    outputString += ErrorAnsiColor + decodedString.Substring(0, ErrorText.Length);
                    substring = decodedString.Length > ErrorText.Length ? decodedString.Substring(ErrorText.Length) : string.Empty;
                    this.HasErrors = true;
                } else if (decodedString.StartsWith(WarningText)) {
                    outputString += WarnAnsiColor + decodedString.Substring(0, WarningText.Length);
                    substring = decodedString.Length > WarningText.Length ? decodedString.Substring(WarningText.Length) : string.Empty;
                } else {
                    substring = decodedString;
                }

                outputString += NormalAnsiColor + substring;

                _window.WriteLine(outputString);
                Debug.WriteLine(decodedString, "REPL npm");
            }

            public override void WriteErrorLine(string line) {
                _window.WriteError(line);
            }
        }
    }
}

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
using System.Collections;
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
        private string _npmPath;

        #region IReplCommand Members

        /// <summary>
        /// Executes npm command in the REPL
        /// </summary>
        /// <param name="window"></param>
        /// <param name="arguments">Expected format "project of path to folder" "arguments"</param>
        /// <returns></returns>
        public async Task<ExecutionResult> Execute(IReplWindow window, string arguments) {

            if (null == _npmPath || !File.Exists(_npmPath)) {
                try {
                    _npmPath = NpmHelpers.GetPathToNpm();
                } catch (NpmNotFoundException) {
                    Nodejs.ShowNodejsNotInstalled();
                    return ExecutionResult.Failure;
                }
            }

            string projectPath = string.Empty;
            string npmArguments = arguments.TrimStart(' ', '\t');

            bool isGlobalInstall = false;
            if (npmArguments.Contains(" -g") || npmArguments.Contains(" --global")) {
                projectPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                isGlobalInstall = true;
            }

            NodejsProjectNode nodejsProjectNode = null;
            var replWindow3 = window as IReplWindow3;
            if (!isGlobalInstall && replWindow3 != null) {
                //Grab the project we should target
                IVsHierarchy hierarchy;
                IVsProject project;
                if (replWindow3.Properties.TryGetProperty<IVsProject>(typeof(IVsProject), out project)) {
                    hierarchy = (IVsHierarchy)project;
                } else {
                    //No project was set, see if we can find the active project
                    var solution = Package.GetGlobalService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
                    
                    solution.get_StartupProject(out hierarchy);
                    if (hierarchy == null) {
                        //Startup project is not node.js project
                        window.WriteError(SR.GetString(SR.NpmReplCommandInvalidProject));
                        return ExecutionResult.Failure;
                    }
                    project = (IVsProject)hierarchy;
                }
                EnvDTE.Project envProject = hierarchy.GetProject();
                if (envProject == null) {
                    window.WriteError(SR.GetString(SR.NpmReplCommandInvalidProject));
                    return ExecutionResult.Failure;
                }
                nodejsProjectNode = envProject.GetNodejsProject();
                if (nodejsProjectNode == null) {
                    window.WriteError(SR.GetString(SR.NpmReplCommandInvalidProject));
                    return ExecutionResult.Failure;
                }
                projectPath = nodejsProjectNode.ProjectHome;
            }

            if (!Directory.Exists(projectPath)) {
                window.WriteError(SR.GetString(SR.NpmReplCommandInvalidProject));
                return ExecutionResult.Failure;
            }

            if (!isGlobalInstall && !(Directory.Exists(projectPath) && File.Exists(Path.Combine(projectPath, "package.json")))) {
                window.WriteError(SR.GetString(SR.NpmReplCommandInvalidProject));
                return ExecutionResult.Failure;
            }

            var npmReplRedirector = new NpmReplRedirector(window);
               
            await ExecuteNpmCommandAsync(npmReplRedirector,
                    _npmPath,
                    projectPath,
                new[] { npmArguments },
                null);

            if (npmReplRedirector.HasErrors) {
                window.WriteError(SR.GetString(SR.NpmReplCommandCompletedWithErrors, arguments));
            } else {
                window.WriteLine(SR.GetString(SR.NpmSuccessfullyCompleted, arguments));
            }

            if (nodejsProjectNode != null) {
                await nodejsProjectNode.CheckForLongPaths(npmArguments);
            }

            return ExecutionResult.Success;
        }

        public string Description {
            get { return SR.GetString(SR.NpmReplCommandHelp); }
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

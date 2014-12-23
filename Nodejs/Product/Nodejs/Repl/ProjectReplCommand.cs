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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

using SR = Microsoft.NodejsTools.Project.SR;

namespace Microsoft.NodejsTools.Repl {
#if INTERACTIVE_WINDOW
    using IReplCommand = IInteractiveWindowCommand;
    using IReplWindow = IInteractiveWindow;    
#endif

    [Export(typeof(IReplCommand))]
    class ProjectReplCommand : IReplCommand {
        #region IReplCommand Members

        /// <summary>
        /// Sets the active project for repl commands to execute against
        /// </summary>
        /// <param name="window"></param>
        /// <param name="arguments">Expected format "project of path to folder" "arguments"</param>
        /// <returns></returns>
        public Task<ExecutionResult> Execute(IReplWindow window, string arguments) {

            IReplWindow3 replWindow3 = window as IReplWindow3;

            if (replWindow3 == null) {
                //The repl window does not support Property collections
                window.WriteError(SR.GetString(SR.ProjectReplCommandUnsupportedRepl));
                return ExecutionResult.Failed;
            }

            if (String.IsNullOrWhiteSpace(arguments)) {
                IVsProject activeProject;
                if (replWindow3.Properties.TryGetProperty<IVsProject>(typeof(IVsProject), out activeProject)) {
                    var hier = (IVsHierarchy)activeProject;
                    window.WriteLine(SR.GetString(SR.ProjectReplCommandActiveProject, hier.GetProject().Name));
                } else {
                    window.WriteLine(SR.GetString(SR.ProjectReplCommandNoActiveProject));
                }
                return ExecutionResult.Succeeded;
            }

            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            foreach (IVsProject project in solution.EnumerateLoadedProjects(onlyNodeProjects: true)) {
                IVsHierarchy hierarchy = (IVsHierarchy)project;
                
                var dteProject = hierarchy.GetProject();
                if (dteProject == null) {
                    continue;
                }

                EnvDTE.Properties properties = dteProject.Properties;
                if (dteProject.Properties == null) {
                    continue;
                }

                string projectName = dteProject.Name;
                if (projectName.Equals(arguments, StringComparison.OrdinalIgnoreCase)) {
                    if (replWindow3.Properties.ContainsProperty(typeof(IVsProject))) {
                        replWindow3.Properties.RemoveProperty(typeof(IVsProject));
                    }
                    replWindow3.Properties.AddProperty(typeof(IVsProject), project);
                    window.WriteLine(SR.GetString(SR.ProjectReplCommandActiveProject, projectName));
                    return ExecutionResult.Succeeded;
                }
            }
            window.WriteError(SR.GetString(SR.ProjectReplCommandNoMatchingProjectFound, arguments));

            return ExecutionResult.Failed;
        }

        public string Description {
            get { return SR.GetString(SR.ProjectReplCommandHelp); }
        }

        public string Command {
            get { return "project"; }
        }

        public object ButtonContent {
            get { return null; }
        }

        #endregion
    }
}

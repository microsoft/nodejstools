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

using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Updates the dynamic project property called StartupFile 
    /// </summary>
    internal class ProjectDocumentsListenerForStartupFileUpdates : ProjectDocumentsListener {
        #region fields
        /// <summary>
        /// The dynamic project who adviced for TrackProjectDocumentsEvents
        /// </summary>
        private CommonProjectNode _project;
        #endregion

        #region ctors
        public ProjectDocumentsListenerForStartupFileUpdates(ServiceProvider serviceProvider, CommonProjectNode project)
            : base(serviceProvider) {
            _project = project;
        }
        #endregion

        #region overriden methods
        public override int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, string[] newFileNames, VSRENAMEFILEFLAGS[] flags) {
            if (!_project.IsRefreshing) {
                //Get the current value of the StartupFile Property
                string currentStartupFile = _project.GetProjectProperty(CommonConstants.StartupFile, true);
                string fullPathToStartupFile = CommonUtils.GetAbsoluteFilePath(_project.ProjectHome, currentStartupFile);

                //Investigate all of the oldFileNames if they are equal to the current StartupFile
                int index = 0;
                foreach (string oldfile in oldFileNames) {
                    FileNode node = null;
                    if ((flags[index] & VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_Directory) != 0) {
                        if (CommonUtils.IsSubpathOf(oldfile, fullPathToStartupFile)) {
                            // Get the newfilename and update the StartupFile property
                            string newfilename = Path.Combine(
                                newFileNames[index],
                                CommonUtils.GetRelativeFilePath(oldfile, fullPathToStartupFile)
                            );

                            node = _project.FindNodeByFullPath(newfilename) as FileNode;
                            Debug.Assert(node != null);
                        }
                    } else if (CommonUtils.IsSamePath(oldfile, fullPathToStartupFile)) {
                        //Get the newfilename and update the StartupFile property
                        string newfilename = newFileNames[index];
                        node = _project.FindNodeByFullPath(newfilename) as FileNode;
                        Debug.Assert(node != null);
                    }

                    if (node != null) {
                        // Startup file has been renamed
                        _project.SetProjectProperty(
                            CommonConstants.StartupFile,
                            CommonUtils.GetRelativeFilePath(_project.ProjectHome, node.Url));
                        break;
                    }
                    index++;
                }
            }
            return VSConstants.S_OK;
        }

        public override int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, VSREMOVEFILEFLAGS[] flags) {
            if (!_project.IsRefreshing) {
                //Get the current value of the StartupFile Property
                string currentStartupFile = _project.GetProjectProperty(CommonConstants.StartupFile, true);
                string fullPathToStartupFile = CommonUtils.GetAbsoluteFilePath(_project.ProjectHome, currentStartupFile);

                //Investigate all of the oldFileNames if they are equal to the current StartupFile
                int index = 0;
                foreach (string oldfile in oldFileNames) {
                    //Compare the files and update the StartupFile Property if the currentStartupFile is an old file
                    if (CommonUtils.IsSamePath(oldfile, fullPathToStartupFile)) {
                        //Startup file has been removed
                        _project.SetProjectProperty(CommonConstants.StartupFile, null);
                        break;
                    }
                    index++;
                }
            }
            return VSConstants.S_OK;
        }
        #endregion
    }
}

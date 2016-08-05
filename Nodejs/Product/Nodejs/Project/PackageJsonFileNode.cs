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

using System.IO;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    sealed class PackageJsonFileNode : CommonNonCodeFileNode {
        private FileSystemWatcher _watcher;

        public PackageJsonFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
            CreateWatcher(Url);
            FileContentsChanged(this, null);
        }

        private void CreateWatcher(string filename) {
            if (CommonUtils.IsSubpathOf(ProjectMgr.ProjectHome, filename)) {
                // we want to subscribe to the project's file system watcher so users
                // can continue to rename the directory which contains this file.
                ProjectMgr.RegisterFileChangeNotification(this, FileContentsChanged);
            } else {
                // this is a link file which lives outside of our project directory,
                // we'll need to watch the file directly, which means we're going to
                // prevent it's parent directory from being renamed.
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename));
                _watcher.EnableRaisingEvents = true;
                _watcher.Changed += FileContentsChanged;
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
            }
        }

        private void FileContentsChanged(object sender, FileSystemEventArgs e) {
            var analyzer = ((NodejsProjectNode)ProjectMgr).Analyzer;
            AnalyzePackageJson(analyzer);
        }

        internal void AnalyzePackageJson(Intellisense.VsProjectAnalyzer analyzer) {
            analyzer.AddPackageJson(Url);
        }

        protected override void Dispose(bool disposing) {
            if (_watcher != null) {
                _watcher.Changed -= FileContentsChanged;
                _watcher.Dispose();
            } else {
                ProjectMgr?.UnregisterFileChangeNotification(this);
            }
            base.Dispose(disposing);
        }
    }
}

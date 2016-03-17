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
using System.Threading;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    class RequireAnalysisUnit : AnalysisUnit {
        private string _dependency;
        private ModuleTree _tree;
        private ModuleTable _table;

        internal RequireAnalysisUnit(ModuleTree tree, ModuleTable table, ProjectEntry entry, string dependency) : base (entry.Tree, entry.EnvironmentRecord) {
            _tree = tree;
            _table = table;
            _dependency = dependency;
        }

        internal override void AnalyzeWorker(DDG ddg, CancellationToken cancel) {
            ModuleTree module = _table.RequireModule(this, _dependency, _tree);
            if (module == null) {
                return;
            }

            AddChildVisibilitiesExcludingNodeModules(module);
        }

        private void AddChildVisibilitiesExcludingNodeModules(ModuleTree moduleTree) {
            foreach (var childTree in moduleTree.GetChildrenExcludingNodeModules()) {
                Debug.Assert(childTree.Name != AnalysisConstants.NodeModulesFolder);
                if (childTree.ProjectEntry == null) {
                    AddChildVisibilitiesExcludingNodeModules(childTree);
                } else {
                    _table.AddVisibility(_tree, childTree.ProjectEntry);
                }
            }
        }
    }
}
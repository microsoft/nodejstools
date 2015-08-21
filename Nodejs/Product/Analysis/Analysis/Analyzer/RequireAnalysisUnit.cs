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
using System.Threading;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    class RequireAnalysisUnit : AnalysisUnit {
        private string _dependency;
        private ModuleTree _tree;
        private ProjectEntry _entry;
        private ModuleTable _table;

        public RequireAnalysisUnit(ModuleTree tree, ModuleTable table, ProjectEntry entry, string dependency) : base (entry.Tree, entry.EnvironmentRecord) {
            _tree = tree;
            _table = table;
            _entry = entry;
            _dependency = dependency;
        }

        internal override void AnalyzeWorker(DDG ddg, CancellationToken cancel) {
            UpdateVisibilities(_tree, _table, _entry, _dependency);
        }

        internal static void UpdateVisibilities(ModuleTree _tree, ModuleTable _table, ProjectEntry entry, string dependency) {
            ModuleTree module = _table.RequireModule(new RequireAnalysisUnit(_tree, _table, entry, dependency), dependency, _tree);
            if (module == null) {
                return;
            }

            var enumerator = module.Children.Values.GetEnumerator();
            while (enumerator.MoveNext()) {
                var childTree = enumerator.Current;
                if (childTree.ProjectEntry != null) {
                    _table.AddVisibility(_tree, childTree.ProjectEntry);
                }
            }
        }

        internal override ModuleEnvironmentRecord GetDeclaringModuleEnvironment() {
            return base.GetDeclaringModuleEnvironment();
        }
    }
}

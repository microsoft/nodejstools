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
using System.Linq;

namespace Microsoft.NodejsTools.Analysis {
    internal static class ModuleTreeExtensions {
        internal static IEnumerable<ModuleTree> GetChildrenExcludingNodeModules(this ModuleTree moduleTree) {
            if (moduleTree == null) {
                return Enumerable.Empty<ModuleTree>();
            }
            // Children.Values returns an IEnumerable
            // The process of resolving modules can lead us to add entries into the underlying array
            // doing so results in exceptions b/c the array has changed under the enumerable
            // To avoid this, we call .ToArray() to create a copy of the array locally which we then Enumerate
            return moduleTree.Children.Values.ToArray().Where(mod => mod != null && !String.Equals(mod.Name, AnalysisConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase));
        }
    }
}

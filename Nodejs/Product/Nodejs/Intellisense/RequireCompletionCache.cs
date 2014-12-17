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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Stores cached completion results for require calls which can be expensive
    /// to calculate in large projects.  The cache is cleared when anything which
    /// would alter require semantics changes and each file will need to be updated.
    /// </summary>
    class RequireCompletionCache {
        private Dictionary<FileNode, CompletionInfo[]> _cachedEntries = new Dictionary<FileNode, CompletionInfo[]>();

        public void Clear() {
            _cachedEntries.Clear();
        }

        public bool TryGetCompletions(FileNode node, out CompletionInfo[] result) {
            return _cachedEntries.TryGetValue(node, out result);
        }

        public void CacheCompletions(FileNode node, CompletionInfo[] completions) {
            _cachedEntries[node] = completions;
        }
    }
}

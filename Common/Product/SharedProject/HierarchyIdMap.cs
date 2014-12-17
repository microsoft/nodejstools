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
using System.Diagnostics;

namespace Microsoft.VisualStudioTools.Project {
    sealed class HierarchyIdMap {
        private readonly List<HierarchyNode> _ids = new List<HierarchyNode>();
        private readonly Stack<int> _freedIds = new Stack<int>();

        public uint Add(HierarchyNode node) {
            UIThread.MustBeCalledFromUIThread();

#if DEBUG
            foreach (var item in _ids) {
                Debug.Assert(node != item);
            }
#endif
            if (_freedIds.Count > 0) {
                var i = _freedIds.Pop();
                _ids[i] = node;
                return (uint)i + 1;
            } else {
                _ids.Add(node);
                // ids are 1 based
                return (uint)_ids.Count;
            }
        }

        public void Remove(HierarchyNode node) {
            UIThread.MustBeCalledFromUIThread();

            int i = (int)node.ID - 1;
            if(i < 0 ||
                i >= _ids.Count ||
                !object.ReferenceEquals(node, _ids[i])) {
                throw new InvalidOperationException("Removing node with invalid ID or map is corrupted");
            }

            _ids[i] = null;
            _freedIds.Push(i);
        }

        public HierarchyNode this[uint itemId] {
            get {
                UIThread.MustBeCalledFromUIThread();

                int i = (int)itemId - 1;
                if (0 <= i && i < _ids.Count) {
                    return _ids[i];
                }
                return null;
            }
        }
    }
}

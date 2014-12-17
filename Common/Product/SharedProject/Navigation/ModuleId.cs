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

using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Navigation {
    /// <summary>
    /// Class used to identify a module. The module is identified using the hierarchy that
    /// contains it and its item id inside the hierarchy.
    /// </summary>
    public sealed class ModuleId {
        private IVsHierarchy _ownerHierarchy;
        private uint _itemId;

        public ModuleId(IVsHierarchy owner, uint id) {
            _ownerHierarchy = owner;
            _itemId = id;
        }

        public IVsHierarchy Hierarchy {
            get { return _ownerHierarchy; }
        }

        public uint ItemID {
            get { return _itemId; }
        }

        public override int GetHashCode() {
            int hash = 0;
            if (null != _ownerHierarchy) {
                hash = _ownerHierarchy.GetHashCode();
            }
            hash = hash ^ (int)_itemId;
            return hash;
        }

        public override bool Equals(object obj) {
            ModuleId other = obj as ModuleId;
            if (null == obj) {
                return false;
            }
            if (!_ownerHierarchy.Equals(other._ownerHierarchy)) {
                return false;
            }
            return (_itemId == other._itemId);
        }
    }
}
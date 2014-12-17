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

namespace Microsoft.VisualStudioTools.Project {
    internal partial class ProjectNode {
        public event EventHandler<ProjectPropertyChangedArgs> OnProjectPropertyChanged;

        protected virtual void RaiseProjectPropertyChanged(string propertyName, string oldValue, string newValue) {
            var onPropChanged = OnProjectPropertyChanged;
            if (onPropChanged != null) {
                onPropChanged(this, new ProjectPropertyChangedArgs(propertyName, oldValue, newValue));
            }
        }
    }
}

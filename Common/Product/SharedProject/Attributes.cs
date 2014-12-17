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
using System.ComponentModel;

namespace Microsoft.VisualStudioTools.Project {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class SRDisplayNameAttribute : DisplayNameAttribute {
        string _name;

        public SRDisplayNameAttribute(string name) {
            _name = name;
        }

        public override string DisplayName {
            get {
                return SR.GetString(_name);
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRDescriptionAttribute : DescriptionAttribute {
        private bool _replaced;

        public SRDescriptionAttribute(string description)
            : base(description) {
        }

        public override string Description {
            get {
                if (!_replaced) {
                    _replaced = true;
                    DescriptionValue = SR.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRCategoryAttribute : CategoryAttribute {
        public SRCategoryAttribute(string category)
            : base(category) {
        }

        protected override string GetLocalizedString(string value) {
            return SR.GetString(value);
        }
    }
}

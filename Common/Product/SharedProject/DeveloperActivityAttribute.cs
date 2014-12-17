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
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools {
    class DeveloperActivityAttribute : RegistrationAttribute {
        private readonly Type _projectType;
        private readonly int _templateSet;
        private readonly string _developerActivity;

        public DeveloperActivityAttribute(string developerActivity, Type projectPackageType) {
            _developerActivity = developerActivity;
            _projectType = projectPackageType;
            _templateSet = 1;
        }

        public DeveloperActivityAttribute(string developerActivity, Type projectPackageType, int templateSet) {
            _developerActivity = developerActivity;
            _projectType = projectPackageType;
            _templateSet = templateSet;
        }

        public override void Register(RegistrationAttribute.RegistrationContext context) {
            var key = context.CreateKey("NewProjectTemplates\\TemplateDirs\\" + _projectType.GUID.ToString("B") + "\\/" + _templateSet);
            key.SetValue("DeveloperActivity", _developerActivity);
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context) {
        }
    }
}

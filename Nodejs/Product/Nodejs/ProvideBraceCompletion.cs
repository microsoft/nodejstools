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

using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools {
    class ProvideBraceCompletionAttribute : RegistrationAttribute {
        private readonly string _languageName;

        public ProvideBraceCompletionAttribute(string languageName) {
            _languageName = languageName;
        }

        public override void Register(RegistrationAttribute.RegistrationContext context) {
            using (Key serviceKey = context.CreateKey(LanguageServicesKeyName)) {
                serviceKey.SetValue("ShowBraceCompletion", (int)1);
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context) {
        }

        private string LanguageServicesKeyName {
            get {
                return string.Format(CultureInfo.InvariantCulture,
                                     "{0}\\{1}",
                                     "Languages\\Language Services",
                                     _languageName);
            }
        }
    }
}

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

namespace Microsoft.NodejsTools.Options {
    internal class NodejsDiagnosticsOptionsPage : NodejsDialogPage {
        private bool _isLiveDiagnosticsEnabled;
        private const string IsLiveDiagnosticsEnabledSetting = "IsLiveDiagnosticsEnabled";

        public NodejsDiagnosticsOptionsPage() : base("Diagnostics") {
            _isLiveDiagnosticsEnabled = !NodejsPackage.Instance.Zombied && (LoadBool(IsLiveDiagnosticsEnabledSetting) ?? false);
        }

        public bool IsLiveDiagnosticsEnabled {
            get {
                return !NodejsPackage.Instance.Zombied && _isLiveDiagnosticsEnabled;
            }
            set {
                _isLiveDiagnosticsEnabled = value;
                SaveBool(IsLiveDiagnosticsEnabledSetting, value);
            }
        }
    }
}

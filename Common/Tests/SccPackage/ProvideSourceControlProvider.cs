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

namespace Microsoft.TestSccPackage {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProvideSourceControlProvider : RegistrationAttribute {
        private readonly string _name;
        private readonly Guid _sourceControlGuid;
        private readonly Type _providerType, _packageType;

        public ProvideSourceControlProvider(string friendlyName, string sourceControlGuid, Type sccPackage, Type sccProvider) {
            _name = friendlyName;
            _providerType = sccProvider;
            _packageType = sccPackage;
            _sourceControlGuid = new Guid(sourceControlGuid);
        }

        public override void Register(RegistrationContext context) {
            // http://msdn.microsoft.com/en-us/library/bb165948.aspx
            using (Key sccProviders = context.CreateKey("SourceControlProviders")) {
                using (Key sccProviderKey = sccProviders.CreateSubkey(_sourceControlGuid.ToString("B"))) {
                    sccProviderKey.SetValue("", _name);
                    sccProviderKey.SetValue("Service", _providerType.GUID.ToString("B"));
                        
                    using (Key sccProviderNameKey = sccProviderKey.CreateSubkey("Name")) {
                        sccProviderNameKey.SetValue("", _name);
                        sccProviderNameKey.SetValue("Package", _packageType.GUID.ToString("B"));
                    }
                }
            }/*
            using (Key currentProvider = context.CreateKey("CurrentSourceControlProvider")) {
                currentProvider.SetValue("", _sourceControlGuid.ToString("B"));
            }*/
        }

        public override void Unregister(RegistrationContext context) {
        }
    }
}

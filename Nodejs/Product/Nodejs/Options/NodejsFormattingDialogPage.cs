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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Options {
    public class NodejsFormattingDialogPage : NodejsDialogPage {
        ISettingsManager _settingsManager;
        private const string TypeScriptBaseName = "TextEditor.TypeScript.Specific.";

        public NodejsFormattingDialogPage(string category) : base(category) {
            uint handle;
            string registryRoot;
            var registry = NodejsPackage.Instance.GetService(typeof(SLocalRegistry)) as ILocalRegistry4;
            var regKey = registry.GetLocalRegistryRootEx((uint)__VsLocalRegistryType.RegType_UserSettings, out handle, out registryRoot);
            _settingsManager = (ISettingsManager)NodejsPackage.Instance.GetService(typeof(SVsSettingsPersistenceManager));
        }

        internal override void SaveBool(string name, bool value) {
            var keyName = GetKeyName(name);
            var registryValue = value ? 1 : 0;
            _settingsManager.SetValueAsync(keyName, registryValue, isMachineLocal: false);
        }

        internal override bool? LoadBool(string name) {
            var value = _settingsManager.GetValueOrDefault<int?>(GetKeyName(name));
            if (value == null) {
                return null;
            } else {
                return value != 0;
            }
        }

        private static string GetKeyName(string name) {
            if (name.EndsWith("_TEMP")) {
                name = name.Remove(name.Length - 5);
            }
            return TypeScriptBaseName + name;
        }

        [Guid("9B164E40-C3A2-4363-9BC5-EB4039DEF653")]
        private class SVsSettingsPersistenceManager { }
    }
}

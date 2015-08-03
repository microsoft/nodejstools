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
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Options {
    /// <summary>
    /// Base class used for saving/loading of settings.  The settings are stored in VSRegistryRoot\NodejsTools\Options\Category\SettingName
    /// where Category is provided in the constructor and SettingName is provided to each call of the Save*/Load* APIs.
    /// x = 42
    /// 
    /// The primary purpose of this class is so that we can be in control of providing reasonable default values.
    /// </summary>
    [ComVisible(true)]
    public class NodejsDialogPage : DialogPage {
        private readonly string _category;
        private const string _optionsKey = "Options";

        internal NodejsDialogPage(string category) {
            _category = category;
        }

        internal void SaveBool(string name, bool value) {
            SaveString(name, value.ToString());
        }

        internal void SaveInt(string name, int value) {
            SaveString(name, value.ToString());
        }

        internal void SaveString(string name, string value) {
            SaveString(name, value, _category);
        }

        internal static void SaveString(string name, string value, string cat) {
            using (var pythonKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(NodejsConstants.BaseRegistryKey)) {
                using (var optionsKey = pythonKey.CreateSubKey(_optionsKey)) {
                    using (var categoryKey = optionsKey.CreateSubKey(cat)) {
                        categoryKey.SetValue(name, value, Win32.RegistryValueKind.String);
                    }
                }
            }
        }

        internal void SaveEnum<T>(string name, T value) where T : struct {
            SaveString(name, value.ToString());
        }

        internal void SaveDateTime(string name, DateTime value) {
            SaveString(name, value.ToString(CultureInfo.InvariantCulture));
        }

        internal int? LoadInt(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            int val;
            if (int.TryParse(res, out val)) {
                return val;
            }
            return null;
        }

        internal bool? LoadBool(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            bool val;
            if (bool.TryParse(res, out val)) {
                return val;
            }
            return null;
        }

        internal string LoadString(string name) {
            return LoadString(name, _category);
        }

        internal static string LoadString(string name, string cat) {
            using (var nodeKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(NodejsConstants.BaseRegistryKey)) {
                using (var optionsKey = nodeKey.CreateSubKey(_optionsKey)) {
                    using (var categoryKey = optionsKey.CreateSubKey(cat)) {
                        return categoryKey.GetValue(name) as string;
                    }
                }
            }
        }

        internal T? LoadEnum<T>(string name) where T : struct {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            T enumRes;
            if (Enum.TryParse<T>(res, out enumRes)) {
                return enumRes;
            }
            return null;
        }

        internal DateTime? LoadDateTime(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            DateTime dateRes;
            if (DateTime.TryParse(res, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateRes)) {
                return dateRes;
            }
            return null;
        }
    }
}

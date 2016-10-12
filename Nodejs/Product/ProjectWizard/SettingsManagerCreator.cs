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
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.Globalization;

namespace Microsoft.VisualStudioTools {
    static class SettingsManagerCreator {
#if DEV14
        const string VSVersion = "14.0";
#elif DEV15
        const string VSVersion = "15.0";
#else
#error Unrecognized VS Version.
#endif

        public static SettingsManager GetSettingsManager(DTE dte) {
            return GetSettingsManager(new ServiceProvider(((IOleServiceProvider)dte)));
        }

        public static SettingsManager GetSettingsManager(IServiceProvider provider) {
            SettingsManager settings = null;
            string devenvPath = null;
            if (provider == null) {
                provider = ServiceProvider.GlobalProvider;
            }

            if (provider != null) {
                try {
                    settings = new ShellSettingsManager(provider);
                } catch (NotSupportedException) {
                    var dte = (DTE)provider.GetService(typeof(DTE));
                    if (dte != null) {
                        devenvPath = dte.FullName;
                    }
                }
            }

            if (settings == null) {
                if (!File.Exists(devenvPath)) {
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    using (var key = root.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"Software\Microsoft\VisualStudio\{0}\Setup\VS", VSVersion))) {
                        if (key == null) {
                            throw new InvalidOperationException("Cannot find settings store for Visual Studio " + VSVersion);
                        }
                        devenvPath = key.GetValue("EnvironmentPath") as string;
                    }
                }
                if (!File.Exists(devenvPath)) {
                    throw new InvalidOperationException("Cannot find settings store for Visual Studio " + VSVersion);
                }
#if DEBUG
                settings = ExternalSettingsManager.CreateForApplication(devenvPath, "Exp");
#else
                settings = ExternalSettingsManager.CreateForApplication(devenvPath);
#endif
            }

            return settings;
        }
    }
}

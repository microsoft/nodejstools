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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Options {
    [ComVisible(true)]
    public class NodejsIntellisenseOptionsPage : NodejsDialogPage {
        private NodejsIntellisenseOptionsControl _window;
        private AnalysisLevel _level;
        private int _analysisLogMax;
        private bool _saveToDisk;
        private bool _onlyTabOrEnterToCommit;
        private bool _showCompletionListAfterCharacterTyped;
        private string _toolsVersion;
        private readonly bool _enableES6Preview;
        private readonly Version _typeScriptMinVersionForES6Preview = new Version("1.6");

        public NodejsIntellisenseOptionsPage()
            : base("IntelliSense") {
            Version version;
            var versionString = GetTypeScriptToolsVersion();
            if (!string.IsNullOrEmpty(versionString) &&
                Version.TryParse(versionString, out version) &&
                version.CompareTo(_typeScriptMinVersionForES6Preview) > -1) {
                    _enableES6Preview = true;
            }
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsIntellisenseOptionsControl();
                    LoadSettingsFromStorage();
                }
                return _window;
            }
        }

        internal bool EnableES6Preview { get { return _enableES6Preview; } }

        internal bool SaveToDisk {
            get { return _saveToDisk; }
            set {
                var oldState = _saveToDisk;
                _saveToDisk = value;
                if (oldState != _saveToDisk) {
                    var changed = SaveToDiskChanged;
                    if (changed != null) {
                        changed(this, EventArgs.Empty);
                    }
                }
            }
        }

        internal AnalysisLevel AnalysisLevel {
            get {
                return _level;
            }
            set {
                var oldLevel = _level;
                _level = value;

                // Fallback to full intellisense (High) if the ES6 intellisense preview isn't enabled
                if (_level == AnalysisLevel.Preview && !_enableES6Preview) {
                    _level = AnalysisLevel.High;
                }

                if (oldLevel != _level) {
                    var changed = AnalysisLevelChanged;
                    if (changed != null) {
                        changed(this, EventArgs.Empty);
                    }
                }
            }
        }

        internal int AnalysisLogMax {
            get {
                return _analysisLogMax;
            }
            set {
                var oldMax = _analysisLogMax;
                _analysisLogMax = value;
                if (oldMax != _analysisLogMax) {
                    var changed = AnalysisLogMaximumChanged;
                    if (changed != null) {
                        changed(this, EventArgs.Empty);
                    }
                }
            }
        }

        internal bool OnlyTabOrEnterToCommit {
            get {
                return _onlyTabOrEnterToCommit;
            }
            set {
                var oldSetting = _onlyTabOrEnterToCommit;
                _onlyTabOrEnterToCommit = value;
                if (oldSetting != _onlyTabOrEnterToCommit) {
                    var changed = OnlyTabOrEnterToCommitChanged;
                    if (changed != null) {
                        changed(this, EventArgs.Empty);
                    }
                }
            }
        }

        internal bool ShowCompletionListAfterCharacterTyped {
            get {
                return _showCompletionListAfterCharacterTyped;
            }
            set {
                var oldSetting = _showCompletionListAfterCharacterTyped;
                _showCompletionListAfterCharacterTyped = value;
                if (oldSetting != _showCompletionListAfterCharacterTyped) {
                    var changed = ShowCompletionListAfterCharacterTypedChanged;
                    if (changed != null) {
                        changed(this, EventArgs.Empty);
                    }
                }
            }
        }
        
        public event EventHandler<EventArgs> AnalysisLevelChanged;
        public event EventHandler<EventArgs> AnalysisLogMaximumChanged;
        public event EventHandler<EventArgs> SaveToDiskChanged;
        public event EventHandler<EventArgs> OnlyTabOrEnterToCommitChanged;
        public event EventHandler<EventArgs> ShowCompletionListAfterCharacterTypedChanged;

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings() {
            AnalysisLevel = AnalysisLevel.High;
            AnalysisLogMax = 100;
        }

        private const string AnalysisLevelSetting = "AnalysisLevel";
        private const string AnalysisLogMaximumSetting = "AnalysisLogMaximum";
        private const string SaveToDiskSetting = "SaveToDisk";
        private const string OnlyTabOrEnterToCommitSetting = "OnlyTabOrEnterToCommit";
        private const string ShowCompletionListAfterCharacterTypedSetting = "ShowCompletionListAfterCharacterTyped";

        public override void LoadSettingsFromStorage() {
            // Load settings from storage.
            AnalysisLevel = LoadEnum<AnalysisLevel>(AnalysisLevelSetting) ?? AnalysisLevel.High;
            AnalysisLogMax = LoadInt(AnalysisLogMaximumSetting) ?? 100;
            SaveToDisk = LoadBool(SaveToDiskSetting) ?? true;
            OnlyTabOrEnterToCommit = LoadBool(OnlyTabOrEnterToCommitSetting) ?? true;
            ShowCompletionListAfterCharacterTyped = LoadBool(ShowCompletionListAfterCharacterTypedSetting) ?? true;

            // Synchronize UI with backing properties.
            if (_window != null) {
                _window.SyncControlWithPageSettings(this);
            }

            // Settings values can change after loading them from storage as there
            // are conditions which could make them fallback to default values.
            // Save the final settings back to storage.
            SaveSettingsToStorage();
        }

        public override void SaveSettingsToStorage() {
            // Synchronize backing properties with UI.
            if (_window != null) {
                _window.SyncPageWithControlSettings(this);
            }

            // Save settings.
            SaveEnum(AnalysisLevelSetting, AnalysisLevel);
            SaveInt(AnalysisLogMaximumSetting, AnalysisLogMax);
            SaveBool(SaveToDiskSetting, SaveToDisk);
            SaveBool(OnlyTabOrEnterToCommitSetting, OnlyTabOrEnterToCommit);
            SaveBool(ShowCompletionListAfterCharacterTypedSetting, ShowCompletionListAfterCharacterTyped);
        }

        private string GetTypeScriptToolsVersion() {
            if (_toolsVersion == null) {
                _toolsVersion = string.Empty;
                try {
                    object installDirAsObject = null;
                    var shell = NodejsPackage.Instance.GetService(typeof(SVsShell)) as IVsShell;
                    if (shell != null) {
                        shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirAsObject);
                    }

                    var idePath = CommonUtils.NormalizeDirectoryPath((string)installDirAsObject) ?? string.Empty;
                    if (string.IsNullOrEmpty(idePath)) {
                        return _toolsVersion;
                    }

                    var typeScriptServicesPath = Path.Combine(idePath, @"CommonExtensions\Microsoft\TypeScript\typescriptServices.js");
                    if (!File.Exists(typeScriptServicesPath)) {
                        return _toolsVersion;
                    }

                    var regex = new Regex(@"toolsVersion = ""(?<version>\d.\d?)"";");
                    var fileText = File.ReadAllText(typeScriptServicesPath);
                    var match = regex.Match(fileText);

                    var version = match.Groups["version"].Value;
                    if (!string.IsNullOrWhiteSpace(version)) {
                        _toolsVersion = version;
                    }
                } catch (Exception ex) {
                    if (ex.IsCriticalException()) {
                        throw;
                    }

                    Debug.WriteLine(string.Format("Failed to obtain TypeScript tools version: {0}", ex.ToString()));
                }
            }

            return _toolsVersion;
        }
    }
}

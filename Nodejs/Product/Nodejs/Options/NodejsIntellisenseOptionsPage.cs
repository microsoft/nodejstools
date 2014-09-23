/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Options {
    [ComVisible(true)]
    public class NodejsIntellisenseOptionsPage : NodejsDialogPage {
        private NodejsIntellisenseOptionsControl _window;
        private AnalysisLevel _level;
        private int _analysisLogMax;
        private string _completionCommittedBy;

        public NodejsIntellisenseOptionsPage()
            : base("IntelliSense") {
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

        internal AnalysisLevel AnalysisLevel {
            get {
                return _level;
            }
            set {
                var oldLevel = _level;
                _level = value;
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

        internal string CompletionCommittedBy {
            get {
                return _completionCommittedBy;
            }
            set {
                var oldChars = _completionCommittedBy;
                _completionCommittedBy = value;
                if (oldChars != _completionCommittedBy) {
                    var changed = CompletionCommittedByChanged;
                    if (changed != null) {
                        changed(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler<EventArgs> AnalysisLevelChanged;
        public event EventHandler<EventArgs> AnalysisLogMaximumChanged;
        public event EventHandler<EventArgs> CompletionCommittedByChanged;

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
        private const string CompletionCommittedBySetting = "CompletionCommittedBy";

        public override void LoadSettingsFromStorage() {
            // Load settings from storage.
            AnalysisLevel = LoadEnum<AnalysisLevel>(AnalysisLevelSetting) ?? AnalysisLevel.High;
            AnalysisLogMax = LoadInt(AnalysisLogMaximumSetting) ?? 100;
            CompletionCommittedBy = LoadString(CompletionCommittedBySetting) ?? NodejsConstants.DefaultIntellisenseCompletionCommittedBy;

            // Synchronize UI with backing properties.
            if (_window != null) {
                _window.SyncControlWithPageSettings(this);
            }
        }

        public override void SaveSettingsToStorage() {
            // Synchronize backing properties with UI.
            if (_window != null) {
                _window.SyncPageWithControlSettings(this);
            }

            // Save settings.
            SaveEnum(AnalysisLevelSetting, AnalysisLevel);
            SaveInt(AnalysisLogMaximumSetting, AnalysisLogMax);
            SaveString(CompletionCommittedBySetting, CompletionCommittedBy);
        }
    }
}

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
    public class NodejsAdvancedOptionsPage : NodejsDialogPage {
        private NodejsAdvancedOptionsControl _window;
        private AnalysisLevel _level;

        public NodejsAdvancedOptionsPage()
            : base("Advanced") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsAdvancedOptionsControl();
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

        public event EventHandler<EventArgs> AnalysisLevelChanged;

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings() {
            AnalysisLevel = AnalysisLevel.High;
        }

        private const string AnalysisLevelSetting = "AnalysisLevel";

        public override void LoadSettingsFromStorage() {
            AnalysisLevel = LoadEnum<AnalysisLevel>(AnalysisLevelSetting) ?? AnalysisLevel.High;
        }

        public override void SaveSettingsToStorage() {
            AnalysisLevel = _window.AnalysisLevel;
            SaveEnum(AnalysisLevelSetting, AnalysisLevel);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options
{
    [ComVisible(true)]
    public class NodejsNpmOptionsPage : NodejsDialogPage
    {
        private const string ShowOutputWindowRunningNpm = "ShowOutputWindowRunningNpm";

        private NodejsNpmOptionsControl _window;

        public NodejsNpmOptionsPage()
            : base("Npm")
        {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override IWin32Window Window
        {
            get
            {
                if (this._window == null)
                {
                    this._window = new NodejsNpmOptionsControl();
                    LoadSettingsFromStorage();
                }
                return this._window;
            }
        }

        /// <summary>
        /// Indicates whether or not the Output window should be shown when
        /// npm commands are being executed.
        /// </summary>
        public bool ShowOutputWindowWhenExecutingNpm { get; set; }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage" /> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings()
        {
            this.ShowOutputWindowWhenExecutingNpm = true;
        }

        public override void LoadSettingsFromStorage()
        {
            // Load settings from storage.
            this.ShowOutputWindowWhenExecutingNpm = LoadBool(ShowOutputWindowRunningNpm) ?? true;

            // Synchronize UI with backing properties.
            if (this._window != null)
            {
                this._window.SyncControlWithPageSettings(this);
            }
        }

        public override void SaveSettingsToStorage()
        {
            // Synchronize backing properties with UI.
            if (this._window != null)
            {
                this._window.SyncPageWithControlSettings(this);
            }

            // Save settings.
            SaveBool(ShowOutputWindowRunningNpm, this.ShowOutputWindowWhenExecutingNpm);
        }
    }
}


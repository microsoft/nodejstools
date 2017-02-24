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
                if (_window == null)
                {
                    _window = new NodejsNpmOptionsControl();
                    LoadSettingsFromStorage();
                }
                return _window;
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
            ShowOutputWindowWhenExecutingNpm = true;
        }

        public override void LoadSettingsFromStorage()
        {
            // Load settings from storage.
            ShowOutputWindowWhenExecutingNpm = LoadBool(ShowOutputWindowRunningNpm) ?? true;

            // Synchronize UI with backing properties.
            if (_window != null)
            {
                _window.SyncControlWithPageSettings(this);
            }
        }

        public override void SaveSettingsToStorage()
        {
            // Synchronize backing properties with UI.
            if (_window != null)
            {
                _window.SyncPageWithControlSettings(this);
            }

            // Save settings.
            SaveBool(ShowOutputWindowRunningNpm, ShowOutputWindowWhenExecutingNpm);
        }
    }
}
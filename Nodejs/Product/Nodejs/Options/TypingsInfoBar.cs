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
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Options {
    public class TypingsInfoBar : IVsInfoBarUIEvents {

        private readonly static InfoBarButton _customizeInfoBarButton;
        private readonly static InfoBarHyperlink _typingsFolderHyperlink;

        private readonly static InfoBarModel _infoBarModel =
            new InfoBarModel(
                new[] {
                    new InfoBarTextSpan(SR.GetString(SR.TypingsInfoBarSpan1)),
                    _typingsFolderHyperlink = new InfoBarHyperlink(SR.GetString(SR.TypingsInfoBarSpan2)),
                    new InfoBarTextSpan(SR.GetString(SR.TypingsInfoBarSpan3))
                },
                new[] {
                    _customizeInfoBarButton = new InfoBarButton(SR.GetString(SR.TypingsOpenOptionsText))
                },
                KnownMonikers.NewFolder,
                true);

        private IServiceProvider _provider;
        private bool _isVisible = false;

        private static readonly Lazy<TypingsInfoBar> _instance =
            new Lazy<TypingsInfoBar>(() => new TypingsInfoBar(NodejsPackage.Instance));

        public static TypingsInfoBar Instance {
            get {
                return _instance.Value;
            }
        }

        public void ShowInfoBar() {
            if (_isVisible) {
                return;
            }

            IVsInfoBarUIElement uiElement;
            uint cookie;
            if (!TryCreateInfoBarUI(_infoBarModel, out uiElement)) {
                return;
            }
            uiElement.Advise(this, out cookie);
            var solutionExplorer = GetSolutionExplorerPane();
            if (solutionExplorer != null) {
                solutionExplorer.AddInfoBar(uiElement);
                _isVisible = true;
            }
        }

        private TypingsInfoBar(IServiceProvider provider) {
            _provider = provider;
        }

        private bool TryCreateInfoBarUI(IVsInfoBar infoBar, out IVsInfoBarUIElement uiElement) {
            IVsInfoBarUIFactory infoBarUIFactory = (_provider.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory);
            if (infoBarUIFactory == null) {
                uiElement = null;
                return false;
            }

            uiElement = infoBarUIFactory.CreateInfoBar(infoBar);
            return uiElement != null;
        }

        private static ToolWindowPane GetSolutionExplorerPane() {
            var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            var slnExplorerGuid = new Guid(ToolWindowGuids80.SolutionExplorer);

            IVsWindowFrame frame;
            if (ErrorHandler.Succeeded(uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref slnExplorerGuid, out frame))) {
                object pane;
                if (ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pane))) {
                    return pane as ToolWindowPane;
                }
            }

            return null;
        }

        #region IVsInfoBarEvents

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem) {
            if (actionItem.Equals(_customizeInfoBarButton)) {
                var optionsCommand = new CommandID(
                    VSConstants.GUID_VSStandardCommandSet97,
                    VSConstants.cmdidToolsOptions);
                var menuCommandService = _provider.GetService(typeof(IMenuCommandService)) as MenuCommandService;
                string intelliSenseOptionsGuidString = typeof(NodejsIntellisenseOptionsPage).GUID.ToString();

                menuCommandService.GlobalInvoke(optionsCommand, intelliSenseOptionsGuidString);
            } else if (actionItem.Equals(_typingsFolderHyperlink)) {
                CommonPackage.OpenVsWebBrowser(_provider, "http://go.microsoft.com/fwlink/?LinkID=808345");
            }
        }

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement) {
            _isVisible = false;
            return;
        }

        #endregion
    }
}

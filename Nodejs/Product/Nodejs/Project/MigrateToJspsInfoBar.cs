using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.NodejsTools.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Project
{
    internal static class MigrateToJspsInfoBar
    {
        private static IVsInfoBarUIElement CurrentInfoBarElement;

        public static void Show(IVsShell vsShell, IVsInfoBarUIFactory infoBarUiFactory)
        {
            if (ErrorHandler.Failed(vsShell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var tmp)))
            {
                return;
            }
            var infoBarHost = (IVsInfoBarHost)tmp;

            CurrentInfoBarElement?.Close();

            Action migrateProjectCmd = MigrateProjectCommand;

            var actionItems = new[]
            {
                new InfoBarHyperlink(Resources.MigrateToJspsClickHere, migrateProjectCmd)
            };
            var infoBarModel = new InfoBarModel(Resources.MigrateToJspsPrompt, actionItems);

            uint eventCookie = 2;
            CurrentInfoBarElement = infoBarUiFactory.CreateInfoBar(infoBarModel);
            CurrentInfoBarElement.Advise(new InfoBarUIEvents(OnClose), out eventCookie);

            infoBarHost.AddInfoBar(CurrentInfoBarElement);

            void OnClose()
            {
                CurrentInfoBarElement.Unadvise(eventCookie); 
            }

            void MigrateProjectCommand()
            {
                NodejsPackage.Instance.DTE.Commands.Raise(Guids.MigrateToJspsCmdSet.ToString(), PkgCmdId.cmdidJspsProjectMigrate, null, null);
            }
        }

        private sealed class InfoBarUIEvents : IVsInfoBarUIEvents
        {
            private readonly Action OnClose;

            public InfoBarUIEvents(Action onClose)
            {
                this.OnClose = onClose;
            }

            public void OnClosed(IVsInfoBarUIElement infoBarUIElement) => this.OnClose();

            public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem) => (actionItem.ActionContext as Action)?.Invoke();
        }
    }
}

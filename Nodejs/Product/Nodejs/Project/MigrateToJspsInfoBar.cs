using System;
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

            var infoBarModel = new InfoBarModel(Resources.MigrateToJspsPrompt);

            uint eventCookie = 2;
            CurrentInfoBarElement = infoBarUiFactory.CreateInfoBar(infoBarModel);
            CurrentInfoBarElement.Advise(new InfoBarUIEvents(OnClose), out eventCookie);

            infoBarHost.AddInfoBar(CurrentInfoBarElement);

            void OnClose()
            {
                CurrentInfoBarElement.Unadvise(eventCookie); 
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

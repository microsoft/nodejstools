// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Project
{
    internal static class MissingNodeInfoBar
    {
        private static IVsInfoBarUIElement CurrentInfoBarElement;

        public static void Show(NodejsProjectNode projectNode)
        {
            var serviceProvider = projectNode.Site;

            serviceProvider.GetUIThread().MustBeCalledFromUIThread();

            var vsShell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
            var infoBarUIFactory = (IVsInfoBarUIFactory)serviceProvider.GetService(typeof(SVsInfoBarUIFactory));

            if (ErrorHandler.Failed(vsShell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var tmp)))
            {
                // we don't want to crash just because we can't show the error bar
                return;
            }
            var infoBarHost = (IVsInfoBarHost)tmp;

            // make sure we close the previous infobar
            CurrentInfoBarElement?.Close();

            Action downloadNode = DownloadNode;
            Action openProjectProps = ShowProjectProperties;

            var actionItems = new[]
            {
                new InfoBarHyperlink(Resources.ConfigureProjectProperties, openProjectProps),
                new InfoBarHyperlink(Resources.DownloadNodejs, downloadNode),
            };

            var infoBarModel = new InfoBarModel(Resources.NodejsNotInstalledInfoBar, actionItems, isCloseButtonVisible: true, image: KnownMonikers.StatusError);

            uint eventCookie = 0;
            CurrentInfoBarElement = infoBarUIFactory.CreateInfoBar(infoBarModel);
            CurrentInfoBarElement.Advise(new InfoBarUIEvents(OnClose), out eventCookie);

            infoBarHost.AddInfoBar(CurrentInfoBarElement);

            void OnClose()
            {
                CurrentInfoBarElement.Unadvise(eventCookie);
            }

            void DownloadNode()
            {
                const string url = @"https://aka.ms/downloadnode";

                VsShellUtilities.OpenBrowser(url, (uint)__VSOSPFLAGS.OSP_LaunchNewBrowser);
            }

            void ShowProjectProperties()
            {
                // open Project Properties
                var logicalView = VSConstants.LOGVIEWID_Primary;
                if (ErrorHandler.Succeeded(projectNode.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)VsHierarchyPropID.ProjectDesignerEditor, out var editorType)) &&
                    ErrorHandler.Succeeded(projectNode.OpenItemWithSpecific(VSConstants.VSITEMID_ROOT, 0, ref editorType, "", ref logicalView, (IntPtr)(-1), out var frame)))
                {
                    frame?.Show();
                }
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

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal class OutputWindowRedirector : Redirector
    {
        private static readonly Guid OutputWindowGuid = new Guid("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");
        private static OutputWindowRedirector _generalPane;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets or creates the specified output pane.
        /// </summary>
        /// <exception cref="InvalidOperationException">The output pane could
        /// not be found or created.</exception>
        public static OutputWindowRedirector Get(IServiceProvider provider, Guid id, string title)
        {
            var outputWindow = provider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
            {
                throw new InvalidOperationException("Unable to get output window service");
            }

            if (ErrorHandler.Failed(outputWindow.GetPane(id, out var pane)) || pane == null)
            {
                if (ErrorHandler.Failed(provider.GetUIThread().Invoke(() => outputWindow.CreatePane(id, title, 1, 0))))
                {
                    throw new InvalidOperationException("Unable to create output pane");
                }
            }
            return new OutputWindowRedirector(provider, id);
        }

        /// <summary>
        /// Gets or creates the "General" output pane.
        /// </summary>
        /// <exception cref="InvalidOperationException">The "General" pane could
        /// not be found or created.</exception>
        public static OutputWindowRedirector GetGeneral(IServiceProvider provider)
        {
            if (_generalPane == null)
            {
                _generalPane = Get(provider, VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "General");
            }
            return _generalPane;
        }

        private readonly IVsWindowFrame _window;
        private readonly IVsOutputWindowPane _pane;

        public IVsOutputWindowPane Pane => this._pane;
        /// <summary>
        /// Creates a redirector to the specified output pane.
        /// </summary>
        /// <param name="provider">
        /// An active service provider.
        /// </param>
        /// <param name="paneGuid">
        /// The ID of the pane to direct output to.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The pane could not be found or the Output Window service is not
        /// available.
        /// </exception>
        public OutputWindowRedirector(IServiceProvider provider, Guid paneGuid)
        {
            this._serviceProvider = provider;
            if (provider.GetService(typeof(SVsUIShell)) is IVsUIShell shell)
            {
                // Ignore errors - we just won't support opening the window if
                // we don't find it.
                var windowGuid = OutputWindowGuid;
                shell.FindToolWindow(0, ref windowGuid, out this._window);
            }
            var outputWindow = provider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
            {
                throw new InvalidOperationException("Unable to get output window service");
            }
            if (ErrorHandler.Failed(outputWindow.GetPane(paneGuid, out this._pane)))
            {
                throw new InvalidOperationException("Unable to get output pane");
            }
        }

        public override void Show()
        {
            this._serviceProvider.GetUIThread().Invoke(() => ErrorHandler.ThrowOnFailure(this._pane.Activate()));
        }

        public override void ShowAndActivate()
        {
            this._serviceProvider.GetUIThread().Invoke(() =>
            {
                ErrorHandler.ThrowOnFailure(this._pane.Activate());
                if (this._window != null)
                {
                    ErrorHandler.ThrowOnFailure(this._window.ShowNoActivate());
                }
            });
        }

        public override void WriteLine(string line)
        {
            this._pane.OutputStringThreadSafe(line + Environment.NewLine);
            Debug.WriteLine(line, "Output Window");
        }

        public override void WriteErrorLine(string line)
        {
            this._pane.OutputStringThreadSafe(line + Environment.NewLine);
            Debug.WriteLine(line, "Output Window");
        }
    }
}

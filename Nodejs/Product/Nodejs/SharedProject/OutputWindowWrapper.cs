// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    [Export(typeof(OutputPaneWrapper))]
    internal sealed class OutputPaneWrapper
    {
        private static readonly Guid OutputWindowGuid = new Guid("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");

        // This is the package manager pane that ships with VS2015, and we should print there if available.
        private static readonly Guid VSPackageManagerPaneGuid = new Guid("C7E31C31-1451-4E05-B6BE-D11B6829E8BB");

        [Import]
        private SVsServiceProvider ServiceProvider { get; set; }

        private IVsOutputWindowPane lazyOutputPane;
        private IVsWindowFrame lazyOutputWindow;

        private IVsOutputWindowPane OutputPane
        {
            get
            {
                if (this.lazyOutputPane == null)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    var outputWindow = (IVsOutputWindow)this.ServiceProvider.GetService(typeof(SVsOutputWindow));

                    // Try to get the workspace pane if it has already been registered
                    var hr = outputWindow.GetPane(VSPackageManagerPaneGuid, out this.lazyOutputPane);

                    // If the workspace pane has not been registered before, create it
                    if (this.lazyOutputPane == null || ErrorHandler.Failed(hr))
                    {
                        if (ErrorHandler.Failed(outputWindow.CreatePane(VSPackageManagerPaneGuid, Resources.NpmOutputPaneTitle, fInitVisible: 1, fClearWithSolution: 1)) ||
                            ErrorHandler.Failed(outputWindow.GetPane(VSPackageManagerPaneGuid, out this.lazyOutputPane)))
                        {
                            return null;
                        }

                        // Must activate the workspace pane for it to show up in the output window
                        this.lazyOutputPane.Activate();
                    }
                }

                return this.lazyOutputPane;
            }
        }

        private IVsWindowFrame OutputWindow
        {
            get
            {
                if (this.lazyOutputWindow == null && this.ServiceProvider.GetService(typeof(SVsUIShell)) is IVsUIShell shell)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    var windowGuid = OutputWindowGuid;
                    var hr = shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fForceCreate, ref windowGuid, out this.lazyOutputWindow);
                    Debug.Assert(ErrorHandler.Succeeded(hr));
                }

                return this.lazyOutputWindow;
            }
        }

        public void WriteLine(string message)
        {
            if (this.lazyOutputWindow == null)
            {
                throw new InvalidOperationException($"Ensure the output window is initialized by calling '{nameof(ShowWindow)}' first.");
            }

            var hr = this.OutputPane.OutputStringThreadSafe(message + Environment.NewLine);
            Debug.Assert(ErrorHandler.Succeeded(hr));
        }

        public void ShowWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var hr = this.OutputWindow?.ShowNoActivate() ?? VSConstants.E_FAIL;
            Debug.Assert(ErrorHandler.Succeeded(hr));
        }
    }
}

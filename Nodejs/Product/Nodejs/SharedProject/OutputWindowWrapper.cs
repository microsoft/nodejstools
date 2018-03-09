// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    [Export(typeof(OutputPaneWrapper))]
    public sealed class OutputPaneWrapper
    {
        private static readonly Guid OutputWindowGuid = new Guid("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");

        // This is the package manager pane that ships with VS2015, and we should print there if available.
        private static readonly Guid VSPackageManagerPaneGuid = new Guid("C7E31C31-1451-4E05-B6BE-D11B6829E8BB");

        private static readonly Guid TscPaneGuid = new Guid("7CCFA622-001E-4459-9D8B-5B60BE0A18C2");

        [Import]
        private SVsServiceProvider ServiceProvider { get; set; }

        private Dictionary<OutputWindowTarget, IVsOutputWindowPane> lazyOutputPaneCollection = new Dictionary<OutputWindowTarget, IVsOutputWindowPane>();
        private IVsWindowFrame lazyOutputWindow;

        private IVsOutputWindowPane GetOutputPane(OutputWindowTarget target)
        {
            if (this.lazyOutputPaneCollection.TryGetValue(target, out var lazyOutputPane))
            {
                return lazyOutputPane;
            }
            else
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var (title, guid) = GetPaneInfo(target);
                var outputWindow = (IVsOutputWindow)this.ServiceProvider.GetService(typeof(SVsOutputWindow));

                // Try to get the workspace pane if it has already been registered
                var hr = outputWindow.GetPane(guid, out lazyOutputPane);

                // If the workspace pane has not been registered before, create it
                if (lazyOutputPane == null || ErrorHandler.Failed(hr))
                {

                    if (ErrorHandler.Failed(outputWindow.CreatePane(guid, title, fInitVisible: 1, fClearWithSolution: 1)) ||
                        ErrorHandler.Failed(outputWindow.GetPane(guid, out lazyOutputPane)))
                    {
                        return null;
                    }

                    // Must activate the workspace pane for it to show up in the output window
                    lazyOutputPane.Activate();
                }

                this.lazyOutputPaneCollection.Add(target, lazyOutputPane);
                return lazyOutputPane;
            }

            (string title, Guid guid) GetPaneInfo(OutputWindowTarget pane)
            {
                switch (pane)
                {
                    case OutputWindowTarget.Npm:
                        return ("Npm", VSPackageManagerPaneGuid);
                    case OutputWindowTarget.Tsc:
                        return ("Tsc", TscPaneGuid); /* no need to localize since name of exe */
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pane));
                }
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

        public void WriteLine(string message, OutputWindowTarget target = OutputWindowTarget.Npm)
        {
            if (this.lazyOutputWindow == null)
            {
                throw new InvalidOperationException($"Ensure the output window is initialized by calling '{nameof(ShowWindow)}' first.");
            }

            var hr = this.GetOutputPane(target).OutputStringThreadSafe(message + Environment.NewLine);
            Debug.Assert(ErrorHandler.Succeeded(hr));
        }

        public void ShowWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var hr = this.OutputWindow?.ShowNoActivate() ?? VSConstants.E_FAIL;
            Debug.Assert(ErrorHandler.Succeeded(hr));
        }
    }

    public enum OutputWindowTarget
    {
        Tsc,
        Npm,
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
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

        private ConcurrentDictionary<OutputWindowTarget, IVsOutputWindowPane> lazyOutputPaneCollection = new ConcurrentDictionary<OutputWindowTarget, IVsOutputWindowPane>();

        private IVsOutputWindowPane InitializeOutputPane(string title, Guid paneId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var outputWindow = (IVsOutputWindow)this.ServiceProvider.GetService(typeof(SVsOutputWindow));

            // Try to get the workspace pane if it has already been registered
            var hr = outputWindow.GetPane(paneId, out var lazyOutputPane);

            // If the workspace pane has not been registered before, create it
            if (lazyOutputPane == null || ErrorHandler.Failed(hr))
            {

                if (ErrorHandler.Failed(outputWindow.CreatePane(paneId, title, fInitVisible: 1, fClearWithSolution: 1)) ||
                    ErrorHandler.Failed(outputWindow.GetPane(paneId, out lazyOutputPane)))
                {
                    return null;
                }

                // Must activate the workspace pane for it to show up in the output window
                lazyOutputPane.Activate();
            }

            return lazyOutputPane;
        }

        public void WriteLine(string message, OutputWindowTarget target = OutputWindowTarget.Npm)
        {
            if (!this.IsInitialized())
            {
                throw new InvalidOperationException("You need to initialize the output panes before using them.");
            }

            var hr = this.lazyOutputPaneCollection[target].OutputStringThreadSafe(message + Environment.NewLine);
            Debug.Assert(ErrorHandler.Succeeded(hr));
        }

        public void InitializeOutputPanes()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!this.IsInitialized())
            {
                var npmPane = InitializeOutputPane("Npm", VSPackageManagerPaneGuid);
                this.lazyOutputPaneCollection.TryAdd(OutputWindowTarget.Npm, npmPane);
                var tscPane = InitializeOutputPane("Tsc", TscPaneGuid);
                this.lazyOutputPaneCollection.TryAdd(OutputWindowTarget.Tsc, tscPane);
            }
        }

        private bool IsInitialized()
        {
            return this.lazyOutputPaneCollection.TryGetValue(OutputWindowTarget.Npm, out var npmWindow) && npmWindow != null
                && this.lazyOutputPaneCollection.TryGetValue(OutputWindowTarget.Tsc, out var tscWindow) && tscWindow != null;
        }
    }

    public enum OutputWindowTarget
    {
        Tsc,
        Npm,
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.NodejsTools.Extras;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(InteractiveWindowProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class InteractiveWindowProvider
    {
        private IVsInteractiveWindow window;
        private int curId = 1;

        private readonly IServiceProvider serviceProvider;
        private readonly IVsInteractiveWindowFactory windowFactory;
        private readonly IContentType typeScriptContentType;
        private readonly IContentType nodeInteractiveContentType;

        [ImportingConstructor]
        public InteractiveWindowProvider(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import] IVsInteractiveWindowFactory2 factory,
            [Import] IContentTypeRegistryService contentTypeService)
        {
            this.serviceProvider = serviceProvider;
            this.windowFactory = factory;

            this.typeScriptContentType = contentTypeService.GetContentType(NodejsConstants.TypeScript);
            this.nodeInteractiveContentType = contentTypeService.GetContentType(InteractiveWindowContentType.ContentType);
        }

        public IVsInteractiveWindow OpenOrCreateWindow(int id)
        {
            if (this.window == null)
            {
                this.window = CreateReplWindow(id);
            }

            return this.window;
        }

        private IVsInteractiveWindow CreateReplWindow(int replId)
        {
            if (replId < 0)
            {
                replId = this.curId++;
            }

            if (this.window != null)
            {
                Debug.Fail("Should not create a window when we already have a window.");
            }

            this.window = CreateReplWindowInternal(this.GetReplEvaluator(), replId, Resources.InteractiveWindowTitle, Guids.TypeScriptLanguageInfo);
            this.window.InteractiveWindow.TextView.Closed += (s, e) =>
            {
                this.window = null;
            };

            return this.window;
        }

        private IInteractiveEvaluator GetReplEvaluator()
        {
            return new NodejsReplEvaluator(this.serviceProvider, this.nodeInteractiveContentType);
        }

        private IVsInteractiveWindow CreateReplWindowInternal(IInteractiveEvaluator evaluator, int id, string title, Guid languageServiceGuid)
        {
            var creationFlags =
                __VSCREATETOOLWIN.CTW_fMultiInstance |
                __VSCREATETOOLWIN.CTW_fActivateWithProject |
                __VSCREATETOOLWIN.CTW_fForceCreate;

            var replWindow = this.windowFactory.Create(
                Guids.NodejsInteractiveWindow,
                id,
                title,
                evaluator,
                creationFlags);

            if (replWindow is ToolWindowPane toolwindow)
            {
                toolwindow.BitmapImageMoniker = KnownMonikers.JSInteractiveWindow;
            }
            replWindow.SetLanguage(languageServiceGuid, this.typeScriptContentType);
            replWindow.InteractiveWindow.InitializeAsync();

            return replWindow;
        }
    }
}

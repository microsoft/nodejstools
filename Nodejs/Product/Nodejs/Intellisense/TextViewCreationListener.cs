/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.ComponentModel.Composition;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Intellisense {
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(NodejsConstants.Nodejs)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class TextViewCreationListener : IVsTextViewCreationListener {
        private readonly IVsEditorAdaptersFactoryService _adaptersFactory;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IComponentModel _compModel;
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;

        [ImportingConstructor]
        public TextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory, IEditorOperationsFactoryService editorOperationsFactory, [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider, IEditorOptionsFactoryService editorOptionsFactory) {
            _adaptersFactory = adaptersFactory;
            _editorOperationsFactory = editorOperationsFactory;
            _compModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _editorOptionsFactory = editorOptionsFactory;
        }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(VisualStudio.TextManager.Interop.IVsTextView textViewAdapter) {
            var textView = _adaptersFactory.GetWpfTextView(textViewAdapter);            
            var editFilter = new EditFilter(
                textView,
                _editorOperationsFactory.GetEditorOperations(textView),
                _editorOptionsFactory.GetOptions(textView),
                _compModel.GetService<IIntellisenseSessionStackMapService>().GetStackForTextView(textView),
                _compModel
            );
            IntellisenseController controller;
            if (textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller)) {
                controller.AttachKeyboardFilter();
            }
            editFilter.AttachKeyboardFilter(textViewAdapter);
        }

        #endregion
    }

    [Export(typeof(IReplWindowCreationListener))]
    [ContentType(NodejsConstants.Nodejs)]
    class ReplWindowTextViewCreationListener : IReplWindowCreationListener {
        private readonly IVsEditorAdaptersFactoryService _adaptersFactory;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IComponentModel _compModel;
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;

        [ImportingConstructor]
        public ReplWindowTextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory, IEditorOperationsFactoryService editorOperationsFactory, [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider, IEditorOptionsFactoryService editorOptionsFactory) {
            _adaptersFactory = adaptersFactory;
            _editorOperationsFactory = editorOperationsFactory;
            _compModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _editorOptionsFactory = editorOptionsFactory;
        }

        public void ReplWindowCreated(IReplWindow window) {
            var textView = window.TextView;
            var editFilter = new EditFilter(
                textView,
                _editorOperationsFactory.GetEditorOperations(textView),
                _editorOptionsFactory.GetOptions(textView),
                _compModel.GetService<IIntellisenseSessionStackMapService>().GetStackForTextView(textView),
                _compModel
            );
            IntellisenseController controller = IntellisenseControllerProvider.GetOrCreateController(_compModel, textView);
            controller.AttachKeyboardFilter();
            
            editFilter.AttachKeyboardFilter(_adaptersFactory.GetViewAdapter(window.TextView));
        }
    }
}

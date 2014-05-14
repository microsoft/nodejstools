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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Intellisense {
    [Export(typeof(IIntellisenseControllerProvider)), ContentType(NodejsConstants.Nodejs), Order]
    class IntellisenseControllerProvider : IIntellisenseControllerProvider {
        [Import]
        internal ICompletionBroker _CompletionBroker = null; // Set via MEF
        [Import]
        internal IEditorOperationsFactoryService _EditOperationsFactory = null; // Set via MEF
        [Import]
        internal IVsEditorAdaptersFactoryService _adaptersFactory { get; set; }
        [Import]
        internal ISignatureHelpBroker _SigBroker = null; // Set via MEF
        [Import]
        internal IQuickInfoBroker _QuickInfoBroker = null; // Set via MEF
        [Import]
        internal IIncrementalSearchFactoryService _IncrementalSearch = null; // Set via MEF
        [Import]
        internal IClassifierAggregatorService _classifierAgg = null; // Set via MEF

        readonly Dictionary<ITextView, Tuple<BufferParser, VsProjectAnalyzer>> _hookedCloseEvents =
            new Dictionary<ITextView, Tuple<BufferParser, VsProjectAnalyzer>>();

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers) {
            IntellisenseController controller;
            if (!textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller)) {
                controller = new IntellisenseController(this, textView);
            }

            var analyzer = textView.GetAnalyzer();
            if (analyzer != null) {
                var buffer = subjectBuffers[0];

                foreach (var subjBuf in subjectBuffers) {
                    controller.PropagateAnalyzer(subjBuf);
                }

                var entry = analyzer.MonitorTextBuffer(textView, buffer);
                _hookedCloseEvents[textView] = Tuple.Create(entry.BufferParser, analyzer);
                textView.Closed += TextView_Closed;

                for (int i = 1; i < subjectBuffers.Count; i++) {
                    entry.BufferParser.AddBuffer(subjectBuffers[i]);
                }
                controller.SetBufferParser(entry.BufferParser);
            }
            return controller;
        }

        private void TextView_Closed(object sender, EventArgs e) {
            var textView = sender as ITextView;
            Tuple<BufferParser, VsProjectAnalyzer> tuple;
            if (textView == null || !_hookedCloseEvents.TryGetValue(textView, out tuple)) {
                return;
            }

            textView.Closed -= TextView_Closed;
            _hookedCloseEvents.Remove(textView);

            if (tuple.Item1.AttachedViews == 0) {
                tuple.Item2.StopMonitoringTextBuffer(tuple.Item1, textView);
            }
        }

        internal static IntellisenseController GetOrCreateController(IComponentModel model, ITextView textView) {
            IntellisenseController controller;
            if (!textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller)) {
                var intellisenseControllerProvider = (
                   from export in model.DefaultExportProvider.GetExports<IIntellisenseControllerProvider, IContentTypeMetadata>()
                   from exportedContentType in export.Metadata.ContentTypes
                   where exportedContentType == NodejsConstants.Nodejs && export.Value.GetType() == typeof(IntellisenseControllerProvider)
                   select export.Value
                ).First();
                controller = new IntellisenseController((IntellisenseControllerProvider)intellisenseControllerProvider, textView);
            }
            return controller;
        }
    }
}

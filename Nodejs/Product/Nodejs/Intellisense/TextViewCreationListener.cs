//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
        private readonly IServiceProvider _serviceProvider;
        private readonly IVsEditorAdaptersFactoryService _adaptersFactory;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IComponentModel _compModel;
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;

        [ImportingConstructor]
        public TextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory, IEditorOperationsFactoryService editorOperationsFactory, [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider, IEditorOptionsFactoryService editorOptionsFactory) {
            _serviceProvider = serviceProvider;
            _adaptersFactory = adaptersFactory;
            _editorOperationsFactory = editorOperationsFactory;
            _compModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _editorOptionsFactory = editorOptionsFactory;
        }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(VisualStudio.TextManager.Interop.IVsTextView textViewAdapter) {
            var textView = _adaptersFactory.GetWpfTextView(textViewAdapter);            
            var editFilter = new EditFilter(
                _serviceProvider,
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IVsEditorAdaptersFactoryService _adaptersFactory;
        private readonly IEditorOperationsFactoryService _editorOperationsFactory;
        private readonly IComponentModel _compModel;
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;

        [ImportingConstructor]
        public ReplWindowTextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory, IEditorOperationsFactoryService editorOperationsFactory, [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider, IEditorOptionsFactoryService editorOptionsFactory) {
            _serviceProvider = serviceProvider;
            _adaptersFactory = adaptersFactory;
            _editorOperationsFactory = editorOperationsFactory;
            _compModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _editorOptionsFactory = editorOptionsFactory;
        }

        public void ReplWindowCreated(IReplWindow window) {
            var textView = window.TextView;
            var editFilter = new EditFilter(
                _serviceProvider,
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

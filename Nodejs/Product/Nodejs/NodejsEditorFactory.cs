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
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudioTools;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.NodejsTools {
    /// <summary>
    /// Common factory for creating our editor
    /// </summary>    
    [Guid(Guids.NodejsEditorFactoryString)]
    class NodejsEditorFactory : IVsEditorFactory {

        [Export, Name(NodejsConstants.Nodejs), BaseDefinition("text")]
        internal static ContentTypeDefinition ContentTypeDefinition = null;
        
        private NodejsPackage _package;
        private ServiceProvider _serviceProvider;
        private readonly bool _promptEncodingOnLoad;

        public NodejsEditorFactory(NodejsPackage package) {
            _package = package;
        }

        public NodejsEditorFactory(NodejsPackage package, bool promptEncodingOnLoad) {
            _package = package;
            _promptEncodingOnLoad = promptEncodingOnLoad;
        }

        #region IVsEditorFactory Members

        public virtual int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp) {
            _serviceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public virtual object GetService(Type serviceType) {
            return _serviceProvider.GetService(serviceType);
        }

        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a 
        // PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a 
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type 
        // of view implementation that an IVsEditorFactory can create. 
        //
        // NOTE: Physical views are identified by a string of your choice with the 
        // one constraint that the default/primary physical view for an editor  
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        //
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the editor.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        //
        // NOTE: The special Logical Views supported by an Editor Factory must also 
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly 
        // supported by all editor types and does not need to be registered.
        // For example, an editor that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //        HKLM\Software\Microsoft\VisualStudio\9.0\Editors\
        //            {...guidEditor...}\
        //                LogicalViews\
        //                    {...LOGVIEWID_TextView...} = s ''
        //                    {...LOGVIEWID_Code...} = s ''
        //                    {...LOGVIEWID_Debugging...} = s ''
        //                    {...LOGVIEWID_Designer...} = s 'Form'
        //
        public virtual int MapLogicalView(ref Guid logicalView, out string physicalView) {
            // initialize out parameter
            physicalView = null;

            bool isSupportedView = false;
            // Determine the physical view
            if (VSConstants.LOGVIEWID_Primary == logicalView ||
                VSConstants.LOGVIEWID_Debugging == logicalView ||
                VSConstants.LOGVIEWID_Code == logicalView ||
                VSConstants.LOGVIEWID_TextView == logicalView) {
                // primary view uses NULL as pbstrPhysicalView
                isSupportedView = true;
            } else if (VSConstants.LOGVIEWID_Designer == logicalView) {
                physicalView = "Design";
                isSupportedView = true;
            }

            if (isSupportedView)
                return VSConstants.S_OK;
            else {
                // E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public virtual int Close() {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grfCreateDoc"></param>
        /// <param name="pszMkDocument"></param>
        /// <param name="pszPhysicalView"></param>
        /// <param name="pvHier"></param>
        /// <param name="itemid"></param>
        /// <param name="punkDocDataExisting"></param>
        /// <param name="ppunkDocView"></param>
        /// <param name="ppunkDocData"></param>
        /// <param name="pbstrEditorCaption"></param>
        /// <param name="pguidCmdUI"></param>
        /// <param name="pgrfCDW"></param>
        /// <returns></returns>
        public virtual int CreateEditorInstance(
                        uint createEditorFlags,
                        string documentMoniker,
                        string physicalView,
                        IVsHierarchy hierarchy,
                        uint itemid,
                        System.IntPtr docDataExisting,
                        out System.IntPtr docView,
                        out System.IntPtr docData,
                        out string editorCaption,
                        out Guid commandUIGuid,
                        out int createDocumentWindowFlags) {
            // Initialize output parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = this.GetType().GUID;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0) {
                return VSConstants.E_INVALIDARG;
            }

            // Get a text buffer
            IVsTextLines textLines = GetTextBuffer(docDataExisting, documentMoniker);

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero) {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            } else {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try {
                docView = CreateDocumentView(documentMoniker, physicalView, hierarchy, itemid, textLines, docDataExisting == IntPtr.Zero, out editorCaption, out commandUIGuid);
            } finally {
                if (docView == IntPtr.Zero) {
                    if (docDataExisting != docData && docData != IntPtr.Zero) {
                        // Cleanup the instance of the docData that we have addref'ed
                        Marshal.Release(docData);
                        docData = IntPtr.Zero;
                    }
                }
            }
            return VSConstants.S_OK;
        }


        #endregion

        #region Helper methods

        private IVsTextLines GetTextBuffer(System.IntPtr docDataExisting, string filename) {
            IVsTextLines textLines;
            if (docDataExisting == IntPtr.Zero) {
                // Create a new IVsTextLines buffer.
                Type textLinesType = typeof(IVsTextLines);
                Guid riid = textLinesType.GUID;
                Guid clsid = typeof(VsTextBufferClass).GUID;
                textLines = _package.CreateInstance(ref clsid, ref riid, textLinesType) as IVsTextLines;

                // set the buffer's site
                ((IObjectWithSite)textLines).SetSite(_serviceProvider.GetService(typeof(IOleServiceProvider)));
            } else {
                // Use the existing text buffer
                Object dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;
                if (textLines == null) {
                    // Try get the text buffer from textbuffer provider
                    IVsTextBufferProvider textBufferProvider = dataObject as IVsTextBufferProvider;
                    if (textBufferProvider != null) {
                        textBufferProvider.GetTextBuffer(out textLines);
                    }
                }
                if (textLines == null) {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }

            }
            return textLines;
        }

        protected void InitializeLanguageService(IVsTextLines textLines, Guid langSid) {
            IVsUserData userData = textLines as IVsUserData;
            if (userData != null) {
                if (langSid != Guid.Empty) {
                    Guid vsCoreSid = Guids.DefaultLanguageService;
                    Guid currentSid;
                    ErrorHandler.ThrowOnFailure(textLines.GetLanguageServiceID(out currentSid));
                    // If the language service is set to the default SID, then
                    // set it to our language
                    if (currentSid == vsCoreSid) {
                        ErrorHandler.ThrowOnFailure(textLines.SetLanguageServiceID(ref langSid));
                    } else if (currentSid != langSid) {
                        // Some other language service has it, so return VS_E_INCOMPATIBLEDOCDATA
                        throw new COMException("Incompatible doc data", VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                    }

                    Guid bufferDetectLang = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
                    ErrorHandler.ThrowOnFailure(userData.SetData(ref bufferDetectLang, false));
                }
            }
        }

        private void InitializeLanguageService(IVsTextLines textLines) {
            InitializeLanguageService(textLines, typeof(NodejsEditorFactory).GUID);
        }

        private IntPtr CreateDocumentView(string documentMoniker, string physicalView, IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, bool createdDocData, out string editorCaption, out Guid cmdUI) {
            //Init out params
            editorCaption = string.Empty;
            cmdUI = Guid.Empty;

            if (string.IsNullOrEmpty(physicalView)) {
                // create code window as default physical view
                return CreateCodeView(documentMoniker, textLines, hierarchy, itemid, createdDocData, ref editorCaption, ref cmdUI);
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_UNSUPPORTEDFORMAT);

            return IntPtr.Zero;
        }

        private IntPtr CreateCodeView(string documentMoniker, IVsTextLines textLines, IVsHierarchy hierarchy, uint itemid, bool createdDocData, ref string editorCaption, ref Guid cmdUI) {
            Type codeWindowType = typeof(IVsCodeWindow);
            Guid riid = codeWindowType.GUID;
            Guid clsid = typeof(VsCodeWindowClass).GUID;
            var compModel = (IComponentModel)_package.GetService(typeof(SComponentModel));
            var adapterService = compModel.GetService<IVsEditorAdaptersFactoryService>();

            var window = adapterService.CreateVsCodeWindowAdapter((IOleServiceProvider)_serviceProvider.GetService(typeof(IOleServiceProvider)));
            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            IVsUserData userData = textLines as IVsUserData;
            if (userData != null) {
                if (_promptEncodingOnLoad) {
                    var guid = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
                    userData.SetData(ref guid, (uint)1);
                }
            }
            var textMgr = (IVsTextManager)_package.GetService(typeof(SVsTextManager));

            var bufferEventListener = new TextBufferEventListener(textLines);
            if (!createdDocData) {
                // we have a pre-created buffer, go ahead and initialize now as the buffer already
                // exists and is initialized.
                bufferEventListener.OnLoadCompleted(0);
            }

            InitializeLanguageService(textLines);

            cmdUI = VSConstants.GUID_TextEditorFactory;

            return Marshal.GetIUnknownForObject(window);
        }

        #endregion

        /// <summary>
        /// Listens for the text buffer to finish loading and then sets up our projection
        /// buffer.
        /// </summary>
        internal sealed class TextBufferEventListener : IVsTextBufferDataEvents {
            private readonly IVsTextLines _textLines;
            private readonly uint _cookie;
            private readonly IConnectionPoint _cp;

            public TextBufferEventListener(IVsTextLines textLines) {
                _textLines = textLines;

                var cpc = textLines as IConnectionPointContainer;
                var bufferEventsGuid = typeof(IVsTextBufferDataEvents).GUID;
                cpc.FindConnectionPoint(ref bufferEventsGuid, out _cp);
                _cp.Advise(this, out _cookie);
            }

            #region IVsTextBufferDataEvents

            public void OnFileChanged(uint grfChange, uint dwFileAttrs) {
            }

            public int OnLoadCompleted(int fReload) {
                _cp.Unadvise(_cookie);

                Guid langSvcGuid = typeof(NodejsLanguageInfo).GUID;
                _textLines.SetLanguageServiceID(ref langSvcGuid);
                
                return VSConstants.S_OK;
            }

            #endregion
        }
    }

    [Guid(Guids.NodejsEditorFactoryPromptEncodingString)]
    class NodejsEditorFactoryPromptForEncoding : NodejsEditorFactory {
        public NodejsEditorFactoryPromptForEncoding(NodejsPackage package) : base(package, true) { }
        public override int CreateEditorInstance(uint createEditorFlags, string documentMoniker, string physicalView, VisualStudio.Shell.Interop.IVsHierarchy hierarchy, uint itemid, IntPtr docDataExisting, out IntPtr docView, out IntPtr docData, out string editorCaption, out Guid commandUIGuid, out int createDocumentWindowFlags) {
            if (docDataExisting != IntPtr.Zero) {
                docView = IntPtr.Zero;
                docData = IntPtr.Zero;
                editorCaption = null;
                commandUIGuid = Guid.Empty;
                createDocumentWindowFlags = 0;
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            return base.CreateEditorInstance(createEditorFlags, documentMoniker, physicalView, hierarchy, itemid, docDataExisting, out docView, out docData, out editorCaption, out commandUIGuid, out createDocumentWindowFlags);
        }
    }
}

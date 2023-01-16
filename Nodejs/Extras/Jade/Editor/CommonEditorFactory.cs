﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Common factory for creating our editor
    /// </summary>    
    public abstract class CommonEditorFactory : IVsEditorFactory
    {
        private Package _package;
        private ServiceProvider _serviceProvider;
        private readonly bool _promptEncodingOnLoad;

        public CommonEditorFactory(Package package)
        {
            this._package = package;
        }

        public CommonEditorFactory(Package package, bool promptEncodingOnLoad)
        {
            this._package = package;
            this._promptEncodingOnLoad = promptEncodingOnLoad;
        }

        #region IVsEditorFactory Members

        public virtual int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            this._serviceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public virtual object GetService(Type serviceType)
        {
            return this._serviceProvider.GetService(serviceType);
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
        public virtual int MapLogicalView(ref Guid logicalView, out string physicalView)
        {
            // initialize out parameter
            physicalView = null;

            var isSupportedView = false;
            // Determine the physical view
            if (VSConstants.LOGVIEWID_Primary == logicalView ||
                VSConstants.LOGVIEWID_Debugging == logicalView ||
                VSConstants.LOGVIEWID_Code == logicalView ||
                VSConstants.LOGVIEWID_TextView == logicalView)
            {
                // primary view uses NULL as pbstrPhysicalView
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_Designer == logicalView)
            {
                physicalView = "Design";
                isSupportedView = true;
            }

            if (isSupportedView)
            {
                return VSConstants.S_OK;
            }
            else
            {
                // E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public virtual int Close()
        {
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
                        out int createDocumentWindowFlags)
        {
            // Initialize output parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = Guid.Empty;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            if (this._promptEncodingOnLoad && docDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Get a text buffer
            var textLines = GetTextBuffer(docDataExisting);

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero)
            {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            }
            else
            {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try
            {
                docView = CreateDocumentView(documentMoniker, physicalView, hierarchy, itemid, textLines, out editorCaption, out commandUIGuid);
            }
            finally
            {
                if (docView == IntPtr.Zero)
                {
                    if (docDataExisting != docData && docData != IntPtr.Zero)
                    {
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
        private IVsTextLines GetTextBuffer(System.IntPtr docDataExisting)
        {
            IVsTextLines textLines;
            if (docDataExisting == IntPtr.Zero)
            {
                // Create a new IVsTextLines buffer.
                var textLinesType = typeof(IVsTextLines);
                var riid = textLinesType.GUID;
                var clsid = typeof(VsTextBufferClass).GUID;
                textLines = this._package.CreateInstance(ref clsid, ref riid, textLinesType) as IVsTextLines;

                // set the buffer's site
                ((IObjectWithSite)textLines).SetSite(this._serviceProvider.GetService(typeof(IOleServiceProvider)));
            }
            else
            {
                // Use the existing text buffer
                var dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;
                if (textLines == null)
                {
                    // Try get the text buffer from textbuffer provider
                    if (dataObject is IVsTextBufferProvider textBufferProvider)
                    {
                        textBufferProvider.GetTextBuffer(out textLines);
                    }
                }
                if (textLines == null)
                {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }
            }
            return textLines;
        }

        private IntPtr CreateDocumentView(string documentMoniker, string physicalView, IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, out string editorCaption, out Guid cmdUI)
        {
            //Init out params
            editorCaption = string.Empty;
            cmdUI = Guid.Empty;

            if (string.IsNullOrEmpty(physicalView))
            {
                // create code window as default physical view
                return CreateCodeView(documentMoniker, textLines, ref editorCaption, ref cmdUI);
            }
            else if (StringComparer.InvariantCultureIgnoreCase.Equals(physicalView, "design"))
            {
                // Create Form view
                return CreateFormView(hierarchy, itemid, textLines, ref editorCaption, ref cmdUI);
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_UNSUPPORTEDFORMAT);

            return IntPtr.Zero;
        }

        private IntPtr CreateFormView(IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI)
        {
            // Request the Designer Service
            var designerService = (IVSMDDesignerService)GetService(typeof(IVSMDDesignerService));

            // Create loader for the designer
            var designerLoader = (IVSMDDesignerLoader)designerService.CreateDesignerLoader("Microsoft.VisualStudio.Designer.Serialization.VSDesignerLoader");

            var loaderInitalized = false;
            try
            {
                var provider = this._serviceProvider.GetService(typeof(IOleServiceProvider)) as IOleServiceProvider;
                // Initialize designer loader 
                designerLoader.Initialize(provider, hierarchy, (int)itemid, textLines);
                loaderInitalized = true;

                // Create the designer
                var designer = designerService.CreateDesigner(provider, designerLoader);

                // Get editor caption
                editorCaption = designerLoader.GetEditorCaption((int)READONLYSTATUS.ROSTATUS_Unknown);

                // Get view from designer
                object docView = designer.View;

                // Get command guid from designer
                cmdUI = designer.CommandGuid;

                return Marshal.GetIUnknownForObject(docView);
            }
            catch
            {
                // The designer loader may have created a reference to the shell or the text buffer.
                // In case we fail to create the designer we should manually dispose the loader
                // in order to release the references to the shell and the textbuffer
                if (loaderInitalized)
                {
                    designerLoader.Dispose();
                }
                throw;
            }
        }

        private IntPtr CreateCodeView(string documentMoniker, IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI)
        {
            var codeWindowType = typeof(IVsCodeWindow);
            var riid = codeWindowType.GUID;
            var clsid = typeof(VsCodeWindowClass).GUID;
            var window = (IVsCodeWindow)this._package.CreateInstance(ref clsid, ref riid, codeWindowType);
            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            if (textLines is IVsUserData userData)
            {
                if (this._promptEncodingOnLoad)
                {
                    var guid = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
                    userData.SetData(ref guid, (uint)1);
                }
            }

            InitializeLanguageService(textLines);

            cmdUI = VSConstants.GUID_TextEditorFactory;
            return Marshal.GetIUnknownForObject(window);
        }

        /// <summary>
        /// Initializes the language service for the given file.  This allows files with different
        /// extensions to be opened by the editor type and get correct highlighting and intellisense
        /// controllers.
        /// 
        /// New in 1.1
        /// </summary>
        protected virtual void InitializeLanguageService(IVsTextLines textLines)
        {
        }

        #endregion

        protected void InitializeLanguageService(IVsTextLines textLines, Guid langSid)
        {
            if (textLines is IVsUserData userData)
            {
                if (langSid != Guid.Empty)
                {
                    var vsCoreSid = new Guid("{8239bec4-ee87-11d0-8c98-00c04fc2ab22}");
                    ErrorHandler.ThrowOnFailure(textLines.GetLanguageServiceID(out var currentSid));
                    // If the language service is set to the default SID, then
                    // set it to our language
                    if (currentSid == vsCoreSid)
                    {
                        ErrorHandler.ThrowOnFailure(textLines.SetLanguageServiceID(ref langSid));
                    }
                    else if (currentSid != langSid)
                    {
                        // Some other language service has it, so return VS_E_INCOMPATIBLEDOCDATA
                        throw new COMException("Incompatible doc data", VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                    }

                    var bufferDetectLang = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
                    ErrorHandler.ThrowOnFailure(userData.SetData(ref bufferDetectLang, false));
                }
            }
        }
    }
}

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools {
    /// <summary>
    /// Represents a projection buffer for Node.js code.  We wrap the user's code inside of a function body
    /// and include appropriate reference tags as well which are dynamically injected.
    /// 
    /// function body() {
    ///     // begin user code
    ///     // end user code
    /// };
    /// 
    /// Which is produced by doing:
    /// 
    ///      +-------------------------+
    ///      | Projection Buffer       |     elides leading and trailing
    ///      +-------------------------+
    ///                  |
    ///   +----------------------------------+
    ///   | Leading | Disk Buffer | Trailing |  Projection Buffer, adds leading and trailing
    ///   +-------------+--------------------+
    ///                 |
    ///                 |
    ///         +----------------+
    ///         |   Disk Buffer  |
    ///         +----------------+
    ///         
    /// Currently the surface buffer is a projection buffer instead of a simpler
    /// elision buffer because elision buffers don't currently work with the
    /// JavaScript language service.
    /// </summary>
    class NodejsProjectionBuffer {
        private readonly IContentTypeRegistryService _contentRegistry;
        private readonly ITextBuffer _diskBuffer;
        private readonly IContentType _contentType;
        private readonly IProjectionBuffer _projBuffer; // the buffer we project into        
        private readonly IProjectionBuffer _elisionBuffer;
        private readonly string _referenceFilename;

        public NodejsProjectionBuffer(IContentTypeRegistryService contentRegistry, IProjectionBufferFactoryService bufferFactory, ITextBuffer diskBuffer, IBufferGraphFactoryService bufferGraphFactory, IContentType contentType, string referenceFileName) {
            _diskBuffer = diskBuffer;
            _contentRegistry = contentRegistry;
            _contentType = contentType;

            _referenceFilename = referenceFileName;
            _projBuffer = CreateProjectionBuffer(bufferFactory);
            _elisionBuffer = CreateElisionBuffer(bufferFactory);
            _elisionBuffer.Properties[typeof(NodejsProjectionBuffer)] = this;
        }

        private IProjectionBuffer CreateProjectionBuffer(IProjectionBufferFactoryService bufferFactory) {
            var res = bufferFactory.CreateProjectionBuffer(
                null,
                new object[] { 
                    LeadingText,
                    _diskBuffer.CurrentSnapshot.CreateTrackingSpan(
                        0,
                        _diskBuffer.CurrentSnapshot.Length,
                        SpanTrackingMode.EdgeInclusive,
                        TrackingFidelityMode.Forward
                    ),
                    TrailingText
                },
                ProjectionBufferOptions.None,
                _contentType
            );
            return res;
        }

        private string LeadingText {
            get {
                IVsRunningDocumentTable rdt = NodejsPackage.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
                IVsHierarchy hierarchy;
                uint itemid;
                IntPtr docData = IntPtr.Zero;
                uint docCookie;
                string asyncFilePath = null;
                try {
                    ErrorHandler.ThrowOnFailure(
                        rdt.FindAndLockDocument(
                            (uint)_VSRDTFLAGS.RDT_NoLock, 
                            _diskBuffer.GetFilePath(), 
                            out hierarchy, 
                            out itemid, 
                            out docData, 
                            out docCookie
                        )
                    );

                    var nodejsProject = hierarchy.GetProject().GetNodeProject();
                    if (nodejsProject != null) {
                        var node = nodejsProject.FindNodeByFullPath(_diskBuffer.GetFilePath()) as NodejsFileNode;
                        if (node != null) {
                            asyncFilePath = node._asyncFilePath;
                        }
                    }
                } finally {
                    if (docData != IntPtr.Zero) {
                        Marshal.Release(docData);
                    }
                }

                return
                    "/// <reference path=\"" + _referenceFilename + "\" />\r\n" +
                    (asyncFilePath != null ? ("/// <reference path=\"" + asyncFilePath + "\" />\r\n") : "") +
                    GetNodeFunctionWrapperHeader("nodejs_tools_for_visual_studio_hidden_module_body", _diskBuffer.GetFilePath());
            }
        }

        /// <summary>
        /// Gets the header function that we wrap code in to make it into the Node.js like
        /// environment for intellisense.
        /// </summary>
        /// <param name="functionName">The name of the outer function</param>
        /// <param name="filename">The .js file which we are emitting, for setting __filename and __dirname.</param>
        /// <param name="localFilenames">True if __filename and __dirname are defined as locals, false to define as globals.  
        /// 
        /// When we emit into the require() body we want these to be locals so we don't have the different
        /// modules trampling on each other.
        /// </param>
        /// <returns></returns>
        internal static string GetNodeFunctionWrapperHeader(string functionName, string filename) {
            // __filename, _dirname http://nodejs.org/api/globals.html#globals_filename
            return "function " + functionName + "() {\r\n" +
                GetFileNameAssignment(filename) +
                GetDirectoryNameAssignment(filename) +
                "var exports = {};\r\n" +
                "var module = {};\r\n" +
                "module.exports = exports;\r\n" ;
        }

        internal static string GetFileNameAssignment(string filename) {
            return "__filename = \"" + filename.Replace("\\", "\\\\") + "\";\r\n";
        }

        internal static string GetDirectoryNameAssignment(string filename) {
            if (String.IsNullOrWhiteSpace(filename)) {
                return "";
            }
            return "__dirname = \"" + Path.GetDirectoryName(filename).Replace("\\", "\\\\") + "\";\r\n";
        }

        internal static string TrailingText {
            get {
                // 
                return "\r\nreturn module.exports;\r\n}";
            }
        }

        private IProjectionBuffer CreateElisionBuffer(IProjectionBufferFactoryService bufferFactory) {
            var res = bufferFactory.CreateProjectionBuffer(
                null,
                new object[] { 
                    new CustomTrackingSpan(
                        _projBuffer.CurrentSnapshot,
                        new Span(LeadingText.Length, _projBuffer.CurrentSnapshot.Length - LeadingText.Length - TrailingText.Length),
                        PointTrackingMode.Negative,
                        PointTrackingMode.Positive
                    )
                },
                ProjectionBufferOptions.None
            );
            return res;
        }

        public IProjectionBuffer ProjectionBuffer {
            get {
                return _projBuffer;
            }
        }

        public IProjectionBuffer EllisionBuffer {
            get {
                return _elisionBuffer;
            }
        }

        public ITextBuffer DiskBuffer {
            get {
                return _diskBuffer;
            }
        }
    }
}

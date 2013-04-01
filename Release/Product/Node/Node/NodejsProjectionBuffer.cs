using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Project;
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
        private readonly NodejsProjectNode _project;

        public NodejsProjectionBuffer(IContentTypeRegistryService contentRegistry, IProjectionBufferFactoryService bufferFactory, ITextBuffer diskBuffer, IBufferGraphFactoryService bufferGraphFactory, IContentType contentType, NodejsProjectNode project) {
            _diskBuffer = diskBuffer;
            _contentRegistry = contentRegistry;
            _contentType = contentType;

            _project = project;
            _projBuffer = CreateProjectionBuffer(bufferFactory);
            _elisionBuffer = CreateElisionBuffer(bufferFactory);
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
                // 
                return "/// <reference path=\"" + _project._referenceFilename + "\" />\r\nfunction module_body() {\r\nexports = {};\r\nvar module = {}; module.exports = exports;/// <Formatting IndentLevel=\"0\">\r\n";
            }
        }

        private string TrailingText {
            get {
                // 
                return "\r\n/// </Formatting>\r\nreturn exports;\r\n}";
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
    }
}

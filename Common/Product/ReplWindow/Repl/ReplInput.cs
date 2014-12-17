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
using System.Diagnostics;
using Microsoft.VisualStudio.Text;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    internal sealed class ReplSpan {
        private readonly object _span; // ITrackingSpan or string
        public readonly ReplSpanKind Kind;

        public ReplSpan(ITrackingSpan span, ReplSpanKind kind) {
            Debug.Assert(!kind.IsPrompt());
            _span = span;
            Kind = kind;
        }

        public ReplSpan(string litaral, ReplSpanKind kind) {
            _span = litaral;
            Kind = kind;
        }

        public object Span {
            get { return _span; }
        }

        public string Prompt {
            get { return (string)_span; }
        }

        public ITrackingSpan TrackingSpan {
            get { return (ITrackingSpan)_span; }
        }

        public int Length {
            get {
                return _span is string ? Prompt.Length : TrackingSpan.GetSpan(TrackingSpan.TextBuffer.CurrentSnapshot).Length;
            }
        }

        public override string ToString() {
            return String.Format("{0}: {1}", Kind, _span);
        }
    }

    internal enum ReplSpanKind {
        None,
        /// <summary>
        /// The span represents output from the program (standard output)
        /// </summary>
        Output,
        /// <summary>
        /// The span represents a prompt for input of code.
        /// </summary>
        Prompt,
        /// <summary>
        /// The span represents a 2ndary prompt for more code.
        /// </summary>
        SecondaryPrompt,
        /// <summary>
        /// The span represents code inputted after a prompt or secondary prompt.
        /// </summary>
        Language,
        /// <summary>
        /// The span represents the prompt for input for standard input (non code input)
        /// </summary>
        StandardInputPrompt,
        /// <summary>
        /// The span represents the input for a standard input (non code input)
        /// </summary>
        StandardInput,
    }

    internal static class ReplSpanKindExtensions {
        internal static bool IsPrompt(this ReplSpanKind kind) {
            switch (kind) {
                case ReplSpanKind.Prompt:
                case ReplSpanKind.SecondaryPrompt:
                case ReplSpanKind.StandardInputPrompt:
                    return true;
                default:
                    return false;
            }
        }
    }
}

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

using Microsoft.VisualStudio.Debugger.Symbols;

namespace Microsoft.NodejsTools.TypeScriptSourceMapReader {
    /// <summary>
    /// The start line/column ranges for a contiguous span of text.
    /// </summary>
    internal class SourceMapSpan {
        /// <summary>
        /// 1-based integer for the starting source line.
        /// </summary>
        internal readonly int StartLine;

        /// <summary>
        /// 1-based integer for the starting source column. If column information is missing
        /// (ex: language service doesn't support it), this value should be set to 0.
        /// </summary>
        internal readonly int StartColumn;

        /// <summary>
        /// Construct new SourceMapSpan
        /// </summary>
        /// <param name="startLine">Start line of the span</param>
        /// <param name="startColumn">Start column of the span</param>
        internal SourceMapSpan(int startLine, int startColumn) {
            this.StartLine = startLine;
            this.StartColumn = startColumn;
        }

        /// <summary>
        /// Determines if this span represents start of the span asked by textSpan
        /// </summary>
        /// <param name="textSpan">Text span asked</param>
        /// <returns></returns>
        internal bool IsStartSpanOfTextSpan(DkmTextSpan textSpan) {
            if (this.StartLine >= textSpan.StartLine &&
                  this.StartColumn >= textSpan.StartColumn) {
                // Value is after than start span. 
                if (this.StartLine < textSpan.EndLine) {
                    return true;
                } else if (this.StartLine == textSpan.EndLine) {
                    return this.StartColumn <= textSpan.EndColumn;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if this span represents end of the span asked by textSpan, 
        /// Note: this function can only be called if start of span is found and we are looking into mappings after that
        /// </summary>
        /// <param name="textSpan">Text span asked</param>
        /// <returns></returns>
        internal bool IsEndSpanOfTextSpan(DkmTextSpan textSpan) {
            if (this.StartLine > textSpan.EndLine) {
                return true;
            } else if (this.StartLine == textSpan.EndLine &&
                  this.StartColumn >= textSpan.EndColumn) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if this span starts before the asked text span
        /// </summary>
        /// <param name="textSpan">Text span asked</param>
        /// <returns></returns>
        internal bool StartsBeforeTextSpan(DkmTextSpan textSpan) {
            if (this.StartLine < textSpan.StartLine) {
                return true;
            } else if (this.StartLine == textSpan.StartLine &&
                  this.StartColumn <= textSpan.StartColumn) {
                return true;
            }

            return false;
        }
    }
}

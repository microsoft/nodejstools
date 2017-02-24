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
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Generic outlining region
    /// </summary>
    internal class OutlineRegion : TextRange
    {
        private ITextBuffer _textBuffer;
        private const string _outlineDisplayText = "...";

        public OutlineRegion(ITextBuffer textBuffer, ITextRange range)
            : this(textBuffer, range.Start, range.Length)
        {
        }

        public OutlineRegion(ITextBuffer textBuffer, int start, int length)
            : base(start, length)
        {
            _textBuffer = textBuffer;
        }

        public static OutlineRegion FromBounds(ITextBuffer textBuffer, int start, int end)
        {
            return new OutlineRegion(textBuffer, start, end - start);
        }

        /// <summary>
        /// Text to display in a tooltip when region is collapsed
        /// </summary>
        public virtual string HoverText
        {
            get
            {
                if (_textBuffer != null)
                {
                    int hoverTextLength = Math.Min(this.Length, 512);
                    hoverTextLength = Math.Min(hoverTextLength, _textBuffer.CurrentSnapshot.Length - this.Start);

                    var text = _textBuffer.CurrentSnapshot.GetText(this.Start, hoverTextLength);
                    if (hoverTextLength < this.Length)
                        text += "...";

                    return text;
                }

                return String.Empty;
            }
        }

        /// <summary>
        /// Text to display instead of a region when region is collapsed
        /// </summary>
        public virtual string DisplayText
        {
            get
            {
                return _outlineDisplayText;
            }
        }
    }
}

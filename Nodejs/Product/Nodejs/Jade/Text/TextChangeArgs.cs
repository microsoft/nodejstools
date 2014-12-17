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
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Text change event arguments. This class abstracts text change information 
    /// allowing code that handles text changes to use <seealso cref="ITextProvider"/>
    /// rather than Visual Studio ITextBuffer or some other editor specific types.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class TextChangeEventArgs : EventArgs {
        /// <summary>
        /// Start position of the change
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// Length of the fragment that was deleted or replaced.
        /// Zero if operation is 'insert' or 'paste' without selection.
        /// </summary>
        public int OldLength { get; private set; }

        /// <summary>
        /// Length of the new fragment. Zero if operation is 'delete'.
        /// </summary>
        public int NewLength { get; private set; }

        /// <summary>
        /// Snaphot before the change
        /// </summary>
        public ITextProvider OldText { get; private set; }

        /// <summary>
        /// Snapshot after the change
        /// </summary>
        public ITextProvider NewText { get; private set; }

        public TextChangeEventArgs(int start, int oldLength, int newLength)
            : this(start, oldLength, newLength, null, null) {
        }

        [DebuggerStepThrough]
        public TextChangeEventArgs(int start, int oldLength, int newLength, ITextProvider oldText, ITextProvider newText) {
            Start = start;
            OldLength = oldLength;
            NewLength = newLength;
            OldText = oldText;
            NewText = newText;
        }
    }
}

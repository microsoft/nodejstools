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

namespace Microsoft.NodejsTools.Jade {
    class TextUtility {
        /// <summary>
        /// Combines multiple changes into one larger change.
        /// </summary>
        /// <param name="e">Text buffer change event argument</param>
        /// <param name="start">Combined span start</param>
        /// <param name="oldLength">Length of the change in the original buffer state</param>
        /// <param name="newLength">Length of the change in the new buffer state</param>
        internal static void CombineChanges(TextContentChangedEventArgs e, out int start, out int oldLength, out int newLength) {
            start = 0;
            oldLength = 0;
            newLength = 0;

            if (e.Changes.Count > 0) {
                // Combine multiple changes into one larger change. The problem is that
                // multiple changes map to one current snapshot and there are no
                // separate snapshots for each change which causes problems
                // in incremental parse analysis code.

                Debug.Assert(e.Changes[0].OldPosition == e.Changes[0].NewPosition);

                start = e.Changes[0].OldPosition;
                int oldEnd = e.Changes[0].OldEnd;
                int newEnd = e.Changes[0].NewEnd;

                for (int i = 1; i < e.Changes.Count; i++) {
                    start = Math.Min(start, e.Changes[i].OldPosition);
                    oldEnd = Math.Max(oldEnd, e.Changes[i].OldEnd);
                    newEnd = Math.Max(newEnd, e.Changes[i].NewEnd);
                }

                oldLength = oldEnd - start;
                newLength = newEnd - start;
            }
        }
    }
}

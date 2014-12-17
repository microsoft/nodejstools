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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Represents an item that has a range in a text document
    /// </summary>
    interface ITextRange {
        /// <summary>
        /// Range start.
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Range end.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "End")]
        int End { get; }

        /// <summary>
        /// Length of the range.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Tru if position is inside the range</returns>
        bool Contains(int position);

        /// <summary>
        /// Shifts range by a given offset.
        /// </summary>
        void Shift(int offset);
    }

    /// <summary>
    /// Represents an item that has a range in a text document
    /// </summary>
    interface IExpandableTextRange : ITextRange {
        /// <summary>
        /// Changes range boundaries by the given offsets
        /// </summary>
        void Expand(int startOffset, int endOffset);
    }
}

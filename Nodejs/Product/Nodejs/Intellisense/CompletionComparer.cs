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
using System.Collections.Generic;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Compares various types of completions.
    /// </summary>
    internal class CompletionComparer : IEqualityComparer<MemberResult>, IComparer<MemberResult>, IComparer<Completion>, IComparer<string> {
        /// <summary>
        /// A CompletionComparer that sorts names beginning with underscores to
        /// the end of the list.
        /// </summary>
        public static readonly CompletionComparer UnderscoresLast = new CompletionComparer(true);
        /// <summary>
        /// A CompletionComparer that determines whether
        /// <see cref="MemberResult" /> structures are equal.
        /// </summary>
        public static readonly IEqualityComparer<MemberResult> MemberEquality = UnderscoresLast;
        /// <summary>
        /// A CompletionComparer that sorts names beginning with underscores to
        /// the start of the list.
        /// </summary>
        public static readonly CompletionComparer UnderscoresFirst = new CompletionComparer(false);

        bool _sortUnderscoresLast;

        /// <summary>
        /// Compares two strings.
        /// </summary>
        public int Compare(string xName, string yName) {
            if (yName == null) {
                return xName == null ? 0 : -1;
            } else if (xName == null) {
                return yName == null ? 0 : 1;
            }

            if (_sortUnderscoresLast) {
                bool xUnder = xName.StartsWith("__") && xName.EndsWith("__");
                bool yUnder = yName.StartsWith("__") && yName.EndsWith("__");

                if (xUnder != yUnder) {
                    // The one that starts with an underscore comes later
                    return xUnder ? 1 : -1;
                }

                bool xSingleUnder = xName.StartsWith("_");
                bool ySingleUnder = yName.StartsWith("_");
                if (xSingleUnder != ySingleUnder) {
                    // The one that starts with an underscore comes later
                    return xSingleUnder ? 1 : -1;
                }
            }
            return String.Compare(xName, yName, StringComparison.CurrentCultureIgnoreCase);
        }

        private CompletionComparer(bool sortUnderscoresLast) {
            _sortUnderscoresLast = sortUnderscoresLast;
        }

        /// <summary>
        /// Compares two instances of <see cref="Completion"/> using their
        /// displayed text.
        /// </summary>
        public int Compare(Completion x, Completion y) {
            return Compare(x.DisplayText, y.DisplayText);
        }

        /// <summary>
        /// Compares two <see cref="MemberResult"/> structures using their
        /// names.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(MemberResult x, MemberResult y) {
            return Compare(x.Name, y.Name);
        }

        /// <summary>
        /// Compares two <see cref="MemberResult"/> structures for equality.
        /// </summary>
        public bool Equals(MemberResult x, MemberResult y) {
            return x.Name.Equals(y.Name);
        }

        /// <summary>
        /// Gets the hash code for a <see cref="MemberResult"/> structure.
        /// </summary>
        public int GetHashCode(MemberResult obj) {
            return obj.Name.GetHashCode();
        }
    }
}

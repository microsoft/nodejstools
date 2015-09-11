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

namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// Mapping between js offset and ts offset
    /// </summary>
    internal class SourceMapSpanMapping
    {
        /// <summary>
        /// Span in .js file
        /// </summary>
        internal readonly SourceMapSpan jsEntry;

        /// <summary>
        /// Span in .ts file
        /// </summary>
        internal readonly SourceMapSpan tsEntry;

        /// <summary>
        /// Name of this span
        /// </summary>
        internal readonly int nameIndex;

        /// <summary>
        /// Construct new SouceMapSpanMapping
        /// </summary>
        /// <param name="jsEntry">Span in the .js file</param>
        /// <param name="tsEntry">Span in the .ts file</param>
        /// <param name="nameIndex">Index of name for this span</param>
        internal SourceMapSpanMapping(SourceMapSpan jsEntry, SourceMapSpan tsEntry, int nameIndex)
        {
            this.jsEntry = jsEntry;
            this.tsEntry = tsEntry;
            this.nameIndex = nameIndex;
        }
    }
}

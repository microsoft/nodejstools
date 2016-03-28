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
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.NodejsTools.TypeScriptSourceMapReader {
    /// <summary>
    /// Entry corresponding to the js file's decoded and cached mapping
    /// </summary>
    public class DecodedSourceMap {
        /// <summary>
        /// List of all the decoded map entries
        /// </summary>
        private List<SourceMapSpanMapping> spanMappings = new List<SourceMapSpanMapping>();

        /// <summary>
        /// List of sources information
        /// </summary>
        public List<SourceMapSourceInfo> tsSourceInfos = new List<SourceMapSourceInfo>();

        /// <summary>
        /// Names
        /// </summary>
        private readonly string[] names;

        /// <summary>
        /// jsFileUrl corresponding to this sourcemap
        /// </summary>
        public readonly string jsFileUrl;

        /// <summary>
        /// Invalid text span
        /// </summary>
        public static DkmTextSpan InvalidTextSpan = new DkmTextSpan(-1, -1, -1, -1);

        /// <summary>
        /// Construct new decoded sourcemap
        /// </summary>
        /// <param name="jsFileUrl">Url of the js file this sourceMapEntry represents</param>
        /// <param name="names">Names for this map</param>
        internal DecodedSourceMap(string jsFileUrl, string[] names) {
            this.names = names;
            this.jsFileUrl = jsFileUrl;
        }

        #region InternalApisToAddSourceMapMappingsAndSourceInformation
        /// <summary>
        /// Adds the source map spanning for the jsEntry -> tsEntry mapping and corresponding name index
        /// </summary>
        /// <param name="jsEntry"></param>
        /// <param name="tsEntry"></param>
        /// <param name="nameIndex"></param>
        internal void AddSourceMapSpanMapping(SourceMapSpan jsEntry, SourceMapSpan tsEntry, int nameIndex) {
            this.spanMappings.Add(new SourceMapSpanMapping(jsEntry, tsEntry, nameIndex));
        }

        /// <summary>
        /// Adds the source information to the decoded source map
        /// </summary>
        /// <param name="tsUrl">url of the ts file</param>
        /// <param name="tsFilePath">tsFile path if the url corresponds to local file</param>
        internal void AddSourceMapSourceInfo(string tsUrl, string tsFilePath) {
            this.tsSourceInfos.Add(new SourceMapSourceInfo(this.spanMappings.Count, tsUrl, tsFilePath));
        }
        #endregion

        #region MapJsPositionToTsPosition
        /// <summary>
        /// Map the .js source span this source map represents into the .ts source Span
        /// </summary>
        /// <param name="jsTextSpan">Js text span in the js Url contents this source map represents</param>
        /// <param name="tsSourceInfo">returns the source info correspoding to the ts file the span maps to</param>
        /// <param name="name">name corresponding to this mapping</param>
        /// <returns>returns the span in the ts File if mapping is present, DkmSpan corresponding to -1 line, column info</returns>
        public DkmTextSpan MapJsSourcePosition(DkmTextSpan jsTextSpan, out SourceMapSourceInfo tsSourceInfo, out string name) {
            tsSourceInfo = null;
            name = null;

            // Find the span that matches this range
            int startIndex = this.spanMappings.FindIndex(spanMapping => spanMapping.jsEntry.IsStartSpanOfTextSpan(jsTextSpan));

            // Couldnt map, return invalid span
            if (startIndex == -1) {
                return DecodedSourceMap.InvalidTextSpan;
            }

            // Get the name for this mapping
            if (this.spanMappings[startIndex].nameIndex != -1) {
                name = this.names[this.spanMappings[startIndex].nameIndex];
            }

            // Get the end of the entry index starting with the start Index
            int endIndex = this.spanMappings.FindIndex(startIndex, spanMapping => spanMapping.jsEntry.IsEndSpanOfTextSpan(jsTextSpan));

            // If we didnt find end, use the last entry as the end
            endIndex = endIndex > startIndex ? endIndex : this.spanMappings.Count - 1;

            // Find the sourceIndex
            int sourceInfoIndex = this.tsSourceInfos.FindLastIndex(sourceInfo =>
                this.spanMappings[sourceInfo.jsStartSpanIndex].jsEntry.StartsBeforeTextSpan(jsTextSpan));
            Debug.Assert(sourceInfoIndex != -1);

            // File information
            tsSourceInfo = this.tsSourceInfos[sourceInfoIndex];

            // Span information
            var startEntry = this.spanMappings[startIndex].tsEntry;
            var endEntry = this.spanMappings[endIndex].tsEntry;
            return new DkmTextSpan(startEntry.StartLine, endEntry.StartLine, startEntry.StartColumn, endEntry.StartColumn);
        }
        #endregion

        #region MapTsPositionToJsPosition
        /// <summary>
        /// Map .ts file position into .js file position
        /// </summary>
        /// <param name="tsFilePathOrUrl">.ts file's path or Url</param>
        /// <param name="tsTextSpan">Text span in the .ts file</param>
        /// <returns>Source position in .js file</returns>
        public DkmTextSpan MapTsSourcePosition(string tsFilePathOrUrl, DkmTextSpan tsTextSpan) {
            // Find the starting span in jsEntries where this ts File emit begins
            var tsSourceIndexInfo = this.tsSourceInfos.Find(sourceInfo => sourceInfo.IsSameTsFile(tsFilePathOrUrl));
            Debug.Assert(tsSourceIndexInfo != null);

            // Start Index
            var startIndex = this.spanMappings.FindIndex(tsSourceIndexInfo.jsStartSpanIndex, spanMapping => spanMapping.tsEntry.IsStartSpanOfTextSpan(tsTextSpan));

            // Couldnt map, return invalid span
            if (startIndex == -1) {
                return DecodedSourceMap.InvalidTextSpan;
            }

            // Find end index
            int endIndex = this.spanMappings.FindIndex(startIndex, spanMapping => spanMapping.tsEntry.IsEndSpanOfTextSpan(tsTextSpan));

            // If we didnt find end, use the last entry as the end
            endIndex = endIndex > startIndex ? endIndex : this.spanMappings.Count - 1;

            var startEntry = this.spanMappings[startIndex].jsEntry;
            var endEntry = this.spanMappings[endIndex].jsEntry;
            return new DkmTextSpan(startEntry.StartLine, endEntry.StartLine, startEntry.StartColumn, endEntry.StartColumn);
        }
        #endregion

        #region GetTsSourceInfoEnumerator
        /// <summary>
        /// Get the ts source file info enumerator
        /// </summary>
        /// <returns></returns>
        public List<SourceMapSourceInfo>.Enumerator GetTsSourceInfoEnumerator() {
            return this.tsSourceInfos.GetEnumerator();
        }
        #endregion

        #region GetTsSourceInfo
        /// <summary>
        /// Returns the sourceInfo for the ts File
        /// </summary>
        /// <param name="tsUrl">url of the ts file</param>
        /// <returns></returns>
        public SourceMapSourceInfo GetTsSourceInfo(string tsUrl) {
            return this.tsSourceInfos.Find(sourceInfo => sourceInfo.tsUrl == tsUrl);
        }
        #endregion
    }
}

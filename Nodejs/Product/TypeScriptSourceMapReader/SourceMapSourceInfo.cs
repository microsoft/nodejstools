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

namespace Microsoft.NodejsTools.TypeScriptSourceMapReader {
    /// <summary>
    /// Source information - contains start span in .js where the new source file mapping starts
    /// </summary>
    public class SourceMapSourceInfo {
        /// <summary>
        /// Index in the mappings where the JS span corresponding to this source starts
        /// </summary>
        internal readonly int jsStartSpanIndex;

        /// <summary>
        /// Typescript file url
        /// </summary>
        public readonly string tsUrl;

        /// <summary>
        /// Document path on disk for the tsFile.
        /// </summary>
        public readonly string tsFilePath;

        /// <summary>
        /// Constructs new source index info
        /// </summary>
        /// <param name="jsStartSpanIndex">Index in the mappings where the JS span corresponding to this source starts</param>
        /// <param name="tsUrl">Typescript file url</param>
        /// <param name="tsFilePath">File path of the ts file</param>
        internal SourceMapSourceInfo(int jsStartSpanIndex, string tsUrl, string tsFilePath) {
            this.tsUrl = tsUrl;
            this.jsStartSpanIndex = jsStartSpanIndex;
            this.tsFilePath = tsFilePath;
        }

        /// <summary>
        /// Returns if the source info represents the document with filePath or Url of ts document passed in
        /// </summary>
        /// <param name="tsFilePathOrUrl">ts documents path or file name</param>
        /// <returns></returns>
        public bool IsSameTsFile(string tsFilePathOrUrl) {
            var lowerTsFilePathOrUrl = tsFilePathOrUrl.ToLowerInvariant();
            if (!string.IsNullOrEmpty(this.tsFilePath) && this.tsFilePath.ToLowerInvariant() == lowerTsFilePathOrUrl) {
                return true;
            }
            return this.tsUrl.ToLowerInvariant() == lowerTsFilePathOrUrl;
        }
    }
}

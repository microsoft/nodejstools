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
    /// SourceMap as we read from the mapping file
    /// </summary>
    public class EncodedSourceMap
    {
        /// <summary>
        /// Version string in the map
        /// </summary>
        public int version;

        /// <summary>
        /// File this map is for
        /// </summary>
        public string file;

        /// <summary>
        /// Souces from which the file was generated
        /// </summary>
        public string[] sources;

        /// <summary>
        /// Names
        /// </summary>
        public string[] names;

        /// <summary>
        /// Mapped Base64VLQ encoded string
        /// </summary>
        public string mappings;

        /// <summary>
        /// SourceRoot to be prepended to each sources entry
        /// </summary>
        public string sourceRoot;
    }
}

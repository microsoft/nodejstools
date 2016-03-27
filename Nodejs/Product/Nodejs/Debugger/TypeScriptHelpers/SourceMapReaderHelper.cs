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
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Runtime Information
    /// </summary>
    internal class SourceMapReaderHelper {
        internal static string FindSourceMapFile(string jsFileName) {
            string sourceMapFilename = null;
            int markerStart;
            if (File.Exists(jsFileName)) {
                string[] contents = File.ReadAllLines(jsFileName);
                const string marker = "# sourceMappingURL=";                
                string markerLine = contents.Reverse().FirstOrDefault(x => x.IndexOf(marker, StringComparison.Ordinal) != -1);
                if (markerLine != null && (markerStart = markerLine.IndexOf(marker, StringComparison.Ordinal)) != -1) {
                    sourceMapFilename = markerLine.Substring(markerStart + marker.Length).Trim();

                    try {
                        if (!File.Exists(sourceMapFilename)) {
                            sourceMapFilename = Path.Combine(Path.GetDirectoryName(jsFileName) ?? string.Empty, Path.GetFileName(sourceMapFilename));
                        }
                    } catch (ArgumentException) {
                    } catch (PathTooLongException) {
                    }
                }
            }
            return sourceMapFilename;
        }
    }
}

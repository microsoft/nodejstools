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
using System.Security;

namespace Microsoft.NodejsTools.TypeScriptSourceMapReader {
    /// <summary>
    /// Runtime Information
    /// </summary>
    internal class SourceMapFileHelper {
        internal static string FindSourceMapFile(string jsFileName) {
            int markerStart;
            try {
                if (File.Exists(jsFileName)) {
                    string[] contents = File.ReadAllLines(jsFileName);
                    const string marker = "# sourceMappingURL=";
                    string markerLine = contents.Reverse().FirstOrDefault(x => x.IndexOf(marker, StringComparison.Ordinal) != -1);
                    if (markerLine != null && (markerStart = markerLine.IndexOf(marker, StringComparison.Ordinal)) != -1) {
                        string sourceMapFileName = markerLine.Substring(markerStart + marker.Length).Trim();

                        if (File.Exists(sourceMapFileName)) {
                            return sourceMapFileName;
                        }

                        sourceMapFileName = Path.Combine(Path.GetDirectoryName(jsFileName) ?? string.Empty, Path.GetFileName(sourceMapFileName));
                        return File.Exists(sourceMapFileName) ? sourceMapFileName : null;
                    }
                }
            } catch (ArgumentException) {
            } catch (IOException) {
            } catch (UnauthorizedAccessException) {
            } catch (SecurityException) {
            } catch (NotSupportedException) {
            }

            return null;
        }
    }
}

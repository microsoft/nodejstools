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

namespace Microsoft.NodejsTools.Analysis {
    [Serializable]
    internal class FileCookie : IAnalysisCookie {
        private readonly string _path;
        private string[] _allLines;

        public FileCookie(string path) {
            _path = path;
        }

        public string Path {
            get {
                return _path;
            }
        }

        #region IFileCookie Members

        public string GetLine(int lineNo) {
            if (_allLines == null) {
                try {
                    _allLines = File.ReadAllLines(Path);
                } catch (IOException) {
                    _allLines = new string[0];
                }
            }

            if (lineNo - 1 < _allLines.Length) {
                return _allLines[lineNo - 1];
            }

            return String.Empty;
        }

        #endregion
    }
}

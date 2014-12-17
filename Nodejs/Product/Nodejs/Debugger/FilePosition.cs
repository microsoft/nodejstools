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

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Stores line and column position in the file.
    /// </summary>
    sealed class FilePosition {
        private readonly int _column;
        private readonly string _fileName;
        private readonly int _line;

        public FilePosition(string fileName, int line, int column) {
            _fileName = fileName;
            _line = line;
            _column = column;
        }

        /// <summary>
        /// Gets a file name.
        /// </summary>
        public string FileName {
            get { return _fileName; }
        }

        /// <summary>
        /// Gets a line number.
        /// </summary>
        public int Line {
            get { return _line; }
        }

        /// <summary>
        /// Gets a column number.
        /// </summary>
        public int Column {
            get { return _column; }
        }
    }
}
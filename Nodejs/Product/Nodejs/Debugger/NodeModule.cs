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

using System.Diagnostics;
using System.IO;

namespace Microsoft.NodejsTools.Debugger {
    class NodeModule {
        private readonly string _fileName;
        private readonly int _id;
        private readonly string _javaScriptFileName;

        public NodeModule(int id, string fileName) : this(id, fileName, fileName) {
        }

        public NodeModule(int id, string fileName, string javaScriptFileName) {
            Debug.Assert(fileName != null);

            _id = id;
            _fileName = fileName;
            _javaScriptFileName = javaScriptFileName;
        }

        public int Id {
            get { return _id; }
        }

        public string Name {
            get {
                if (_fileName.IndexOfAny(Path.GetInvalidPathChars()) == -1) {
                    return Path.GetFileName(_fileName);
                }
                return _fileName;
            }
        }

        public string JavaScriptFileName {
            get { return _javaScriptFileName; }
        }

        public string FileName {
            get { return _fileName; }
        }

        public string Source { get; set; }

        public bool BuiltIn {
            get {
                // No directory separator characters implies builtin
                return (_fileName.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) == -1);
            }
        }

        public object Document { get; set; }
    }
}
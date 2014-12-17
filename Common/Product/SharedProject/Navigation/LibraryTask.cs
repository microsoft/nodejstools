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

using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudioTools.Navigation {
    /// <summary>
    /// Class storing the data about a parsing task on a language module.
    /// A module in dynamic languages is a source file, so here we use the file name to
    /// identify it.
    /// </summary>
    public class LibraryTask {
        private string _fileName;
        private ITextBuffer _textBuffer;
        private ModuleId _moduleId;

        public LibraryTask(string fileName, ITextBuffer textBuffer, ModuleId moduleId) {
            _fileName = fileName;
            _textBuffer = textBuffer;
            _moduleId = moduleId;
        }

        public string FileName {
            get { return _fileName; }
        }

        public ModuleId ModuleID {
            get { return _moduleId; }
            set { _moduleId = value; }
        }

        public ITextBuffer TextBuffer {
            get { return _textBuffer; }
        }
    }

}

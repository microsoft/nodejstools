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
using System.Text;
using Microsoft.VisualStudio.Text;

namespace TestUtilities.Mocks {
    public class MockTextDocument : ITextDocument {
        private readonly string _filePath;
        public MockTextDocument(string filePath) {
            _filePath = filePath;
        }


        #region ITextDocument Members

        public event EventHandler DirtyStateChanged {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public Encoding Encoding {
            get {
                return Encoding.UTF8;
            }
            set {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<EncodingChangedEventArgs> EncodingChanged {
            add {  }
            remove {  }
        }

        public event EventHandler<TextDocumentFileActionEventArgs> FileActionOccurred {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public string FilePath {
            get { return _filePath; }
        }

        public bool IsDirty {
            get { throw new NotImplementedException(); }
        }

        public bool IsReloading {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastContentModifiedTime {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastSavedTime {
            get { throw new NotImplementedException(); }
        }

        public ReloadResult Reload(EditOptions options) {
            throw new NotImplementedException();
        }

        public ReloadResult Reload() {
            throw new NotImplementedException();
        }

        public void Rename(string newFilePath) {
            throw new NotImplementedException();
        }

        public void Save() {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite, bool createFolder, Microsoft.VisualStudio.Utilities.IContentType newContentType) {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite, Microsoft.VisualStudio.Utilities.IContentType newContentType) {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite, bool createFolder) {
            throw new NotImplementedException();
        }

        public void SaveAs(string filePath, bool overwrite) {
            throw new NotImplementedException();
        }

        public void SaveCopy(string filePath, bool overwrite, bool createFolder) {
            throw new NotImplementedException();
        }

        public void SaveCopy(string filePath, bool overwrite) {
            throw new NotImplementedException();
        }

        public void SetEncoderFallback(EncoderFallback fallback) {
            throw new NotImplementedException();
        }

        public ITextBuffer TextBuffer {
            get { throw new NotImplementedException(); }
        }

        public void UpdateDirtyState(bool isDirty, DateTime lastContentModifiedTime) {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            throw new NotImplementedException();
        }

        #endregion
    }
}

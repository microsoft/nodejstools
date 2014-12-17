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


namespace Microsoft.VisualStudioTools.Project {
    class PublishFile : IPublishFile {
        private readonly string _filename, _destFile;

        public PublishFile(string filename, string destFile) {
            _filename = filename;
            _destFile = destFile;
        }

        #region IPublishFile Members

        public string SourceFile {
            get { return _filename; }
        }

        public string DestinationFile {
            get { return _destFile; }
        }

        #endregion
    }
}

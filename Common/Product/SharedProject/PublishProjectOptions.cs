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

using System.Collections.Generic;

namespace Microsoft.VisualStudioTools.Project {
    public sealed class PublishProjectOptions {
        private readonly IPublishFile[] _additionalFiles;
        private readonly string _destination;
        public static readonly PublishProjectOptions Default = new PublishProjectOptions(new IPublishFile[0]);

        public PublishProjectOptions(IPublishFile[] additionalFiles = null, string destinationUrl = null) {
            _additionalFiles = additionalFiles ?? Default._additionalFiles;
            _destination = destinationUrl;
        }

        public IList<IPublishFile> AdditionalFiles {
            get {
                return _additionalFiles;
            }
        }

        /// <summary>
        /// Gets an URL which overrides the project publish settings or returns null if no override is specified.
        /// </summary>
        public string DestinationUrl {
            get {
                return _destination;
            }
        }
    }
}

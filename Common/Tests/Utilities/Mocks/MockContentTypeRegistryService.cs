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
using System.Collections.Generic;
using Microsoft.VisualStudio.Utilities;

namespace TestUtilities.Mocks {
    public class MockContentTypeRegistryService : IContentTypeRegistryService {
        #region IContentTypeRegistryService Members

        public IContentType AddContentType(string typeName, IEnumerable<string> baseTypeNames) {
            throw new NotImplementedException();
        }

        public IEnumerable<IContentType> ContentTypes {
            get { throw new NotImplementedException(); }
        }

        public IContentType GetContentType(string typeName) {
            if (typeName == "Python" || typeName == "Node.js") {
                return new MockContentType(typeName, new IContentType[0]);
            }
            throw new NotImplementedException();
        }

        public void RemoveContentType(string typeName) {
            throw new NotImplementedException();
        }

        public IContentType UnknownContentType {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}

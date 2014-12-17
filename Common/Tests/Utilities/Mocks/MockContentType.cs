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
using Microsoft.VisualStudio.Utilities;

namespace TestUtilities.Mocks {
    public class MockContentType : IContentType {
        private readonly string _name;
        private readonly IContentType[] _bases;

        public MockContentType(string name, IContentType[] bases) {
            _name = name;
            _bases = bases;
        }

        public IEnumerable<IContentType> BaseTypes {
            get { return _bases; }
        }

        public bool IsOfType(string type) {
            if (type == _name) {
                return true;
            }

            foreach (var baseType in BaseTypes) {
                if (baseType.IsOfType(type)) {
                    return true;
                }
            }
            return false;
        }

        public string DisplayName {
            get { return _name; }
        }

        public string TypeName {
            get { return _name; }
        }
    }
}

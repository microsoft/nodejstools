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
using Microsoft.VisualStudio.Text.Classification;

namespace TestUtilities.Mocks {
    public class MockClassificationType : IClassificationType {
        private readonly string _name;
        private readonly IClassificationType[] _bases;

        public MockClassificationType(string name, IClassificationType[] bases) {
            _name = name;
            _bases = bases;
        }

        public IEnumerable<IClassificationType> BaseTypes {
            get { return _bases; }
        }

        public string Classification {
            get { return _name; }
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
    }
}

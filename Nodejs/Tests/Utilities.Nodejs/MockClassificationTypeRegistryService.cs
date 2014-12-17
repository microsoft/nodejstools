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
using System.Linq;
using System.Reflection;
using Microsoft.NodejsTools.Classifier;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace TestUtilities.Mocks {
    public class MockClassificationTypeRegistryService : IClassificationTypeRegistryService {
        private static Dictionary<string, MockClassificationType> _types = new Dictionary<string,MockClassificationType>();

        public MockClassificationTypeRegistryService() {
            foreach (FieldInfo fi in typeof(PredefinedClassificationTypeNames).GetFields()) {
                string name = (string)fi.GetValue(null);
                _types[name] = new MockClassificationType(name, new IClassificationType[0]);
            }

            foreach (FieldInfo fi in typeof(NodejsPredefinedClassificationTypeNames).GetFields()) {
                string name = (string)fi.GetValue(null);
                _types[name] = new MockClassificationType(name, new IClassificationType[0]);
            }
        }

        public IClassificationType CreateClassificationType(string type, IEnumerable<IClassificationType> baseTypes) {
            return _types[type] = new MockClassificationType(type, baseTypes.ToArray());
        }

        public IClassificationType CreateTransientClassificationType(params IClassificationType[] baseTypes) {
            return new MockClassificationType(String.Empty, baseTypes);
        }

        public IClassificationType CreateTransientClassificationType(IEnumerable<IClassificationType> baseTypes) {
            return new MockClassificationType(String.Empty, baseTypes.ToArray());
        }

        public IClassificationType GetClassificationType(string type) {
            MockClassificationType result;
            return _types.TryGetValue(type, out result) ? result : null;
        }
    }
}

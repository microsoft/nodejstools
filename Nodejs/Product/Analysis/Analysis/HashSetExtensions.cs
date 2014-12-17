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

namespace Microsoft.NodejsTools.Analysis {
    internal static class HashSetExtensions {
        internal static bool AddValue<T>(ref ISet<T> references, T value) {
            if (references == null) {
                references = new SetOfOne<T>(value);
                return true;
            } else if (references is SetOfOne<T>) {
                if (!references.Contains(value)) {
                    references = new SetOfTwo<T>(((SetOfOne<T>)references).Value, value);
                    return true;
                }
            } else if (references is SetOfTwo<T>) {
                if (!references.Contains(value)) {
                    references = new HashSet<T>(references);
                    return references.Add(value);
                }
            } else {
                return references.Add(value);
            }
            return false;
        }

    }
}

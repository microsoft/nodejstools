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
using System.Linq;

namespace Microsoft.NodejsTools.Analysis {
    static class IEnumerableExtensions {
        private static T Identity<T>(T source) {
            return source;
        }

        public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source) {
            return source.SelectMany(Identity<IEnumerable<T>>);
        }

        public static bool AnyContains<T>(this IEnumerable<IEnumerable<T>> source, T value) {
            foreach (var set in source) {
                if (set.Contains(value)) {
                    return true;
                }
            }
            return false;
        }

        public static bool AnyContains(this IEnumerable<IAnalysisSet> source, AnalysisValue value) {
            foreach (var set in source) {
                if (set.Contains(value.Proxy)) {
                    return true;
                }
            }
            return false;
        }
    }
}

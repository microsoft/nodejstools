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

namespace Microsoft.NodejsTools.Jade {
    internal static class JadeTagKeywords {
        public static bool IsKeyword(string candidate) {
            string lower = candidate.ToLowerInvariant();
            return Array.BinarySearch<string>(_keywords, lower) >= 0;
        }

        // must be sorted
        private static string[] _keywords = new string[] { 
            "block",
            "extends",
            "include",
            "javascripts",
            "mixin",
            "model",
            "scripts",
            "stylesheets",
        };
    }
}


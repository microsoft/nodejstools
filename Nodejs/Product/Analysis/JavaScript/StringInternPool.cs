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
using System.Diagnostics;

namespace Microsoft.NodejsTools.Parsing {
    /// <summary>
    /// Pool that caches strings for memory reuse.
    /// 
    /// Used over `String.Intern` since that cannot release held strings.
    /// 
    /// This class is not thread safe.
    /// </summary>
    internal class StringInternPool {
        private readonly Dictionary<string, LinkedListNode<string>> _dict = new Dictionary<string, LinkedListNode<string>>();
        private readonly LinkedList<string> _list = new LinkedList<string>();
        private readonly int _maxSize;

        /// <summary>
        /// Creates a new string pool.
        /// </summary>
        /// <param name="maxSize">The maximum number of elements to store.</param>
        internal StringInternPool(int maxSize) {
            _maxSize = maxSize;
        }

        /// <summary>
        /// Interns a string.
        /// Returns the reused string if the string is already in the cache, otherwise adds the string to the cache.
        /// </summary>
        internal  string Intern(string str) {
            if (string.IsNullOrEmpty(str)) {
                return string.Empty;
            }

            LinkedListNode<string> existingNode;
            if (_dict.TryGetValue(str, out existingNode) && existingNode != null) {
                // Move node to head of list
                _list.Remove(existingNode);
                _list.AddFirst(existingNode);
                return existingNode.Value;
            }

            // Trim the size of the cache.
            while (_list.Count >= _maxSize) {
                var last = _list.Last;
                _list.RemoveLast();
                bool res = _dict.Remove(last.Value);
                Debug.Assert(res);
            }
            // Add new value to head of list
            var newNode = new LinkedListNode<string>(str);
            _list.AddFirst(newNode);
            _dict[newNode.Value] = newNode;
            return newNode.Value;
        }
    }
}

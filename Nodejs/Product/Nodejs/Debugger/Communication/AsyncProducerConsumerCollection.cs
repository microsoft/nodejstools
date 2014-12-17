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
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger.Communication {
    public class AsyncProducerConsumerCollection<T> {
        private readonly Queue<T> _collection = new Queue<T>();
        private readonly Queue<TaskCompletionSource<T>> _waiting = new Queue<TaskCompletionSource<T>>();

        public void Add(T item) {
            TaskCompletionSource<T> tcs = null;
            lock (_collection) {
                if (_waiting.Count > 0) {
                    tcs = _waiting.Dequeue();
                } else {
                    _collection.Enqueue(item);
                }
            }
            if (tcs != null) {
                tcs.TrySetResult(item);
            }
        }

        public Task<T> TakeAsync() {
            lock (_collection) {
                if (_collection.Count > 0) {
                    return Task.FromResult(_collection.Dequeue());
                }
                var tcs = new TaskCompletionSource<T>();
                _waiting.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }
}
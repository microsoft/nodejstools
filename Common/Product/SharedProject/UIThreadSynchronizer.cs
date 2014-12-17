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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudioTools.Project {

    /// <summary>
    /// Implements ISynchronizeInvoke in terms of System.Threading.Tasks.Task.
    /// 
    /// This class will handle marshalling calls back onto the UI thread for us.
    /// </summary>
    class UIThreadSynchronizer : ISynchronizeInvoke {
        private readonly TaskScheduler _scheduler;
        private readonly Thread _thread;

        /// <summary>
        /// Creates a new UIThreadSynchronizer which will invoke callbacks within the 
        /// current synchronization context.
        /// </summary>
        public UIThreadSynchronizer()
            : this(TaskScheduler.FromCurrentSynchronizationContext(), Thread.CurrentThread) {
        }

        public UIThreadSynchronizer(TaskScheduler scheduler, Thread target) {
            _scheduler = scheduler;
            _thread = target;
        }

        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, params object[] args) {
            return Task.Factory.StartNew(() => method.DynamicInvoke(args), default(System.Threading.CancellationToken), TaskCreationOptions.None, _scheduler);
        }

        public object EndInvoke(IAsyncResult result) {
            return ((Task<object>)result).Result;
        }

        public object Invoke(Delegate method, params object[] args) {
            var task = Task.Factory.StartNew(() => method.DynamicInvoke(args), default(System.Threading.CancellationToken), TaskCreationOptions.None, _scheduler);
            task.Wait();
            return task.Result;
        }

        public bool InvokeRequired {
            get { return Thread.CurrentThread != _thread; }
        }

        #endregion
    }
}

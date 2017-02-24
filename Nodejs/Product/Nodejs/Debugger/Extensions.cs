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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger
{
    internal static class Extensions
    {
        internal static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            linkedTokenSource.CancelAfter(timeout);
            // If the token will time out, this await will throw TaskCanceledException, which is automatically propagated to our caller.
            await task.ContinueWith(t => { }, linkedTokenSource.Token).ConfigureAwait(false);
            // If we're still here, the token didn't time out, so the original task has completed execution (by succeeding or failing);
            // return its result or propagate the exception.
            return await task.ConfigureAwait(false);
        }

        internal static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            // Same logic as above but for a regular Task without a return value.
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            linkedTokenSource.CancelAfter(timeout);
            await task.ContinueWith(t => { }, linkedTokenSource.Token).ConfigureAwait(false);
            await task.ConfigureAwait(false);
        }
    }
}

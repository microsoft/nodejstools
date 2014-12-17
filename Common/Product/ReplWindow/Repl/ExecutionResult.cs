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


using System.Threading.Tasks;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    /// <summary>
    /// The result of command execution.  
    /// </summary>
    public struct ExecutionResult {
        public static readonly ExecutionResult Success = new ExecutionResult(true);
        public static readonly ExecutionResult Failure = new ExecutionResult(false);
        public static readonly Task<ExecutionResult> Succeeded = MakeSucceeded();
        public static readonly Task<ExecutionResult> Failed = MakeFailed();
 
        private readonly bool _successful;

        public ExecutionResult(bool isSuccessful) {
            _successful = isSuccessful;
        }

        public bool IsSuccessful {
            get {
                return _successful;
            }
        }

        private static Task<ExecutionResult> MakeSucceeded() {
            var taskSource = new TaskCompletionSource<ExecutionResult>();
            taskSource.SetResult(Success);
            return taskSource.Task;
        }

        private static Task<ExecutionResult> MakeFailed() {
            var taskSource = new TaskCompletionSource<ExecutionResult>();
            taskSource.SetResult(Failure);
            return taskSource.Task;
        }
    }
}

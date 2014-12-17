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
using System.Diagnostics;

namespace Microsoft.VisualStudioTools {
    struct DebugTimer : IDisposable {
#if DEBUG
        internal static Stopwatch _timer = MakeStopwatch();
        private readonly long _start, _minReportTime;
        private readonly string _description;

        private static Stopwatch MakeStopwatch() {
            var res = new Stopwatch();
            res.Start();
            return res;
        }
#endif

        /// <summary>
        /// Creates a new DebugTimer which logs timing information from when it's created
        /// to when it's disposed.
        /// </summary>
        /// <param name="description">The message which is logged in addition to the timing information</param>
        /// <param name="minReportTime">The minimum amount of time (in milliseconds) which needs to elapse for a message to be logged</param>
        public DebugTimer(string description, long minReportTime = 0) {
#if DEBUG
            _start = _timer.ElapsedMilliseconds;
            _description = description;
            _minReportTime = minReportTime;
#endif
        }


        #region IDisposable Members

        public void Dispose() {
#if DEBUG
            var elapsed = _timer.ElapsedMilliseconds - _start;
            if (elapsed >= _minReportTime) {
                Debug.WriteLine(String.Format("{0}: {1}ms elapsed", _description, elapsed));
                Console.WriteLine(String.Format("{0}: {1}ms elapsed", _description, elapsed));
            }
#endif
        }

        #endregion
    }
}

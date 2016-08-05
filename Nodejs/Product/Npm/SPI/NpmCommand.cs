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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal abstract class NpmCommand : AbstractNpmLogSource {
        private readonly string _fullPathToRootPackageDirectory;
        private string _pathToNpm;
        private readonly ManualResetEvent _cancellation;
        private readonly StringBuilder _output = new StringBuilder();
        private StringBuilder _error = new StringBuilder();
        private readonly object _bufferLock = new object();

        protected NpmCommand(
            string fullPathToRootPackageDirectory,
            string pathToNpm = null) {
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _pathToNpm = pathToNpm;
            _cancellation = new ManualResetEvent(false);
        }

        protected string Arguments { get; set; }

        internal string FullPathToRootPackageDirectory {
            get { return _fullPathToRootPackageDirectory; }
        }

        protected string GetPathToNpm() {
            if (null == _pathToNpm || !File.Exists(_pathToNpm)) {
                _pathToNpm = NpmHelpers.GetPathToNpm();
            }
            return _pathToNpm;
        }

        public string StandardOutput {
            get {
                lock (_bufferLock) {
                    return _output.ToString();
                }
            }
        }

        public string StandardError {
            get {
                lock (_bufferLock) {
                    return _error.ToString();
                }
            }
        }

        public void CancelCurrentTask() {
            _cancellation.Set();
        }

        public virtual async Task<bool> ExecuteAsync() {
            OnCommandStarted();
            var redirector = new NpmCommandRedirector(this);

            try {
                GetPathToNpm();
            } catch (NpmNotFoundException) {
                redirector.WriteErrorLine(Resources.CouldNotFindNpm);
                return false;
            }
            redirector.WriteLine(
                string.Format("===={0}====\r\n\r\n",
                string.Format(Resources.ExecutingCommand, Arguments)));

            var cancelled = false;
            try {
                await NpmHelpers.ExecuteNpmCommandAsync(
                    redirector,
                    GetPathToNpm(),
                    _fullPathToRootPackageDirectory,
                    new[] { Arguments },
                    _cancellation);
            } catch (OperationCanceledException) {
                cancelled = true;
            }
            OnCommandCompleted(Arguments, redirector.HasErrors, cancelled);
            return !redirector.HasErrors;
        }

        internal class NpmCommandRedirector : Redirector {
            NpmCommand _owner;
            
            public NpmCommandRedirector(NpmCommand owner) {
                _owner = owner;
            }

            public bool HasErrors { get; private set; }

            private string AppendToBuffer(StringBuilder buffer, string data) {
                if (data != null) {
                    lock (_owner._bufferLock) {
                        buffer.Append(data + Environment.NewLine);
                    }
                }
                return data;
            }

            public override void WriteLine(string line) {
                _owner.OnOutputLogged(AppendToBuffer(_owner._output, line));
            }

            public override void WriteErrorLine(string line) {
                HasErrors = true;
                _owner.OnErrorLogged(AppendToBuffer(_owner._error, line));
            }
        }
    }
}
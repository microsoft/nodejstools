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
using Microsoft.PythonTools.Interpreter;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI {
    public class DefaultInterpreterSetter : IDisposable {
        public readonly object OriginalInterpreter, OriginalVersion;
        private bool _isDisposed;

        public DefaultInterpreterSetter(IPythonInterpreterFactory factory) {
            var props = VsIdeTestHostContext.Dte.get_Properties("Python Tools", "Interpreters");
            Assert.IsNotNull(props);

            OriginalInterpreter = props.Item("DefaultInterpreter").Value;
            OriginalVersion = props.Item("DefaultInterpreterVersion").Value;

            props.Item("DefaultInterpreter").Value = factory.Id;
            props.Item("DefaultInterpreterVersion").Value = string.Format("{0}.{1}", factory.Configuration.Version.Major, factory.Configuration.Version.Minor);
        }

        public void Dispose() {
            if (!_isDisposed) {
                _isDisposed = true;

                var props = VsIdeTestHostContext.Dte.get_Properties("Python Tools", "Interpreters");
                Assert.IsNotNull(props);

                props.Item("DefaultInterpreter").Value = OriginalInterpreter;
                props.Item("DefaultInterpreterVersion").Value = OriginalVersion;
            }
        }
    }
}

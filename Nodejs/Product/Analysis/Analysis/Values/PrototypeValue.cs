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
using Microsoft.NodejsTools.Analysis.Analyzer;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class PrototypeValue : ObjectValue {
        private readonly FunctionValue _function;

        public PrototypeValue(ProjectEntry projectEntry, FunctionValue function, string description = null)
            : base(projectEntry, description: description) {
            _function = function;
            projectEntry.Analyzer.AnalysisValueCreated(typeof(PrototypeValue));
        }

        public override string ObjectDescription {
            get {
                return "object prototype";
            }
        }

        public override string Name {
            get {
                return _function.Name;
            }
        }

        public override void SetMember(Parsing.Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            foreach (var obj in value) {
                // function Class() {
                //     this.abc = 42;
                // }
                //   
                // Class.prototype.foo = function(fn) {
                //     var x = this.abc;
                // }
                // this now includes us.

                UserFunctionValue userFunc = obj.Value as UserFunctionValue;
                if (userFunc != null) {
                    var env = (FunctionEnvironmentRecord)(userFunc.AnalysisUnit._env);

                    env._this.AddTypes(unit, _function._instance.SelfSet, declaringScope: DeclaringModule);
                }
            }
            base.SetMember(node, unit, name, value);
        }
    }
}

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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class SpecializedUserFunctionValue : UserFunctionValue {
        private readonly CallDelegate _call;
        private readonly bool _callBase;

        public SpecializedUserFunctionValue(CallDelegate call, FunctionObject node, AnalysisUnit declUnit, EnvironmentRecord declScope, bool callBase) :
            base(node, declUnit, declScope) {
            _call = call;
            _callBase = callBase;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            var res = _call(this, node, unit, @this, args);

            if (_callBase) {
                return res.Union(base.Call(node, unit, @this, args));
            }
            return res;
        }
    }
}

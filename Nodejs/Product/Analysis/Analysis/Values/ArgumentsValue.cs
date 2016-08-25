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

using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    class ArgumentsValue : AnalysisValue {
        private readonly UserFunctionValue _function;
        public ArgumentsValue(UserFunctionValue function)
            : base(function.ProjectEntry) {
            _function = function;
        }

        public override IAnalysisSet GetIndex(Parsing.Node node, AnalysisUnit unit, IAnalysisSet index) {
            var res = AnalysisSet.Empty;
            if (_function._curArgs != null) {
                foreach (var value in index) {
                    var numIndex = value.Value.GetNumberValue();

                    if (numIndex != null &&
                        numIndex.Value >= 0 &&
                        (numIndex.Value % 1) == 0 &&    // integer number...
                        ((int)numIndex.Value) < _function._curArgs.Args.Length) {
                        res = res.Union(_function._curArgs.Args[(int)numIndex.Value]);
                    }
                }
            }
            return res;
        }

        internal override IAnalysisSet[] GetIndices(Node node, AnalysisUnit unit) {
            if (_function._curArgs != null) {
                return _function._curArgs.Args;
            }

            var function = _function.FunctionObject;
            if (function.ParameterDeclarations != null) {
                IAnalysisSet[] res = new IAnalysisSet[function.ParameterDeclarations.Length];
                for (int i = 0; i < res.Length; i++) {
                    VariableDef var;
                    if (_function.AnalysisUnit._env.TryGetVariable(function.ParameterDeclarations[i].Name, out var)) {
                        res[i] = var.GetTypes(unit, DeclaringModule);
                    } else {
                        res[i] = AnalysisSet.Empty;
                    }
                }
                return res;
            }

            return ExpressionEvaluator.EmptySets;
        }
    }
}

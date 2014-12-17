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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Analyzer {
    [Serializable]
    sealed class FunctionEnvironmentRecord : DeclarativeEnvironmentRecord {
        private readonly UserFunctionValue _function;
        internal VariableDef _this;
        public readonly FunctionAnalysisUnit AnalysisUnit;
        //public readonly GeneratorInfo Generator;

        public FunctionEnvironmentRecord(
            UserFunctionValue function,
            FunctionAnalysisUnit analysisUnit,
            Node node,
            EnvironmentRecord declScope,
            IJsProjectEntry declModule
        )
            : base(node, declScope) {
            _function = function;
            _this = new VariableDef();
            AnalysisUnit = analysisUnit;
#if FALSE
            if (Function.FunctionObject.IsGenerator) {
                Generator = new GeneratorInfo(function.ProjectState, declModule);
                ReturnValue.AddTypes(function.ProjectEntry, Generator.SelfSet, false);
            }
#endif
        }

        public override IAnalysisSet ThisValue {
            get {
                if (_this != null) {
                    return _this.GetTypes(AnalysisUnit).Union(Function.NewThis);
                }

                return this.Function.NewThis;
            }
        }

        internal void AddReturnTypes(Node node, AnalysisUnit unit, IAnalysisSet types, bool enqueue = true) {
#if FALSE
            if (Generator != null) {
                Generator.AddReturn(node, unit, types, enqueue);
            } else 
#endif
            {
                Function.ReturnValue.MakeUnionStrongerIfMoreThan(unit.Analyzer.Limits.ReturnTypes, types);
                Function.ReturnValue.AddTypes(unit, types, enqueue);
            }
        }

        internal void EnsureParameters(FunctionAnalysisUnit unit) {
            var astParams = Function.FunctionObject.ParameterDeclarations;
            if (astParams != null) {
                for (int i = 0; i < astParams.Length; ++i) {
                    VariableDef param;
                    if (!TryGetVariable(astParams[i].Name, out param)) {
                        param = new LocatedVariableDef(unit.ProjectEntry, astParams[i]);
                        AddVariable(astParams[i].Name, param);
                    }
                }
            }
        }

        internal void AddParameterReferences(AnalysisUnit caller, Lookup[] names) {
            foreach (var name in names) {
                VariableDef param;
                if (name != null && TryGetVariable(name.Name, out param)) {
                    param.AddReference(name, caller);
                }
            }
        }

        public UserFunctionValue Function {
            get {
                return _function;
            }
        }

        public AnalysisValue AnalysisValue {
            get {
                return _function;
            }
        }

        public override int GetBodyStart(JsAst ast) {
            return ((FunctionObject)Node).Body.GetStartIndex(ast.LocationResolver);
        }

        public override string Name {
            get { return Function.FunctionObject.Name;  }
        }
    }
}

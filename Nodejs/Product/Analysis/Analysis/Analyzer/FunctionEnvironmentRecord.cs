/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Analyzer {
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
                    return _this.Types.Union(Function.NewThis);
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
                for (int i = 0; i < astParams.Count; ++i) {
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

        public override AnalysisValue AnalysisValue {
            get {
                return _function;
            }
        }

        public override int GetBodyStart(JsAst ast) {
            return ((FunctionObject)Node).Body.Span.Start;
        }

        public override string Name {
            get { return Function.FunctionObject.Name;  }
        }
    }
}

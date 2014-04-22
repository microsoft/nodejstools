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
    sealed class FunctionScope : EnvironmentRecord {
#if FALSE
        private ListParameterVariableDef _seqParameters;
        private DictParameterVariableDef _dictParameters;
#endif
        public readonly VariableDef ReturnValue;
        //public readonly GeneratorInfo Generator;

        public FunctionScope(
            UserFunctionInfo function,
            Node node,
            EnvironmentRecord declScope,
            IPythonProjectEntry declModule
        )
            : base(function, node, declScope) {
            ReturnValue = new VariableDef();
#if FALSE
            if (Function.FunctionObject.IsGenerator) {
                Generator = new GeneratorInfo(function.ProjectState, declModule);
                ReturnValue.AddTypes(function.ProjectEntry, Generator.SelfSet, false);
            }
#endif
        }

        public UserFunctionInfo FunctionInfo {
            get { return (UserFunctionInfo)AnalysisValue; }
        }

        public override IAnalysisSet ThisValue {
            get {
                return this.FunctionInfo.NewThis;
            }
        }

        internal void AddReturnTypes(Node node, AnalysisUnit unit, IAnalysisSet types, bool enqueue = true) {
#if FALSE
            if (Generator != null) {
                Generator.AddReturn(node, unit, types, enqueue);
            } else 
#endif
            {
                ReturnValue.MakeUnionStrongerIfMoreThan(unit.Analyzer.Limits.ReturnTypes, types);
                ReturnValue.AddTypes(unit, types, enqueue);
            }
        }

        internal void EnsureParameters(FunctionAnalysisUnit unit) {
            var astParams = Function.FunctionObject.ParameterDeclarations;
            for (int i = 0; i < astParams.Count; ++i) {
                VariableDef param;
                if (!Variables.TryGetValue(astParams[i].Name, out param)) {
#if FALSE
                    if (astParams[i].Kind == ParameterKind.List) {
                        param = _seqParameters = _seqParameters ?? new ListParameterVariableDef(unit, astParams[i]);
                    } else if (astParams[i].Kind == ParameterKind.Dictionary) {
                        param = _dictParameters = _dictParameters ?? new DictParameterVariableDef(unit, astParams[i]);
                    } else 
#endif
                    {
                        param = new LocatedVariableDef(unit.ProjectEntry, astParams[i].Context);
                    }
                    AddVariable(astParams[i].Name, param);
                }
            }
        }

        internal void AddParameterReferences(AnalysisUnit caller, Lookup[] names) {
            foreach (var name in names) {
                VariableDef param;
                if (name != null && Variables.TryGetValue(name.Name, out param)) {
                    param.AddReference(name, caller);
                }
            }
        }

        internal bool UpdateParameters(FunctionAnalysisUnit unit, ArgumentSet others, bool enqueue = true, FunctionScope scopeWithDefaultParameters = null) {
            EnsureParameters(unit);

            var astParams = Function.FunctionObject.ParameterDeclarations;
            bool added = false;
            var entry = unit.ProjectEntry;
            var state = unit.Analyzer;
            var limits = state.Limits;

            for (int i = 0; i < others.Args.Length && i < astParams.Count; ++i) {
                VariableDef param;
                if (!Variables.TryGetValue(astParams[i].Name, out param)) {
                    Debug.Assert(false, "Parameter " + astParams[i].Name + " has no variable in this scope");
                    param = AddVariable(astParams[i].Name);
                }
                param.MakeUnionStrongerIfMoreThan(limits.NormalArgumentTypes, others.Args[i]);
                added |= param.AddTypes(entry, others.Args[i], false);
            }
#if FALSE
            if (_seqParameters != null) {
                _seqParameters.List.MakeUnionStrongerIfMoreThan(limits.ListArgumentTypes, others.SequenceArgs);
                added |= _seqParameters.List.AddTypes(unit, new[] { others.SequenceArgs });
            }
            if (_dictParameters != null) {
                _dictParameters.Dict.MakeUnionStrongerIfMoreThan(limits.DictArgumentTypes, others.DictArgs);
                added |= _dictParameters.Dict.AddTypes(Function.FunctionObject, unit, state.GetConstant(""), others.DictArgs);
            }
#endif
            if (scopeWithDefaultParameters != null) {
                for (int i = 0; i < others.Args.Length && i < astParams.Count; ++i) {
                    VariableDef defParam, param;
                    if (Variables.TryGetValue(astParams[i].Name, out param) &&
                        !param.TypesNoCopy.Any() &&
                        scopeWithDefaultParameters.Variables.TryGetValue(astParams[i].Name, out defParam)) {
                        param.MakeUnionStrongerIfMoreThan(limits.NormalArgumentTypes, defParam.TypesNoCopy);
                        added |= param.AddTypes(entry, defParam.TypesNoCopy, false);
                    }
                }
            }

            if (enqueue && added) {
                unit.Enqueue();
            }
            return added;
        }


        public UserFunctionInfo Function {
            get {
                return (UserFunctionInfo)AnalysisValue;
            }
        }

        public override IEnumerable<KeyValuePair<string, VariableDef>> GetAllMergedVariables() {
            if (this != Function.AnalysisUnit.Scope) {
                // Many scopes reference one FunctionInfo, which references one
                // FunctionAnalysisUnit which references one scope. Since we
                // are not that scope, we won't look at _allCalls for other
                // variables.
                return Variables;
            }
            
            var scopes = new HashSet<EnvironmentRecord>();
            var result = Variables.AsEnumerable();
            if (Function._allCalls != null) {
                foreach (var callUnit in Function._allCalls.Values) {
                    scopes.Add(callUnit.Scope);
                }
                scopes.Remove(this);
                foreach (var scope in scopes) {
                    result = result.Concat(scope.GetAllMergedVariables());
                }
            }
            return result;
        }

        public override IEnumerable<VariableDef> GetMergedVariables(string name) {
            VariableDef res;
            FunctionScope fnScope;

            var nodes = new HashSet<Node>();
            var seen = new HashSet<EnvironmentRecord>();
            var queue = new Queue<FunctionScope>();
            queue.Enqueue(this);

            while (queue.Any()) {
                var scope = queue.Dequeue();
                if (scope == null || !seen.Add(scope)) {
                    continue;
                }

                if (scope.Node == Node && scope.Variables.TryGetValue(name, out res)) {
                    yield return res;
                }

                if (scope.Function._allCalls != null) {
                    foreach (var callUnit in scope.Function._allCalls.Values) {
                        fnScope = callUnit.Scope as FunctionScope;
                        if (fnScope != null && fnScope != this) {
                            queue.Enqueue(fnScope);
                        }
                    }
                }

                foreach (var keyValue in scope.NodeScopes.Where(kv => nodes.Contains(kv.Key))) {
                    if ((fnScope = keyValue.Value as FunctionScope) != null) {
                        queue.Enqueue(fnScope);
                    }
                }

                if ((fnScope = scope.OuterScope as FunctionScope) != null) {
                    nodes.Add(scope.Node);
                    queue.Enqueue(fnScope);
                }
            }
        }

        public override IEnumerable<AnalysisValue> GetMergedAnalysisValues() {
            yield return AnalysisValue;
            if (Function._allCalls != null) {
                foreach (var callUnit in Function._allCalls.Values) {
                    if (callUnit.Scope != this) {
                        yield return callUnit.Scope.AnalysisValue;
                    }
                }
            }
        }

        public override int GetBodyStart(JsAst ast) {
            return ((FunctionObject)Node).Body.Context.StartPosition;
        }

        public override string Name {
            get { return Function.FunctionObject.Name;  }
        }
    }
}

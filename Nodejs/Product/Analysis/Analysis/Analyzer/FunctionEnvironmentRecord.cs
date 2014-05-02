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
        public readonly VariableDef ReturnValue;
        private readonly VariableDef _this;
        //public readonly GeneratorInfo Generator;

        public FunctionEnvironmentRecord(
            UserFunctionValue function,
            Node node,
            EnvironmentRecord declScope,
            IJsProjectEntry declModule
        )
            : base(node, declScope) {
            _function = function;
            ReturnValue = new VariableDef();
            _this = new VariableDef();
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

        public override IAnalysisSet MergedThisValue {
            get {
                IAnalysisSet res = AnalysisSet.Empty;
                FunctionEnvironmentRecord fnEnv;

                var nodes = new HashSet<Node>();
                var seen = new HashSet<EnvironmentRecord>();
                var queue = new Queue<FunctionEnvironmentRecord>();
                queue.Enqueue(this);

                while (queue.Any()) {
                    var env = queue.Dequeue();
                    if (env == null || !seen.Add(env)) {
                        continue;
                    }

                    if (env.Node == Node) {
                        res = res.Union(env.ThisValue);
                    }

                    if (env.Function._allCalls != null) {
                        foreach (var callUnit in env.Function._allCalls.Values) {
                            fnEnv = callUnit.Environment as FunctionEnvironmentRecord;
                            if (fnEnv != null && fnEnv != this) {
                                queue.Enqueue(fnEnv);
                            }
                        }
                    }

                    foreach (var keyValue in env.NodeEnvironments.Where(kv => nodes.Contains(kv.Key))) {
                        if ((fnEnv = keyValue.Value as FunctionEnvironmentRecord) != null) {
                            queue.Enqueue(fnEnv);
                        }
                    }

                    if ((fnEnv = env.Parent as FunctionEnvironmentRecord) != null) {
                        nodes.Add(env.Node);
                        queue.Enqueue(fnEnv);
                    }
                }
                return res;
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
                if (!TryGetVariable(astParams[i].Name, out param)) {
                    param = new LocatedVariableDef(unit.ProjectEntry, astParams[i]);
                    AddVariable(astParams[i].Name, param);
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

        internal bool UpdateParameters(FunctionAnalysisUnit unit, IAnalysisSet @this, ArgumentSet others, bool enqueue = true, FunctionEnvironmentRecord envWithDefaultParams = null) {
            EnsureParameters(unit);

            var astParams = Function.FunctionObject.ParameterDeclarations;
            bool added = false;
            var entry = unit.ProjectEntry;
            var state = unit.Analyzer;
            var limits = state.Limits;

            if (@this != null) {
                added |= _this.AddTypes(entry, @this, false);
            }

            for (int i = 0; i < others.Args.Length && i < astParams.Count; ++i) {
                VariableDef param;
                if (!TryGetVariable(astParams[i].Name, out param)) {
                    Debug.Assert(false, "Parameter " + astParams[i].Name + " has no variable in this scope");
                    param = AddVariable(astParams[i].Name);
                }
                param.MakeUnionStrongerIfMoreThan(limits.NormalArgumentTypes, others.Args[i]);
                added |= param.AddTypes(entry, others.Args[i], false);
            }

            if (envWithDefaultParams != null) {
                for (int i = 0; i < others.Args.Length && i < astParams.Count; ++i) {
                    VariableDef defParam, param;
                    if (TryGetVariable(astParams[i].Name, out param) &&
                        !param.TypesNoCopy.Any() &&
                        envWithDefaultParams.TryGetVariable(astParams[i].Name, out defParam)) {
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

        public override IEnumerable<KeyValuePair<string, VariableDef>> GetAllMergedVariables() {
            if (this != Function.AnalysisUnit.Environment) {
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
                    scopes.Add(callUnit.Environment);
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
            FunctionEnvironmentRecord fnEnv;

            var nodes = new HashSet<Node>();
            var seen = new HashSet<EnvironmentRecord>();
            var queue = new Queue<FunctionEnvironmentRecord>();
            queue.Enqueue(this);

            while (queue.Any()) {
                var env = queue.Dequeue();
                if (env == null || !seen.Add(env)) {
                    continue;
                }

                if (env.Node == Node && env.TryGetVariable(name, out res)) {
                    yield return res;
                }

                if (env.Function._allCalls != null) {
                    foreach (var callUnit in env.Function._allCalls.Values) {
                        fnEnv = callUnit.Environment as FunctionEnvironmentRecord;
                        if (fnEnv != null && fnEnv != this) {
                            queue.Enqueue(fnEnv);
                        }
                    }
                }

                foreach (var keyValue in env.NodeEnvironments.Where(kv => nodes.Contains(kv.Key))) {
                    if ((fnEnv = keyValue.Value as FunctionEnvironmentRecord) != null) {
                        queue.Enqueue(fnEnv);
                    }
                }

                if ((fnEnv = env.Parent as FunctionEnvironmentRecord) != null) {
                    nodes.Add(env.Node);
                    queue.Enqueue(fnEnv);
                }
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

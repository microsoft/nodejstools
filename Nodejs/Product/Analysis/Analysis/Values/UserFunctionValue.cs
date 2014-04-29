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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    class UserFunctionValue : FunctionValue {
        private readonly FunctionObject _funcObject;
        private readonly FunctionAnalysisUnit _analysisUnit;
        private int _callDepthLimit;
        private int _callsSinceLimitChange;
        internal CallChainSet<FunctionAnalysisUnit> _allCalls;

        public UserFunctionValue(FunctionObject node, AnalysisUnit declUnit, EnvironmentRecord declScope)
            : base(declUnit.ProjectEntry) {
            _funcObject = node;
            _analysisUnit = new FunctionAnalysisUnit(this, declUnit, declScope, ProjectEntry);
            
            object value;
            if (!ProjectEntry.Properties.TryGetValue(AnalysisLimits.CallDepthKey, out value) ||
                (_callDepthLimit = (value as int?) ?? -1) < 0) {
                _callDepthLimit = ProjectEntry.Analyzer.Limits.CallDepth;
            }
        }

        public FunctionObject FunctionObject {
            get {
                return _funcObject;
            }
        }

        public override string Documentation {
            get {
#if FALSE
                if (FunctionObject.Body != null) {
                    return FunctionObject.Body.Documentation.TrimDocumentation();
                }
#endif
                return "";
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                return new[] { 
                    new LocationInfo(
                        ProjectEntry,
                        FunctionObject.Context.StartLineNumber,
                        FunctionObject.Context.StartColumn
                    )
                };
            }
        }

        public override AnalysisUnit AnalysisUnit {
            get {
                return _analysisUnit;
            }
        }

        public override string ToString() {
            return "FunctionInfo " + _analysisUnit.FullName + " (" + DeclaringVersion + ")";
        }

        public override string Description {
            get {
                var result = new StringBuilder();
                {
                    result.Append("function ");
                    if (FunctionObject.Name != null) {
                        result.Append(FunctionObject.Name);
                    }
                    result.Append("(");
                    AddParameterString(result);
                    result.Append(")");
                }

                AddReturnTypeString(result);
                AddDocumentationString(result);

                return result.ToString();
            }
        }

        private static void AppendDescription(StringBuilder result, AnalysisValue key) {
            result.Append(key.ShortDescription);
        }

        public override string Name {
            get {
                if (FunctionObject != null) {
                    return FunctionObject.Name;
                }
                return "<unknown function>";
            }
        }

        internal void AddParameterString(StringBuilder result) {
            for (int i = 0; i < FunctionObject.ParameterDeclarations.Count; i++) {
                if (i != 0) {
                    result.Append(", ");
                }
                var p = FunctionObject.ParameterDeclarations[i];

                var name = MakeParameterName(p);
                result.Append(name);
            }
        }

        internal void AddReturnTypeString(StringBuilder result) {
            bool first = true;
            for (int strength = 0; strength <= UnionComparer.MAX_STRENGTH; ++strength) {
                var retTypes = GetReturnValue(strength);
                if (retTypes.Count == 0) {
                    first = false;
                    break;
                }
                if (retTypes.Count <= 10) {
                    var seenNames = new HashSet<string>();
                    foreach (var av in retTypes) {
                        if (av == null) {
                            continue;
                        }

                        if (av.Push()) {
                            try {
                                if (!string.IsNullOrWhiteSpace(av.ShortDescription) && seenNames.Add(av.ShortDescription)) {
                                    if (first) {
                                        result.Append(" -> ");
                                        first = false;
                                    } else {
                                        result.Append(", ");
                                    }
                                    AppendDescription(result, av);
                                }
                            } finally {
                                av.Pop();
                            }
                        } else {
                            result.Append("...");
                        }
                    }
                    break;
                }
            }
        }

        internal IAnalysisSet GetReturnValue(int unionStrength = 0) {
            var result = (unionStrength >= 0 && unionStrength <= UnionComparer.MAX_STRENGTH)
                ? AnalysisSet.CreateUnion(UnionComparer.Instances[unionStrength])
                : AnalysisSet.Empty;

            var units = new HashSet<AnalysisUnit>();
            units.Add(AnalysisUnit);
            if (_allCalls != null) {
                units.UnionWith(_allCalls.Values);
            }

            result = result.UnionAll(units.OfType<FunctionAnalysisUnit>().Select(unit => unit.ReturnValue.TypesNoCopy));

            return result;
        }

        internal void AddDocumentationString(StringBuilder result) {
            if (!String.IsNullOrEmpty(Documentation)) {
                result.AppendLine();
                result.Append(Documentation);
            }
        }

        public override IEnumerable<OverloadResult> Overloads {
            get {
                var references = new Dictionary<string[], IEnumerable<AnalysisVariable>[]>(new StringArrayComparer());

                var units = new HashSet<AnalysisUnit>();
                units.Add(AnalysisUnit);
                if (_allCalls != null) {
                    units.UnionWith(_allCalls.Values);
                }

                foreach (var unit in units) {
                    var vars = FunctionObject.ParameterDeclarations.Select(p => {
                        VariableDef param;
                        if (unit.Scope.Variables.TryGetValue(p.Name, out param)) {
                            return param;
                        }
                        return null;
                    }).ToArray();

                    var parameters = vars
                        .Select(p => string.Join(", ", p.TypesNoCopy.Select(av => av.ShortDescription).OrderBy(s => s).Distinct()))
                        .ToArray();

                    IEnumerable<AnalysisVariable>[] refs;
                    if (references.TryGetValue(parameters, out refs)) {
                        refs = refs.Zip(vars, (r, v) => r.Concat(ProjectEntry.Analysis.ToVariables(v))).ToArray();
                    } else {
                        refs = vars.Select(v => ProjectEntry.Analysis.ToVariables(v)).ToArray();
                    }
                    references[parameters] = refs;
                }

                foreach (var keyValue in references) {
                    yield return new SimpleOverloadResult(
                        FunctionObject.ParameterDeclarations.Select((p, i) => {
                            var name = MakeParameterName(p);
                            var type = keyValue.Key[i];
                            var refs = keyValue.Value[i];
                            return new ParameterResult(name, string.Empty, type, false, refs);
                        }).ToArray(),
                        FunctionObject.Name,
                        Documentation
                    );
                }
            }
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            var callArgs = ArgumentSet.FromArgs(FunctionObject, unit, args);

            FunctionAnalysisUnit calledUnit;
            bool updateArguments = true;

            if (callArgs.Count == 0 || _callDepthLimit == 0) {
                calledUnit = (FunctionAnalysisUnit)AnalysisUnit;
            } else {
                if (_allCalls == null) {
                    _allCalls = new CallChainSet<FunctionAnalysisUnit>();
                }

                var chain = new CallChain(node, unit, _callDepthLimit);
                if (!_allCalls.TryGetValue(unit.ProjectEntry, chain, _callDepthLimit, out calledUnit)) {
                    if (unit.ForEval) {
                        // Call expressions that weren't analyzed get the union result
                        // of all calls to this function.
                        var res = AnalysisSet.Empty;
                        foreach (var call in _allCalls.Values) {
                            res = res.Union(call.ReturnValue.TypesNoCopy);
                        }
                        return res;
                    } else {
                        _callsSinceLimitChange += 1;
                        if (_callsSinceLimitChange >= ProjectState.Limits.DecreaseCallDepth && _callDepthLimit > 1) {
                            _callDepthLimit -= 1;
                            _callsSinceLimitChange = 0;
                            AnalysisLog.ReduceCallDepth(this, _allCalls.Count, _callDepthLimit);

                            _allCalls.Clear();
                            chain = chain.Trim(_callDepthLimit);
                        }
                        calledUnit = new FunctionAnalysisUnit((FunctionAnalysisUnit)AnalysisUnit, chain, callArgs);
                        _allCalls.Add(unit.ProjectEntry, chain, calledUnit);
                        updateArguments = false;
                    }
                }
            }

            if (updateArguments && calledUnit.UpdateParameters(callArgs)) {
                AnalysisLog.UpdateUnit(calledUnit);
            }

            calledUnit.ReturnValue.AddDependency(unit);
            return calledUnit.ReturnValue.Types;
        }

        internal IAnalysisSet[] GetParameterTypes(int unionStrength = 0) {
            var result = new IAnalysisSet[FunctionObject.ParameterDeclarations.Count];
            var units = new HashSet<AnalysisUnit>();
            units.Add(AnalysisUnit);
            if (_allCalls != null) {
                units.UnionWith(_allCalls.Values);
            }

            for (int i = 0; i < result.Length; ++i) {
                result[i] = (unionStrength >= 0 && unionStrength <= UnionComparer.MAX_STRENGTH)
                    ? AnalysisSet.CreateUnion(UnionComparer.Instances[unionStrength])
                    : AnalysisSet.Empty;

                VariableDef param;
                foreach (var unit in units) {
                    if (unit != null && unit.Scope != null && unit.Scope.Variables.TryGetValue(FunctionObject.ParameterDeclarations[i].Name, out param)) {
                        result[i] = result[i].Union(param.TypesNoCopy);
                    }
                }
            }

            return result;
        }

        private class StringArrayComparer : IEqualityComparer<string[]> {
            private IEqualityComparer<string> _comparer;

            public StringArrayComparer() {
                _comparer = StringComparer.Ordinal;
            }

            public StringArrayComparer(IEqualityComparer<string> comparer) {
                _comparer = comparer;
            }

            public bool Equals(string[] x, string[] y) {
                if (x == null || y == null) {
                    return x == null && y == null;
                }

                if (x.Length != y.Length) {
                    return false;
                }

                for (int i = 0; i < x.Length; ++i) {
                    if (!_comparer.Equals(x[i], y[i])) {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(string[] obj) {
                if (obj == null) {
                    return 0;
                }
                int hc = 261563 ^ obj.Length;
                foreach (var p in obj) {
                    hc ^= _comparer.GetHashCode(p);
                }
                return hc;
            }
        }
    }
}

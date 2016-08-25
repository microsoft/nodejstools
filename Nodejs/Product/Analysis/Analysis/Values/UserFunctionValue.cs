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
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class UserFunctionValue : FunctionValue, IReferenceable {
        private readonly FunctionObject _funcObject;
        private readonly FunctionAnalysisUnit _analysisUnit;
        public readonly ArgumentsValue Arguments;
        public readonly VariableDef ArgumentsVariable;
        public CallArgs _curArgs;
        public VariableDef ReturnValue;
        internal Dictionary<CallArgs, CallInfo> _allCalls;
        private OverflowState _overflowed;
        private const int MaximumCallCount = 5;


        public UserFunctionValue(FunctionObject node, AnalysisUnit declUnit, EnvironmentRecord declScope, bool isNested = false)
            : base(declUnit.ProjectEntry, null, node.Name ?? node.NameGuess) {
            ReturnValue = new VariableDef();
            _funcObject = node;
            _analysisUnit = new FunctionAnalysisUnit(this, declUnit, declScope, ProjectEntry);

            declUnit.Analyzer.AnalysisValueCreated(typeof(UserFunctionValue));
            var argsWalker = new ArgumentsWalker();
            FunctionObject.Body.Walk(argsWalker);

            if (argsWalker.UsesArguments) {
                Arguments = new ArgumentsValue(this);
                ArgumentsVariable = new VariableDef();
                ArgumentsVariable.AddTypes(_analysisUnit, Arguments.SelfSet);
            }
        }

        public override IAnalysisSet ReturnTypes {
            get {
                return ReturnValue.Types;
            }
        }

        public FunctionObject FunctionObject {
            get {
                return _funcObject;
            }
        }

        public override string Documentation {
            get {
                string doclet = FunctionObject.Doclet;
                if (doclet == null || doclet.Length < 4) {
                    return "";
                }

                // Strip /* and */ and normalize line endings
                doclet = doclet.Substring(2, doclet.Length - 4).Replace("\r\n", "\n");

                // Split into lines, trim whitespace, and remove * at the beginning of every line.
                var lines = doclet.Split('\n').Select(s => s.TrimStart().TrimStart('*').Trim());

                var sb = new StringBuilder(doclet.Length);
                bool bol = true;
                foreach (var line in lines) {
                    if (line == "") {
                        // Blank line is a paragraph separator - keep it, but fold any adjacent blank lines into one.
                        if (!bol) {
                            sb.AppendLine();
                            sb.AppendLine();
                            bol = true;
                        }
                    } else if (line.StartsWith("@")) {
                        // @tag also begins a new paragraph, but no blank line is inserted to separate it from the previous one.
                        if (bol) {
                            bol = false;
                        } else {
                            sb.AppendLine();
                        }
                        sb.Append(line);
                    } else {
                        // Adjacent lines are concatenated into a single paragraph, with newlines replaced by spaces.
                        if (bol) {
                            bol = false;
                        } else {
                            sb.Append(' ');
                        }
                        sb.Append(line);
                    }
                }

                return sb.ToString().Trim();
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                return new[] { 
                    ProjectEntry.Tree.ResolveLocation(ProjectEntry, FunctionObject)
                };
            }
        }

        public override AnalysisUnit AnalysisUnit {
            get {
                return _analysisUnit;
            }
        }

        public override string ToString() {
            return String.Format("UserFunction {0} {1}\r\n{2}",
                FunctionObject.Name,
                FunctionObject.GetStart(ProjectEntry.Tree.LocationResolver),
                ProjectEntry.FilePath
            );
        }

        public override string Description {
            get {
                var result = new StringBuilder();
                {
                    result.Append("function ");
                    var name = FunctionObject.Name ?? FunctionObject.NameGuess ?? "<anonymous>";
                    result.Append(name);
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
                    return FunctionObject.Name ?? FunctionObject.NameGuess;
                }
                return "<unknown function>";
            }
        }

        internal void AddParameterString(StringBuilder result) {
            if (FunctionObject.ParameterDeclarations != null) {
                for (int i = 0; i < FunctionObject.ParameterDeclarations.Length; i++) {
                    if (i != 0) {
                        result.Append(", ");
                    }
                    var p = FunctionObject.ParameterDeclarations[i];

                    var name = MakeParameterName(p);
                    result.Append(name);
                }
            }
        }

        internal void AddReturnTypeString(StringBuilder result) {
            bool first = true;
            var retTypes = ReturnValue.Types;
            if (retTypes.Count == 0) {
                return;
            }
            if (retTypes.Count <= 10) {
                var seenNames = new HashSet<string>();
                foreach (var av in retTypes) {
                    if (av == null) {
                        continue;
                    }

                    if (av.Value.Push()) {
                        try {
                            if (!string.IsNullOrWhiteSpace(av.Value.ShortDescription) && seenNames.Add(av.Value.ShortDescription)) {
                                if (first) {
                                    result.Append(" -> ");
                                    first = false;
                                } else {
                                    result.Append(", ");
                                }
                                AppendDescription(result, av.Value);
                            }
                        } finally {
                            av.Value.Pop();
                        }
                    } else {
                        result.Append("...");
                    }
                }
            }
        }

        internal void AddDocumentationString(StringBuilder result) {
            if (!String.IsNullOrEmpty(Documentation)) {
                result.AppendLine();
                result.Append(Documentation);
            }
        }

        public override string OwnerName {
            get {
                if (FunctionObject.Name == null) {
                    return "";
                }

                return FunctionObject.Name;
            }
        }

        class ArgumentsWalker : AstVisitor {
            public bool UsesArguments;

            public override bool Walk(Lookup node) {
                if (node.Name == "arguments") {
                    UsesArguments = true;
                }
                return base.Walk(node);
            }

            public override bool Walk(FunctionObject node) {
                return false;
            }
        }

        public override IEnumerable<OverloadResult> Overloads {
            get {
                var references = new Dictionary<string[], IEnumerable<AnalysisVariable>[]>(new StringArrayComparer());

                var vars = FunctionObject.ParameterDeclarations.Select(p => {
                    VariableDef param;
                    if (AnalysisUnit.Environment.TryGetVariable(p.Name, out param)) {
                        return param;
                    }
                    return null;
                }).ToArray();

                var parameters = vars
                    .Select(p => string.Join(" or ", p.TypesNoCopy.Select(av => av.Value.ShortDescription).Where(s => !String.IsNullOrWhiteSpace(s)).OrderBy(s => s).Distinct()))
                    .ToArray();

                IEnumerable<AnalysisVariable>[] refs = vars.Select(v => VariableTransformer.OtherToVariables.ToVariables(_analysisUnit, v)).ToArray();

                ParameterResult[] parameterResults = FunctionObject.ParameterDeclarations == null ?
                    new ParameterResult[0] :
                    FunctionObject.ParameterDeclarations.Select((p, i) =>
                        new ParameterResult(
                            MakeParameterName(p),
                            string.Empty,
                            parameters[i],
                            false,
                            refs[i]
                        )
                    ).ToArray();

                if (Arguments != null) {
                    parameterResults = parameterResults.Concat(
                        new[] { new ParameterResult("...") }
                    ).ToArray();
                }

                yield return new SimpleOverloadResult(FunctionObject.Name ?? FunctionObject.NameGuess ?? "<anonymous function>", Documentation, parameterResults);
            }
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // if the arguments are complex then we'll do a merged analysis, otherwise we'll analyze
            // this set of arguments independently.
            bool skipDeepAnalysis = false;
            if (@this != null) {
                skipDeepAnalysis |= CheckTooManyValues(unit, @this);
            }

            if (!skipDeepAnalysis) {
                for (int i = 0; i < args.Length; i++) {
                    skipDeepAnalysis |= CheckTooManyValues(unit, args[i]);
                    if (skipDeepAnalysis) {
                        break;
                    }
                }
            }

            if (skipDeepAnalysis || _overflowed == OverflowState.OverflowedBigTime) {
                // start merging all arguments into a single call analysis                
                if (_analysisUnit.AddArgumentTypes(
                    (FunctionEnvironmentRecord)_analysisUnit._env,
                    @this,
                    args,
                    _analysisUnit.Analyzer.Limits.MergedArgumentTypes
                )) {
                    _analysisUnit.Enqueue();
                }
                _analysisUnit.ReturnValue.AddDependency(unit);
                return _analysisUnit.ReturnValue.GetTypes(unit, ProjectEntry);
            }

            var callArgs = new CallArgs(@this, args, _overflowed == OverflowState.OverflowedOnce);

            CallInfo callInfo;

            if (_allCalls == null) {
                _allCalls = new Dictionary<CallArgs, CallInfo>();
            }

            if (!_allCalls.TryGetValue(callArgs, out callInfo)) {
                if (unit.ForEval) {
                    return ReturnValue.GetTypes(unit, ProjectEntry);
                }

                _allCalls[callArgs] = callInfo = new CallInfo(
                    this,
                    _analysisUnit.Environment,
                    _analysisUnit._declUnit,
                    callArgs
                );

                if (_allCalls.Count > MaximumCallCount) {
                    // try and compress args using UnionEquality...
                    if (_overflowed == OverflowState.None) {
                        _overflowed = OverflowState.OverflowedOnce;
                        var newAllCalls = new Dictionary<CallArgs, CallInfo>();
                        foreach (var keyValue in _allCalls) {
                            newAllCalls[new CallArgs(@this, keyValue.Key.Args, overflowed: true)] = keyValue.Value;
                        }
                        _allCalls = newAllCalls;
                    }

                    if (_allCalls.Count > MaximumCallCount) {
                        _overflowed = OverflowState.OverflowedBigTime;
                        _analysisUnit.ReturnValue.AddDependency(unit);
                        return _analysisUnit.ReturnValue.GetTypes(unit, ProjectEntry);
                    }
                }

                callInfo.ReturnValue.AddDependency(unit);
                callInfo.AnalysisUnit.Enqueue();
                return AnalysisSet.Empty;
            } else {
                callInfo.ReturnValue.AddDependency(unit);
                return callInfo.ReturnValue.GetTypes(unit, ProjectEntry);
            }
        }

        private bool CheckTooManyValues(AnalysisUnit unit, IAnalysisSet @this) {
            int argCount = @this.Count;
            if (argCount > 1) {
                foreach (var arg in @this) {
                    if (arg.Value == unit.Analyzer._undefined || arg.Value == unit.Analyzer._nullInst) {
                        argCount--;
                    }
                }
                if (argCount > 1) {
                    _overflowed = OverflowState.OverflowedBigTime;
                    return true;
                }
            }
            return false;
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

        private const int FunctionMergeStrength = 2;
        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= FunctionMergeStrength) {
                var func = av as UserFunctionValue;
                if (func != null) {
                    return true;
                }
            }

            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= FunctionMergeStrength) {
                return ProjectState._functionPrototype.GetHashCode();
            }

            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= FunctionMergeStrength) {
                var func = av as UserFunctionValue;
                if (func != null) {
                    return this;
                }
            }

            return base.UnionMergeTypes(av, strength);
        }

        /// <summary>
        /// Hashable set of arguments for analyzing the cartesian product of the received arguments.
        /// 
        /// Each time we get called we check and see if we've seen the current argument set.  If we haven't
        /// then we'll schedule the function to be analyzed for those args (and if that results in a new
        /// return type then we'll use the return type to analyze afterwards).
        /// </summary>
        [Serializable]
        internal class CallArgs : IEquatable<CallArgs> {
            public readonly IAnalysisSet This;
            public readonly IAnalysisSet[] Args;
            private int _hashCode;

            public CallArgs(IAnalysisSet @this, IAnalysisSet[] args, bool overflowed) {
                if (!overflowed) {
                    for (int i = 0; i < args.Length; i++) {
                        if (args[i].Count >= 10) {
                            overflowed = true;
                            break;
                        }
                    }

                    if (@this != null && @this.Count >= 10) {
                        overflowed = true;
                    }
                }
                if (overflowed) {
                    for (int i = 0; i < args.Length; i++) {
                        args[i] = args[i].AsUnion(MergeStrength.ToObject);
                    }
                    if (@this != null) {
                        @this = @this.AsUnion(MergeStrength.ToObject);
                    }
                }
                This = @this;
                Args = args;
            }

            public override string ToString() {
                StringBuilder res = new StringBuilder();

                if (This != null) {
                    bool appended = false;
                    foreach (var @this in This) {
                        if (appended) {
                            res.Append(", ");
                        }
                        res.Append(@this.ToString());
                        appended = true;
                    }
                    res.Append(" ");
                }

                res.Append("{");
                foreach (var arg in Args) {
                    res.Append("{");
                    bool appended = false;
                    foreach (var argVal in arg) {
                        if (appended) {
                            res.Append(", ");
                        }
                        res.Append(argVal.ToString());
                        res.Append(" ");
                        res.Append(GetComparer().GetHashCode(argVal.Value));
                        appended = true;
                    }
                    res.Append("}");
                }
                res.Append("}");
                return res.ToString();
            }

            public override bool Equals(object obj) {
                CallArgs other = obj as CallArgs;
                if (other != null) {
                    return Equals(other);
                }
                return false;
            }

            #region IEquatable<CallArgs> Members

            public bool Equals(CallArgs other) {
                if (Object.ReferenceEquals(this, other)) {
                    return true;
                }

                if (Args.Length != other.Args.Length) {
                    return false;
                }

                if (This != null) {
                    if (other.This != null) {
                        if (This.Count != other.This.Count) {
                            return false;
                        }
                    } else if (This.Count != 0) {
                        return false;
                    }
                } else if (other.This != null && other.This.Count > 0) {
                    return false;
                }

                if (This != null) {
                    foreach (var @this in This) {
                        if (!other.This.Contains(@this)) {
                            return false;
                        }
                    }
                }

                for (int i = 0; i < Args.Length; i++) {
                    if (Args[i].Count != other.Args[i].Count) {
                        return false;
                    }
                }

                for (int i = 0; i < Args.Length; i++) {
                    foreach (var arg in Args[i]) {
                        if (!other.Args[i].Contains(arg)) {
                            return false;
                        }
                    }
                }
                return true;
            }

            #endregion

            public override int GetHashCode() {
                if (_hashCode == 0) {
                    int hc = 6551;
                    IEqualityComparer<AnalysisValue> comparer = GetComparer();
                    if (Args.Length > 0) {
                        for (int i = 0; i < Args.Length; i++) {
                            foreach (var value in Args[i]) {
                                hc ^= comparer.GetHashCode(value.Value);
                            }
                        }
                    }
                    if (This != null) {
                        foreach (var value in This) {
                            hc ^= comparer.GetHashCode(value.Value);
                        }
                    }

                    _hashCode = hc;
                }
                return _hashCode;
            }

            private IEqualityComparer<AnalysisValue> GetComparer() {
                var @this = This as HashSet<AnalysisValue>;
                if (@this != null) {
                    return @this.Comparer;
                }
                if (Args.Length > 0) {
                    var arg0 = Args[0] as HashSet<AnalysisValue>;
                    if (arg0 != null) {
                        return arg0.Comparer;
                    }
                }
                return EqualityComparer<AnalysisValue>.Default;
            }
        }

        [Serializable]
        internal class CallInfo {
            public readonly VariableDef ReturnValue;
            public readonly CartesianProductFunctionAnalysisUnit AnalysisUnit;

            public CallInfo(UserFunctionValue funcValue, EnvironmentRecord environment, AnalysisUnit outerUnit, CallArgs args) {
                ReturnValue = new VariableDef();
                AnalysisUnit = new CartesianProductFunctionAnalysisUnit(
                    funcValue,
                    environment,
                    outerUnit,
                    args,
                    ReturnValue
                );
            }
        }

        enum OverflowState {
            None,
            OverflowedOnce,
            OverflowedBigTime
        }

        public IEnumerable<KeyValuePair<ProjectEntry, EncodedSpan>> Definitions {
            get {
                yield return new KeyValuePair<ProjectEntry, EncodedSpan>(
                    ProjectEntry,
                    _funcObject.EncodedSpan
                );
            }
        }

        public new IEnumerable<KeyValuePair<ProjectEntry, EncodedSpan>> References {
            get {
                if (_references != null) {
                    foreach (var keyValue in _references) {
                        if (keyValue.Value.References != null) {
                            foreach (var loc in keyValue.Value.References) {
                                yield return new KeyValuePair<ProjectEntry, EncodedSpan>(
                                    keyValue.Key,
                                    loc
                                );
                            }
                        }
                    }
                }
            }
        }

    }
}

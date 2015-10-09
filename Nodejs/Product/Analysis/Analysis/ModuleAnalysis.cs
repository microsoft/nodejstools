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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Encapsulates all of the information about a single module which has been analyzed.  
    /// 
    /// Can be queried for various information about the resulting analysis.
    /// </summary>
    [Serializable]
    internal sealed class ModuleAnalysis {
        private readonly AnalysisUnit _unit;
        private readonly ModuleEnvironmentRecord _scope;
        private readonly IAnalysisCookie _cookie;
        private static Regex _otherPrivateRegex = new Regex("^_[a-zA-Z_]\\w*__[a-zA-Z_]\\w*$");
        private static HashSet<string> _exprKeywords = new HashSet<string>() { 
            "function", "null", "true", "false", "this", "void", 
            "typeof", "delete", "in", "new" };
        private static HashSet<string> _stmtKeywords = new HashSet<string>() { 
            "debugger", "var", "if", "for", "do", "while", "continue", "break",
            "return", "with", "switch", "throw", "try", "else",
        };

        internal ModuleAnalysis(AnalysisUnit unit, ModuleEnvironmentRecord scope, IAnalysisCookie cookie) {
            _unit = unit;
            _scope = scope;
            _cookie = cookie;
        }

        #region Public API

        /// <summary>
        /// Returns the IAnalysisCookie which was used to produce this ModuleAnalysis.
        /// </summary>
        public IAnalysisCookie AnalysisCookie {
            get {
                return _cookie;
            }
        }

        /// <summary>
        /// Evaluates the given expression in at the provided line number and returns the values
        /// that the expression can evaluate to.
        /// </summary>
        /// <param name="exprText">The expression to determine the result of.</param>
        /// <param name="index">The 0-based absolute index into the file where the expression should be evaluated within the module.</param>
        public IEnumerable<AnalysisValue> GetValuesByIndex(string exprText, int index) {
            var scope = FindEnvironment(index);
            var expr = Statement.GetExpression(GetAstFromText(exprText).Block);

            var unit = GetEvalAnalysisUnit(scope);
            var eval = new ExpressionEvaluator(unit, scope);
            var res = AnalysisSet.EmptyUnion;

            if (expr != null) {
                var values = eval.Evaluate(expr);
                foreach (var v in values) {
                    if (v.Value.IsCurrent) {
                        res = res.Add(v);
                    }
                }
            }

            return res.Select(x => x.Value);
        }

        internal IEnumerable<AnalysisVariable> ReferencablesToVariables(IEnumerable<IReferenceable> defs) {
            foreach (var def in defs) {
                foreach (var res in VariableTransformer.OtherToVariables.ToVariables(_unit, def)) {
                    yield return res;
                }
            }
        }

        internal IEnumerable<AnalysisVariable> ToVariables(IReferenceable referenceable) {
            LocatedVariableDef locatedDef = referenceable as LocatedVariableDef;

            if (locatedDef != null &&
                locatedDef.Entry.Tree != null &&    // null tree if there are errors in the file
                locatedDef.DeclaringVersion == locatedDef.Entry.AnalysisVersion) {
                var start = locatedDef.Node;

                yield return new AnalysisVariable(VariableType.Definition, locatedDef.Entry.Tree.ResolveLocation(locatedDef.Entry, start));
            }

            VariableDef def = referenceable as VariableDef;
            if (def != null) {
                foreach (var location in def.GetTypesNoCopy(_unit).SelectMany(type => type.Value.Locations)) {
                    yield return new AnalysisVariable(VariableType.Value, location);
                }
            }

            foreach (var reference in referenceable.Definitions) {
                yield return new AnalysisVariable(VariableType.Definition, reference.Value.GetLocationInfo(reference.Key));
            }

            foreach (var reference in referenceable.References) {
                yield return new AnalysisVariable(VariableType.Reference, reference.Value.GetLocationInfo(reference.Key));
            }
        }

        /// <summary>
        /// Gets the variables the given expression evaluates to.  Variables include parameters, locals, and fields assigned on classes, modules and instances.
        /// 
        /// Variables are classified as either definitions or references.  Only parameters have unique definition points - all other types of variables
        /// have only one or more references.
        /// 
        /// index is a 0-based absolute index into the file.
        /// </summary>
        public IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index) {
            var scope = FindEnvironment(index);
            var expr = Statement.GetExpression(GetAstFromText(exprText).Block);

            var unit = GetEvalAnalysisUnit(scope);
            Lookup name = expr as Lookup;
            if (name != null) {
                var defScope = scope.EnumerateTowardsGlobal.FirstOrDefault(s =>
                    s.ContainsVariable(name.Name));

                if (defScope == null) {
                    defScope = scope.GlobalEnvironment;
                }

                return GetVariablesInScope(name, defScope).Distinct();
            }

            var member = expr as Member;
            if (member != null) {
                var eval = new ExpressionEvaluator(unit, scope);
                var objects = eval.Evaluate(member.Root);

                var refs = Enumerable.Empty<IAnalysisVariable>();
                foreach (var v in objects) {
                    var container = v.Value as IReferenceableContainer;
                    if (container != null) {
                        refs = refs.Concat(ReferencablesToVariables(container.GetDefinitions(member.Name)));
                    }
                }
                return refs;
            }

            return Enumerable.Empty<IAnalysisVariable>();
        }

        private IEnumerable<IAnalysisVariable> GetVariablesInScope(Lookup name, EnvironmentRecord scope) {
            var result = new List<IAnalysisVariable>();

            VariableDef var;
            if (scope.TryGetVariable(name.Name, out var)) {
                result.AddRange(VariableTransformer.ScopeToVariables.ToVariables(_unit, var));
            }

            // if a variable is imported from another module then also yield the defs/refs for the 
            // value in the defining module.
            var linked = scope.GetLinkedVariablesNoCreate(name.Name);
            if (linked != null) {
                result.AddRange(linked.SelectMany(x => VariableTransformer.ScopeToVariables.ToVariables(_unit, x)));
            }
            return result;
        }

#if FALSE
        public MemberResult[] GetModules(bool topLevelOnly = false) {
            List<MemberResult> res = new List<MemberResult>(ProjectState.GetModules(topLevelOnly));

            var children = GlobalScope.GetChildrenPackages();

            foreach (var child in children) {
                res.Add(new MemberResult(child.Key, PythonMemberType.Module));
            }

            return res.ToArray();
        }


        public MemberResult[] GetModuleMembers(string[] names, bool includeMembers = false) {
            var res = new List<MemberResult>(ProjectState.GetModuleMembers(names, includeMembers));
            var children = GlobalScope.GetChildrenPackages();

            foreach (var child in children) {
                var mod = (ModuleInfo)child.Value;
                var childName = mod.Name.Substring(this.GlobalScope.Name.Length + 1);

                if (childName.StartsWith(names[0])) {
                    res.AddRange(JsAnalyzer.GetModuleMembers(names, includeMembers, mod as IModule));
                }
            }

            return res.ToArray();
        }


        private static bool IsFirstLineOfFunction(EnvironmentRecord innerScope, EnvironmentRecord outerScope, int index) {
            if (innerScope.OuterScope == outerScope && innerScope is Microsoft.NodejsTools.Analysis.Analyzer.FunctionScope) {
                var funcScope = (Microsoft.NodejsTools.Analysis.Analyzer.FunctionScope)innerScope;
                var def = funcScope.Function.FunctionObject;

                // TODO: Use indexes rather than lines to check location
                int lineNo = def.GlobalParent.IndexToLocation(index).Line;
                if (lineNo == def.GetStart(def.GlobalParent).Line) {
                    return true;
                }
            }
            return false;
        }
#endif

#if DEBUG
        public string Dump() {
            StringBuilder res = new StringBuilder();
            var curScope = _scope;
            DumpScope(res, curScope);
            return res.ToString();
        }

        private static void DumpScope(StringBuilder output, EnvironmentRecord curScope, int level = 0) {
            curScope.DumpScope(output, level);
            foreach (var child in curScope.Children) {
                DumpScope(output, child, level + 1);
            }
        }
#endif

        /// <summary>
        /// Evaluates a given expression and returns a list of members which exist in the expression.
        /// 
        /// If the expression is an empty string returns all available members at that location.
        /// 
        /// index is a zero-based absolute index into the file.
        /// </summary>
        public IEnumerable<MemberResult> GetMembersByIndex(string exprText, int index, GetMemberOptions options = GetMemberOptions.None) {
            if (exprText.Length == 0) {
                return GetAllAvailableMembersByIndex(index, options);
            }

            var scope = FindEnvironment(index);

            var expr = Statement.GetExpression(GetAstFromText(exprText).Block);
            if (expr == null) {
                return new MemberResult[0];
            }
            if (expr is ConstantWrapper && ((ConstantWrapper)expr).Value is int) {
                // no completions on integer ., the user is typing a float
                return new MemberResult[0];
            }

            var unit = GetEvalAnalysisUnit(scope);
            var lookup = new ExpressionEvaluator(unit, scope).Evaluate(expr);
            return GetMemberResults(lookup, scope, options);
        }

        /// <summary>
        /// Gets information about the available signatures for the given expression.
        /// </summary>
        /// <param name="exprText">The expression to get signatures for.</param>
        /// <param name="index">The 0-based absolute index into the file.</param>
        public IEnumerable<IOverloadResult> GetSignaturesByIndex(string exprText, int index) {
            try {
                var scope = FindEnvironment(index);
                var unit = GetEvalAnalysisUnit(scope);
                var eval = new ExpressionEvaluator(unit, scope);

                var expr = Statement.GetExpression(GetAstFromText(exprText).Block);
                if (expr == null ||
                    expr is ArrayLiteral) {
                    return Enumerable.Empty<IOverloadResult>();
                }
                var lookup = eval.Evaluate(expr);

                var result = new HashSet<OverloadResult>(OverloadResultComparer.Instance);

                foreach (var ns in lookup) {
                    if (ns.Value.Overloads != null) {
                        result.UnionWith(ns.Value.Overloads);
                    }
                }

                return result;
            } catch (Exception) {
                // TODO: log exception
                return new[] { new SimpleOverloadResult("Unknown", "IntellisenseError_Sigs", new ParameterResult[0]) };
            }
        }

        /// <summary>
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, GetMemberOptions options = GetMemberOptions.None) {
            var result = new Dictionary<string, List<AnalysisValue>>();

            // collect variables from user defined scopes
            var scope = FindEnvironment(index);
            foreach (var s in scope.EnumerateTowardsGlobal) {
                foreach (var kvp in s.Variables) {
                    result[kvp.Key] = new List<AnalysisValue>(kvp.Value.GetTypesNoCopy(_unit).Select(x => x.Value));
                }
            }

            foreach (var kvp in this._unit.Analyzer._globalObject.GetAllMembers(_unit.ProjectEntry)) {
                List<AnalysisValue> values;
                if (!result.TryGetValue(kvp.Key, out values)) {
                    result[kvp.Key] = new List<AnalysisValue>(kvp.Value.Select(x => x.Value));
                } else {
                    values.AddRange(kvp.Value.Select(x => x.Value));
                }
            }

            var res = MemberDictToResultList(options, result);
            if (options.Keywords()) {
                res = GetKeywordMembers(options, scope).Union(res);
            }

            return res;
        }

        private IEnumerable<MemberResult> GetKeywordMembers(GetMemberOptions options, EnvironmentRecord scope) {
            IEnumerable<string> keywords = null;
            if (options.ExpressionKeywords()) {
                // keywords available in any context
                keywords = _exprKeywords;
            } else {
                keywords = Enumerable.Empty<string>();
            }

            if (options.StatementKeywords()) {
                keywords = keywords.Union(_stmtKeywords);
            }

            return keywords.Select(kw => new MemberResult(name: kw, documentation: kw, type: JsMemberType.Keyword));
        }

        #endregion

        /// <summary>
        /// Returns a list of valid names available at the given position in the analyzed source code minus the builtin variables.
        /// 
        /// TODO: This should go away, it's only used for tests.
        /// </summary>
        /// <param name="index">The index where the available mebmers should be looked up.</param>
        internal IEnumerable<string> GetVariablesNoBuiltinsByIndex(int index) {
            var result = Enumerable.Empty<string>();
            var chain = FindEnvironment(index);
            foreach (var scope in chain.EnumerateFromGlobal) {
                result = result.Concat(scope.Variables.Select(val => val.Key));
            }
            return result.Distinct();
        }

        public JsAnalyzer ProjectState {
            get { return _scope.ProjectEntry.Analyzer; }
        }

        internal EnvironmentRecord Environment {
            get { return _scope; }
        }

        internal IEnumerable<MemberResult> GetMemberResults(IAnalysisSet vars, EnvironmentRecord scope, GetMemberOptions options) {
            IList<AnalysisValue> namespaces = new List<AnalysisValue>();
            foreach (var ns in vars) {
                if (ns != null) {
                    namespaces.Add(ns.Value);
                }
            }

            if (namespaces.Count == 1) {
                // optimize for the common case of only a single namespace
                var newMembers = namespaces[0].GetAllMembers(_unit.ProjectEntry);
                if (newMembers == null || newMembers.Count == 0) {
                    return new MemberResult[0];
                }

                return SingleMemberResult(options, newMembers);
            }

            Dictionary<string, List<AnalysisValue>> memberDict = null;
            Dictionary<string, List<AnalysisValue>> ownerDict = null;
            HashSet<string> memberSet = null;
            int namespacesCount = namespaces.Count;
            foreach (AnalysisValue ns in namespaces) {
                if (ProjectState._nullInst == ns) {
                    namespacesCount -= 1;
                    continue;
                }

                var newMembers = ns.GetAllMembers(_unit.ProjectEntry);
                // IntersectMembers(members, memberSet, memberDict);
                if (newMembers == null || newMembers.Count == 0) {
                    namespacesCount -= 1;
                    continue;
                }

                if (memberSet == null) {
                    // first namespace, add everything
                    memberSet = new HashSet<string>(newMembers.Keys);
                    memberDict = new Dictionary<string, List<AnalysisValue>>();
                    ownerDict = new Dictionary<string, List<AnalysisValue>>();
                    foreach (var kvp in newMembers) {
                        var tmp = new List<AnalysisValue>(kvp.Value.Select(x => x.Value));
                        memberDict[kvp.Key] = tmp;
                        ownerDict[kvp.Key] = new List<AnalysisValue> { ns };
                    }
                } else {
                    // 2nd or nth namespace, union or intersect
                    HashSet<string> toRemove;
                    IEnumerable<string> adding;

                    // we're adding all of newMembers keys
                    adding = newMembers.Keys;
                    toRemove = null;

                    // update memberDict
                    foreach (var name in adding) {
                        List<AnalysisValue> values;
                        if (!memberDict.TryGetValue(name, out values)) {
                            memberDict[name] = values = new List<AnalysisValue>();
                        }
                        values.AddRange(newMembers[name].Select(x => x.Value));
                        if (!ownerDict.TryGetValue(name, out values)) {
                            ownerDict[name] = values = new List<AnalysisValue>();
                        }
                        values.Add(ns);
                    }

                    if (toRemove != null) {
                        foreach (var name in toRemove) {
                            memberDict.Remove(name);
                            ownerDict.Remove(name);
                        }
                    }
                }
            }

            if (memberDict == null) {
                return new MemberResult[0];
            }
            return MemberDictToResultList(options, memberDict, ownerDict, namespacesCount);
        }

        /// <summary>
        /// Gets the expression for the given text.  
        /// 
        /// This overload shipped in v1 but does not take into account private members 
        /// prefixed with __'s.   Calling the GetExpressionFromText(string exprText, int lineNumber)
        /// overload will take into account the current class and therefore will
        /// work properly with name mangled private members.  
        /// </summary>
        public Expression GetExpressionFromText(string exprText) {
            return Statement.GetExpression(GetAstFromText(exprText).Block);
        }

        /// <summary>
        /// Gets the AST for the given text as if it appeared at the specified line number.
        /// 
        /// If the expression is a member expression such as "fob.__bar" and the line number is
        /// inside of a class definition this will return a MemberExpression with the mangled name
        /// like "fob.__ClassName_Bar".
        /// 
        /// index is a 0-based absolute index into the file.
        /// 
        /// New in 1.1.
        /// </summary>
        public JsAst GetAstFromTextByIndex(string exprText, int index) {
            var scopes = FindEnvironment(index);
            return GetAstFromText(exprText);
        }

        public string ModuleName {
            get {
                return _scope.GlobalEnvironment.Name;
            }
        }

        private JsAst GetAstFromText(string exprText) {
            return new JSParser(exprText).Parse(new CodeSettings());
        }

        internal static Expression GetExpression(Statement statement) {
            if (statement is ExpressionStatement) {
                return ((ExpressionStatement)statement).Expression;
            } else if (statement is ReturnNode) {
                return ((ReturnNode)statement).Operand;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Gets the chain of environments which are associated with the given position in the code.
        /// </summary>
        private EnvironmentRecord FindEnvironment(int index) {
            EnvironmentRecord curEnv = Environment;
            EnvironmentRecord prevEnv = null;
            var parent = _unit.Tree;

            while (curEnv != prevEnv) {
                prevEnv = curEnv;

                // TODO: Binary search?
                // We currently search backwards because the end positions are sometimes unreliable
                // and go onto the next line overlapping w/ the previous definition.  Therefore searching backwards always 
                // hits the valid method first matching on Start.  For example:
                // def f():  # Starts on 1, ends on 3
                //     pass
                // def g():  # starts on 3, ends on 4
                //     pass
                int lastStart = curEnv.GetStart(parent) - 1;

                if (curEnv.HasChildren) {
                    for (int i = curEnv.Children.Count - 1; i >= 0; i--) {
                        var env = curEnv.Children[i];
                        var curStart = env.GetBodyStart(parent);


                        if (curStart < index) {
                            var curEnd = env.GetStop(parent);

                            if (curEnd >= index) {
                                curEnv = env;
                                break;
                            }
                        } else if (env is FunctionEnvironmentRecord) {
                            var initialStart = env.GetStart(parent);
                            if (initialStart < curStart) {
                                // we could be on a parameter or we could be on a default value.
                                // If we're on a parameter then we're logically in the function
                                // scope.  If we're on a default value then we're in the outer
                                // scope.
                                var funcDef = (FunctionObject)((FunctionEnvironmentRecord)env).Node;

                                if (funcDef.ParameterDeclarations != null) {
                                    bool isParam = false;
                                    foreach (var param in funcDef.ParameterDeclarations) {
                                        string paramName = /*param.GetVerbatimImage(_unit.Tree) ??*/ param.Name;
                                        var nameStart = param.GetStartIndex(_unit.Tree.LocationResolver);

                                        if (index >= nameStart && index <= (nameStart + paramName.Length)) {
                                            curEnv = env;
                                            isParam = true;
                                            break;
                                        }

                                    }

                                    if (isParam) {
                                        break;
                                    }
                                }

                            }
                        }

                        lastStart = env.GetStart(parent);
                    }
                }
            }
            return curEnv;
        }

        private static IEnumerable<MemberResult> MemberDictToResultList(GetMemberOptions options, Dictionary<string, List<AnalysisValue>> memberDict,
            Dictionary<string, List<AnalysisValue>> ownerDict = null, int maximumOwners = 0) {
            foreach (var kvp in memberDict) {
                string name = GetMemberName(options, kvp.Key);
                string completion = name;
                if (name != null) {
                    List<AnalysisValue> owners;
                    if (ownerDict != null && ownerDict.TryGetValue(kvp.Key, out owners) &&
                        owners.Count >= 1 && owners.Count < maximumOwners) {
                        // This member came from less than the full set of types.
                        var seenNames = new HashSet<string>();
                        var newName = new StringBuilder(name);
                        bool appended = false;
                        foreach (var v in owners) {
                            if (!string.IsNullOrWhiteSpace(v.OwnerName) && seenNames.Add(v.OwnerName)) {
                                if (!appended) {
                                    newName.Append(" (");
                                    appended = true;
                                }
                                // Restrict each displayed type to 25 characters
                                if (v.OwnerName.Length > 25) {
                                    newName.Append(v.OwnerName.Substring(0, 22));
                                    newName.Append("...");
                                } else {
                                    newName.Append(v.OwnerName);
                                }
                                newName.Append(", ");
                            }
                            if (newName.Length > 200)
                                break;
                        }
                        // Restrict the entire completion string to 200 characters
                        if (newName.Length > 200) {
                            newName.Length = 197;
                            // Avoid showing more than three '.'s in a row
                            while (newName[newName.Length - 1] == '.') {
                                newName.Length -= 1;
                            }
                            newName.Append("...");
                        } else if (appended) {
                            newName.Length -= 2;
                        }
                        if (appended) {
                            newName.Append(")");
                        }
                        name = newName.ToString();
                    }
                    yield return new MemberResult(name, completion, kvp.Value, null);
                }
            }
        }

        private static IEnumerable<MemberResult> SingleMemberResult(GetMemberOptions options, IDictionary<string, IAnalysisSet> memberDict) {
            foreach (var kvp in memberDict) {
                string name = GetMemberName(options, kvp.Key);
                if (name != null) {
                    yield return new MemberResult(name, kvp.Value.Select(x => x.Value));
                }
            }
        }

        private static string GetMemberName(GetMemberOptions options, string name) {
            if (!_otherPrivateRegex.IsMatch(name) || !options.HideAdvanced()) {
                return name;
            }
            return null;
        }

#if FALSE
        private int LineToIndex(int line) {
            if (line <= 1) {    // <= because v1 allowed zero even though we take 1 based lines.
                return 0;
            }

            // line is 1 based, and index 0 in the array is the position of the 2nd line in the file.
            line -= 2;
            return _unit.Tree._lineLocations[line];
        }
#endif

        /// <summary>
        /// Finds the best available analysis unit for lookup. This will be the one that is provided
        /// by the nearest enclosing scope that is capable of providing one.
        /// </summary>
        private AnalysisUnit GetEvalAnalysisUnit(EnvironmentRecord scope) {
            var projEntry = scope.GlobalEnvironment.ProjectEntry;

            return new EvalAnalysisUnit(
                projEntry.Tree,
                projEntry.Tree,
                scope
            );
        }

        internal IEnumerable<MemberResult> GetModules() {
            //TODO - Handle require('../a/b/c.js'
            
            List<MemberResult> res = new List<MemberResult>();

            //Add the builtIns
            foreach (var m in ProjectState.Modules.BuiltinModules) {
                res.Add(new MemberResult(
                    m.ModuleName,
                    m.Module.Documentation,
                    JsMemberType.Module));
            }

            ModuleTree moduleTree = ProjectState.Modules.GetModuleTree(ModuleName);
            if (moduleTree == null || moduleTree.Parent == null) {
                return res;
            }

            ModuleTree parentMT = moduleTree.Parent;
            //Walk up the tree looking for node_modules
            do {
                ModuleTree nodeModules;
                if (parentMT.Children.TryGetValue(AnalysisConstants.NodeModulesFolder, out nodeModules)) {
                    foreach (var child in nodeModules.GetChildrenExcludingNodeModules()) {
                        var module = ModuleTable.ResolveModule(child.Parent, child.Name);
                        if (module != null) {
                            res.Add(MakeProjectMemberResult(module.Name));
                        }
                    }
                }
                parentMT = parentMT.Parent;
            } while (parentMT != null);

            foreach (var sibling in moduleTree.Parent.GetChildrenExcludingNodeModules()) {
                if (sibling == moduleTree) {
                    //Don't let us require ourself
                    continue;
                }
                if (sibling.ProjectEntry != null) {
                    res.Add(MakeProjectMemberResult("./" + sibling.Name));
                } else {
                    //We're looking at a folder include the children
                    GetChildModules(res, sibling, "./" + sibling.Name + "/");
                }
            }
            return res;
        }

        private MemberResult MakeProjectMemberResult(string module) {
            return new MemberResult(
                module,
                module + " (in project)",
                JsMemberType.Module);
        }

        private void GetChildModules(List<MemberResult> res, ModuleTree moduleTree, string projectRelative) {
            foreach (var child in moduleTree.GetChildrenExcludingNodeModules()) {
                Debug.Assert(child.Name != AnalysisConstants.NodeModulesFolder);
                if (child.ProjectEntry == null) {
                    GetChildModules(res, child, projectRelative + child.Name + "/");
                } else {
                    res.Add(MakeProjectMemberResult(projectRelative + child.Name));
                }
            }
        }
    }

    class VariableTransformer {
        private readonly bool _definitionsAreReferences;
        public static VariableTransformer OtherToVariables = new VariableTransformer(false);
        public static VariableTransformer ScopeToVariables = new VariableTransformer(true);

        public VariableTransformer(bool definitionsAreReferences) {
            _definitionsAreReferences = definitionsAreReferences;
        }

        internal IEnumerable<AnalysisVariable> ToVariables(AnalysisUnit unit, IReferenceable referenceable) {
            LocatedVariableDef locatedDef = referenceable as LocatedVariableDef;

            if (locatedDef != null &&
                locatedDef.Entry.Tree != null &&    // null tree if there are errors in the file
                locatedDef.DeclaringVersion == locatedDef.Entry.AnalysisVersion) {
                var start = locatedDef.Node;

                yield return new AnalysisVariable(VariableType.Definition, locatedDef.Entry.Tree.ResolveLocation(locatedDef.Entry, start));
            }

            VariableDef def = referenceable as VariableDef;
            if (def != null) {
                foreach (var location in def.GetTypesNoCopy(unit).SelectMany(type => type.Value.Locations)) {
                    yield return new AnalysisVariable(VariableType.Value, location);
                }
            }

            UserFunctionValue func = referenceable as UserFunctionValue;
            if (func != null &&
                func.ProjectEntry.Tree != null &&
                func.DeclaringVersion == func.ProjectEntry.AnalysisVersion) {
                var start = func.FunctionObject;

                yield return new AnalysisVariable(VariableType.Definition, func.ProjectEntry.Tree.ResolveLocation(func.ProjectEntry, start));
            }

            foreach (var reference in referenceable.Definitions) {
                yield return new AnalysisVariable(
                    _definitionsAreReferences ? VariableType.Reference : VariableType.Definition,
                    reference.Value.GetLocationInfo(reference.Key)
                );
            }

            foreach (var reference in referenceable.References) {
                yield return new AnalysisVariable(VariableType.Reference, reference.Value.GetLocationInfo(reference.Key));
            }
        }
    }

}

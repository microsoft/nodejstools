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
    public sealed class ModuleAnalysis {
        private readonly AnalysisUnit _unit;
        private readonly EnvironmentRecord _scope;
        private readonly IAnalysisCookie _cookie;
        private static Regex _otherPrivateRegex = new Regex("^_[a-zA-Z_]\\w*__[a-zA-Z_]\\w*$");
        private static HashSet<string> _exprKeywords = new HashSet<string>() { 
            "function", "null", "true", "false", "this", "void", 
            "typeof", "delete", "in", "new" };
        private static HashSet<string> _stmtKeywords = new HashSet<string>() { 
            "debugger", "var", "if", "for", "do", "while", "continue", "break",
            "return", "with", "switch", "throw", "try", "else",
        };

        internal ModuleAnalysis(AnalysisUnit unit, EnvironmentRecord scope, IAnalysisCookie cookie) {
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
            var scope = FindScope(index);
            var expr = Statement.GetExpression(GetAstFromText(exprText).Block);

            var unit = GetNearestEnclosingAnalysisUnit(scope);
            var eval = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true);

            var values = eval.Evaluate(expr);
            var res = AnalysisSet.EmptyUnion;
            foreach (var v in values) {
                if (v.IsCurrent) {
                    res = res.Add(v);
                }
            }
            return res;
        }

        internal IEnumerable<AnalysisVariable> ReferencablesToVariables(IEnumerable<IReferenceable> defs) {
            foreach (var def in defs) {
                foreach (var res in ToVariables(def)) {
                    yield return res;
                }
            }
        }

        internal IEnumerable<AnalysisVariable> ToVariables(IReferenceable referenceable) {
            LocatedVariableDef locatedDef = referenceable as LocatedVariableDef;

            if (locatedDef != null &&
                locatedDef.Entry.Tree != null &&    // null tree if there are errors in the file
                locatedDef.DeclaringVersion == locatedDef.Entry.AnalysisVersion) {
                var start = locatedDef.Context;
                yield return new AnalysisVariable(VariableType.Definition, new LocationInfo(locatedDef.Entry, start.StartLineNumber, start.StartColumn));
            }

            VariableDef def = referenceable as VariableDef;
            if (def != null) {
                foreach (var location in def.TypesNoCopy.SelectMany(type => type.Locations)) {
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
            var scope = FindScope(index);
            var expr = Statement.GetExpression(GetAstFromText(exprText).Block);

            var unit = GetNearestEnclosingAnalysisUnit(scope);
            Lookup name = expr as Lookup;
            if (name != null) {
                var defScope = scope.EnumerateTowardsGlobal.FirstOrDefault(s =>
                    s.Variables.ContainsKey(name.Name));

                return GetVariablesInScope(name, defScope).Distinct();
            }

            var member = expr as Member;
            if (member != null) {
                var eval = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true);
                var objects = eval.Evaluate(member.Root);

                foreach (var v in objects) {
                    var container = v as IReferenceableContainer;
                    if (container != null) {
                        return ReferencablesToVariables(container.GetDefinitions(member.Name));
                    }
                }
            }

            return Enumerable.Empty<IAnalysisVariable>();
        }

        private IEnumerable<IAnalysisVariable> GetVariablesInScope(Lookup name, EnvironmentRecord scope) {
            var result = new List<IAnalysisVariable>();

            result.AddRange(scope.GetMergedVariables(name.Name).SelectMany(ToVariables));

            // if a variable is imported from another module then also yield the defs/refs for the 
            // value in the defining module.
            var linked = scope.GetLinkedVariablesNoCreate(name.Name);
            if (linked != null) {
                result.AddRange(linked.SelectMany(ToVariables));
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

        /// <summary>
        /// Evaluates a given expression and returns a list of members which exist in the expression.
        /// 
        /// If the expression is an empty string returns all available members at that location.
        /// 
        /// index is a zero-based absolute index into the file.
        /// </summary>
        public IEnumerable<MemberResult> GetMembersByIndex(string exprText, int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults) {
            if (exprText.Length == 0) {
                return GetAllAvailableMembersByIndex(index, options);
            }

            var scope = FindScope(index);

            var expr = Statement.GetExpression(GetAstFromText(exprText).Block);
            if (expr == null) {
                return new MemberResult[0];
            }
            if (expr is ConstantWrapper && ((ConstantWrapper)expr).Value is int)
            {
                // no completions on integer ., the user is typing a float
                return new MemberResult[0];
            }

            var unit = GetNearestEnclosingAnalysisUnit(scope);
            var lookup = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true).Evaluate(expr);
            return GetMemberResults(lookup, scope, options);
        }

        /// <summary>
        /// Gets information about the available signatures for the given expression.
        /// </summary>
        /// <param name="exprText">The expression to get signatures for.</param>
        /// <param name="index">The 0-based absolute index into the file.</param>
        public IEnumerable<IOverloadResult> GetSignaturesByIndex(string exprText, int index) {
            try {
                var scope = FindScope(index);
                var unit = GetNearestEnclosingAnalysisUnit(scope);
                var eval = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true);

                var expr = Statement.GetExpression(GetAstFromText(exprText).Block);
                if (expr == null ||
                    expr is ArrayLiteral) {
                    return Enumerable.Empty<IOverloadResult>();
                }
                var lookup = eval.Evaluate(expr);

                var result = new HashSet<OverloadResult>(OverloadResultComparer.Instance);

                foreach (var ns in lookup) {
                    if (ns.Overloads != null) {
                        result.UnionWith(ns.Overloads);
                    }
                }

                return result;
            } catch (Exception) {
                // TODO: log exception
                return new[] { new SimpleOverloadResult(new ParameterResult[0], "Unknown", "IntellisenseError_Sigs") };
            }
        }

        /// <summary>
        /// Gets the hierarchy of class and function definitions at the specified index.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file.</param>
        public IEnumerable<MemberResult> GetDefinitionTreeByIndex(int index) {
            try {
                var result = new List<MemberResult>();

                foreach (var scope in FindScope(index).EnumerateTowardsGlobal) {
                    result.Add(new MemberResult(scope.Name, scope.GetMergedAnalysisValues()));
                }

                return result;
            } catch (Exception) {
                // TODO: log exception
                return new[] { new MemberResult("Unknown", null) };
            }
        }

        /// <summary>
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults) {
            var result = new Dictionary<string, List<AnalysisValue>>();

            // collect variables from user defined scopes
            var scope = FindScope(index);
            foreach (var s in scope.EnumerateTowardsGlobal) {
                foreach (var kvp in s.GetAllMergedVariables()) {
                    result[kvp.Key] = new List<AnalysisValue>(kvp.Value.TypesNoCopy);
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
            } else  {
                keywords = Enumerable.Empty<string>();
            }

            if (options.StatementKeywords()) {
                keywords = keywords.Union(_stmtKeywords);
            }

            return keywords.Select(kw => new MemberResult(kw, JsMemberType.Keyword));
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
            var chain = FindScope(index);
            foreach (var scope in chain.EnumerateFromGlobal) {
                result = result.Concat(scope.GetAllMergedVariables().Select(val => val.Key));
            }
            return result.Distinct();
        }

        /// <summary>
        /// Gets the top-level scope for the module.
        /// </summary>
        internal ModuleValue ModuleValue {
            get {
                var result = (ModuleScope)Scope;
                return result.Module;
            }
        }

        public JsAnalyzer ProjectState {
            get { return ModuleValue.ProjectEntry.Analyzer; }
        }

        internal EnvironmentRecord Scope {
            get { return _scope; }
        }

        internal IEnumerable<MemberResult> GetMemberResults(IEnumerable<AnalysisValue> vars, EnvironmentRecord scope, GetMemberOptions options) {
            IList<AnalysisValue> namespaces = new List<AnalysisValue>();
            foreach (var ns in vars) {
                if (ns != null) {
                    namespaces.Add(ns);
                }
            }

            if (namespaces.Count == 1) {
                // optimize for the common case of only a single namespace
                var newMembers = namespaces[0].GetAllMembers();
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

                var newMembers = ns.GetAllMembers();
                // IntersectMembers(members, memberSet, memberDict);
                if (newMembers == null || newMembers.Count == 0) {
                    continue;
                }

                if (memberSet == null) {
                    // first namespace, add everything
                    memberSet = new HashSet<string>(newMembers.Keys);
                    memberDict = new Dictionary<string, List<AnalysisValue>>();
                    ownerDict = new Dictionary<string, List<AnalysisValue>>();
                    foreach (var kvp in newMembers) {
                        var tmp = new List<AnalysisValue>(kvp.Value);
                        memberDict[kvp.Key] = tmp;
                        ownerDict[kvp.Key] = new List<AnalysisValue> { ns };
                    }
                } else {
                    // 2nd or nth namespace, union or intersect
                    HashSet<string> toRemove;
                    IEnumerable<string> adding;

                    if (options.Intersect()) {
                        adding = new HashSet<string>(newMembers.Keys);
                        // Find the things only in memberSet that we need to remove from memberDict
                        // toRemove = (memberSet ^ adding) & memberSet

                        toRemove = new HashSet<string>(memberSet);
                        toRemove.SymmetricExceptWith(adding);
                        toRemove.IntersectWith(memberSet);

                        // intersect memberSet with what we're adding
                        memberSet.IntersectWith(adding);

                        // we're only adding things they both had
                        adding = memberSet;
                    } else {
                        // we're adding all of newMembers keys
                        adding = newMembers.Keys;
                        toRemove = null;
                    }

                    // update memberDict
                    foreach (var name in adding) {
                        List<AnalysisValue> values;
                        if (!memberDict.TryGetValue(name, out values)) {
                            memberDict[name] = values = new List<AnalysisValue>();
                        }
                        values.AddRange(newMembers[name]);
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
            if (options.Intersect()) {
                // No need for this information if we're only showing the
                // intersection. Setting it to null saves lookups later.
                ownerDict = null;
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
            var scopes = FindScope(index);
            return GetAstFromText(exprText);
        }

        public string ModuleName {
            get {
                return _scope.GlobalScope.Name;
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
        /// Gets the chain of scopes which are associated with the given position in the code.
        /// </summary>
        private EnvironmentRecord FindScope(int index, bool useIndent = false) {
            EnvironmentRecord curScope = Scope;
            EnvironmentRecord prevScope = null;
            var parent = _unit.Tree;

            while (curScope != prevScope) {
                prevScope = curScope;

                // TODO: Binary search?
                // We currently search backwards because the end positions are sometimes unreliable
                // and go onto the next line overlapping w/ the previous definition.  Therefore searching backwards always 
                // hits the valid method first matching on Start.  For example:
                // def f():  # Starts on 1, ends on 3
                //     pass
                // def g():  # starts on 3, ends on 4
                //     pass
                int lastStart = curScope.GetStart(parent) - 1;

                for (int i = curScope.Children.Count - 1; i >= 0; i--) {
                    var scope = curScope.Children[i];
                    var curStart = scope.GetBodyStart(parent);


                    if (curStart < index) {
                        var curEnd = scope.GetStop(parent);

                        bool inScope = curEnd >= index;
                        if (!inScope || useIndent) {
                            // Artificially extend the scope up to the start of the following
                            // child, but only if index is at an indentation that is not as
                            // deep as the current child.
                            int scopeIndent = int.MaxValue/*, indexIndent*/;
                            if (scope.Node != null) {
                                scopeIndent = scope.Node.GetStart(parent).Column;
                            }
#if FALSE
                            if (index <= parent.EndIndex) {
                                indexIndent = parent.IndexToLocation(index).Column;
                            } else {
                                // This implicitly includes one character for a newline.
                                // When using CRLF, this may place the indent at one
                                // character less than it should be, but this will only
                                // affect scope resolution if the user is indenting by
                                // one space instead of four.
                                indexIndent = index - parent.EndIndex;
                            }
#endif
                            inScope = (inScope || i == curScope.Children.Count - 1 || index < lastStart) /*&& indexIndent > scopeIndent*/;
                        }

                        if (inScope) {
                            if (!(scope is StatementScope)) {
                                curScope = scope;
                            }
                            break;
                        }
                    } else if (scope is Microsoft.NodejsTools.Analysis.Analyzer.FunctionScope) {
                        var initialStart = scope.GetStart(parent);
                        if (initialStart < curStart) {
                            // we could be on a parameter or we could be on a default value.
                            // If we're on a parameter then we're logically in the function
                            // scope.  If we're on a default value then we're in the outer
                            // scope.
                            var funcDef = (FunctionObject)((Microsoft.NodejsTools.Analysis.Analyzer.FunctionScope)scope).Node;

                            if (funcDef.ParameterDeclarations != null) {
                                bool isParam = false;
                                foreach (var param in funcDef.ParameterDeclarations)
                                {
                                    string paramName = /*param.GetVerbatimImage(_unit.Tree) ??*/ param.Name;
                                    var nameStart = param.IndexSpan.Start;

                                    if (index >= nameStart && index <= (nameStart + paramName.Length)) {
                                        curScope = scope;
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

                    lastStart = scope.GetStart(parent);
                }
            }
            return curScope;
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
                        newName.Append(" (");
                        foreach (var v in owners) {
                            if (!string.IsNullOrWhiteSpace(v.ShortDescription) && seenNames.Add(v.ShortDescription)) {
                                // Restrict each displayed type to 25 characters
                                if (v.ShortDescription.Length > 25) {
                                    newName.Append(v.ShortDescription.Substring(0, 22));
                                    newName.Append("...");
                                } else {
                                    newName.Append(v.ShortDescription);
                                }
                                newName.Append(", ");
                            }
                            if (newName.Length > 200) break;
                        }
                        // Restrict the entire completion string to 200 characters
                        if (newName.Length > 200) {
                            newName.Length = 197;
                            // Avoid showing more than three '.'s in a row
                            while (newName[newName.Length - 1] == '.') {
                                newName.Length -= 1;
                            }
                            newName.Append("...");
                        } else {
                            newName.Length -= 2;
                        }
                        newName.Append(")");
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
                    yield return new MemberResult(name, kvp.Value);
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
        private AnalysisUnit GetNearestEnclosingAnalysisUnit(EnvironmentRecord scopes) {
            var units = from scope in scopes.EnumerateTowardsGlobal
                        let ns = scope.AnalysisValue
                        where ns != null
                        let unit = ns.AnalysisUnit
                        where unit != null
                        select unit;
            return units.FirstOrDefault() ?? _unit;
        }
    }
}

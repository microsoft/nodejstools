using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {

    /// <summary>
    /// Provides analysis of a function called with a specific set of arguments.  We analyze each function
    /// with each unique set of arguments (the cartesian product of the arguments).
    /// 
    /// It's possible that we still need to perform the analysis multiple times which can occur 
    /// if we take a dependency on something which later gets updated.
    /// </summary>
    class CartesianProductFunctionAnalysisUnit : FunctionAnalysisUnit {
        private readonly UserFunctionValue.CallArgs _callArgs;
        private readonly VariableDef _this;
        private readonly VariableDef _returnValue;
        //private readonly VariableDef[] _newParams;
        private readonly CartesianLocalVariable[] _specializedLocals;

        public CartesianProductFunctionAnalysisUnit(UserFunctionValue funcInfo, EnvironmentRecord environment, AnalysisUnit outerUnit, UserFunctionValue.CallArgs callArgs, VariableDef returnValue)
            : base(funcInfo, outerUnit, environment.Parent, outerUnit.ProjectEntry, environment) {
            _callArgs = callArgs;
            _returnValue = returnValue;
            _this = new VariableDef();

            var funcScope = environment as FunctionEnvironmentRecord;

            var specLocals = new List<CartesianLocalVariable>();
            ProcessVariablesForScope(funcScope, specLocals);
            _specializedLocals = specLocals.ToArray();
        }

        private static void ProcessVariablesForScope(EnvironmentRecord scope, List<CartesianLocalVariable> specLocals) {
            foreach (var variable in scope.LocalVariables) {
                specLocals.Add(
                    new CartesianLocalVariable(
                        variable.Key,
                        scope,
                        CopyVariableDef(variable.Value),
                        variable.Value
                    )
                );
            }

            if (scope.HasChildren) {
                foreach (var childScope in scope.Children) {
                    if (!(childScope is DeclarativeEnvironmentRecord)) {
                        ProcessVariablesForScope(childScope, specLocals);
                    }
                }
            }
        }

        private static VariableDef CopyVariableDef(VariableDef original) {
            LocatedVariableDef locVarDef = original as LocatedVariableDef;
            if (locVarDef != null) {
                return new LocatedVariableDef(
                    locVarDef.Entry,
                    locVarDef.Node
                );
            }
            return new VariableDef();
        }

        /// <summary>
        /// Walks the AST and makes sure function definitions are properly initialized.
        /// </summary>
        class FunctionAssignmentWalker : AstVisitor {
            private readonly EnvironmentRecord _scope;
            private readonly ProjectEntry _projectEntry;

            public FunctionAssignmentWalker(EnvironmentRecord functionScope, ProjectEntry project) {
                _scope = functionScope;
                _projectEntry = project;
            }

            public override bool Walk(FunctionObject node) {
                IAnalysisSet value;
                EnvironmentRecord record;
                if (_scope.GlobalEnvironment.TryGetNodeEnvironment(node, out record) && 
                    _scope.GlobalEnvironment.TryGetNodeValue(NodeEnvironmentKind.UserFunctionValue, node, out value) &&
                    node.Name != null) {
                    if (node.IsExpression) {
                        // Only assign if the variable wasn't defined explicitly in the
                        // functions scope.
                        var varDef = record.GetVariable(node.Name);
                        if (varDef != null && varDef is LocatedVariableDef &&
                            ((LocatedVariableDef)varDef).Node == node) {
                            varDef.AddTypes(
                                _projectEntry,
                                value,
                                false
                            );
                        }
                    } else {
                        Debug.Assert(record.Parent.ContainsVariable(node.Name));
                        record.Parent.GetVariable(node.Name).AddTypes(
                            _projectEntry,
                            value,
                            false
                        );
                    }
                }

                return true;
            }
        }

        internal override void AnalyzeWorker(DDG ddg, CancellationToken cancel) {
            if (_env.AnalysisValue.DeclaringVersion != ProjectEntry.AnalysisVersion) {
                // we were enqueued and a new version became available, don't re-analyze against
                // the old version.
                return;
            }

            var args = _callArgs;
            var funcScope = (FunctionEnvironmentRecord)Environment;
            var function = Function;

            // Set the specialized versions of the locals
            foreach (var local in _specializedLocals) {
                local.DefiningScope.ReplaceVariable(local.Name, local.Specialized);
            }

            var originalThis = funcScope._this;
            funcScope._this = _this;
            if (_callArgs.This != null) {
                _this.AddTypes(this, _callArgs.This, false);
            }

            // Propagate the call types into the variables...
            if (Ast.ParameterDeclarations != null) {
                for (int i = 0; i < Ast.ParameterDeclarations.Count && i < args.Args.Length; i++) {
                    funcScope.GetVariable(Ast.ParameterDeclarations[i].Name).AddTypes(
                        this,
                        args.Args[i],
                        false
                    );
                }
            }

            var unifiedReturn = function.ReturnValue;
            function.ReturnValue = _returnValue;

            function.FunctionObject.Walk(new FunctionAssignmentWalker(funcScope, ProjectEntry));

            try {
                base.AnalyzeWorker(ddg, cancel);
            } finally {
                funcScope._this = originalThis;
                function.ReturnValue = unifiedReturn;
                function.ReturnValue.AddTypes(this, _returnValue.TypesNoCopy, false);

                // restore the locals, merging types back into the shared...
                foreach (var variable in _specializedLocals) {
                    var newVar = variable.Specialized;
                    var oldVar = variable.Shared;

                    newVar.CopyTo(oldVar);

                    variable.DefiningScope.ReplaceVariable(variable.Name, oldVar);
                }
            }
        }

        public override string ToString() {
            StringBuilder res = new StringBuilder(base.ToString());
            res.AppendLine();
            res.Append(_callArgs.ToString());
            return res.ToString();
        }

        /// <summary>
        /// A pair of variable defs - the old one, and the new one.
        /// </summary>
        struct CartesianLocalVariable {
            /// <summary>
            /// The specialized variable which is used for each individual analysis.
            /// </summary>
            public readonly VariableDef Specialized;
            /// <summary>
            /// The shared variable which has the merged locals from all of the analysis.
            /// </summary>
            public readonly VariableDef Shared;
            public readonly string Name;
            public readonly EnvironmentRecord DefiningScope;

            public CartesianLocalVariable(string name, EnvironmentRecord definingScope, VariableDef specialized, VariableDef shared) {
                Specialized = specialized;
                Shared = shared;
                DefiningScope = definingScope;
                Name = name;
            }
        }
    }
}

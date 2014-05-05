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
using System.Linq;
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
    class FunctionAnalysisUnit : AnalysisUnit {
        private readonly FunctionAnalysisUnit _originalUnit;
        public readonly UserFunctionValue Function;

        public readonly CallChain CallChain;
        private readonly AnalysisUnit _declUnit;

        internal FunctionAnalysisUnit(
            UserFunctionValue function,
            AnalysisUnit declUnit,
            EnvironmentRecord declScope,
            IJsProjectEntry declEntry
        )
            : base(function.FunctionObject, null) {
            _declUnit = declUnit;
            Function = function;

            var scope = new FunctionEnvironmentRecord(Function, Function.FunctionObject, declScope, declEntry);
            scope.EnsureParameters(this);
            _env = scope;

            AnalysisLog.NewUnit(this);
        }

        public FunctionAnalysisUnit(FunctionAnalysisUnit originalUnit, CallChain callChain, IAnalysisSet @this, ArgumentSet callArgs)
            : base(originalUnit.Ast, null) {
            _originalUnit = originalUnit;
            _declUnit = originalUnit._declUnit;
            Function = originalUnit.Function;

            CallChain = callChain;

            var scope = new FunctionEnvironmentRecord(
                Function,
                Ast,
                originalUnit.Environment.Parent,
                originalUnit.DeclaringModuleEnvironment.ProjectEntry
            );
            scope.UpdateParameters(this, @this, callArgs, false, originalUnit.Environment as FunctionEnvironmentRecord);
            _env = scope;

            var walker = new OverviewWalker(originalUnit.ProjectEntry, this);
            if (Ast.Body != null) {
                Ast.Body.Walk(walker);
            }

            AnalysisLog.NewUnit(this);
            Enqueue();
        }

        internal bool UpdateParameters(IAnalysisSet @this, ArgumentSet callArgs, bool enqueue = true) {
            var defScope = _originalUnit != null ? _originalUnit.Environment as FunctionEnvironmentRecord : null;
            return ((FunctionEnvironmentRecord)Environment).UpdateParameters(this, @this, callArgs, enqueue, defScope);
        }

        internal void AddNamedParameterReferences(AnalysisUnit caller, Lookup[] names) {
            ((FunctionEnvironmentRecord)Environment).AddParameterReferences(caller, names);
        }

        internal override ModuleEnvironmentRecord GetDeclaringModuleEnvironment() {
            return base.GetDeclaringModuleEnvironment() ?? _declUnit.DeclaringModuleEnvironment;
        }

        internal override void AnalyzeWorker(DDG ddg, CancellationToken cancel) {
            // Resolve default parameters and decorators in the outer scope but
            // continue to associate changes with this unit.
            ddg.Scope = _declUnit.Environment;

            // Set the scope to within the function
            ddg.Scope = Environment;

            Ast.Body.Walk(ddg);
        }


        public new FunctionObject Ast {
            get {
                return (FunctionObject)base.Ast;
            }
        }

        public VariableDef ReturnValue {
            get {
                return ((FunctionEnvironmentRecord)Environment).ReturnValue;
            }
        }

        public override string ToString() {
            return string.Format("{0}{1}({2})->{3}",
                base.ToString(),
                _originalUnit == null ? " def:" : "",
                string.Join(", ", Ast.ParameterDeclarations.Select(p => Environment.GetVariable(p.Name).TypesNoCopy.ToString())),
                ((FunctionEnvironmentRecord)Environment).ReturnValue.TypesNoCopy.ToString()
            );
        }
    }

}

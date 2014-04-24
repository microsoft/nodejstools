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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents a function which is not backed by a user defined function.
    /// 
    /// Calling the function returns no values.  The ReturningFunctionInfo 
    /// subclass can beused if a set of types can be specified for the 
    /// return value.
    /// </summary>
    internal class BuiltinFunctionInfo : FunctionValue {
        private readonly string _name;
        private readonly string[] _signature;
        private readonly string _documentation;

        public BuiltinFunctionInfo(ProjectEntry projectEntry,
            string name,
            string documentation = null,
            params string[] signature)
            : base(projectEntry) {
            _name = name;
            _documentation = documentation;
            _signature = signature;

            Add("length", projectEntry.Analyzer.GetConstant(1.0));
            Add("name", projectEntry.Analyzer.GetConstant(name));
            Add("arguments", projectEntry.Analyzer._nullInst);
            Add("caller", projectEntry.Analyzer._nullInst);
        }

        public override IEnumerable<OverloadResult> Overloads {
            get {
                return new[] {
                    new SimpleOverloadResult(
                        _signature.Select(x => new ParameterResult(x)).ToArray(),
                        _name,
                        _documentation
                    )
                };
            }
        }

        public override string Name {
            get {
                return _name;
            }
        }

        public override string Documentation {
            get {
                return String.Format("function {0} (built-in)", Name);
            }
        }
    }

    /// <summary>
    /// Represents a functoin not backed by user code which returns a known
    /// set of types.
    /// </summary>
    internal class ReturningFunctionInfo : BuiltinFunctionInfo {
        private readonly IAnalysisSet _retValue;

        public ReturningFunctionInfo(ProjectEntry projectEntry, string name, IAnalysisSet retValue, string documentation = null, params string[] signature)
            : base(projectEntry, name, documentation, signature) {
            _retValue = retValue;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return _retValue;
        }
    }

    internal class SpecializedFunctionInfo : BuiltinFunctionInfo {
        private readonly Func<SpecializedFunctionInfo, Node, AnalysisUnit, IAnalysisSet, IAnalysisSet[], IAnalysisSet> _func;

        public SpecializedFunctionInfo(ProjectEntry projectEntry, string name, Func<SpecializedFunctionInfo, Node, AnalysisUnit, IAnalysisSet, IAnalysisSet[], IAnalysisSet> func, string documentation = null, params string[] signature)
            : base(projectEntry, name, documentation, signature) {
            _func = func;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return _func(this, node, unit, @this, args);
        }
    }

    /// <summary>
    /// Helper class for building up built-in objects and functions.  Specifies
    /// the name and value so it can use C#'s initializer syntax which calls
    /// the Add method.
    /// </summary>
    struct MemberAddInfo {
        internal readonly string Name;
        internal readonly AnalysisValue Value;
        internal readonly bool IsProperty;

        public MemberAddInfo(string name, AnalysisValue value, bool isProperty = false) {
            Name = name;
            Value = value;
            IsProperty = isProperty;
        }
    }
}

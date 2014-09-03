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
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents a function which is not backed by a user defined function.
    /// 
    /// Calling the function returns no values.  The ReturningFunctionInfo 
    /// subclass can beused if a set of types can be specified for the 
    /// return value.
    /// </summary>
    [Serializable]
    internal class BuiltinFunctionValue : FunctionValue {
        private readonly string _name;
        private readonly OverloadResult[] _overloads;
        private readonly string _documentation;
        private readonly HashSet<string> _immutableMembers = new HashSet<string>();

        public BuiltinFunctionValue(ProjectEntry projectEntry,
            string name,
            string documentation = null,
            bool createPrototype = true,
            params ParameterResult[] signature)
            : this(projectEntry, name, new[] { new SimpleOverloadResult(name, documentation, signature) }, documentation, createPrototype) {
        }

        public BuiltinFunctionValue(ProjectEntry projectEntry,
            string name,
            OverloadResult[] overloads,
            string documentation = null,
            bool createPrototype = true)
            : base(projectEntry, createPrototype, name) {
            _name = name;
            _documentation = documentation;
            _overloads = overloads;

            Add("length", projectEntry.Analyzer.GetConstant(1.0).Proxy);
            Add("name", projectEntry.Analyzer.GetConstant(name).Proxy);
            Add("arguments", projectEntry.Analyzer._nullInst.Proxy);
            Add("caller", projectEntry.Analyzer._nullInst.Proxy);

            projectEntry.Analyzer.AnalysisValueCreated(typeof(BuiltinFunctionValue));
        }

        public override VariableDef Add(string name, IAnalysisSet value) {
            _immutableMembers.Add(name);
            return base.Add(name, value);
        }

        public override void AddProperty(MemberAddInfo member) {
            _immutableMembers.Add(member.Name);
            base.AddProperty(member);
        }

        public override bool IsMutable(string name) {
            return !_immutableMembers.Contains(name);
        }

        public override IEnumerable<OverloadResult> Overloads {
            get {
                return _overloads;
            }
        }

        public override string Name {
            get {
                return _name;
            }
        }

        public override string Description {
            get {
                return String.Format("built-in function {0}", Name);
            }
        }

        public override string Documentation {
            get {
                return _documentation;
            }
        }
    }

    /// <summary>
    /// Represents a function which is the constructor function for a
    /// Node.js class.  These functions can be called with or without
    /// new and always return their instance value.
    /// </summary>
    internal class ClassBuiltinFunctionValue : BuiltinFunctionValue {
        public ClassBuiltinFunctionValue(ProjectEntry projectEntry,
            string name,
            OverloadResult[] overloads,
            string documentation = null,
            bool createPrototype = true)
            : base(projectEntry, name, overloads, documentation, createPrototype) {
        }

        public override IAnalysisSet Construct(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            return _instance.Proxy;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return _instance.Proxy;
        }
    }

    /// <summary>
    /// Represents a function not backed by user code which returns a known
    /// set of types.
    /// </summary>
    [Serializable]
    internal class ReturningFunctionValue : BuiltinFunctionValue {
        private readonly IAnalysisSet _retValue;

        public ReturningFunctionValue(ProjectEntry projectEntry, string name, IAnalysisSet retValue, string documentation = null, bool createPrototype = true, params ParameterResult[] signature)
            : base(projectEntry, name, documentation, createPrototype, signature) {
            _retValue = retValue;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return _retValue;
        }

        public override IAnalysisSet ReturnTypes {
            get {
                return _retValue;
            }
        }
    }

    [Serializable]
    internal class ReturningConstructingFunctionValue : BuiltinFunctionValue {
        private readonly IAnalysisSet _retValue;

        public ReturningConstructingFunctionValue(ProjectEntry projectEntry, string name, IAnalysisSet retValue, string documentation = null, bool createPrototype = true, params ParameterResult[] signature)
            : base(projectEntry, name, documentation, createPrototype, signature) {
            _retValue = retValue;
        }

        public override IAnalysisSet Construct(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            return _retValue;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return _retValue;
        }
    }

    [Serializable]
    internal class SpecializedFunctionValue : BuiltinFunctionValue {
        private readonly CallDelegate _func;

        public SpecializedFunctionValue(ProjectEntry projectEntry, string name, CallDelegate func, string documentation = null, params ParameterResult[] signature)
            : base(projectEntry, name, documentation, true, signature) {
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
        internal readonly string Documentation;

        public MemberAddInfo(string name, AnalysisValue value, string documentation = null, bool isProperty = false) {
            Name = name;
            Value = value;
            IsProperty = isProperty;
            Documentation = documentation;
        }
    }

    delegate IAnalysisSet CallDelegate(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[]args);
}

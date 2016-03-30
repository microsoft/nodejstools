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
using Microsoft.NodejsTools.Parsing;
using Microsoft.NodejsTools.Analysis.Analyzer;


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
            ExpandoValue prototype = null,
            params ParameterResult[] signature)
            : this(projectEntry, name, new[] { new SimpleOverloadResult(name, documentation, signature) }, documentation, prototype) {
        }

        public BuiltinFunctionValue(ProjectEntry projectEntry,
            string name,
            OverloadResult[] overloads,
            string documentation = null,
            ExpandoValue prototype = null)
            : base(projectEntry, prototype, name) {
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
            string documentation = null)
            : base(projectEntry, name, overloads, documentation) {
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

        public ReturningFunctionValue(ProjectEntry projectEntry, string name, IAnalysisSet retValue, string documentation = null, params ParameterResult[] signature)
            : base(projectEntry, name, documentation, null, signature) {
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

        public ReturningConstructingFunctionValue(ProjectEntry projectEntry, string name, IAnalysisSet retValue, string documentation = null, params ParameterResult[] signature)
            : base(projectEntry, name, documentation, null, signature) {
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

        public SpecializedFunctionValue(ProjectEntry projectEntry, string name, CallDelegate func, string documentation = null, ExpandoValue prototype = null, params ParameterResult[] signature)
            : base(projectEntry, name, documentation, prototype, signature) {
            _func = func;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return _func(this, node, unit, @this, args);
        }
    }

    [Serializable]
    class CallbackArgInfo {
        public readonly string Module, Member;

        public CallbackArgInfo(string module, string member) {
            Module = module;
            Member = member;
        }
    }

    [Serializable]
    internal class LazyPropertyFunctionValue : BuiltinFunctionValue {
        private readonly string _module;
        private IAnalysisSet _retValue;

        public LazyPropertyFunctionValue(ProjectEntry projectEntry, string name, string module)
            : base(projectEntry, name, "", null) {
            _module = module;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (_retValue == null) {
                string moduleName = _module;
                string memberName = Name;

                _retValue = ResolveMember(node, unit, moduleName, memberName);
            }
            return _retValue ?? AnalysisSet.Empty;
        }

        internal static IAnalysisSet ResolveMember(Node node, AnalysisUnit unit, string moduleName, string memberName) {
            var module = unit.ProjectEntry.Analyzer.Modules.RequireModule(node, unit, moduleName);
            return module.Get(node, unit, memberName, false);
        }

        public override IAnalysisSet ReturnTypes {
            get {
                return _retValue ?? AnalysisSet.Empty;
            }
        }
    }


    /// <summary>
    /// Specialized function for functions which take a function for a callback
    /// such as http.createServer.
    /// 
    /// Performs a call back to the function so that the types are propagated.
    /// </summary>
    [Serializable]
    internal class CallbackReturningFunctionValue : ReturningFunctionValue {
        private readonly CallbackArgInfo[] _args;
        private readonly IAnalysisSet[] _types;
        private readonly int _index;

        public CallbackReturningFunctionValue(ProjectEntry projectEntry, string name, IAnalysisSet retValue, int callbackArg, CallbackArgInfo[] args, string documentation = null, params ParameterResult[] signature)
            : base(projectEntry, name, retValue, documentation, signature) {
            _index = callbackArg;
            _args = args;
            _types = new IAnalysisSet[_args.Length];
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (_index < args.Length) {
                IAnalysisSet[] callbackArgs = new IAnalysisSet[_args.Length];
                for (int i = 0; i < _args.Length; i++) {
                    if (_types[i] == null) {
                        _types[i] = LazyPropertyFunctionValue.ResolveMember(
                            node,
                            unit,
                            _args[i].Module,
                            _args[i].Member
                        ).Call(node, unit, null, ExpressionEvaluator.EmptySets);
                    }
                    callbackArgs[i] = _types[i];
                }
                args[_index].Call(node, unit, null, callbackArgs);
            }

            return base.Call(node, unit, @this, args);
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

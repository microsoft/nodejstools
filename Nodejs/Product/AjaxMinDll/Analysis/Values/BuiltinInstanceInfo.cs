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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
#if FALSE
    class BuiltinInstanceInfo : BuiltinNamespace<IPythonType>, IReferenceableContainer {
        private readonly BuiltinClassInfo _klass;

        public BuiltinInstanceInfo(BuiltinClassInfo klass)
            : base(klass._type, klass.ProjectState) {
            _klass = klass;
        }

        public BuiltinClassInfo ClassInfo {
            get {
                return _klass;
            }
        }

        public override IPythonType JsType {
            get { return _type; }
        }

        public override IAnalysisSet GetInstanceType() {
            if (_klass.TypeId == BuiltinTypeId.Type) {
                return ProjectState.ClassInfos[BuiltinTypeId.Object].Instance;
            }
            return base.GetInstanceType();
        }

        public override IEnumerable<OverloadResult> Overloads {
            get {
                // TODO: look for __call__ and return overloads
                return base.Overloads;
            }
        }

        public override string Description {
            get {
                return _klass._type.Name;
            }
        }

        public override string Documentation {
            get {
                return _klass.Documentation;
            }
        }

        public override PythonMemberType MemberType {
            get {
                switch (_klass.MemberType) {
                    case PythonMemberType.Enum: return PythonMemberType.EnumInstance;
                    case PythonMemberType.Delegate: return PythonMemberType.DelegateInstance;
                    default:
                        return PythonMemberType.Instance;
                }
            }
        }

        public override IAnalysisSet GetMember(Node node, AnalysisUnit unit, string name) {
            // Must unconditionally call the base implementation of GetMember
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _klass.AddMemberReference(node, unit, name);
                return res.GetDescriptor(node, this, _klass, unit);
            }
            return res;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _klass.AddMemberReference(node, unit, name);
            }
        }
#if FALSE
        public override IAnalysisSet BinaryOperation(Node node, AnalysisUnit unit, JSToken operation, IAnalysisSet rhs) {
            return ConstantInfo.NumericOp(node, this, unit, operation, rhs) ?? NumericOp(node, unit, operation, rhs) ?? AnalysisSet.Empty;
        }

        private IAnalysisSet NumericOp(Node node, AnalysisUnit unit, Parsing.JSToken operation, IAnalysisSet rhs) {
            string methodName = InstanceInfo.BinaryOpToString(operation);
            if (methodName != null) {
                var method = GetMember(node, unit, methodName);
                if (method.Count > 0) {
                    var res = method.Call(
                        node,
                        unit,
                        new[] { this, rhs }
                    );

                    if (res.IsObjectOrUnknown()) {
                        // the type defines the operator, assume it returns 
                        // some combination of the input types.
                        return SelfSet.Union(rhs);
                    }

                    return res;
                }
            }

            return base.BinaryOperation(node, unit, operation, rhs);
        }

        public override IAnalysisSet GetIndex(Node node, AnalysisUnit unit, IAnalysisSet index) {
            var getItem = GetMember(node, unit, "__getitem__");
            if (getItem.Count > 0) {
                var res = getItem.Call(node, unit, new[] { index });
                if (res.IsObjectOrUnknown() && index.Contains(SliceInfo.Instance)) {
                    // assume slicing returns a type of the same object...
                    return this;
                }
                return res;
            }
            return AnalysisSet.Empty;
        }
#endif
        internal override bool IsOfType(IAnalysisSet klass) {
            if (klass.Contains(this.ClassInfo)) {
                return true;
            }

            if (TypeId != BuiltinTypeId.Null &&
                TypeId != BuiltinTypeId.Type &&
                TypeId != BuiltinTypeId.Function &&
                TypeId != BuiltinTypeId.BuiltinFunction) {
                return klass.Contains(ProjectState.ClassInfos[BuiltinTypeId.Object]);
            }

            return false;
        }

        internal override BuiltinTypeId TypeId {
            get {
                return ClassInfo.JsType.TypeId;
            }
        }

        internal override bool UnionEquals(AnalysisValue ns, int strength) {
#if FALSE
            var dict = ProjectState.ClassInfos[BuiltinTypeId.Dict];
            if (strength < MergeStrength.IgnoreIterableNode && (this is DictionaryInfo || this == dict.Instance)) {
                if (ns is DictionaryInfo || ns == dict.Instance) {
                    return true;
                }
                var ci = ns as ConstantInfo;
                if (ci != null && ci.ClassInfo == dict) {
                    return true;
                }
                return false;
            }
            
            if (strength >= MergeStrength.ToObject) {
                if (TypeId == BuiltinTypeId.NullType || ns.TypeId == BuiltinTypeId.NullType) {
                    // BII + BII(None) => do not merge
                    // Unless both types are None, since they could be various
                    // combinations of BuiltinInstanceInfo or ConstantInfo that
                    // need to be merged.
                    return TypeId == BuiltinTypeId.NullType && ns.TypeId == BuiltinTypeId.NullType;
                }

                var func = ProjectState.ClassInfos[BuiltinTypeId.Function];
                if (this == func.Instance) {
                    // FI + BII(function) => BII(function)
                    return ns is FunctionInfo || ns is BuiltinFunctionInfo || ns == func.Instance;
                } else if (ns == func.Instance) {
                    return false;
                }

                var type = ProjectState.ClassInfos[BuiltinTypeId.Type];
                if (this == type.Instance) {
                    // CI + BII(type) => BII(type)
                    // BCI + BII(type) => BII(type)
                    return /*ns is ClassInfo || */ns is BuiltinClassInfo || ns == type.Instance;
                } else if (ns == type.Instance) {
                    return false;
                }

                /// BII + II => BII(object)
                /// BII + BII => BII(object)
                return ns is InstanceInfo || ns is BuiltinInstanceInfo;

            } else if (strength >= MergeStrength.ToBaseClass) {
                var bii = ns as BuiltinInstanceInfo;
                if (bii != null) {
                    return ClassInfo.UnionEquals(bii.ClassInfo, strength);
                }
                var ii = ns as InstanceInfo;
                if (ii != null) {
                    return ClassInfo.UnionEquals(ii.ClassInfo, strength);
                }
            } else if (this is ConstantInfo || ns is ConstantInfo) {
                // ConI + BII => BII if CIs match
                var bii = ns as BuiltinInstanceInfo;
                return bii != null && ClassInfo.Equals(bii.ClassInfo);
            }
#endif

                                                                               return base.UnionEquals(ns, strength);
        }

        internal override int UnionHashCode(int strength) {
            return ClassInfo.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue ns, int strength) {
            if (strength >= MergeStrength.ToObject) {
                if (TypeId == BuiltinTypeId.Null || ns.TypeId == BuiltinTypeId.Null) {
                    // BII + BII(None) => do not merge
                    // Unless both types are None, since they could be various
                    // combinations of BuiltinInstanceInfo or ConstantInfo that
                    // need to be merged.
                    return ProjectState.ClassInfos[BuiltinTypeId.Null].Instance;
                }

                var func = ProjectState.ClassInfos[BuiltinTypeId.Function];
                if (this == func.Instance) {
                    // FI + BII(function) => BII(function)
                    return func.Instance;
                }

                var type = ProjectState.ClassInfos[BuiltinTypeId.Type];
                if (this == type.Instance) {
                    // CI + BII(type) => BII(type)
                    // BCI + BII(type) => BII(type)
                    return type;
                }

                /// BII + II => BII(object)
                /// BII + BII => BII(object)
                return ProjectState.ClassInfos[BuiltinTypeId.Object].Instance;

            } else if (strength >= MergeStrength.ToBaseClass) {
                var bii = ns as BuiltinInstanceInfo;
                if (bii != null) {
                    return ClassInfo.UnionMergeTypes(bii.ClassInfo, strength).GetInstanceType().Single();
                }
#if FALSE
                var ii = ns as InstanceInfo;
                if (ii != null) {
                    return ClassInfo.UnionMergeTypes(ii.ClassInfo, strength).GetInstanceType().Single();
                }
            } else if (this is ConstantInfo || ns is ConstantInfo) {
                return ClassInfo.Instance;
#endif
            }

            return base.UnionMergeTypes(ns, strength);
        }

        #region IReferenceableContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            return _klass.GetDefinitions(name);
        }

        #endregion
    }
#endif
}

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
using System;


namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents an instance of a class implemented in Python
    /// </summary>
    internal class ObjectInfo : ExpandoValue {
        private readonly FunctionInfo _creator;

        public ObjectInfo(ProjectEntry projectEntry, FunctionInfo creator = null)
            : base(projectEntry) {
            _creator = creator;
        }

        public override Dictionary<string, IAnalysisSet> GetAllMembers() {
            // TODO: include prototype
            var res = base.GetAllMembers();
            if (_creator != null) {
                PropertyDescriptor prototype;
                if (_creator.InstanceAttributes.TryGetValue("prototype", out prototype) &&
                    prototype.Values != null) {
                    foreach (var value in prototype.Values.TypesNoCopy) {
                        foreach (var kvp in value.GetAllMembers()) {
                            MergeTypes(res, kvp.Key, kvp.Value);
                        }
                    }
                }
            }
            return res;
        }

        public override IAnalysisSet GetMember(Node node, AnalysisUnit unit, string name) {
            // Must unconditionally call the base implementation of GetMember
            var res = base.GetMember(node, unit, name);

            if (_creator != null) {
                var prototype = _creator.GetMember(node, unit, "prototype");
                res = res.Union(prototype.GetMember(node, unit, name));
            }

#if FALSE
            foreach (var b in _classInfo.Bases) {
                foreach (var ns in b) {
                    if (ns.Push()) {
                        try {
                            ClassInfo baseClass = ns as ClassInfo;
                            if (baseClass != null &&
                                baseClass.Instance._instanceAttrs != null &&
                                baseClass.Instance._instanceAttrs.TryGetValue(name, out def)) {
                                res = res.Union(def.TypesNoCopy);
                            }
                        } finally {
                            ns.Pop();
                        }
                    }
                }
            }
            if (res.Count == 0) {
                // and if that doesn't exist fall back to __getattr__
                var getAttr = _classInfo.GetMemberNoReferences(node, unit, "__getattr__");
                if (getAttr.Count > 0) {
                    foreach (var getAttrFunc in getAttr) {
                        // TODO: We should really do a get descriptor / call here
                        //FIXME: new string[0]
                        getattrRes = getattrRes.Union(getAttrFunc.Call(node, unit, new[] { SelfSet, _classInfo.AnalysisUnit.ProjectState.ClassInfos[BuiltinTypeId.Str].Instance.SelfSet }, ExpressionEvaluator.EmptyNames));
                    }
                }
                return getattrRes;
            }
#endif
            return res;
        }
#if FALSE
        public override IAnalysisSet GetDescriptor(Node node, AnalysisValue instance, AnalysisValue context, AnalysisUnit unit) {
            var getter = _classInfo.GetMemberNoReferences(node, unit, "__get__");
            if (getter.Count > 0) {
                var get = getter.GetDescriptor(node, this, _classInfo, unit);
                return get.Call(node, unit, new[] { instance, context }, ExpressionEvaluator.EmptyNames);
            }
            return SelfSet;
        }
#endif

#if FALSE
        public override IAnalysisSet BinaryOperation(Node node, AnalysisUnit unit, JSToken operation, IAnalysisSet rhs) {
            string op = BinaryOpToString(operation);

            if (op != null) {
                var invokeMem = GetMember(node, unit, op);
                if (invokeMem.Count > 0) {
                    // call __*__ method
                    return invokeMem.Call(node, unit, new[] { rhs });
                }
            }

            return base.BinaryOperation(node, unit, operation, rhs);
        }


        internal static string BinaryOpToString(JSToken operation) {
            string op = null;
            switch (operation) {
                case JSToken.Multiply: op = "__mul__"; break;
                case JSToken.Add: op = "__add__"; break;
                case JSToken.Subtract: op = "__sub__"; break;
                case JSToken.Xor: op = "__xor__"; break;
                case JSToken.BitwiseAnd: op = "__and__"; break;
                case JSToken.BitwiseOr: op = "__or__"; break;
                case JSToken.Divide: op = "__div__"; break;
                case JSToken.FloorDivide: op = "__floordiv__"; break;
                case JSToken.LeftShift: op = "__lshift__"; break;
                case JSToken.Mod: op = "__mod__"; break;
                case JSToken.Power: op = "__pow__"; break;
                case JSToken.RightShift: op = "__rshift__"; break;
                case JSToken.TrueDivide: op = "__truediv__"; break;
            }
            return op;
        }

        public override IAnalysisSet ReverseBinaryOperation(Node node, AnalysisUnit unit, JSToken operation, IAnalysisSet rhs) {
            string op = ReverseBinaryOpToString(operation);

            if (op != null) {
                var invokeMem = GetMember(node, unit, op);
                if (invokeMem.Count > 0) {
                    // call __r*__ method
                    return invokeMem.Call(node, unit, new[] { rhs });
                }
            }

            return base.ReverseBinaryOperation(node, unit, operation, rhs);
        }
#endif
#if FALSE
        private static string ReverseBinaryOpToString(JSToken operation) {
            string op = null;
            switch (operation) {
                case JSToken.Multiply: op = "__rmul__"; break;
                case JSToken.Add: op = "__radd__"; break;
                case JSToken.Subtract: op = "__rsub__"; break;
                case JSToken.Xor: op = "__rxor__"; break;
                case JSToken.BitwiseAnd: op = "__rand__"; break;
                case JSToken.BitwiseOr: op = "__ror__"; break;
                case JSToken.Divide: op = "__rdiv__"; break;
                case JSToken.FloorDivide: op = "__rfloordiv__"; break;
                case JSToken.LeftShift: op = "__rlshift__"; break;
                case JSToken.Mod: op = "__rmod__"; break;
                case JSToken.Power: op = "__rpow__"; break;
                case JSToken.RightShift: op = "__rrshift__"; break;
                case JSToken.TrueDivide: op = "__rtruediv__"; break;
            }
            return op;
        }
#endif

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Object;
            }
        }

#if FALSE
        public override string Description {
            get {
                return ClassInfo.ClassDefinition.Name + " instance";
            }
        }

        public override string Documentation {
            get {
                return ClassInfo.Documentation;
            }
        }
#endif

        public override PythonMemberType MemberType {
            get {
                return PythonMemberType.Instance;
            }
        }

#if FALSE
        public ClassInfo ClassInfo {
            get { return _classInfo; }
        }

        public override string ToString() {
            return ClassInfo.AnalysisUnit.FullName + " instance";
        }
#endif

        internal override bool UnionEquals(AnalysisValue ns, int strength) {
            if (strength >= MergeStrength.ToObject) {
                if (ns.TypeId == BuiltinTypeId.Null) {
                    // II + BII(None) => do not merge
                    return false;
                }

                // II + II => BII(object)
                // II + BII => BII(object)
#if FALSE
                var obj = ProjectState.ClassInfos[BuiltinTypeId.Object];
                return ns is InstanceInfo || 
                    //(ns is BuiltinInstanceInfo && ns.TypeId != BuiltinTypeId.Type && ns.TypeId != BuiltinTypeId.Function) ||
                    ns == obj.Instance;
#endif
#if FALSE
            } else if (strength >= MergeStrength.ToBaseClass) {
                var ii = ns as InstanceInfo;
                if (ii != null) {
                    return ii.ClassInfo.UnionEquals(ClassInfo, strength);
                }
                var bii = ns as BuiltinInstanceInfo;
                if (bii != null) {
                    return bii.ClassInfo.UnionEquals(ClassInfo, strength);
                }
#endif
            }

            return base.UnionEquals(ns, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToObject) {
                //return ProjectState.ClassInfos[BuiltinTypeId.Object].Instance.UnionHashCode(strength);
#if FALSE
            } else if (strength >= MergeStrength.ToBaseClass) {
                return ClassInfo.UnionHashCode(strength);
#endif
            }

            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue ns, int strength) {
            if (strength >= MergeStrength.ToObject) {
                // II + II => BII(object)
                // II + BII => BII(object)
                //return ProjectState.ClassInfos[BuiltinTypeId.Object].Instance;
#if FALSE
            } else if (strength >= MergeStrength.ToBaseClass) {
                var ii = ns as InstanceInfo;
                if (ii != null) {
                    return ii.ClassInfo.UnionMergeTypes(ClassInfo, strength).GetInstanceType().Single();
                }
                var bii = ns as BuiltinInstanceInfo;
                if (bii != null) {
                    return bii.ClassInfo.UnionMergeTypes(ClassInfo, strength).GetInstanceType().Single();
                }
#endif
            }

            return base.UnionMergeTypes(ns, strength);
        }
    }
}

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
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    internal class FunctionValue : ExpandoValue {
        private readonly ObjectValue _instance;
        private ReferenceDict _references;

        internal FunctionValue(ProjectEntry projectEntry, bool createPrototype = true, string name = null)
            : base(projectEntry) {
            _instance = new ObjectValue(ProjectEntry, this, description: "instance of " + Name ?? name);
            if (createPrototype) {
                string description = null;
#if DEBUG
                if (String.IsNullOrWhiteSpace(Name ?? name)) {
                    if (AnalysisUnit != null) {
                        var loc = Locations.First();
                        description = "prototype object of " + AnalysisUnit.FullName + " " + loc.FilePath + "(" + loc.Column + ")";
                    } else {
                        description = "prototype object of <unknown objects>";
                    }
                } else {
                    description = "prototype object of " + (Name ?? name);
                }
#endif
                var prototype = new ObjectValue(ProjectEntry, description: description);
                Add("prototype", prototype);
                prototype.Add("constructor", this);
            }
        }

        public IAnalysisSet NewThis {
            get {
                return _instance.SelfSet;
            }
        }

        public override IAnalysisSet Construct(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            var result = Call(node, unit, _instance, args);
            if (result.Count != 0) {
                // function returned a value, we want to return any values
                // which are typed to object.
                foreach (var resultValue in result) {
                    if (!resultValue.IsObject) {
                        // we need to do some filtering
                        var tmpRes = AnalysisSet.Empty;
                        foreach (var resultValue2 in result) {
                            if (resultValue2.IsObject) {
                                tmpRes = tmpRes.Add(resultValue2);
                            }
                        }
                        result = tmpRes;
                        break;
                    }
                }

                if (result.Count != 0) {
                    return result;
                }
            }
            // we didn't return a value or returned a non-object
            // value.  The result is our newly created instance object.
            return _instance;
        }

        public override Dictionary<string, IAnalysisSet> GetAllMembers() {
            var res = base.GetAllMembers();

            if (this != ProjectState._functionPrototype) {
                foreach (var keyValue in ProjectState._functionPrototype.GetAllMembers()) {
                    IAnalysisSet existing;
                    if (!res.TryGetValue(keyValue.Key, out existing)) {
                        res[keyValue.Key] = keyValue.Value;
                    } else {
                        res[keyValue.Key] = existing.Union(keyValue.Value);
                    }
                }
            }

            return res;
        }

        public override IAnalysisSet GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            // we won't recurse on prototype because we have a prototype
            // value, and it's correct.  Recursing on prototype results in
            // prototypes getting merged and the analysis bloating
            if (this != ProjectState._functionPrototype && name != "prototype") {
                res = res.Union(ProjectState._functionPrototype.GetMember(node, unit, name));
            }
            return res;
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Function;
            }
        }

        internal static string MakeParameterName(ParameterDeclaration curParam) {
            return curParam.Name;
        }

        internal override void AddReference(Node node, AnalysisUnit unit) {
            if (!unit.ForEval) {
                if (_references == null) {
                    _references = new ReferenceDict();
                }
                _references.GetReferences(unit.DeclaringModule.ProjectEntry).AddReference(new EncodedLocation(unit.Tree, node));
            }
        }

        internal override IEnumerable<LocationInfo> References {
            get {
                if (_references != null) {
                    return _references.AllReferences;
                }
                return new LocationInfo[0];
            }
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Function;
            }
        }
        
        internal override bool UnionEquals(AnalysisValue av, int strength) {
#if FALSE
            if (strength >= MergeStrength.ToObject) {
                return av is FunctionValue /*|| av is BuiltinFunctionInfo || av == ProjectState.ClassInfos[BuiltinTypeId.Function].Instance*/;
            }
#endif
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
#if FALSE
            if (strength >= MergeStrength.ToObject) {
                return ProjectState._numberPrototype.GetHashCode();
            }
#endif
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
#if FALSE
            if (strength >= MergeStrength.ToObject) {
                return ProjectState.ClassInfos[BuiltinTypeId.Function].Instance;
            }
#endif

            return base.UnionMergeTypes(av, strength);
        }
    }
}

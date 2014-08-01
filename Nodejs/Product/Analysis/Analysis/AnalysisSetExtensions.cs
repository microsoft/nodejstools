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

using System.Diagnostics;
using System.Linq;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Provides operations which can be performed in bulk over a set of 
    /// analysis values, which results in a new analysis set.
    /// </summary>
    public static class AnalysisSetExtensions {
        /// <summary>
        /// Performs a GetMember operation for the given name and returns the
        /// types of variables which are associated with that name.
        /// </summary>
        public static IAnalysisSet Get(this IAnalysisSet self, Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var res = AnalysisSet.Empty;
            // name can be empty if we have "fob."
            if (name != null && name.Length > 0) {
                foreach (var ns in self) {
                    res = res.Union(ns.Value.Get(node, unit, name, addRef));
                }
            }
            return res;
        }

        /// <summary>
        /// Performs a SetMember operation for the given name and propagates the
        /// given values types for the provided member name.
        /// </summary>
        public static void SetMember(this IAnalysisSet self, Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            if (name != null && name.Length > 0) {
                foreach (var ns in self) {
                    ns.Value.SetMember(node, unit, name, value);
                }
            }
        }

        /// <summary>
        /// Performs a delete index operation propagating the index types into
        /// the provided object.
        /// </summary>
        public static void DeleteMember(this IAnalysisSet self, Node node, AnalysisUnit unit, string name) {
            if (name != null && name.Length > 0) {
                foreach (var ns in self) {
                    ns.Value.DeleteMember(node, unit, name);
                }
            }
        }

        /// <summary>
        /// Performs a call operation propagating the argument types into any
        /// user defined functions or classes and returns the set of types which
        /// result from the call.
        /// </summary>
        public static IAnalysisSet Call(this IAnalysisSet self, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            var res = AnalysisSet.Empty;
            foreach (var ns in self) {
                var call = ns.Value.Call(node, unit, @this, args);
                Debug.Assert(call != null);

                res = res.Union(call);
            }

            return res;
        }

        /// <summary>
        /// Performs a get index operation propagating any index types into the
        /// value and returns the associated types associated with the object.
        /// </summary>
        public static IAnalysisSet GetIndex(this IAnalysisSet self, Node node, AnalysisUnit unit, IAnalysisSet index) {
            var res = AnalysisSet.Empty;
            foreach (var ns in self) {
                res = res.Union(ns.Value.GetIndex(node, unit, index));
            }

            return res;
        }

        /// <summary>
        /// Performs a get index operation propagating any index types into the
        /// value and returns the associated types associated with the object.
        /// </summary>
        public static IAnalysisSet GetEnumerationValues(this IAnalysisSet self, Node node, AnalysisUnit unit) {
            var res = AnalysisSet.Empty;
            foreach (var ns in self) {
                res = res.Union(ns.Value.GetEnumerationValues(node, unit));
            }

            return res;
        }

        /// <summary>
        /// Performs a set index operation propagating the index types and value
        /// types into the provided object.
        /// </summary>
        public static void SetIndex(this IAnalysisSet self, Node node, AnalysisUnit unit, IAnalysisSet index, IAnalysisSet value) {
            foreach (var ns in self) {
                ns.Value.SetIndex(node, unit, index, value);
            }
        }

        /// <summary>
        /// Performs a delete index operation propagating the index types into
        /// the provided object.
        /// </summary>
        public static void DeleteIndex(this IAnalysisSet self, Node node, AnalysisUnit unit, IAnalysisSet index) {
        }

        /// <summary>
        /// Performs an augmented assignment propagating the value into the
        /// object.
        /// </summary>
        public static void AugmentAssign(this IAnalysisSet self, BinaryOperator node, AnalysisUnit unit, IAnalysisSet value) {
            foreach (var ns in self) {
                ns.Value.AugmentAssign(node, unit, value);
            }
        }

        /// <summary>
        /// Performs the specified operation on the value.
        /// </summary>
        public static IAnalysisSet UnaryOperation(this IAnalysisSet self, Node node, AnalysisUnit unit, JSToken operation) {
            var res = AnalysisSet.Empty;
            foreach (var ns in self) {
                res = res.Union(ns.Value.UnaryOperation(node, unit, operation));
            }

            return res;
        }

        internal static AnalysisProxy GetUnionType(this IAnalysisSet types) {
            var union = AnalysisSet.CreateUnion(types, UnionComparer.Instances[0]);
            AnalysisProxy type = null;
            if (union.Count == 2) {
                type = union.FirstOrDefault(t => t.Value.GetConstantValue() != null);
            }
            return type ?? union.FirstOrDefault();
        }

        /// <summary>
        /// Returns true if the set contains no or only the object type
        /// </summary>
        internal static bool IsObjectOrUnknown(this IAnalysisSet res) {
            return res.Count == 0 || (res.Count == 1 && res.First().Value.TypeId == BuiltinTypeId.Object);
        }

    }
}

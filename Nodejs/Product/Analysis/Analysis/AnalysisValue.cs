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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// An analysis value represents a set of variables and code.  Examples of 
    /// analysis values include top-level code, classes, and functions.
    /// </summary>
    [Serializable]
    public class AnalysisValue : IAnalysisSet {
        [ThreadStatic]
        private static HashSet<AnalysisValue> _processing;

        protected AnalysisValue() { }

        /// <summary>
        /// Returns an immutable set which contains just this AnalysisValue.
        /// 
        /// Currently implemented as returning the AnalysisValue object directly which implements ISet{AnalysisValue}.
        /// </summary>
        public IAnalysisSet SelfSet {
            get { return this; }
        }

        /// <summary>
        /// Gets the name of the value if it has one, or null if it's a non-named item.
        /// 
        /// The name property here is typically the same value you'd get by accessing __name__
        /// on the real JavaScript object.
        /// </summary>
        public virtual string Name {
            get {
                return null;
            }
        }

        /// <summary>
        /// Gets the documentation of the value.
        /// </summary>
        public virtual string Documentation {
            get {
                return null;
            }
        }

        /// <summary>
        /// Gets a list of locations where this value is defined.
        /// </summary>
        public virtual IEnumerable<LocationInfo> Locations {
            get { return LocationInfo.Empty; }
        }

        public virtual IEnumerable<OverloadResult> Overloads {
            get {
                return Enumerable.Empty<OverloadResult>();
            }
        }

        public virtual string Description {
            get { return null; }
        }

        public virtual string ShortDescription {
            get {
                return Description;
            }
        }

        /// <summary>
        /// Provides the owner descrpition in a member completion list when there are multiple owners.
        /// </summary>
        public virtual string OwnerName {
            get {
                return ShortDescription;
            }
        }

        /// <summary>
        /// Checks to see if the value is of type object.
        /// 
        /// This differs from the 'typeof' operator as the typeof
        /// operator also takes into account whether or not the
        /// object has the internal [[Call]] method when
        /// determing the result of typeof rather than basing
        /// the decision solely on the object type.
        /// </summary>
        public bool IsObject {
            get {
                return TypeId == BuiltinTypeId.Object ||
                    TypeId == BuiltinTypeId.Function;
            }
        }

        /// <summary>
        /// Returns the member type of the analysis value, or JsMemberType.Unknown if it's unknown.
        /// </summary>
        public virtual JsMemberType MemberType {
            get {
                return JsMemberType.Unknown;
            }
        }

        public virtual Dictionary<string, IAnalysisSet> GetAllMembers() {
            return new Dictionary<string, IAnalysisSet>();
        }
        
        /// <summary>
        /// Gets the constant value that this object represents, if it's a constant.
        /// 
        /// Returns Type.Missing if the value is not constant (because it returns null
        /// if the type is None).
        /// </summary>
        /// <returns></returns>
        public virtual object GetConstantValue() {
            return Type.Missing;
        }

        /// <summary>
        /// Returns the constant value as a string.  This returns a string if the constant
        /// value is either a unicode or ASCII string.
        /// </summary>
        public string GetConstantValueAsString() {
            var constName = GetConstantValue();
            if (constName != null) {
                string unicodeName = constName as string;
                if (unicodeName != null) {
                    return unicodeName;
                } 
            }
            return null;
        }

        public virtual IJsProjectEntry DeclaringModule {
            get {
                return null;
            }
        }

        public virtual int DeclaringVersion {
            get {
                return -1;
            }
        }

        public bool IsCurrent {
            get {
                return DeclaringModule == null || DeclaringVersion == DeclaringModule.AnalysisVersion;
            }
        }

        public virtual AnalysisUnit AnalysisUnit {
            get { return null; }
        }
        
        #region Dynamic Operations

        /// <summary>
        /// Attempts to call this object and returns the set of possible types it can return.
        /// 
        /// Implements the internal [[Call]] method.
        /// </summary>
        /// <param name="node">The node which is triggering the call, for reference tracking</param>
        /// <param name="unit">The analysis unit performing the analysis</param>
        /// <param name="args">The arguments being passed to the function</param>
        public virtual IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return AnalysisSet.Empty;
        }

        /// <summary>
        /// Implements the internal [[Construct]] method.
        /// </summary>
        /// </summary>
        /// <param name="node">The node which is triggering the call, for reference tracking</param>
        /// <param name="unit">The analysis unit performing the analysis</param>
        /// <param name="args">The arguments provided to construct the object.</param>
        public virtual IAnalysisSet Construct(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            return AnalysisSet.Empty;
        }

        /// <summary>
        /// Attempts to get a member from this object with the specified name.
        /// 
        /// Implements the internal [[Get]] method.
        /// </summary>
        /// <param name="node">The node which is triggering the call, for reference tracking</param>
        /// <param name="unit">The analysis unit performing the analysis</param>
        /// <param name="name">The name of the member.</param>
        public virtual IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return AnalysisSet.Empty;
        }

        public virtual void SetMember(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
        }

        public virtual void DeleteMember(Node node, AnalysisUnit unit, string name) {
        }

        public virtual void AugmentAssign(BinaryOperator node, AnalysisUnit unit, IAnalysisSet value) {
        }

        public virtual IAnalysisSet GetEnumerationValues(Node node, AnalysisUnit unit) {
            return AnalysisSet.Empty;
        }

        public virtual IAnalysisSet UnaryOperation(Node node, AnalysisUnit unit, JSToken operation) {
            return this.SelfSet;
        }

        public virtual IAnalysisSet GetIndex(Node node, AnalysisUnit unit, IAnalysisSet index) {
            return AnalysisSet.Empty;
            //throw new NotImplementedException("GetIndex");
            //return GetMember(node, unit, "__getitem__").Call(node, unit, new[] { index });
        }

        public virtual void SetIndex(Node node, AnalysisUnit unit, IAnalysisSet index, IAnalysisSet value) {
        }

        public virtual BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Unknown;
            }
        }

        #endregion

        #region Union Equality

        /// <summary>
        /// Returns an analysis value representative of both this and another
        /// analysis value. This should only be called when
        /// <see cref="UnionEquals"/> returns true for the two values.
        /// </summary>
        /// <param name="av">The value to merge with.</param>
        /// <param name="strength">A value matching that passed to
        /// <see cref="UnionEquals"/>.</param>
        /// <returns>A merged analysis value.</returns>
        /// <remarks>
        /// <para>Calling this function when <see cref="UnionEquals"/> returns
        /// false for the same parameters is undefined.</para>
        /// 
        /// <para>Where there is no analysis value representative of those
        /// provided, it is preferable to return this rather than
        /// <paramref name="av"/>.</para>
        /// 
        /// <para>
        /// <paramref name="strength"/> is used as a key in this function and must
        /// match the value used in <see cref="UnionEquals"/>.
        /// </para>
        /// </remarks>
        internal virtual AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            return this;
        }

        /// <summary>
        /// Determines whether two analysis values are effectively equivalent.
        /// </summary>
        /// <remarks>
        /// The intent of <paramref name="strength"/> is to allow different
        /// types to merge more aggressively. For example, string constants
        /// may merge into a non-specific string instance at a low strength,
        /// while distinct user-defined types may merge into <c>object</c> only
        /// at higher strengths. There is no defined maximum value.
        /// </remarks>
        internal virtual bool UnionEquals(AnalysisValue av, int strength) {
            return Equals(av);
        }

        /// <summary>
        /// Returns a hash code for this analysis value for the given strength.
        /// </summary>
        /// <remarks>
        /// <paramref name="strength"/> must match the value that will be
        /// passed to <see cref="UnionEquals"/> and
        /// <see cref="UnionMergeTypes"/> to ensure valid results.
        /// </remarks>
        internal virtual int UnionHashCode(int strength) {
            return GetHashCode();
        }

        #endregion

        #region Recursion Tracking

        /// <summary>
        /// Tracks whether or not we're currently processing this value to
        /// prevent stack overflows. Returns true if the the variable should be
        /// processed.
        /// </summary>
        /// <returns>
        /// True if the variable should be processed. False if it should be
        /// skipped.
        /// </returns>
        public bool Push() {
            if (_processing == null) {
                _processing = new HashSet<AnalysisValue>();
            }

            return _processing.Add(this);
        }

        public void Pop() {
            bool wasRemoved = _processing.Remove(this);
            Debug.Assert(wasRemoved, string.Format("Popped {0} but it wasn't pushed", GetType().FullName));
        }

        #endregion

        internal virtual void AddReference(Node node, AnalysisUnit analysisUnit) {
        }

        internal virtual IEnumerable<LocationInfo> References {
            get {
                yield break;
            }
        }

        public override string ToString() {
            return ShortDescription;
        }

        IAnalysisSet IAnalysisSet.Add(AnalysisValue item, bool canMutate) {
            if (((IAnalysisSet)this).Comparer.Equals(this, item)) {
                return this;
            }
            return new AnalysisSetDetails.AnalysisSetTwoObject(this, item);
        }

        IAnalysisSet IAnalysisSet.Add(AnalysisValue item, out bool wasChanged, bool canMutate) {
            if (((IAnalysisSet)this).Comparer.Equals(this, item)) {
                wasChanged = false;
                return this;
            }
            wasChanged = true;
            return new AnalysisSetDetails.AnalysisSetTwoObject(this, item);
        }

        IAnalysisSet IAnalysisSet.Union(IEnumerable<AnalysisValue> items, bool canMutate) {
            if (items.All(av => ((IAnalysisSet)this).Comparer.Equals(this, av))) {
                return this;
            }
            return AnalysisSet.Create(items).Add(this, false);
        }

        IAnalysisSet IAnalysisSet.Union(IEnumerable<AnalysisValue> items, out bool wasChanged, bool canMutate) {
            if (items.All(av => ((IAnalysisSet)this).Comparer.Equals(this, av))) {
                wasChanged = false;
                return this;
            }
            wasChanged = true;
            return AnalysisSet.Create(items).Add(this, false);
        }

        IAnalysisSet IAnalysisSet.Clone() {
            return this;
        }

        bool IAnalysisSet.Contains(AnalysisValue item) {
            return ((IAnalysisSet)this).Comparer.Equals(this, item);
        }

        bool IAnalysisSet.SetEquals(IAnalysisSet other) {
            if (other.Count != 1) {
                return false;
            }
            var av = other as AnalysisValue;
            if (av != null) {
                return ((IAnalysisSet)this).Comparer.Equals(this, av);
            }

            return ((IAnalysisSet)this).Comparer.Equals(this, other.First());
        }

        int IAnalysisSet.Count {
            get { return 1; }
        }

        IEqualityComparer<AnalysisValue> IAnalysisSet.Comparer {
            get { return ObjectComparer.Instance; }
        }

        IEnumerator<AnalysisValue> IEnumerable<AnalysisValue>.GetEnumerator() {
            yield return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((IEnumerable<AnalysisValue>)this).GetEnumerator();
        }
    }
}

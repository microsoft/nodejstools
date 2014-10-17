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
using Microsoft.NodejsTools.Analysis.AnalysisSetDetails;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Represents an unordered collection of <see cref="AnalysisProxy" /> objects;
    /// in effect, a set of JavaScript objects. There are multiple implementing
    /// classes, including <see cref="AnalysisProxy" /> itself, to improve memory
    /// usage for small sets.
    /// 
    /// <see cref="AnalysisSet" /> does not implement this interface, but
    /// provides factory and extension methods.
    /// </summary>
    internal interface IAnalysisSet : IEnumerable<AnalysisProxy> {
        IAnalysisSet Add(AnalysisProxy item, out bool wasChanged);
        IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged);
        IAnalysisSet Clone();

        bool Contains(AnalysisProxy item);

        int Count { get; }
        IEqualityComparer<AnalysisProxy> Comparer { get; }
    }    

    /// <summary>
    /// Marker interface to indicate that an analysis set is a read-only copy on write
    /// analysis set.
    /// </summary>
    internal interface IImmutableAnalysisSet : IAnalysisSet {
    }

    /// <summary>
    /// Provides factory and extension methods for objects implementing
    /// <see cref="IAnalysisSet" />.
    /// </summary>
    internal static class AnalysisSet {
        /// <summary>
        /// An empty set that does not combine types. This object is immutable
        /// and can be used without cloning.
        /// </summary>
        public static readonly IAnalysisSet Empty = Create();
        /// <summary>
        /// An empty set that combines types with a strength of zero. This
        /// object is immutable and can be used without cloning.
        /// </summary>
        public static readonly IAnalysisSet EmptyUnion = CreateUnion(UnionComparer.Instances[0]);

        #region Constructors

        /// <summary>
        /// Returns an empty set that does not combine types. This is exactly
        /// equivalent to accessing <see cref="Empty" />.
        /// </summary>
        public static IAnalysisSet Create() {
            return AnalysisSetDetails.AnalysisSetEmptyObject.Instance;
        }

        /// <summary>
        /// Returns a set containing only <paramref name="ns" />. This is
        /// exactly equivalent to casting <paramref name="ns" /> to <see
        /// cref="IAnalysisSet" />.
        /// </summary>
        /// <param name="ns">The namespace to contain in the set.</param>
        public static IAnalysisSet Create(AnalysisProxy ns) {
            return ns;
        }

        /// <summary>
        /// Returns a set containing all the types in <paramref name="ns" />.
        /// This is the usual way of creating a new set from any sequence.
        /// </summary>
        /// <param name="ns">The namespaces to contain in the set.</param>
        public static IAnalysisSet Create(IEnumerable<AnalysisProxy> ns) {
            var hs = ns as AnalysisHashSet;
            if (hs != null) {
                if (hs.Comparer == ObjectComparer.Instance) {
                    return hs.Clone();
                }
                return new AnalysisHashSet(hs);
            }

            if (ns is IImmutableAnalysisSet && ((IImmutableAnalysisSet)ns).Comparer == ObjectComparer.Instance) {
                return (IAnalysisSet)ns;
            }

            switch (ns.Count()) {
                case 0: return Empty;
                case 1: return ns.First();
                case 2: return new AnalysisSetDetails.AnalysisSetTwoObject(ns);
                default: 
                    // this should be unreachable...
                    return new AnalysisSetDetails.AnalysisHashSet(ns);
            }
        }

        /// <summary>
        /// Returns a set containing all the types in <paramref name="ns" />
        /// with the specified comparer. This function uses the type of
        /// <paramref name="comparer" /> to determine which factory method
        /// should be used.
        /// 
        /// If <paramref name="ns" /> is a set with the same comparer as
        /// <paramref name="comparer"/>, it may be returned without
        /// modification.
        /// </summary>
        /// <param name="ns">The namespaces to contain in the set.</param>
        /// <param name="comparer">The comparer to use for the set.</param>
        /// <exception name="InvalidOperationException"><paramref
        /// name="comparer" /> is not an instance of <see cref="ObjectComparer"
        /// /> or <see cref="UnionComparer" />.</exception>
        public static IAnalysisSet Create(IEnumerable<AnalysisProxy> ns, IEqualityComparer<AnalysisProxy> comparer) {
            var set = ns as IAnalysisSet;
            if (set == null) {
                UnionComparer uc;
                if (comparer is ObjectComparer) {
                    return ns == null ? Create() : Create(ns);
                } else if ((uc = comparer as UnionComparer) != null) {
                    return ns == null ? CreateUnion(uc) : CreateUnion(ns, uc);
                }
            } else if (comparer == set.Comparer) {
                return set;
            } else if (comparer != null && comparer.GetType() == set.Comparer.GetType()) {
                return set;
            } else if (comparer is ObjectComparer) {
                return Create(set);
            } else if (comparer is UnionComparer) {
                bool dummy;
                return set.AsUnion((UnionComparer)comparer, out dummy);
            }

            throw new InvalidOperationException(string.Format("cannot use {0} as a comparer", comparer));
        }

        /// <summary>
        /// Returns an empty set that uses a <see cref="UnionComparer" /> with
        /// the specified strength.
        /// </summary>
        /// <param name="strength">The strength to use for the comparer.
        /// </param>
        public static IAnalysisSet CreateUnion(int strength) {
            if (strength < 0) {
                strength = 0;
            } else if (strength > UnionComparer.MAX_STRENGTH) {
                strength = UnionComparer.MAX_STRENGTH;
            }
            return AnalysisSetDetails.AnalysisSetEmptyUnion.Instances[strength];
        }

        /// <summary>
        /// Returns an empty set that uses the specified <see
        /// cref="UnionComparer" />.
        /// </summary>
        /// <param name="comparer">The comparer to use for the set.</param>
        internal static IAnalysisSet CreateUnion(UnionComparer comparer) {
            return AnalysisSetDetails.AnalysisSetEmptyUnion.Instances[comparer.Strength];
        }

        /// <summary>
        /// Returns a set containing only <paramref name="ns" /> that uses the
        /// specified <see cref="UnionComparer" />.
        /// 
        /// This is different to casting from <see cref="AnalysisProxy" /> to <see
        /// cref="IAnalysisSet" />, because the implementation in <see
        /// cref="AnalysisProxy" /> always uses <see cref="ObjectComparer" />.
        /// </summary>
        /// <param name="ns">The namespace to contain in the set.</param>
        /// <param name="comparer">The comparer to use for the set.</param>
        internal static IAnalysisSet CreateUnion(AnalysisProxy ns, UnionComparer comparer) {
            return new AnalysisSetDetails.AnalysisSetOneUnion(ns, comparer);
        }

        /// <summary>
        /// Returns a set containing all the types in <paramref name="ns" />
        /// after merging using the specified <see cref="UnionComparer" />. For
        /// large sets, this operation may require significant time and memory.
        /// The returned set is always a copy of the original.
        /// </summary>
        /// <param name="ns">The namespaces to contain in the set.</param>
        /// <param name="comparer">The comparer to use for the set.</param>
        internal static IAnalysisSet CreateUnion(IEnumerable<AnalysisProxy> ns, UnionComparer comparer) {
            bool dummy;
            // TODO: Replace Trim() call with more intelligent enumeration.
            return new AnalysisSetDetails.AnalysisHashSet(ns.UnionIter(comparer, out dummy), comparer).Trim();
        }

        /// <summary>
        /// Returns a set containing all types in all the sets in <paramref
        /// name="sets" />.
        /// </summary>
        /// <param name="sets">The sets to contain in the set.</param>
        public static IAnalysisSet UnionAll(IEnumerable<IAnalysisSet> sets) {
            return Empty.UnionAll(sets);
        }

        /// <summary>
        /// Returns a set containing all types in all the sets in <paramref
        /// name="sets" />.
        /// </summary>
        /// <param name="sets">The sets to contain in the set.</param>
        /// <param name="wasChanged">Returns True if the result is not an empty
        /// set.</param>
        /// be modified.</param>
        public static IAnalysisSet UnionAll(IEnumerable<IAnalysisSet> sets, out bool wasChanged) {
            return Empty.UnionAll(sets, out wasChanged);
        }

        #endregion

        #region Extension Methods

        public static IAnalysisSet Add(this IAnalysisSet set, AnalysisProxy item) {
            bool dummy;
            return set.Add(item, out dummy);
        }

        public static IAnalysisSet Union(this IAnalysisSet set, IEnumerable<AnalysisProxy> items) {
            bool dummy;
            return set.Union(items, out dummy);
        }

        /// <summary>
        /// Returns <paramref name="set"/> with a comparer with increased
        /// strength. If the strength cannot be increased, <paramref
        /// name="set"/> is returned unmodified.
        /// </summary>
        /// <param name="set">The set to increase the strength of.</param>
        public static IAnalysisSet AsStrongerUnion(this IAnalysisSet set) {
            var comparer = set.Comparer as UnionComparer;
            if (comparer != null) {
                return set.AsUnion(comparer.Strength + 1);
            } else {
                return set.AsUnion(0);
            }
        }

        /// <summary>
        /// Returns <paramref name="set"/> with a comparer with the specified
        /// strength. If the strength does not need to be changed, <paramref
        /// name="set"/> is returned unmodified.
        /// </summary>
        /// <param name="set">The set to convert to a union.</param>
        /// <param name="strength">The strength of the union.</param>
        public static IAnalysisSet AsUnion(this IAnalysisSet set, int strength) {
            bool dummy;
            return set.AsUnion(strength, out dummy);
        }

        /// <summary>
        /// Returns <paramref name="set"/> with a comparer with the specified
        /// strength. If the strength does not need to be changed, <paramref
        /// name="set"/> is returned unmodified.
        /// </summary>
        /// <param name="set">The set to convert to a union.</param>
        /// <param name="strength">The strength of the union.</param>
        /// <param name="wasChanged">Returns True if the contents of the
        /// returned set are different to <paramref name="set"/>.</param>
        public static IAnalysisSet AsUnion(this IAnalysisSet set, int strength, out bool wasChanged) {
            if (strength > UnionComparer.MAX_STRENGTH) {
                strength = UnionComparer.MAX_STRENGTH;
            } else if (strength < 0) {
                strength = 0;
            }
            var comparer = UnionComparer.Instances[strength];
            return AsUnion(set, comparer, out wasChanged);
        }

        /// <summary>
        /// Returns <paramref name="set"/> with the specified comparer. If the
        /// comparer does not need to be changed, <paramref name="set"/> is
        /// returned unmodified.
        /// </summary>
        /// <param name="set">The set to convert to a union.</param>
        /// <param name="comparer">The comparer to use for the set.</param>
        /// <param name="wasChanged">Returns True if the contents of the
        /// returned set are different to <paramref name="set"/>.</param>
        internal static IAnalysisSet AsUnion(this IAnalysisSet set, UnionComparer comparer, out bool wasChanged) {
            if ((set is AnalysisSetDetails.AnalysisSetOneUnion ||
                set is AnalysisSetDetails.AnalysisSetTwoUnion ||
                set is AnalysisSetDetails.AnalysisSetEmptyUnion ||
                set is AnalysisSetDetails.AnalysisHashSet) &&
                set.Comparer == comparer) {
                wasChanged = false;
                return set;
            }

            wasChanged = true;

            var ns = set as AnalysisProxy;
            if (ns != null) {
                return CreateUnion(ns, comparer);
            }
            var ns2 = set as AnalysisSetDetails.AnalysisSetTwoObject;
            if (ns2 != null) {
                if (ns2.Value1 == null) {
                    if (ns2.Value2 == null) {
                        wasChanged = false;
                        return AnalysisSetEmptyUnion.Instances[comparer.Strength];
                    }
                    wasChanged = false;
                    return new AnalysisSetOneUnion(ns2.Value2, comparer);
                } else if (ns2.Value2 == null) {
                    wasChanged = false;
                    return new AnalysisSetOneUnion(ns2.Value1, comparer);
                }
                if (comparer.Equals(ns2.Value1, ns2.Value2)) {
                    bool dummy;
                    return new AnalysisSetDetails.AnalysisSetOneUnion(comparer.MergeTypes(ns2.Value1, ns2.Value2, out dummy), comparer);
                } else {
                    return new AnalysisSetDetails.AnalysisSetTwoUnion(ns2.Value1, ns2.Value2, comparer);
                }
            }
            return new AnalysisSetDetails.AnalysisHashSet(set, comparer);
        }

        /// <summary>
        /// Merges the provided sequence using the specified <see
        /// cref="UnionComparer"/>.
        /// </summary>
#if FULL_VALIDATION
        internal static IEnumerable<AnalysisWrapper> UnionIter(this IEnumerable<AnalysisWrapper> items, UnionComparer comparer, out bool wasChanged) {
            var originalItems = items.ToList();
            var newItems = UnionIterInternal(items, comparer, out wasChanged).ToList();

            Validation.Assert(newItems.Count <= originalItems.Count);
            if (wasChanged) {
                Validation.Assert(newItems.Count < originalItems.Count);
                foreach (var x in newItems) {
                    foreach (var y in newItems) {
                        if (object.ReferenceEquals(x, y)) continue;

                        Validation.Assert(!comparer.Equals(x, y));
                        Validation.Assert(!comparer.Equals(y, x));
                    }
                }
            }

            return newItems;
        }

        private static IEnumerable<AnalysisWrapper> UnionIterInternal(IEnumerable<AnalysisWrapper> items, UnionComparer comparer, out bool wasChanged) {
#else
        internal static IEnumerable<AnalysisProxy> UnionIter(this IEnumerable<AnalysisProxy> items, UnionComparer comparer, out bool wasChanged) {
#endif
            wasChanged = false;

            var asSet = items as IAnalysisSet;
            if (asSet != null && asSet.Comparer == comparer) {
                return items;
            }

            var newItems = new List<AnalysisProxy>();
            var anyMerged = true;

            while (anyMerged) {
                anyMerged = false;
                var matches = new Dictionary<AnalysisProxy, List<AnalysisProxy>>(comparer);

                foreach (var ns in items) {
                    List<AnalysisProxy> list;
                    if (matches.TryGetValue(ns, out list)) {
                        if (list == null) {
                            matches[ns] = list = new List<AnalysisProxy>();
                        }
                        list.Add(ns);
                    } else {
                        matches[ns] = null;
                    }
                }

                newItems.Clear();

                foreach (var keyValue in matches) {
                    var item = keyValue.Key;
                    if (keyValue.Value != null) {
                        foreach (var other in keyValue.Value) {
                            bool merged;
#if FULL_VALIDATION
                            Validation.Assert(comparer.Equals(item, other));
#endif
                            item = comparer.MergeTypes(item, other, out merged);
                            if (merged) {
                                anyMerged = true;
                                wasChanged = true;
                            }
                        }
                    }
                    newItems.Add(item);
                }
                items = newItems;
            }

            return items;
        }

        /// <summary>
        /// Removes excess capacity from <paramref name="set"/>.
        /// </summary>
        public static IAnalysisSet Trim(this IAnalysisSet set) {
            if (set is AnalysisSetDetails.AnalysisHashSet) {
                switch (set.Count) {
                    case 0:
                        if (set.Comparer is UnionComparer) {
                            return AnalysisSetDetails.AnalysisSetEmptyUnion.Instances[((UnionComparer)set.Comparer).Strength];
                        }
                        return Empty;
                    case 1:
                        if (set.Comparer is UnionComparer) {
                            return new AnalysisSetDetails.AnalysisSetOneUnion(set.First(), (UnionComparer)set.Comparer);
                        }
                        return set.First();
                    case 2:
                        if (set.Comparer is UnionComparer) {
                            var tup = AnalysisSetDetails.AnalysisSetTwoUnion.FromEnumerable(set, (UnionComparer)set.Comparer);
                            if (tup == null) {
                                return set;
                            } else if (tup.Item1 == null && tup.Item2 == null) {
                                return AnalysisSetDetails.AnalysisSetEmptyUnion.Instances[((UnionComparer)set.Comparer).Strength];
                            } else if (tup.Item2 == null) {
                                return new AnalysisSetDetails.AnalysisSetOneUnion(tup.Item1, (UnionComparer)set.Comparer);
                            } else {
                                return new AnalysisSetDetails.AnalysisSetTwoUnion(tup.Item1, tup.Item2, (UnionComparer)set.Comparer);
                            }
                        }
                        return new AnalysisSetDetails.AnalysisSetTwoObject(set);
                }
            } 
            return set;
        }

        /// <summary>
        /// Merges all the types in <paramref name="sets" /> into this set.
        /// </summary>
        /// <param name="sets">The sets to merge into this set.</param>
        public static IAnalysisSet UnionAll(this IAnalysisSet set, IEnumerable<IAnalysisSet> sets) {
            bool dummy;
            return set.UnionAll(sets, out dummy);
        }

        /// <summary>
        /// Merges all the types in <paramref name="sets" /> into this set.
        /// </summary>
        /// <param name="sets">The sets to merge into this set.</param>
        /// <param name="wasChanged">Returns True if the contents of the
        /// returned set are different to the original set.</param>
        public static IAnalysisSet UnionAll(this IAnalysisSet set, IEnumerable<IAnalysisSet> sets, out bool wasChanged) {
            bool changed;
            wasChanged = false;
            foreach (var s in sets) {
                var newSet = set.Union(s, out changed);
                if (changed) {
                    wasChanged = true;
                }
                set = newSet;
            }
            return set;
        }

        #endregion
    }

    sealed class ObjectComparer : IEqualityComparer<AnalysisProxy>, IEqualityComparer<IAnalysisSet> {
        public static readonly ObjectComparer Instance = new ObjectComparer();

        public bool Equals(AnalysisProxy x, AnalysisProxy y) {
#if FULL_VALIDATION
            if (x != null && y != null) {
                Validation.Assert(x.Equals(y) == y.Equals(x));
                if (x.Equals(y)) {
                    Validation.Assert(x.GetHashCode() == y.GetHashCode());
                }
            }
#endif
            return (x == null) ? (y == null) : x.Equals(y);
        }

        public int GetHashCode(AnalysisProxy obj) {
            return (obj == null) ? 0 : obj.GetHashCode();
        }

        public bool Equals(IAnalysisSet set1, IAnalysisSet set2) {
            if (set1.Comparer == this) {
                return new HashSet<AnalysisProxy>(set1, set1.Comparer).SetEquals(set2);
            } else if (set2.Comparer == this) {
                return new HashSet<AnalysisProxy>(set2, set2.Comparer).SetEquals(set1);
            } else {
                return set1.All(ns => set2.Contains(ns, this)) &&
                       set2.All(ns => set1.Contains(ns, this));
            }
        }

        public int GetHashCode(IAnalysisSet obj) {
            return obj.Aggregate(GetHashCode(), (hash, ns) => hash ^ GetHashCode(ns));
        }
    }

    sealed class UnionComparer : IEqualityComparer<AnalysisProxy> {
        public const int MAX_STRENGTH = 3;
        public static readonly UnionComparer[] Instances = Enumerable.Range(0, MAX_STRENGTH + 1).Select(i => new UnionComparer(i)).ToArray();


        public readonly int Strength;

        public UnionComparer(int strength = 0) {
            Strength = strength;
        }

        public bool Equals(AnalysisProxy x, AnalysisProxy y) {
#if FULL_VALIDATION
            if (x != null && y != null) {
                Validation.Assert(x.UnionEquals(y, Strength) == y.UnionEquals(x, Strength), string.Format("{0}\n{1}\n{2}", Strength, x, y));
                if (x.UnionEquals(y, Strength)) {
                    Validation.Assert(x.UnionHashCode(Strength) == y.UnionHashCode(Strength), string.Format("Strength:{0}\n{1} - {2}\n{3} - {4}", Strength, x, x.UnionHashCode(Strength), y, y.UnionHashCode(Strength)));
                }
            }
#endif
            if (Object.ReferenceEquals(x, y)) {
                return true;
            } else if (x == null) {
                return y == null;
            } else if (y == null || x.Value == null || y.Value == null) {
                return false;
            }
            return x.Value.UnionEquals(y.Value, Strength);
        }

        public int GetHashCode(AnalysisProxy obj) {
            return (obj == null || obj.Value == null) ? 0 : obj.Value.UnionHashCode(Strength);
        }

        public AnalysisProxy MergeTypes(AnalysisProxy x, AnalysisProxy y, out bool wasChanged) {
            if (Object.ReferenceEquals(x, y)) {
                wasChanged = false;
                return x;
            }
            var z = x.Value.UnionMergeTypes(y.Value, Strength).Proxy;
            wasChanged = !Object.ReferenceEquals(x, z);
#if FULL_VALIDATION
            var z2 = y.UnionMergeTypes(x, Strength);
            if (!object.ReferenceEquals(z, z2)) {
                Validation.Assert(z.UnionEquals(z2, Strength), string.Format("{0}\n{1} + {2} => {3}\n{2} + {1} => {4}", Strength, x, y, z, z2));
                Validation.Assert(z2.UnionEquals(z, Strength), string.Format("{0}\n{1} + {2} => {3}\n{2} + {1} => {4}", Strength, y, x, z2, z));
            }
#endif
            return z;
        }

        public int GetHashCode(IAnalysisSet obj) {
            return obj.Aggregate(GetHashCode(), (hash, ns) => hash ^ GetHashCode(ns));
        }
    }



    namespace AnalysisSetDetails {
        sealed class DebugViewProxy {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public const string DisplayString = "{this}, {Comparer.GetType().Name,nq}";

            public DebugViewProxy(IAnalysisSet source) {
                Data = source.ToArray();
                Comparer = source.Comparer;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public AnalysisProxy[] Data;

            public override string ToString() {
                return ToString(Data);
            }

            public static string ToString(IAnalysisSet source) {
                return ToString(source.ToArray());
            }

            public static string ToString(AnalysisProxy[] source) {
                var data = source.ToArray();
                if (data.Length == 0) {
                    return "{}";
                } else if (data.Length < 5) {
                    return "{" + string.Join(", ", data.AsEnumerable()) + "}";
                } else {
                    return string.Format("{{Size = {0}}}", data.Length);
                }
            }

            public IEqualityComparer<AnalysisProxy> Comparer {
                get;
                private set;
            }

            public int Size {
                get { return Data.Length; }
            }
        }

        [DebuggerDisplay(DebugViewProxy.DisplayString), DebuggerTypeProxy(typeof(DebugViewProxy))]
        sealed class AnalysisSetEmptyObject : IAnalysisSet, IImmutableAnalysisSet {
            public static readonly IAnalysisSet Instance = new AnalysisSetEmptyObject();

            public IAnalysisSet Add(AnalysisProxy item, out bool wasChanged) {
                if (!item.IsAlive) {
                    wasChanged = false;
                    return this;
                }

                wasChanged = true;
                return item;
            }

            public IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
                if (items == null || items is AnalysisSetEmptyObject || items is AnalysisSetEmptyUnion) {
                    wasChanged = false;
                    return this;
                }
                if (items is AnalysisProxy || items is AnalysisSetTwoObject) {
                    wasChanged = true;
                    return (IAnalysisSet)items;
                }
                wasChanged = items.Any();
                return wasChanged ? AnalysisSet.Create(items) : this;
            }

            public IAnalysisSet Clone() {
                return this;
            }

            public bool Contains(AnalysisProxy item) {
                return false;
            }

            public bool SetEquals(IAnalysisSet other) {
                return other != null && other.Count == 0;
            }

            public int Count {
                get { return 0; }
            }

            public IEnumerator<AnalysisProxy> GetEnumerator() {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public IEqualityComparer<AnalysisProxy> Comparer {
                get { return ObjectComparer.Instance; }
            }

            public override string ToString() {
                return DebugViewProxy.ToString(this);
            }
        }
        
        [DebuggerDisplay(DebugViewProxy.DisplayString), DebuggerTypeProxy(typeof(DebugViewProxy))]
        [Serializable]
        sealed class AnalysisSetTwoObject : IImmutableAnalysisSet {
            public readonly AnalysisProxy Value1, Value2;

            public AnalysisSetTwoObject(AnalysisProxy value1, AnalysisProxy value2) {
                Value1 = value1;
                Value2 = value2;
            }

            public AnalysisSetTwoObject(IEnumerable<AnalysisProxy> set) {
                using (var e = set.GetEnumerator()) {
                    if (!e.MoveNext()) {
                        throw new InvalidOperationException("Sequence requires exactly two values");
                    }
                    Value1 = e.Current;
                    if (!e.MoveNext() && !ObjectComparer.Instance.Equals(e.Current, Value1)) {
                        throw new InvalidOperationException("Sequence requires exactly two values");
                    }
                    Value2 = e.Current;
                    if (e.MoveNext()) {
                        throw new InvalidOperationException("Sequence requires exactly two values");
                    }
                }
            }

            public IAnalysisSet Add(AnalysisProxy item, out bool wasChanged) {
                if (!item.IsAlive) {
                    wasChanged = false;
                    return this;
                }

                if (ObjectComparer.Instance.Equals(Value1, item) || ObjectComparer.Instance.Equals(Value2, item)) {
                    wasChanged = false;
                    return this;
                }
                wasChanged = true;
                if (!Value1.IsAlive) {
                    if (!Value2.IsAlive) {
                        return item;
                    }
                    return new AnalysisSetTwoObject(Value2, item);
                } else if (!Value2.IsAlive) {
                    return new AnalysisSetTwoObject(Value1, item);
                }

                return new AnalysisHashSet(this).Add(item);
            }

            public IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
                AnalysisProxy ns;
                if (items == null) {
                    wasChanged = false;
                    return this;
                } else if ((ns = items as AnalysisProxy) != null) {
                    return Add(ns, out wasChanged);
                } else {
                    int count = items.Count();
                    if (Value1.IsAlive) {
                        if (!items.Contains(Value1)) {
                            count++;
                        }
                    }
                    if (Value2.IsAlive) {
                        if (!items.Contains(Value2)) {
                            count++;
                        }
                    }

                    switch (count) {
                        case 0:
                            wasChanged = false;
                            return AnalysisSetEmptyObject.Instance;
                        case 1:
                            if (Value1.IsAlive) {
                                wasChanged = false;
                                return Value1;
                            } else if (Value2.IsAlive) {
                                wasChanged = false;
                                return Value2;
                            }
                            wasChanged = true;
                            return items.First();
                        case 2:
                            AnalysisProxy first = null;
                            if (Value1.IsAlive) {
                                first = Value1;
                            }
                            if (Value2.IsAlive) {
                                if (first == null) {
                                    first = Value2;
                                } else {
                                    // items is empty...
                                    wasChanged = false;
                                    return this;
                                }
                            }
                            if (first == null) {
                                // Value1 & Value2 are gone
                                wasChanged = true;
                                return new AnalysisSetTwoObject(items);
                            }
                            // Value1 or Value2 is gone...
                            if (!items.First().IsAlive) {
                                // and so is the 1 item we have...
                                wasChanged = false;
                                return first;
                            }
                            wasChanged = true;
                            return new AnalysisSetTwoObject(first, items.First());
                        default:
                            var res = new AnalysisHashSet();
                            if (Value1.IsAlive) {
                                res.Add(Value1);
                            }
                            if (Value2.IsAlive) {
                                res.Add(Value2);
                            }
                            res.Union(items, out wasChanged);
                            return res;
                    }
                }
            }

            public IAnalysisSet Clone() {
                return this;
            }

            public bool Contains(AnalysisProxy item) {
                return ObjectComparer.Instance.Equals(Value1, item) || ObjectComparer.Instance.Equals(Value2, item);
            }

            public int Count {
                get {
                    if (Value1.IsAlive) {
                        if (Value2.IsAlive) {
                            return 2;
                        }
                        return 1;
                    } else if (Value2.IsAlive) {
                        return 1;
                    }
                    return 0; 
                }
            }

            public IEnumerator<AnalysisProxy> GetEnumerator() {
                if (Value1.IsAlive) {
                    yield return Value1;
                } 
                if (Value2.IsAlive) {
                    yield return Value2;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public IEqualityComparer<AnalysisProxy> Comparer {
                get { return ObjectComparer.Instance; }
            }

            public override string ToString() {
                return DebugViewProxy.ToString(this);
            }
        }

        [DebuggerDisplay(DebugViewProxy.DisplayString), DebuggerTypeProxy(typeof(DebugViewProxy))]
        [Serializable]
        sealed class AnalysisSetEmptyUnion : IAnalysisSet, IImmutableAnalysisSet {
            public static readonly IAnalysisSet[] Instances = UnionComparer.Instances.Select(cmp => new AnalysisSetEmptyUnion(cmp)).ToArray();

            private readonly UnionComparer _comparer;

            public AnalysisSetEmptyUnion(UnionComparer comparer) {
                _comparer = comparer;
            }

            public IAnalysisSet Add(AnalysisProxy item, out bool wasChanged) {
                if (!item.IsAlive) {
                    wasChanged = false;
                    return this;
                }

                wasChanged = true;
                return new AnalysisSetOneUnion(item, Comparer);
            }

            public IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
                if (items == null || items is AnalysisSetEmptyObject || items is AnalysisSetEmptyUnion) {
                    wasChanged = false;
                    return this;
                }
                if (items is AnalysisSetOneUnion || items is AnalysisSetTwoUnion) {
                    wasChanged = true;
                    return (IAnalysisSet)items;
                }
                wasChanged = items.Any();
                return wasChanged ? AnalysisSet.CreateUnion(items, Comparer) : this;
            }

            public IAnalysisSet Clone() {
                return this;
            }

            public bool Contains(AnalysisProxy item) {
                return false;
            }

            public bool SetEquals(IAnalysisSet other) {
                return other != null && other.Count == 0;
            }

            public int Count {
                get { return 0; }
            }

            public IEnumerator<AnalysisProxy> GetEnumerator() {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            internal UnionComparer Comparer {
                get { return _comparer; }
            }

            IEqualityComparer<AnalysisProxy> IAnalysisSet.Comparer {
                get { return ((AnalysisSetEmptyUnion)this).Comparer; }
            }

            public override string ToString() {
                return DebugViewProxy.ToString(this);
            }
        }

        [DebuggerDisplay(DebugViewProxy.DisplayString), DebuggerTypeProxy(typeof(DebugViewProxy))]
        [Serializable]
        sealed class AnalysisSetOneUnion : IImmutableAnalysisSet {
            public readonly AnalysisProxy Value;
            private readonly UnionComparer _comparer;

            public AnalysisSetOneUnion(AnalysisProxy value, UnionComparer comparer) {
                Value = value;
                _comparer = comparer;
            }

            public IAnalysisSet Add(AnalysisProxy item, out bool wasChanged) {
                if (!item.IsAlive) {
                    wasChanged = false;
                    return this;
                }

                if (!Value.IsAlive) {
                    wasChanged = true;
                    return item;
                }

                if (Object.ReferenceEquals(Value, item)) {
                    wasChanged = false;
                    return this;
                } else if (Comparer.Equals(Value, item)) {
                    var newItem = Comparer.MergeTypes(Value, item, out wasChanged);
                    return wasChanged ? new AnalysisSetOneUnion(newItem, Comparer) : this;
                } else {
                    wasChanged = true;
                    return new AnalysisSetTwoUnion(Value, item, Comparer);
                }
            }

            public IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
                AnalysisProxy ns;
                AnalysisSetOneUnion ns1;
                AnalysisSetTwoUnion ns2;
                if (items == null) {
                    wasChanged = false;
                    return this;
                } else if ((ns = items as AnalysisProxy) != null) {
                    return Add(ns, out wasChanged);
                } else if ((ns1 = items as AnalysisSetOneUnion) != null) {
                    return Add(ns1.Value, out wasChanged);
                } else if ((ns2 = items as AnalysisSetTwoUnion) != null && ns2.Comparer == Comparer) {
                    return ns2.Add(Value, out wasChanged);
                } else {
                    return new AnalysisHashSet(Value, Comparer).Union(items, out wasChanged).Trim();
                }
            }

            public IAnalysisSet Clone() {
                return this;
            }

            public bool Contains(AnalysisProxy item) {
                return Comparer.Equals(Value, item);
            }

            public int Count {
                get {
                    if (Value.IsAlive) {
                        return 1;
                    }
                    return 0;
                }
            }

            public IEnumerator<AnalysisProxy> GetEnumerator() {
                return new SetOfOneEnumerator<AnalysisProxy>(Value);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            internal UnionComparer Comparer {
                get { return _comparer; }
            }

            IEqualityComparer<AnalysisProxy> IAnalysisSet.Comparer {
                get { return ((AnalysisSetOneUnion)this).Comparer; }
            }

            public override string ToString() {
                return DebugViewProxy.ToString(this);
            }
        }

        [DebuggerDisplay(DebugViewProxy.DisplayString), DebuggerTypeProxy(typeof(DebugViewProxy))]
        [Serializable]
        sealed class AnalysisSetTwoUnion : IImmutableAnalysisSet {
            public readonly AnalysisProxy Value1, Value2;
            private readonly UnionComparer _comparer;

            public AnalysisSetTwoUnion(AnalysisProxy value1, AnalysisProxy value2, UnionComparer comparer) {
                Debug.Assert(!comparer.Equals(value1, value2));
                Value1 = value1;
                Value2 = value2;
                _comparer = comparer;
            }

            internal static Tuple<AnalysisProxy, AnalysisProxy> FromEnumerable(IEnumerable<AnalysisProxy> set, UnionComparer comparer) {
                using (var e = set.GetEnumerator()) {
                    if (!e.MoveNext()) {
                        return new Tuple<AnalysisProxy, AnalysisProxy>(null, null);
                    }
                    var value1 = e.Current;
                    if (!e.MoveNext()) {
                        return new Tuple<AnalysisProxy, AnalysisProxy>(value1, null);
                    }
                    var value2 = e.Current;
                    if (comparer.Equals(e.Current, value1)) {
                        bool dummy;
                        return new Tuple<AnalysisProxy, AnalysisProxy>(comparer.MergeTypes(value1, value2, out dummy), null);
                    }
                    if (e.MoveNext()) {
                        return null;
                    }
                    return new Tuple<AnalysisProxy, AnalysisProxy>(value1, value2);
                }
            }

            public AnalysisSetTwoUnion(IEnumerable<AnalysisProxy> set, UnionComparer comparer) {
                _comparer = comparer;
                var tup = FromEnumerable(set, comparer);
                if (tup == null || tup.Item2 == null) {
                    throw new InvalidOperationException("Sequence requires exactly two values");
                }
                Value1 = tup.Item1;
                Value2 = tup.Item2;
            }

            public IAnalysisSet Add(AnalysisProxy item, out bool wasChanged) {
                if (!item.IsAlive) {
                    wasChanged = false;
                    return this;
                }

                bool dummy;
                if (Object.ReferenceEquals(Value1, item) || Object.ReferenceEquals(Value2, item)) {
                    wasChanged = false;
                    return this;
                } else if (Comparer.Equals(Value1, item)) {
                    var newValue = Comparer.MergeTypes(Value1, item, out wasChanged);
                    if (!wasChanged) {
                        return this;
                    }
                    if (Comparer.Equals(Value2, newValue)) {
                        return new AnalysisSetOneUnion(Comparer.MergeTypes(Value2, newValue, out dummy), Comparer);
                    } else {
                        return new AnalysisSetTwoUnion(newValue, Value2, Comparer);
                    }
                } else if (Comparer.Equals(Value2, item)) {
                    var newValue = Comparer.MergeTypes(Value2, item, out wasChanged);
                    if (!wasChanged) {
                        return this;
                    }
                    if (Comparer.Equals(Value1, newValue)) {
                        return new AnalysisSetOneUnion(Comparer.MergeTypes(Value1, newValue, out dummy), Comparer);
                    } else {
                        return new AnalysisSetTwoUnion(Value1, newValue, Comparer);
                    }
                }
                wasChanged = true;
                return new AnalysisHashSet(this, Comparer).Add(item);
            }

            public IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
                AnalysisProxy ns;
                AnalysisSetOneUnion ns1u;
                if (items == null) {
                    wasChanged = false;
                    return this;
                } else if ((ns = items as AnalysisProxy) != null) {
                    return Add(ns, out wasChanged);
                } else if ((ns1u = items as AnalysisSetOneUnion) != null) {
                    return Add(ns1u.Value, out wasChanged);
                } else {
                    return new AnalysisHashSet(this, Comparer).Union(items, out wasChanged);
                }
            }

            public IAnalysisSet Clone() {
                return this;
            }

            public bool Contains(AnalysisProxy item) {
                return Comparer.Equals(Value1, item) || Comparer.Equals(Value2, item);
            }

            public int Count {
                get {
                    if (Value1.IsAlive) {
                        if (Value2.IsAlive) {
                            return 2;
                        }
                        return 1;
                    } else if (Value2.IsAlive) {
                        return 2;
                    }
                    return 0; 
                }
            }

            public IEnumerator<AnalysisProxy> GetEnumerator() {
                if (Value1.IsAlive) {
                    yield return Value1;
                }
                if (Value2.IsAlive) {
                    yield return Value2;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            internal UnionComparer Comparer {
                get { return _comparer; }
            }

            IEqualityComparer<AnalysisProxy> IAnalysisSet.Comparer {
                get { return ((AnalysisSetTwoUnion)this).Comparer; }
            }

            public override string ToString() {
                return DebugViewProxy.ToString(this);
            }
        }

    }

}

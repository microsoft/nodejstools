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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.NodejsTools.Analysis.AnalysisSetDetails {
    /// <summary>
    /// HashSet used in analysis engine.
    /// 
    /// This set is thread safe for a single writer and multiple readers.  
    /// 
    /// Reads and writes on the dictionary are all lock free, but have memory
    /// barriers in place.  The key is used to indicate the current state of a bucket.
    /// When adding a bucket the key is updated last after all other values
    /// have been added.  When removing a bucket the key is cleared first.  Memory
    /// barriers are used to ensure that the writes to the key bucket are not
    /// re-ordered.
    /// 
    /// When resizing the set the buckets are replaced atomically so that the reader
    /// sees the new buckets or the old buckets.  When reading the reader first reads
    /// the buckets and then calls a static helper function to do the read from the bucket
    /// array to ensure that readers are not seeing multiple bucket arrays.
    /// </summary>
    [DebuggerDisplay(AnalysisSetDetails.DebugViewProxy.DisplayString), DebuggerTypeProxy(typeof(AnalysisSetDetails.DebugViewProxy))]
    [Serializable]
    internal sealed class AnalysisHashSet : IAnalysisSet  {

        private Bucket[] _buckets;
        private int _count;
        private readonly IEqualityComparer<AnalysisProxy> _comparer;

        private const int InitialBucketSize = 3;
        private const int ResizeMultiplier = 2;
        private const double Load = .9;

        // Marker object used to indicate we have a removed value
        private static readonly AnalysisProxy _removed = new AnalysisProxy(null);

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public AnalysisHashSet() {
            _comparer = ObjectComparer.Instance;
        }

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public AnalysisHashSet(int count) {
            _buckets = new Bucket[AnalysisDictionary<object, object>.GetPrime((int)(count / Load + 2))];
            _comparer = ObjectComparer.Instance;
        }

        public AnalysisHashSet(IEqualityComparer<AnalysisProxy> comparer) {
            _comparer = comparer;
        }

        public AnalysisHashSet(int count, IEqualityComparer<AnalysisProxy> comparer) {
            _buckets = new Bucket[AnalysisDictionary<object, object>.GetPrime((int)(count / Load + 2))];
            _comparer = comparer;
        }

        public AnalysisHashSet(IEnumerable<AnalysisProxy> enumerable, IEqualityComparer<AnalysisProxy> comparer)
            : this(comparer) {
            Union(enumerable);
        }

        public AnalysisHashSet(IEnumerable<AnalysisProxy> enumerable) : this() {
            Union(enumerable);
        }

        public IEqualityComparer<AnalysisProxy> Comparer {
            get {
                return _comparer;
            }
        }

        public IAnalysisSet Add(AnalysisProxy item, out bool wasChanged) {
            wasChanged = AddOne(item);
            return this;
        }

        public IAnalysisSet Union(IEnumerable<AnalysisProxy> items) {
            bool wasChanged;
            return Union(items, out wasChanged);
        }

        public IAnalysisSet Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
            AnalysisHashSet otherHc = items as AnalysisHashSet;
            if (otherHc != null) {
                bool anyChanged = false;

                if (otherHc._count != 0) {
                    // do a fast copy from the other hash set...
                    for (int i = 0; i < otherHc._buckets.Length; i++) {
                        var key = otherHc._buckets[i].Key;
                        if (key != null && key != AnalysisDictionaryRemovedValue.Instance) {
                            anyChanged |= AddOne(key);
                        }
                    }
                }
                wasChanged = anyChanged;
                return this;
            }

            // some other set, copy it the slow way...
            wasChanged = false;
            foreach (var item in items) {
                wasChanged |= AddOne(item);
            }
            return this;
        }

        public IAnalysisSet Clone() {            
            var buckets = new Bucket[_buckets.Length];
            for (int i = 0; i < buckets.Length; i++) {
                buckets[i] = _buckets[i];
            }
            var res = new AnalysisHashSet(Comparer);
            res._buckets = buckets;
            res._count = _count;
            return res;
        }

        /// <summary>
        /// Adds a new item to the dictionary, replacing an existing one if it already exists.
        /// </summary>
        private bool AddOne(AnalysisProxy key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (key.IsAlive) {
                if (_buckets == null) {
                    Initialize();
                }

                if (Add(_buckets, key)) {
                    _count++;

                    CheckGrow();
                    return true;
                }
            }
            return false;
        }

        private void CheckGrow() {
            if (_count >= (_buckets.Length * Load)) {
                // grow the hash table
                EnsureSize((int)(_buckets.Length / Load) * ResizeMultiplier);
            }
        }

        private void EnsureSize(int newSize) {            
            // see if we can reclaim collected buckets before growing...
            var oldBuckets = _buckets;
            if (_buckets == null) {
                _buckets = new Bucket[newSize];
                return;
            }

            if (oldBuckets != null) {
                for (int i = 0; i < oldBuckets.Length; i++) {
                    var curBucket = oldBuckets[i];
                    if (curBucket.Key != null && !curBucket.Key.IsAlive) {
                        oldBuckets[i].Key = _removed;
                        newSize--;
                        _count--;
                    }
                }
            }

            if (newSize > oldBuckets.Length) {
                newSize = AnalysisDictionary<object,object>.GetPrime(newSize);

                var newBuckets = new Bucket[newSize];

                for (int i = 0; i < oldBuckets.Length; i++) {
                    var curBucket = oldBuckets[i];
                    if (curBucket.Key != null &&
                        curBucket.Key != _removed) {
                        AddOne(newBuckets, curBucket.Key, curBucket.HashCode);
                    }
                }

                _buckets = newBuckets;
            }
        }

        /// <summary>
        /// Initializes the buckets to their initial capacity, the caller
        /// must check if the buckets are empty first.
        /// </summary>
        private void Initialize() {
            _buckets = new Bucket[InitialBucketSize];
        }

        /// <summary>
        /// Add helper that works over a single set of buckets.  Used for
        /// both the normal add case as well as the resize case.
        /// </summary>
        private bool Add(Bucket[] buckets, AnalysisProxy key) {
            int hc = _comparer.GetHashCode(key) & Int32.MaxValue;

            return AddOne(buckets, key, hc);
        }

        /// <summary>
        /// Add helper which adds the given key/value (where the key is not null) with
        /// a pre-computed hash code.
        /// </summary>
        private bool AddOne(Bucket[] buckets, AnalysisProxy/*!*/ key, int hc) {
            Debug.Assert(key != null);

            Debug.Assert(_count < buckets.Length);
            int index = hc % buckets.Length;
            int startIndex = index;
            int addIndex = -1;

            for (; ; ) {
                Bucket cur = buckets[index];
                var existingKey = cur.Key;
                if (existingKey == null || existingKey == _removed || !existingKey.IsAlive) {
                    if (addIndex == -1) {
                        addIndex = index;
                    }
                    if (cur.Key == null) {
                        break;
                    }
                } else if (Object.ReferenceEquals(key, existingKey) ||
                    (cur.HashCode == hc && _comparer.Equals(key, (AnalysisProxy)existingKey))) {
                    return false;
                }

                index = ProbeNext(buckets, index);

                if (index == startIndex) {
                    break;
                }
            }

            if (buckets[addIndex].Key != null &&
                buckets[addIndex].Key != _removed && 
                !buckets[addIndex].Key.IsAlive) {
                _count--;
            }
            buckets[addIndex].HashCode = hc;
            Thread.MemoryBarrier();
            // we write the key last so that we can check for null to
            // determine if a bucket is available.
            buckets[addIndex].Key = key;            

            return true;
        }

        private static int ProbeNext(Bucket[] buckets, int index) {
            // probe to next bucket    
            return (index + ((buckets.Length - 1) / 2)) % buckets.Length;
        }

        /// <summary>
        /// Checks to see if the key exists in the dictionary.
        /// </summary>
        public bool Contains(AnalysisProxy key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            return Contains(_buckets, key);
        }

        /// <summary>
        /// Static helper to try and get the value from the dictionary.
        /// 
        /// Used so the value lookup can run against a buckets while a writer
        /// replaces the buckets.
        /// </summary>
        private bool Contains(Bucket[] buckets, AnalysisProxy/*!*/ key) {
            Debug.Assert(key != null);

            if (_count > 0 && buckets != null) {
                int hc = _comparer.GetHashCode(key) & Int32.MaxValue;

                return Contains(buckets, key, hc);
            }

            return false;
        }

        private bool Contains(Bucket[] buckets, AnalysisProxy key, int hc) {
            int index = hc % buckets.Length;
            int startIndex = index;
            do {
                var existingKey = buckets[index].Key;
                if (existingKey == null) {
                    break;
                } else {
                    if (Object.ReferenceEquals(key, existingKey) ||
                        (existingKey != _removed &&
                        buckets[index].HashCode == hc &&
                        _comparer.Equals(key, (AnalysisProxy)existingKey))) {

                        return true;
                    }
                }

                index = ProbeNext(buckets, index);
            } while (startIndex != index);

            return false;
        }

        /// <summary>
        /// Returns the number of key/value pairs currently in the dictionary.
        /// </summary>
        public int Count {
            get {
                return _count;
            }
        }

        public void Clear() {
            if (_buckets != null && _count != 0) {
                _buckets = new Bucket[InitialBucketSize];
                _count = 0;
            }
        }

        public IEnumerator<AnalysisProxy> GetEnumerator() {
            var buckets = _buckets;
            if (buckets != null) {
                for (int i = 0; i < buckets.Length; i++) {
                    var key = buckets[i].Key;
                    if (key != null && key != _removed && key.IsAlive) {
                        yield return key;
                    }
                }
            }
        }

        /// <summary>
        /// Used to store a single hashed key/value.
        /// 
        /// Bucket is not serializable because it stores the computed hash
        /// code which could change between serialization and deserialization.
        /// </summary>
        struct Bucket {
            public AnalysisProxy Key;          // the key to be hashed
            public int HashCode;        // the hash code of the contained key.
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// A simple dictionary like object which has efficient storage when there's only a single item in the dictionary.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    struct SingleDict<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private object _data; // Dictionary<TKey, TValue>, SingleEntry<TKey, TValue>

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private KeyValuePair<TKey, TValue>[] AllItems {
            get {
                var single = _data as SingleDependency;
                if (single != null) {
                    return new[] { new KeyValuePair<TKey, TValue>(single.Key, single.Value) };
                }

                var dict = _data as Dictionary<TKey, TValue>;
                if (dict != null) {
                    return dict.ToArray();
                }

                return new KeyValuePair<TKey, TValue>[0];
            }
        }


        [Serializable]
        internal sealed class SingleDependency {
            public readonly TKey Key;
            public TValue Value;

            public SingleDependency(TKey key, TValue value) {
                Key = key;
                Value = value;
            }
        }


        public bool ContainsKey(TKey key) {
            var single = _data as SingleDependency;
            if (single != null) {
                return EqualityComparer<TKey>.Default.Equals(single.Key, key);
            }
            var dict = _data as Dictionary<TKey, TValue>;
            if (dict != null) {
                return dict.ContainsKey(key);
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            SingleDependency single = _data as SingleDependency;
            if (single != null) {
                if (EqualityComparer<TKey>.Default.Equals(single.Key, key)) {
                    value = single.Value;
                    return true;
                }
                value = default(TValue);
                return false;
            }

            var dict = _data as Dictionary<TKey, TValue>;
            if (dict != null) {
                return dict.TryGetValue(key, out value);
            }

            value = default(TValue);
            return false;
        }

        public TValue this[TKey key] {
            get {
                TValue res;
                if (TryGetValue(key, out res)) {
                    return res;
                }

                throw new KeyNotFoundException();
            }
            set {
                if (_data == null) {
                    _data = new SingleDependency(key, value);
                    return;
                }

                var single = _data as SingleDependency;
                if (single != null) {
                    if (EqualityComparer<TKey>.Default.Equals(single.Key, key)) {
                        single.Value = value;
                        return;
                    }

                    var data = new Dictionary<TKey, TValue>();
                    data[single.Key] = single.Value;
                    data[key] = value;
                    _data = data;
                    return;
                }

                var dict = _data as Dictionary<TKey, TValue>;
                if (dict == null) {
                    _data = dict = new Dictionary<TKey, TValue>();
                }
                dict[key] = value;
            }
        }

        internal void Remove(TKey fromModule) {
            var single = _data as SingleDependency;
            if (single != null) {
                if (EqualityComparer<TKey>.Default.Equals(single.Key, fromModule)) {
                    _data = null;
                }
                return;
            }

            var dict = _data as Dictionary<TKey, TValue>;
            if (dict != null) {
                dict.Remove(fromModule);
            }
        }

        public bool TryGetSingleValue(out TValue value) {
            var single = _data as SingleDependency;
            if (single != null) {
                value = single.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Dictionary<TKey, TValue>.ValueCollection DictValues {
            get {
                Debug.Assert(_data is Dictionary<TKey, TValue>);

                return ((Dictionary<TKey, TValue>)_data).Values;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal Dictionary<TKey, TValue> InternalDict {
            get {
                return _data as Dictionary<TKey, TValue>;
            }
            set {
                if (value.Count == 1) {
                    using (var e = value.GetEnumerator()) {
                        e.MoveNext();
                        _data = new SingleDependency(e.Current.Key, e.Current.Value);
                    }
                } else {
                    _data = value;
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<TValue> Values {
            get {
                SingleDependency single;
                var dict = _data as Dictionary<TKey, TValue>;
                if (dict != null) {
                    return dict.Values;
                } else if ((single = _data as SingleDependency) != null) {
                    return new[] { single.Value };
                }
                return new TValue[0];
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<TKey> Keys {
            get {
                var single = _data as SingleDependency;
                if (single != null) {
                    yield return single.Key;
                }

                var dict = _data as Dictionary<TKey, TValue>;
                if (dict != null) {
                    foreach (var value in dict.Keys) {
                        yield return value;
                    }
                }
            }
        }

        public int Count {
            get {
                var single = _data as SingleDependency;
                if (single != null) {
                    return 1;
                }

                var dict = _data as Dictionary<TKey, TValue>;
                if (dict != null) {
                    return dict.Count;
                }

                return 0;
            }
        }

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            SingleDependency single = _data as SingleDependency;
            if (single != null) {
                yield return new KeyValuePair<TKey, TValue>(single.Key, single.Value);
            }

            Dictionary<TKey, TValue> dict = _data as Dictionary<TKey, TValue>;
            if (dict != null) {
                foreach (var keyValue in dict) {
                    yield return keyValue;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        #endregion

    }

}

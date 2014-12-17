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

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Lightweight ISet which holds onto a single value (we have lots of sets)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    sealed class SetOfOne<T> : ISet<T> {
        private T _value;   // not readonly for serialization perf

        public SetOfOne(T value) {
            _value = value;
        }

        public T Value {
            get {
                return _value;
            }
        }
        public bool Add(T item) {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other) {
            var enumerator = other.GetEnumerator();
            if (enumerator.MoveNext()) {
                if (EqualityComparer<T>.Default.Equals(enumerator.Current, _value)) {
                    return !enumerator.MoveNext();
                }
            }
            return false;
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(T item) {
            return EqualityComparer<T>.Default.Equals(item, _value);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            array[arrayIndex] = _value;
        }

        public int Count {
            get { return 1; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove(T item) {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator() {
            return new SetOfOneEnumerator<T>(_value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new SetOfOneEnumerator<T>(_value);
        }
    }
}

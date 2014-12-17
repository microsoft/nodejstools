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
using Microsoft.VisualStudio.Text;

namespace TestUtilities.Mocks {
    class MockNormalizedTextChangeCollection : INormalizedTextChangeCollection {
        private readonly ITextChange[] _changes;

        public MockNormalizedTextChangeCollection(params ITextChange[] changes) {
            _changes = changes;
        }

        public bool IncludesLineChanges {
            get {
                foreach (var change in _changes) {
                    if (change.OldText.IndexOfAny(new[] { '\r', '\n' }) != -1 ||
                        change.NewText.IndexOfAny(new[] { '\r', '\n' }) != -1) {
                        return true;
                    }
                }
                return false;
            }
        }

        public int IndexOf(ITextChange item) {
            for (int i = 0; i < _changes.Length; i++) {
                if (_changes[i] == item) {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, ITextChange item) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public ITextChange this[int index] {
            get {
                return _changes[index];
            }
            set {
                throw new NotImplementedException();
            }
        }

        public void Add(ITextChange item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(ITextChange item) {
            throw new NotImplementedException();
        }

        public void CopyTo(ITextChange[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return _changes.Length; }
        }

        public bool IsReadOnly {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(ITextChange item) {
            throw new NotImplementedException();
        }

        public IEnumerator<ITextChange> GetEnumerator() {
            foreach (var change in _changes) {
                yield return change;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            foreach (var change in _changes) {
                yield return change;
            }
        }
    }
}

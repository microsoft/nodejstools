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

    class SetOfOneEnumerator<T> : IEnumerator<T> {
        private readonly T _value;
        private bool _enumerated;

        public SetOfOneEnumerator(T value) {
            _value = value;
        }

        #region IEnumerator<T> Members

        T IEnumerator<T>.Current {
            get { return _value; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current {
            get { return _value; }
        }

        bool System.Collections.IEnumerator.MoveNext() {
            if (_enumerated || !DetachableProxy.IsAlive(_value)) {
                return false;
            }
            _enumerated = true;
            return true;
        }

        void System.Collections.IEnumerator.Reset() {
            _enumerated = false;
        }

        #endregion
    }

}

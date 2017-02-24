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

using Microsoft.VisualStudio;

namespace Microsoft.NodejsTools.Debugger.Remote
{
    internal class NodeRemoteEnumDebug<T>
        where T : class
    {
        private readonly T _elem;
        private bool _done;

        public NodeRemoteEnumDebug(T elem = null)
        {
            _elem = elem;
            Reset();
        }

        protected T Element
        {
            get { return _elem; }
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (_elem == null) ? 0u : 1u;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, T[] rgelt, ref uint pceltFetched)
        {
            if (_done)
            {
                pceltFetched = 0;
                return VSConstants.S_FALSE;
            }
            else
            {
                pceltFetched = 1;
                rgelt[0] = _elem;
                _done = true;
                return VSConstants.S_OK;
            }
        }

        public int Reset()
        {
            _done = (_elem == null);
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            if (celt == 0)
            {
                return VSConstants.S_OK;
            }
            else if (_done)
            {
                return VSConstants.S_FALSE;
            }
            else
            {
                _done = true;
                return celt > 1 ? VSConstants.S_FALSE : VSConstants.S_OK;
            }
        }
    }
}

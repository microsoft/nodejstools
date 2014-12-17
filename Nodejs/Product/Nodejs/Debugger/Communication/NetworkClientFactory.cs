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

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class NetworkClientFactory : INetworkClientFactory {
        public INetworkClient CreateNetworkClient(Uri uri) {
            if (uri == null) {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri) {
                switch (uri.Scheme) {
                    case "tcp":
                        if (uri.Port < 0) {
                            throw new ArgumentException("tcp:// URI must include port number", "uri");
                        }
                        return new TcpNetworkClient(uri.Host, uri.Port);
                    case "ws":
                    case "wss":
                        return new WebSocketNetworkClient(uri);
                }
            }

            throw new ArgumentException("tcp://, ws:// or wss:// URI required", "uri");
        }
    }
}
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

using System.IO;
using System.Net.Sockets;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class TcpNetworkClient : INetworkClient
    {
        private readonly TcpClient _tcpClient;

        public TcpNetworkClient(string hostName, int portNumber)
        {
            this._tcpClient = new TcpClient(hostName, portNumber);
        }

        public bool Connected => this._tcpClient.Connected;
        public void Dispose()
        {
            this._tcpClient.Close();
        }

        public Stream GetStream()
        {
            return this._tcpClient.GetStream();
        }
    }
}
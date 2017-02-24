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

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal interface IDebuggerConnection : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether connection established.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets a Node.js version.
        /// </summary>
        Version NodeVersion { get; }

        /// <summary>
        /// Connect to specified debugger endpoint.
        /// </summary>
        /// <param name="uri">URI identifying the endpoint to connect to.</param>
        void Connect(Uri uri);

        /// <summary>
        /// Close connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message.</param>
        void SendMessage(string message);

        /// <summary>
        /// Fired when received inbound message.
        /// </summary>
        event EventHandler<MessageEventArgs> OutputMessage;

        /// <summary>
        /// Fired when connection was closed.
        /// </summary>
        event EventHandler<EventArgs> ConnectionClosed;
    }
}
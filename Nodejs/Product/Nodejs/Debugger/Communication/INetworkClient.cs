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
using System.IO;

namespace Microsoft.NodejsTools.Debugger.Communication {
    interface INetworkClient : IDisposable {
        /// <summary>
        /// Gets a value indicating whether client is connected to a remote host.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Returns the <see cref="T:System.IO.Stream" /> used to send and receive data.
        /// </summary>
        /// <returns>The underlying <see cref="T:System.IO.Stream" /></returns>
        Stream GetStream();
    }
}
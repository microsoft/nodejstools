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
using System.Web;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger
{
    public class WebSocketProxy : WebSocketProxyBase
    {
        public override int DebuggerPort
        {
            get { return 5858; }
        }

        public override bool AllowConcurrentConnections
        {
            get { return false; }
        }

        public override void ProcessHelpPageRequest(HttpContext context)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream("Microsoft.NodejsTools.WebRole.WebSocketProxy.html"))
            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                var wsUri = new UriBuilder(context.Request.Url) { Scheme = "wss", Port = -1 };
                context.Response.Write(html.Replace("{{WS_URI}}", wsUri.ToString()));
                context.Response.End();
            }
        }
    }
}

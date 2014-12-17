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
using System.Net;

namespace TestUtilities {
    public static class WebDownloadUtility {
        public static string GetString(Uri siteUri) {
            string text;
            var req = HttpWebRequest.CreateHttp(siteUri);
            
            using (var resp = req.GetResponse())
            using (StreamReader reader = new StreamReader(resp.GetResponseStream())) {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}

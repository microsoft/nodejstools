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
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.TypeScriptSourceMapReader {
    /// <summary>
    /// Helpers for source map urls
    /// </summary>
    public class SourceMapUrlHelper {
        #region ApiIsScriptUrlJsUrl
        /// <summary>
        /// Returns if the given url is js file url
        /// </summary>
        /// <param name="urlOfScript">url of the script</param>
        /// <returns>true if the url is .js</returns>
        public static bool IsScriptUrlJsUrl(string urlOfScript) {
            if (!string.IsNullOrEmpty(urlOfScript)) {
                // Simple check if the file name ends with .js and assume that it is javascript file
                // this is needed to make sure debugging works with cscript as it doesnt give qualified file names 
                if (urlOfScript.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                // Normalise file paths
                try {
                    // Even though path might not end with .js it could be a valid js url 
                    // eg. with optional parameters passed in like something.js?param
                    var scriptUri = new Uri(urlOfScript);
                    if (scriptUri.AbsolutePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) {
                        return true;
                    }
                } catch {
                    // Ignore exceptions when converting the url names, just assume it is not js file 
                }
            }
            return false;
        }
        #endregion

        #region ApiGetSourceMapUrlOfScript
        /// <summary>
        /// Checks if the script document has source mapping information and returns the map file name if present
        /// </summary>
        /// <param name="scriptUrl">url of the script</param>
        /// <param name="scriptContents">contents of the script</param>
        /// <returns>name of source map file if present otherwise null</returns>
        public static string GetSourceMapUrlOfScript(string scriptUrl, string scriptContents) {
            if (IsScriptUrlJsUrl(scriptUrl) && !string.IsNullOrEmpty(scriptContents)) {
                var indexMapSyntax = scriptContents.LastIndexOf("//# sourceMappingURL=", StringComparison.Ordinal);
                if (indexMapSyntax <= 0) {
                    indexMapSyntax = scriptContents.LastIndexOf("//@ sourceMappingURL=", StringComparison.Ordinal);
                }

                if (indexMapSyntax > 0) {
                    // souceMapUrl should be the last line of the code
                    var sourceMapUrl = scriptContents.Substring(indexMapSyntax + 21).Trim();
                    if (!sourceMapUrl.Contains(Environment.NewLine)) {
                        return sourceMapUrl;
                    }
                }
            }

            return null;
        }
        #endregion

        #region ApiGetWebResponse
        /// <summary>
        /// Gets the WebResponse to downloading the contents of Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static WebResponse GetWebResponse(Uri uri) {
            var webRequest = WebRequest.Create(uri);
            webRequest.UseDefaultCredentials = true;

#if !SILVERLIGHT
            // Add the default network credentials for the the default credentials only support NTLM, so create a credential cache that supports
            // both digest and NTLM
            var ntlm = CredentialCache.DefaultNetworkCredentials.GetCredential(uri, "NTLM");
            var digest = CredentialCache.DefaultNetworkCredentials.GetCredential(uri, "Digest");
            var credentials = new CredentialCache();
            credentials.Add(uri, "NTLM", ntlm);
            credentials.Add(uri, "Digest", digest);
            webRequest.Credentials = credentials;
#endif

            var delayTask = Task.Delay(TimeSpan.FromSeconds(10));
            var getResponseTask = Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse, null);

            var result = Task.WhenAny(delayTask, getResponseTask).Result;
            if (result == getResponseTask) {
                return getResponseTask.Result;
            }

            Debug.Assert(result == delayTask);

            // default message is good enough since this is wrapped in a more specific exception 
            // further up the stack
            throw new TimeoutException();
        }
        #endregion
    }
}

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
using System.Reflection;

namespace Microsoft.NodejsTools
{
    public static class NodejsToolsInstallPath
    {
        private static string GetFromAssembly(Assembly assembly, string filename)
        {
            string path = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                filename);
            if (File.Exists(path))
            {
                return path;
            }
            return string.Empty;
        }

        public static string GetFile(string filename)
        {
            string path = GetFromAssembly(typeof(NodejsToolsInstallPath).Assembly, filename);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            throw new InvalidOperationException("Unable to determine Node.js Tools installation path");
        }
    }
}

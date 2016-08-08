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
using System.IO;

namespace Microsoft.NodejsTools.TestFrameworks {
    class TestFrameworkDirectories {
        public const string ExportRunnerFramework = "ExportRunner";
        private const string TestFrameworksDirectory = "TestFrameworks";

        private readonly Dictionary<string, string> _frameworkDirectories;

        public TestFrameworkDirectories() {
            _frameworkDirectories = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (string directory in Directory.GetDirectories(GetBaseTestframeworkFolder())) {
                string name = Path.GetFileName(directory);
                _frameworkDirectories.Add(name, directory);
            }
            string defaultFx;
            _frameworkDirectories.TryGetValue(ExportRunnerFramework, out defaultFx);
            if (defaultFx == null) {
                throw new InvalidOperationException("Missing generic test framework");
            }
        }

        public List<string> GetFrameworkNames() {
            return new List<string>(_frameworkDirectories.Keys);
        }

        public List<string> GetFrameworkDirectories() {
            return new List<string>(_frameworkDirectories.Values);
        }

        private static string GetBaseTestframeworkFolder() {
            string installFolder = GetExecutingAssemblyPath();
            string baseDirectory = Path.Combine(installFolder, TestFrameworksDirectory);
#if DEBUG
            // To allow easier debugging of the test adapter, try to use the local directory as a fallback.
            baseDirectory = Directory.Exists(baseDirectory) ? baseDirectory : Path.Combine(Directory.GetCurrentDirectory(), TestFrameworksDirectory);
#endif
            return baseDirectory;
        }

        private static string GetExecutingAssemblyPath() {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}

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

namespace NpmTests
{
    public static class FilesystemPackageJsonTestHelpers
    {
        public static void CreatePackageJson(string filename, string json)
        {
            using (var fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(fout))
                {
                    writer.Write(json);
                }
            }
        }

        public static string CreateRootPackageDir(TemporaryFileManager manager)
        {
            return manager.GetNewTempDirectory().FullName;
        }

        public static string CreateRootPackage(TemporaryFileManager manager, string json)
        {
            var dir = CreateRootPackageDir(manager);
            var path = Path.Combine(dir, "package.json");
            CreatePackageJson(path, json);
            return dir;
        }
    }
}
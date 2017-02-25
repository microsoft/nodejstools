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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Handles file name mapping while remote debugging.
    /// </summary>
    internal sealed class FuzzyLogicFileNameMapper : IFileNameMapper
    {
        private readonly ScriptTree _scripts = new ScriptTree(null);

        /// <summary>
        /// Constructs mapping based on list project files.
        /// </summary>
        /// <param name="files">List of project files.</param>
        public FuzzyLogicFileNameMapper(IEnumerable<string> files)
        {
            foreach (var fileName in files)
            {
                AddModuleToTree(fileName);
            }
        }

        /// <summary>
        /// Returns a local file name for a remote.
        /// </summary>
        /// <param name="remoteFileName">Remote file name.</param>
        /// <returns>Local file name.</returns>
        public string GetLocalFileName(string remoteFileName)
        {
            // Try to find best file name match
            var pathComponents = GetPathComponents(remoteFileName);
            var curTree = this._scripts;

            // Walk up the remote path, matching it against known local files.
            var matchedCount = 0;
            foreach (var component in pathComponents.Reverse())
            {
                ScriptTree nextTree;
                if (!curTree.Parents.TryGetValue(component, out nextTree))
                {
                    // Can't walk up the local tree any further - this means that we're at the point at which local and remote
                    // filesystems begin to differ, yet we have more than one candidate.

                    // If we haven't even matched the filename yet, then this is a module that is not a part of the project
                    // (e.g. a built-in module), so we can't map it at all, and should just return it as is.
                    if (matchedCount == 0)
                    {
                        return remoteFileName;
                    }

                    // Otherwise, the right candidate is the one that's closest to that point (i.e. shortest path).
                    // For example, if remote path is:
                    //      /wwwroot/bbb/ccc.js"
                    // and local files are:
                    //      C:\MyProject\bbb\ccc.js
                    //      C:\MyProject\aaa\bbb\ccc.js
                    // then we stop at wwwroot, as it doesn't map to anything locally, but both files are still candidates -
                    // and we want the first one rather than the second one.
                    return curTree.Children.OrderBy(s => s.Length).FirstOrDefault() ?? remoteFileName;
                }

                // Short-circuit the walk if we end up with only a single candidate at any point.
                if (nextTree.Children.Count == 1)
                {
                    return nextTree.Children.First();
                }

                curTree = nextTree;
                ++matchedCount;
            }

            return remoteFileName;
        }

        private void AddModuleToTree(string fileName)
        {
            var curTree = this._scripts;
            var pathComponents = GetPathComponents(fileName);
            foreach (var component in pathComponents.Reverse())
            {
                ScriptTree nextTree;
                if (!curTree.Parents.TryGetValue(component, out nextTree))
                {
                    curTree.Parents[component] = nextTree = new ScriptTree(component);
                }

                curTree.Children.Add(fileName);
                curTree = nextTree;
            }
        }

        private static IEnumerable<string> GetPathComponents(string path)
        {
            return path.Split('\\', '/', ':');
        }
    }
}
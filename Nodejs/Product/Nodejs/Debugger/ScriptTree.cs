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

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Stores all of the scripts which are loaded in the debuggee in reverse order based
    /// upon their file components.  The first entry is the filename, then the parent directory,
    /// then parent of that directory, etc...
    /// This is used to do fuzzy filename matching when a breakpoint is hit.
    /// </summary>
    internal sealed class ScriptTree
    {
        public readonly HashSet<string> Children = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly string Filename;
        public readonly Dictionary<string, ScriptTree> Parents = new Dictionary<string, ScriptTree>(StringComparer.OrdinalIgnoreCase);

        public ScriptTree(string filename)
        {
            Filename = filename;
        }
    }
}
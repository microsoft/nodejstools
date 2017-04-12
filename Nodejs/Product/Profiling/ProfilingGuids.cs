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

// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.NodejsTools.Profiling {
    internal static class ProfilingGuids {
        // Profiling guids
        public const string NodejsProfilingPkgString = "B515653F-FB69-4B64-9D3F-F1FCF8421DD0";
        public const string NodejsProfilingCmdSetString = "3F2BC93C-CA2D-450B-9BFC-0C96288F1ED6";
        public const string ProfilingEditorFactoryString = "3585dc22-81a0-409e-85ae-cae5d02d99cd";

        public static readonly Guid NodejsProfilingPkg = new Guid(NodejsProfilingPkgString);
        public static readonly Guid NodejsProfilingCmdSet = new Guid(NodejsProfilingCmdSetString);
        public static readonly Guid ProfilingEditorFactory = new Guid(ProfilingEditorFactoryString);
    }
}

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
using System.Linq;
using System.Text;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    public static class ReplConstants {
#if NTVS_FEATURE_INTERACTIVEWINDOW
        public const string ReplContentTypeName = "NodejsREPLCode";
        public const string ReplOutputContentTypeName = "NodejsREPLOutput";

        /// <summary>
        /// The additional role found in any REPL editor window.
        /// </summary>
        public const string ReplTextViewRole = "NodejsREPL";
#else
        public const string ReplContentTypeName = "REPLCode";
        public const string ReplOutputContentTypeName = "REPLOutput";

        /// <summary>
        /// The additional role found in any REPL editor window.
        /// </summary>
        public const string ReplTextViewRole = "REPL";
#endif
    }
}

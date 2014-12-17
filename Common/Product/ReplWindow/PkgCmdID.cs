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

// PkgCmdID.cs
// MUST match PkgCmdID.h

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    static class PkgCmdIDList {
        public const uint cmdidSmartExecute = 0x103;
        public const uint cmdidBreakRepl = 0x104;
        public const uint cmdidResetRepl = 0x105;
        public const uint cmdidReplHistoryNext = 0x0106;
        public const uint cmdidReplHistoryPrevious = 0x0107;
        public const uint cmdidReplClearScreen = 0x0108;
        public const uint cmdidBreakLine = 0x0109;
        public const uint cmdidReplSearchHistoryNext = 0x010A;
        public const uint cmdidReplSearchHistoryPrevious = 0x010B;
        public const uint menuIdReplToolbar = 0x2000;

        public const uint comboIdReplScopes = 0x3000;
        public const uint comboIdReplScopesGetList = 0x3001;
    };
}
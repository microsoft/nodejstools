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
using System.Threading.Tasks;

namespace Microsoft.NodejsTools {
    class PkgCmdId {
        public const int cmdidReplWindow                    = 0x200;
        public const int cmdidOpenReplWindow                = 0x201;
        public const int cmdidOpenRemoteDebugProxyFolder    = 0x202;
        public const int cmdidSetAsNodejsStartupFile        = 0x203;

        public const int cmdidSurveyNews                    = 0x204;
        public const int cmdidImportWizard                  = 0x205;
        public const int cmdidOpenRemoteDebugDocumentation  = 0x206;

        public const uint cmdidAzureExplorerAttachNodejsDebugger = 0x207;

        public const int cmdidDiagnostics                   = 0x208;
        public const int cmdidSetAsContent                  = 0x209;
        public const int cmdidSetAsCompile                  = 0x210;

        public const int cmdidNpmManageModules              = 0x300;
        public const int cmdidNpmInstallModules             = 0x301;
        public const int cmdidNpmUpdateModules              = 0x302;
        public const int cmdidNpmUninstallModule            = 0x303;
        public const int cmdidNpmInstallSingleMissingModule = 0x304;
        public const int cmdidNpmUpdateSingleModule         = 0x305;
        public const int cmdidNpmOpenModuleHomepage         = 0x306;
        public const int menuIdNpm                          = 0x3000;
    }
}

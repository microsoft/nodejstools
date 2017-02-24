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
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Extension to Vs Launch Debugger to handle js files from a Node Js project
    /// </summary>
    [ExportVsDebugLaunchTarget(ProviderType, new[] { ".js" }, ProviderPriority.Highest)]
    internal class NodeJsDebugLaunchProvider : IVsDebugLaunchTargetProvider
    {
        private const string ProviderType = "6C01D598-DE83-4D5B-B7E5-757FBA8443DD";
        private const string NodeExeKey = "nodeExe";

        private const string NodeJsSchema =
@"{
  ""definitions"": {
    ""nodejs"": {
      ""type"": ""object"",
      ""properties"": {
        ""type"": {""type"": ""string"",""enum"": [ ""nodejs"" ]},
        ""nodeExe"": { ""type"": ""string"" }
      }
    },
    ""nodejsFile"": {
      ""allOf"": [
        { ""$ref"": ""#/definitions/default"" },
        { ""$ref"": ""#/definitions/nodejs"" }
      ]
    }
  },
    ""defaults"": {
        ""nodejs"": { ""$ref"": ""#/definitions/nodejs"" }
    },
    ""configuration"": ""#/definitions/nodejsFile""
}";

        public void SetupDebugTargetInfo(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext)
        {
            string target = vsDebugTargetInfo.bstrExe;
            vsDebugTargetInfo.bstrExe = debugLaunchContext.LaunchConfiguration.GetValue<string>(NodeExeKey, Nodejs.GetPathToNodeExecutableFromEnvironment());
            string nodeJsArgs = vsDebugTargetInfo.bstrArg;
            vsDebugTargetInfo.bstrArg = "\"" + target + "\"";
            if (!string.IsNullOrEmpty(nodeJsArgs))
            {
                vsDebugTargetInfo.bstrArg += " ";
                vsDebugTargetInfo.bstrArg += nodeJsArgs;
            }

            vsDebugTargetInfo.clsidCustom = DebugEngine.AD7Engine.DebugEngineGuid;
            vsDebugTargetInfo.bstrOptions = "WAIT_ON_ABNORMAL_EXIT=true";
            vsDebugTargetInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        [ExportLaunchConfigurationProvider(LaunchConfigurationProviderType, new[] { ".js" }, "nodejs", NodeJsSchema)]
        public class LaunchConfigurationProvider : ILaunchConfigurationProvider
        {
            private const string LaunchConfigurationProviderType = "1DB21619-2C53-4BEF-84E4-B1C4D6771A51";

            public void CustomizeLaunchConfiguration(DebugLaunchActionContext debugLaunchActionContext, IPropertySettings launchSettings)
            {
                // noop
            }

            /// <inheritdoc />
            public bool IsDebugLaunchActionSupported(DebugLaunchActionContext debugLaunchActionContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
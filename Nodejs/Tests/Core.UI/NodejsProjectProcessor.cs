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

using System.ComponentModel.Composition;
using TestUtilities.SharedProject;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.Nodejs.Tests.UI {
    [Export(typeof(IProjectProcessor))]
    [ProjectExtension(".njsproj")]
    public class NodejsProjectProcessor : IProjectProcessor {
        public void PreProcess(MSBuild.Project project) {
            if (project.ProjectFileLocation.File.EndsWith(".user", System.StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            // Node.js projects are flavored which enables a lot of our functionality, so
            // setup our flavor.
            project.SetProperty("ProjectTypeGuids", "{3AF33F2E-1136-4D97-BBB7-1795711AC8B8};{349c5851-65df-11da-9384-00065b846f21};{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}");

            var prop = project.SetProperty("VisualStudioVersion", "11.0");

            project.Xml.AddProperty("VisualStudioVersion", "11.0").Condition = "'$(VisualStudioVersion)' == ''";
            project.Xml.AddProperty("VSToolsPath", "$(MSBuildExtensionsPath32)\\Microsoft\\VisualStudio\\v$(VisualStudioVersion)").Condition = "'$(VSToolsPath)' == ''";

            var import = project.Xml.AddImport("$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props");
            import.Condition = "Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')";
            project.Xml.AddImport("$(VSToolsPath)\\Node.js Tools\\Microsoft.NodejsTools.targets");

            project.SetProperty("ProjectHome", ".");
            project.SetProperty("WorkingDirectory", ".");
            project.SetProperty("ProjectView", "ShowAllFiles");
            project.SetProperty("OutputPath", ".");
        }

        public void PostProcess(MSBuild.Project project) {
            var projectExt = project.Xml.CreateProjectExtensionsElement();
            projectExt.Content = @"
    <VisualStudio>
      <FlavorProperties GUID=""{349c5851-65df-11da-9384-00065b846f21}"">
        <WebProjectProperties>
          <UseIIS>False</UseIIS>
          <AutoAssignPort>True</AutoAssignPort>
          <DevelopmentServerPort>0</DevelopmentServerPort>
          <DevelopmentServerVPath>/</DevelopmentServerVPath>
          <IISUrl>http://localhost:48022/</IISUrl>
          <NTLMAuthentication>False</NTLMAuthentication>
          <UseCustomServer>True</UseCustomServer>
          <CustomServerUrl>http://localhost:1337</CustomServerUrl>
          <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
        </WebProjectProperties>
      </FlavorProperties>
      <FlavorProperties GUID=""{349c5851-65df-11da-9384-00065b846f21}"" User="""">
        <WebProjectProperties>
          <StartPageUrl>
          </StartPageUrl>
          <StartAction>CurrentPage</StartAction>
          <AspNetDebugging>True</AspNetDebugging>
          <SilverlightDebugging>False</SilverlightDebugging>
          <NativeDebugging>False</NativeDebugging>
          <SQLDebugging>False</SQLDebugging>
          <ExternalProgram>
          </ExternalProgram>
          <StartExternalURL>
          </StartExternalURL>
          <StartCmdLineArguments>
          </StartCmdLineArguments>
          <StartWorkingDirectory>
          </StartWorkingDirectory>
          <EnableENC>False</EnableENC>
          <AlwaysStartWebServerOnDebug>False</AlwaysStartWebServerOnDebug>
        </WebProjectProperties>
      </FlavorProperties>
    </VisualStudio>
";
            project.Xml.AppendChild(projectExt);
        }
    }
}

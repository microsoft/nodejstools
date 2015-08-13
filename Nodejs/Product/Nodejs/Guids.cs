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

namespace Microsoft.NodejsTools
{
    static class Guids
    {
        public const string NodejsPackageString = "FE8A8C3D-328A-476D-99F9-2A24B75F8C7F";
        public const string NodejsCmdSetString = "695e37e2-c6df-4e0a-8833-f688e4c65f1f";
        public const string NodejsDebugLanguageString = "{65791609-BA29-49CF-A214-DBFF8AEC3BC2}";
        public const string NodejsEditorFactoryString = "88941496-93F4-4E37-83AF-AFE087415334";
        public const string NodejsEditorFactoryPromptEncodingString = "C8576E92-EFB6-4414-8F63-C84D474A539E";
        public const string NodejsLanguageInfoString = "ABD5E8A5-5A35-4BE9-BCAF-E10C1212CB40";
        public const string NodejsNpmCmdSetString = "9F4B31B4-09AC-4937-A2E7-F4BC02BB7DBA";
        public const string NodejsProjectFactoryString = "3AF33F2E-1136-4D97-BBB7-1795711AC8B8";
        public const string NodejsBaseProjectFactoryString = "9092AA53-FB77-4645-B42D-1CCCA6BD08BD";
        public const string TypeScriptLanguageInfoString = "87bdf188-e6e8-4fcf-a82a-9b8506e01847";
        public const string JadeEditorFactoryString = "6CB69EF8-1329-4DC0-84B4-FA134EA59BE3";

        internal static readonly Guid DefaultLangaugeService = new Guid("{8239BEC4-EE87-11D0-8C98-00C04FC2AB22}");
 
        //Guid for our formatting service
        internal const string JavaScriptFormattingServiceString = "F414C260-6AC0-11CF-B6D1-00AA00BBBB58";

        public const string ScriptDebugLanguageString = "{F7FA31DA-C32A-11D0-B442-00A0244A1DD2}";

        // Profiling guids
        public const string NodejsProfilingPkgString = "B515653F-FB69-4B64-9D3F-F1FCF8421DD0";
        public const string NodejsProfilingCmdSetString = "3F2BC93C-CA2D-450B-9BFC-0C96288F1ED6";
        public const string ProfilingEditorFactoryString = "3585dc22-81a0-409e-85ae-cae5d02d99cd";

        // Debug guids
        public const string DebugEngine = "FC5B45BA-5B9C-46EA-887A-82073AE065FE";
        public const string DebugProgramProvider = "472CD331-218C-451E-929E-98C9408F11DD";
        public const string RemoteDebugPortSupplier = "A241707C-7DB3-464F-8D3E-F3D33E86AE99";

        public static readonly Guid NodejsBaseProjectFactory = new Guid(NodejsBaseProjectFactoryString);
        public static readonly Guid NodejsCmdSet = new Guid(NodejsCmdSetString);
        public static readonly Guid NodejsEditorFactory = new Guid(NodejsEditorFactoryString);
        public static readonly Guid NodejsDebugLanguage = new Guid(NodejsDebugLanguageString);
        public static readonly Guid NodejsNpmCmdSet = new Guid(NodejsNpmCmdSetString);
        public static readonly Guid TypeScriptDebugLanguage = new Guid(TypeScriptLanguageInfoString);
        
        public static readonly Guid ScriptDebugLanguage = new Guid(ScriptDebugLanguageString);

        public static readonly Guid VenusCommandId = new Guid("c7547851-4e3a-4e5b-9173-fa6e9c8bd82c");
        public static readonly Guid Eureka = new Guid("30947ebe-9147-45f9-96cf-401bfc671a82");  //  Microsoft.VisualStudio.Web.Eureka.dll package, includes page inspector
        public static readonly Guid WebPackageCommandId = new Guid("822e3603-e573-47d2-acf0-520e4ce641c2");
        public static readonly Guid WebPackage = new Guid("d9a342d1-a429-4059-808a-e55ee6351f7f");
        public static readonly Guid WebAppCmdId = new Guid("CB26E292-901A-419c-B79D-49BD45C43929");
                        
        public static readonly Guid NodejsProfilingCmdSet = new Guid(NodejsProfilingCmdSetString);
        public static readonly Guid VsUIHierarchyWindow = new Guid("{7D960B07-7AF8-11D0-8E5E-00A0C911005A}");
        public static readonly Guid ProfilingEditorFactory = new Guid(ProfilingEditorFactoryString);
        public static readonly Guid PerfPkg = new Guid("{F4A63B2A-49AB-4b2d-AA59-A10F01026C89}");

        public const string OfficeToolsBootstrapperCmdSetString = "{D26C976C-8EE8-4EC4-8746-F5F7702A17C5}";
        public static readonly Guid OfficeToolsBootstrapperCmdSet = new Guid(OfficeToolsBootstrapperCmdSetString);
    };
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// MUST match guids.h

using System;

namespace Microsoft.NodejsTools
{
    internal static class Guids
    {
        // note: When you use a guid in a GuidAttribute it can not have '{' and '}'. The compiler doesn't like that.
        //       So we prefer to have no braces in any Guid strings.

        public const string NodejsPackageString = "FE8A8C3D-328A-476D-99F9-2A24B75F8C7F";
        public const string NodejsCmdSetString = "695e37e2-c6df-4e0a-8833-f688e4c65f1f";
        public const string NodejsNpmCmdSetString = "9F4B31B4-09AC-4937-A2E7-F4BC02BB7DBA";
        public const string NodeToolsWorkspaceCmdSetString = "0D701F33-94A3-421C-9865-6F65E7E4B689";
        public const string NodejsProjectFactoryString = "3AF33F2E-1136-4D97-BBB7-1795711AC8B8";
        public const string NodejsBaseProjectFactoryString = "9092AA53-FB77-4645-B42D-1CCCA6BD08BD";
        public const string TypeScriptLanguageInfoString = "4a0dddb5-7a95-4fbf-97cc-616d07737a77";
        public const string JadeEditorFactoryString = "6CB69EF8-1329-4DC0-84B4-FA134EA59BE3";
        public const string DefaultLanguageServiceString = "8239BEC4-EE87-11D0-8C98-00C04FC2AB22";

        public static readonly Guid DefaultLanguageService = new Guid(DefaultLanguageServiceString);

        //Guid for our formatting service
        internal const string JavaScriptFormattingServiceString = "F414C260-6AC0-11CF-B6D1-00AA00BBBB58";

        // Debug guids
        // However the debugger requires the '{' and '}'
        public const string NodejsDebugLanguageString = "{65791609-BA29-49CF-A214-DBFF8AEC3BC2}";
        public const string TypeScriptDebuggerLanguageInfoString = "{87bdf188-e6e8-4fcf-a82a-9b8506e01847}";
        public const string ScriptDebugLanguageString = "{F7FA31DA-C32A-11D0-B442-00A0244A1DD2}";

        public const string DebugProgramProvider = "472CD331-218C-451E-929E-98C9408F11DD";
        public const string DebugEngine = "FC5B45BA-5B9C-46EA-887A-82073AE065FE";
        public const string RemoteDebugPortSupplier = "A241707C-7DB3-464F-8D3E-F3D33E86AE99";

        public static readonly Guid NodejsBaseProjectFactory = new Guid(NodejsBaseProjectFactoryString);
        public static readonly Guid NodejsCmdSet = new Guid(NodejsCmdSetString);
        public static readonly Guid NodejsDebugLanguage = new Guid(NodejsDebugLanguageString);
        public static readonly Guid NodejsNpmCmdSet = new Guid(NodejsNpmCmdSetString);
        public static readonly Guid NodeToolsWorkspaceCmdSet = new Guid(NodeToolsWorkspaceCmdSetString);

        public static readonly Guid TypeScriptLanguageInfo = new Guid(TypeScriptLanguageInfoString);
        public static readonly Guid TypeScriptDebugLanguage = new Guid(TypeScriptDebuggerLanguageInfoString);

        public static readonly Guid ScriptDebugLanguage = new Guid(ScriptDebugLanguageString);

        public static readonly Guid VenusCommandId = new Guid("c7547851-4e3a-4e5b-9173-fa6e9c8bd82c");
        public static readonly Guid Eureka = new Guid("30947ebe-9147-45f9-96cf-401bfc671a82");  //  Microsoft.VisualStudio.Web.Eureka.dll package, includes page inspector
        public static readonly Guid WebPackageCommandId = new Guid("822e3603-e573-47d2-acf0-520e4ce641c2");
        public static readonly Guid WebPackage = new Guid("d9a342d1-a429-4059-808a-e55ee6351f7f");
        public static readonly Guid WebAppCmdId = new Guid("CB26E292-901A-419c-B79D-49BD45C43929");

        public static readonly Guid VsUIHierarchyWindow = new Guid("7D960B07-7AF8-11D0-8E5E-00A0C911005A");

        public static readonly Guid PerfPkg = new Guid("F4A63B2A-49AB-4b2d-AA59-A10F01026C89");

        public const string OfficeToolsBootstrapperCmdSetString = "D26C976C-8EE8-4EC4-8746-F5F7702A17C5";
        public static readonly Guid OfficeToolsBootstrapperCmdSet = new Guid(OfficeToolsBootstrapperCmdSetString);

        public static readonly Guid ConnectedServicesCmdSet = new Guid("A114CF9C-BD45-4A48-92EF-D9BBBC0B3DF0");
        public static readonly Guid NuGetManagerCmdSet = new Guid("25fd982b-8cae-4cbd-a440-e03ffccde106");

        // UWP project flavor guid
        public const string NodejsUwpProjectFlavor = "00251F00-BA30-4CE4-96A2-B8A1085F37AA";

        // Workspace guids
        public const string PackageJsonContextTypeString = "78F43160-2968-4FCA-8829-7E30E9B610CF";
        public readonly static Guid PackageJsonContextType = new Guid(PackageJsonContextTypeString);

        public const string TypeScriptContextTypeString = "0E78FB31-F2A2-4AB8-A93D-1D123B3F677B";
        public readonly static Guid TypeScriptContextType = new Guid(TypeScriptContextTypeString);

        public const string WorkspaceExplorerDebugActionCmdSetString = "5ea148a6-40af-4ff2-ab0f-60ed173c9f98";
        public readonly static Guid WorkspaceExplorerDebugActionCmdSet = new Guid(WorkspaceExplorerDebugActionCmdSetString);
        public const string GuidWorkspaceExplorerBuildActionCmdSetString = "16537f6e-cb14-44da-b087-d1387ce3bf57";
        public static readonly Guid GuidWorkspaceExplorerBuildActionCmdSet = new Guid(GuidWorkspaceExplorerBuildActionCmdSetString);
        
        // Interactive window guids
        public const string NodejsInteractiveWindowString = "2153A414-267E-4731-B891-E875ADBA1993";
        public static readonly Guid NodejsInteractiveWindow = new Guid(NodejsInteractiveWindowString);
        
        public const string NodeExtrasPackageString = "64BADECB-C679-4D59-944A-A3A46FB53E31";
    }
}

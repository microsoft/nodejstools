// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.NodejsTools
{
    public static class NodejsConstants
    {
        public const string JavaScriptExtension = ".js";
        public const string JavaScriptJsxExtension = ".jsx";
        public const string TypeScriptExtension = ".ts";
        public const string TypeScriptJsxExtension = ".tsx";
        public const string TypeScriptDeclarationExtension = ".d.ts";
        public const string MapExtension = ".map";

        public const string NodejsProjectExtension = ".njsproj";
        public const string CSharpProjectExtension = ".csproj";
        public const string VisualBasicProjectExtension = ".vbproj";
        public const string JavaScriptProjectExtension = ".esproj";

        public const string JavaScript = "JavaScript";
        public const string CSS = "CSS";
        public const string HTML = "HTML";
        public const string Nodejs = "Node.js";

        public const string ProjectFileFilter = "Node.js Project File (*.njsproj)\n*.njsproj\nAll Files (*.*)\n*.*\n";

        public const string NodeModulesFolder = "node_modules";
        public const string NodeModulesFolderWithSeparators = "\\" + NodeModulesFolder + "\\";
        public const string NodeModulesStagingFolder = "node_modules\\.staging\\";
        public const string BowerComponentsFolder = "bower_components";

        /// <summary>
        /// The name of the package.json file
        /// </summary>
        public const string PackageJsonFile = "package.json";
        public const string PackageJsonMainFileKey = "main";
        public const string DefaultPackageMainFile = "index.js";

        public const string TsConfigJsonFile = "tsconfig.json";
        public const string JsConfigJsonFile = "jsconfig.json";

        public const string BaseRegistryKey = "NodejsTools";

        public const ushort DefaultDebuggerPort = 5858;

        public const string NoneItemType = "None";
        public const string ContentItemType = "Content";

        [Obsolete("This property will be removed as an effort to eliminate the TypeScript SDK. Use \"None\" item type instead.")]
        public const string TypeScriptCompileItemType = "TypeScriptCompile";
        [Obsolete("This property will be removed as an effort to eliminate the TypeScript SDK. Consider designing for tsconfig.json instead.")]
        public const string CommonJSModuleKind = "CommonJS";
        public const string TypeScript = "TypeScript";

        public const string NodeToolsProcessIdEnvironmentVariable = "_NTVS_PID";
        public const string NodeToolsVsInstallRootEnvironmentVariable = "_NTVS_VSINSTALLROOT";

        public static string NtvsLocalAppData => Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft",
                    "Node.js Tools");

        /// <summary>
        /// Path to the private package where NTVS tools are installed.
        /// </summary>
        public static string ExternalToolsPath => Path.Combine(NtvsLocalAppData, "ExternalTools");
        /// <summary>
        /// Path to where NTVS caches Npm data.
        /// </summary>
        public static string NpmCachePath => Path.Combine(NtvsLocalAppData, "NpmCache");

        /// <summary>
        /// Checks whether a relative and double-backslashed seperated path contains a folder name.
        /// </summary>
        public static bool ContainsNodeModulesOrBowerComponentsFolder(string path)
        {
            var tmp = "\\" + path + "\\";
            return tmp.IndexOf("\\" + NodeModulesFolder + "\\", StringComparison.OrdinalIgnoreCase) >= 0
                || tmp.IndexOf("\\" + BowerComponentsFolder + "\\", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private const string TestRootDataValueGuidString = "{FF41BE7F-6D8C-4D27-91D4-51E4233BC7E4}";
        public readonly static Guid TestRootDataValueGuid = new Guid(TestRootDataValueGuidString);

        public const string TestRootDataValueName = nameof(TestRootDataValueName);

        public const string ChromeApplicationName = "chrome.exe";
    }

    internal static class NodeProjectProperty
    {
        public const string DebuggerPort = "DebuggerPort";
        public const string EnableTypeScript = "EnableTypeScript";
        public const string Environment = "Environment";
        public const string EnvironmentVariables = "EnvironmentVariables";
        public const string LaunchUrl = "LaunchUrl";
        public const string NodeExeArguments = "NodeExeArguments";
        public const string NodeExePath = "NodeExePath";
        public const string NodejsPort = "NodejsPort";
        public const string ScriptArguments = "ScriptArguments";
        public const string StartWebBrowser = "StartWebBrowser";
        public const string TypeScriptCfgProperty = "CfgPropertyPagesGuidsAddTypeScript";
        [Obsolete("This property is part of the TypeScript SDK which is planned for removal. Instead consider designing for using tsconfig.json")]
        public const string TypeScriptModuleKind = "TypeScriptModuleKind";
        [Obsolete("This property is part of the TypeScript SDK which is planned for removal. Instead consider designing for using tsconfig.json. Note that this property is also used on the TestAdapter.")]
        public const string TypeScriptOutDir = "TypeScriptOutDir";
        [Obsolete("This property is part of the TypeScript SDK which is planned for removal. Instead consider designing for using tsconfig.json")]
        public const string TypeScriptSourceMap = "TypeScriptSourceMap";
        public const string SaveNodeJsSettingsInProjectFile = "SaveNodeJsSettingsInProjectFile";
        public const string TestRoot = "JavaScriptTestRoot";
        public const string TestFramework = "JavaScriptTestFramework";
    }
}

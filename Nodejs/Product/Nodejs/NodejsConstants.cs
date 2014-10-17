/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace Microsoft.NodejsTools {
    internal class NodejsConstants {
        internal const string JavaScriptExtension = ".js";
        internal const string TypeScriptExtension = ".ts";
        internal const string MapExtension = ".map";
        internal const string NodejsProjectExtension = ".njsproj";

        internal const string JavaScript = "JavaScript";
        internal const string CSS = "CSS";
        internal const string Nodejs = "Node.js";

        internal const string IssueTrackerUrl = "https://go.microsoft.com/fwlink/?LinkId=507637";

        internal const string DebuggerPort = "DebuggerPort";
        internal const string Environment = "Environment";
        internal const string EnvironmentVariables = "EnvironmentVariables";
        internal const string LaunchUrl = "LaunchUrl";
        internal const string NodeExePath = "NodeExePath";
        internal const string NodeExeArguments = "NodeExeArguments";
        internal const string NodejsPort = "NodejsPort";
        internal const string ProjectFileFilter = "Node.js Project File (*.njsproj)\n*.njsproj\nAll Files (*.*)\n*.*\n";
        internal const string ScriptArguments = "ScriptArguments";
        internal const string StartWebBrowser = "StartWebBrowser";

        internal const string NodeModulesFolder = "node_modules";
        internal const string AnalysisIgnoredDirectories = "AnalysisIgnoredDirectories";
        internal const string DefaultIntellisenseCompletionCommittedBy = "{}[]().,:;+-*/%&|^!~=<>?@#'\"\\";

        /// <summary>
        /// The name of the package.json file
        /// </summary>
        internal const string PackageJsonFile = "package.json";
        internal const string PackageJsonMainFileKey = "main";
        internal const string DefaultPackageMainFile = "index.js";

        internal const string BaseRegistryKey = "NodejsTools";

        internal const string NodejsHiddenUserModule = "nodejs_tools_for_visual_studio_hidden_usermodule_";
        internal const string NodejsHiddenUserModuleInstance = "nodejs_tools_for_visual_studio_hidden_module_instance_";

        internal const string TypeScriptCfgProperty = "CfgPropertyPagesGuidsAddTypeScript";

        internal const ushort DefaultDebuggerPort = 5858;

        internal const string TypeScriptCompileItemType = "TypeScriptCompile";
        internal const string EnableTypeScript = "EnableTypeScript";
        internal const string TypeScriptSourceMap = "TypeScriptSourceMap";
        internal const string TypeScriptModuleKind = "TypeScriptModuleKind";
        internal const string CommonJSModuleKind = "CommonJS";
        internal const string TypeScript = "TypeScript";
    }
}

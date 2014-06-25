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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools {
    class NodejsConstants {
        public const string FileExtension = ".js";
        public const string TypeScriptExtension = ".ts";
        
        public const string JavaScript = "JavaScript";
        public const string CSS = "CSS";
        public const string Nodejs = "Node.js";
        public const string NodejsRepl = "Node.jsRepl";

        public const string DebuggerPort = "DebuggerPort";
        public const string EnvironmentVariables = "EnvironmentVariables";
        public const string LaunchUrl = "LaunchUrl";
        public const string NodeExePath = "NodeExePath";
        public const string NodeExeArguments = "NodeExeArguments";
        public const string NodejsPort = "NodejsPort";
        public const string ProjectFileFilter = "Node.js Project File (*.njsproj)\n*.njsproj\nAll Files (*.*)\n*.*\n";
        public const string ScriptArguments = "ScriptArguments";
        public const string StartWebBrowser = "StartWebBrowser";

        public const string NodeModulesFolder = "node_modules";
        
        /// <summary>
        /// The name of the package.json file
        /// </summary>
        public const string PackageJsonFile = "package.json";
        public const string PackageJsonMainFileKey = "main";
        public const string DefaultPackageMainFile = "index.js";

        public const string BaseRegistryKey = "NodejsTools";

        public const string NodejsHiddenUserModule = "nodejs_tools_for_visual_studio_hidden_usermodule_";
        public const string NodejsHiddenUserModuleInstance = "nodejs_tools_for_visual_studio_hidden_module_instance_";

        public const string TypeScriptCfgProperty = "CfgPropertyPagesGuidsAddTypeScript";

        public const ushort DefaultDebuggerPort = 5858;

        [Export, Name(Nodejs), BaseDefinition("text")]
        internal static ContentTypeDefinition ContentTypeDefinition = null;

        [Export, Name(NodejsRepl), BaseDefinition(Nodejs), BaseDefinition("JavaScript")]
        internal static ContentTypeDefinition ReplContentTypeDefinition = null;

        public const string TypeScriptCompileItemType = "TypeScriptCompile";
        public const string EnableTypeScript = "EnableTypeScript";
        public const string TypeScriptSourceMap = "TypeScriptSourceMap";
        public const string TypeScriptModuleKind = "TypeScriptModuleKind";
        public const string CommonJSModuleKind = "CommonJS";
        public const string TypeScript = "TypeScript";
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IReplCommand))]
    internal class InfoReplCommand : IReplCommand
    {
        public Task<ExecutionResult> Execute(IReplWindow window, string arguments)
        {
            if (window.Evaluator is NodejsReplEvaluator nodeEval)
            {
                try
                {
                    var nodeExePath = nodeEval.NodeExePath;
                    if (string.IsNullOrEmpty(nodeExePath))
                    {
                        nodeExePath = nodeEval.GetNodeExePath();
                    }
                    var nodeVersion = FileVersionInfo.GetVersionInfo(nodeExePath);

                    window.WriteLine($"Using Node.js exe from: '{nodeExePath}'");
                    window.WriteLine($"Node.js Version: {nodeVersion.ProductVersion}");
                }
                catch(Exception e)
                {
                    window.WriteLine("Failed to retrieve Nodejs.exe information.");
                    window.WriteError(e);
                }
            }

            return ExecutionResult.Succeeded;
        }

        public string Description => Resources.ReplInfoDescription;

        public string Command => "info";

        public object ButtonContent => null;
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Utilities;
using Microsoft.NodejsTools.Extras;


namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IInteractiveWindowCommand))]
    [ContentType(InteractiveWindowContentType.ContentType)]
    internal sealed class InfoReplCommand : InteractiveWindowCommand
    {
        public override Task<ExecutionResult> Execute(IInteractiveWindow window, string arguments)
        {
            if (window.Evaluator is NodejsReplEvaluator nodeEval)
            {
                try
                {
                    if (!nodeEval.EnsureNodeInstalled())
                    {
                        return ExecutionResult.Failed;

                    }
                    var nodeExePath = nodeEval.NodeExePath;
                    var nodeVersion = FileVersionInfo.GetVersionInfo(nodeExePath);

                    window.WriteLine(string.Format(CultureInfo.CurrentUICulture, Resources.ReplNodeInfo, nodeExePath));
                    window.WriteLine(string.Format(CultureInfo.CurrentUICulture, Resources.ReplNodeVersion, nodeVersion.ProductVersion));
                }
                catch (Exception e)
                {
                    window.WriteLine(Resources.ReplNodeError);
                    window.WriteError(e.Message);
                    return ExecutionResult.Failed;
                }
            }
            return ExecutionResult.Succeeded;
        }

        public override string Description => Resources.ReplInfoDescription;

        public override string Command => "info";
    }
}

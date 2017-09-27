// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IReplCommand))]
    internal class EchoReplCommand : IReplCommand
    {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                var curValue = (bool)window.GetOptionValue(ReplOptions.ShowOutput);
                window.WriteLine(string.Format(Resources.EchoState, curValue ? "\'ON\'" : "\'OFF\'"));
                return ExecutionResult.Succeeded;
            }

            if (arguments.Equals("on", System.StringComparison.InvariantCultureIgnoreCase))
            {
                window.SetOptionValue(ReplOptions.ShowOutput, true);
                return ExecutionResult.Succeeded;
            }
            else if (arguments.Equals("off", System.StringComparison.InvariantCultureIgnoreCase))
            {
                window.SetOptionValue(ReplOptions.ShowOutput, false);
                return ExecutionResult.Succeeded;
            }

            //Any other value passed to .echo we treat as a message
            window.WriteLine(arguments);

            return ExecutionResult.Succeeded;
        }

        public string Description
        {
            get { return Resources.EchoDescription; }
        }

        public string Command
        {
            get { return "echo"; }
        }

        public object ButtonContent
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}

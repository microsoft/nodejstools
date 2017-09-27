// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Repl
{
#if INTERACTIVE_WINDOW
    using IReplCommand = IInteractiveWindowCommand;
    using IReplWindow = IInteractiveWindow;    
#endif

    [Export(typeof(IReplCommand))]
    internal class ClearReplCommand : IReplCommand
    {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments)
        {
            ((NodejsReplEvaluator)window.Evaluator).Clear();
            return ExecutionResult.Succeeded;
        }

        public string Description => Resources.ReplClearDescription;
        public string Command => "clear";
        public object ButtonContent => null;

        #endregion
    }
}

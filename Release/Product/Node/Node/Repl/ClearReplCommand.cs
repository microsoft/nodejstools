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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.NodejsTools.Repl;

namespace Microsoft.VisualStudio.Repl {
#if INTERACTIVE_WINDOW
    using IReplCommand = IInteractiveWindowCommand;
    using IReplWindow = IInteractiveWindow;    
#endif

    [Export(typeof(IReplCommand))]
    class ClearReplCommand : IReplCommand {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments) {
            ((NodeReplEvaluator)window.Evaluator).Clear();
            return ExecutionResult.Succeeded;
        }

        public string Description {
            get { return "Resets the context object to an empty object and clears any multi-line expression."; }
        }

        public string Command {
            get { return "clear"; }
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}

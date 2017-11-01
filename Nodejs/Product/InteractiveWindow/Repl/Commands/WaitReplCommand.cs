// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IReplCommand))]
    internal class WaitReplCommand : IReplCommand
    {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments)
        {
            var delay = new TimeSpan(0, 0, 0, 0, int.Parse(arguments));
            var start = DateTime.UtcNow;
            while ((start + delay) > DateTime.UtcNow)
            {
                var frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action<DispatcherFrame>(f => f.Continue = false),
                    frame
                    );
                Dispatcher.PushFrame(frame);
            }
            return ExecutionResult.Succeeded;
        }

        public string Description
        {
            get { return Resources.WaitDescription; }
        }

        public string Command
        {
            get { return "wait"; }
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

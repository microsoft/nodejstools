// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal sealed partial class TestExecutorWorker
    {
        private sealed class TestExecutionRedirector : Redirector
        {
            private readonly Action<string> writer;

            public TestExecutionRedirector(Action<string> onWriteLine)
            {
                this.writer = onWriteLine;
            }

            public override void WriteErrorLine(string line) => this.writer(line);

            public override void WriteLine(string line) => this.writer(line);

            public override bool CloseStandardInput() => false;
        }
    }
}

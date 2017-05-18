// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text;

namespace TestUtilities
{
    public class OutputReceiver
    {
        public readonly StringBuilder Output = new StringBuilder();

        public void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Output.AppendLine(e.Data);
            }
        }
    }
}


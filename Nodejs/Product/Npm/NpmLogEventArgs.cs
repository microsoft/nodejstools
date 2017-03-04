// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmLogEventArgs : EventArgs
    {
        public NpmLogEventArgs(string logText)
        {
            LogText = logText;
        }

        public string LogText { get; private set; }
    }
}


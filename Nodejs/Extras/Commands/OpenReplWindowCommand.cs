// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Extras;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands
{
    internal sealed class OpenReplWindowCommand : Command
    {
        public const int cmdidReplWindow = 0x200;

        public override void DoCommand(object sender, EventArgs args)
        {
            NodeExtrasPackage.Instance.OpenReplWindow();
        }

        public override int CommandId => (int)cmdidReplWindow;
    }
}

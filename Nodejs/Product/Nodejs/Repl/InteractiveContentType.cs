// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Repl
{
    internal static class ReplConstants
    {
        public const string ContentType = "Nodejs Interactive";

        [Export, Name(ContentType), BaseDefinition(PredefinedInteractiveCommandsContentTypes.InteractiveCommandContentTypeName)]
        internal static ContentTypeDefinition ContentTypeDefinition = null;
    }
}

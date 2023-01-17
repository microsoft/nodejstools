// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Repl
{
    internal static class InteractiveWindowContentType
    {
        /// <summary>
        /// Used for binding the commands to the interactive window.
        /// Note: we use the TypeScript content type for colorization.
        /// </summary>
        public const string ContentType = "NodeInteractiveWindow";

        [Export, Name(ContentType), BaseDefinition(PredefinedInteractiveCommandsContentTypes.InteractiveCommandContentTypeName)]
        internal static ContentTypeDefinition ContentTypeDefinition = null;
    }
}

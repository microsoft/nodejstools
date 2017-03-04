// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl
{
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    /// <summary>
    /// Provides the content type for our REPL error buffer.
    /// </summary>
    internal class ReplOutputContentType
    {
        [Export, Name(ReplConstants.ReplOutputContentTypeName), BaseDefinition("text")]
        internal static ContentTypeDefinition ContentTypeDefinition = null;
    }
}


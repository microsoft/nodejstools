// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl
{
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    public static class ReplConstants
    {
#if NTVS_FEATURE_INTERACTIVEWINDOW
        public const string ReplContentTypeName = "NodejsREPLCode";
        public const string ReplOutputContentTypeName = "NodejsREPLOutput";

        /// <summary>
        /// The additional role found in any REPL editor window.
        /// </summary>
        public const string ReplTextViewRole = "NodejsREPL";
#else
        public const string ReplContentTypeName = "REPLCode";
        public const string ReplOutputContentTypeName = "REPLOutput";

        /// <summary>
        /// The additional role found in any REPL editor window.
        /// </summary>
        public const string ReplTextViewRole = "REPL";
#endif
    }
}


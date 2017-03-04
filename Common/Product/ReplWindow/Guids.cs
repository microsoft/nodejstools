// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// MUST match guids.h

using System;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl
{
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    internal static class Guids
    {
#if NTVS_FEATURE_INTERACTIVEWINDOW
        public const string guidReplWindowPkgString = "29102E6C-34F2-4FF1-BA2F-C02ADE3846E8";
        public const string guidReplWindowCmdSetString = "220C57E5-228F-46B5-AF80-D0AB55A44902";
#else
        public const string guidReplWindowPkgString = "ce8d8e55-ad29-423e-aca2-810d0b16cdc4";
        public const string guidReplWindowCmdSetString = "68cb76e6-98c5-464a-aba9-9f2db66fa0fd";
#endif
        public static readonly Guid guidReplWindowCmdSet = new Guid(guidReplWindowCmdSetString);
    };
}


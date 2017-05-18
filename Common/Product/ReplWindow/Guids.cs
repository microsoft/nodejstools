// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// MUST match guids.h

using System;

namespace Microsoft.NodejsTools.Repl
{
    internal static class Guids
    {

        public const string guidReplWindowPkgString = "29102E6C-34F2-4FF1-BA2F-C02ADE3846E8";
        public const string guidReplWindowCmdSetString = "220C57E5-228F-46B5-AF80-D0AB55A44902";

        public static readonly Guid guidReplWindowCmdSet = new Guid(guidReplWindowCmdSetString);
    };
}

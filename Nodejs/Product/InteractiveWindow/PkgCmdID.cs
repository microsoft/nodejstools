// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// MUST match PkgCmdID.h


namespace Microsoft.NodejsTools.Repl
{

    internal static class PkgCmdIDList
    {
        public const uint cmdidSmartExecute = 0x103;
        public const uint cmdidBreakRepl = 0x104;
        public const uint cmdidResetRepl = 0x105;
        public const uint cmdidReplHistoryNext = 0x0106;
        public const uint cmdidReplHistoryPrevious = 0x0107;
        public const uint cmdidReplClearScreen = 0x0108;
        public const uint cmdidBreakLine = 0x0109;
        public const uint cmdidReplSearchHistoryNext = 0x010A;
        public const uint cmdidReplSearchHistoryPrevious = 0x010B;
        public const uint cmdidToolBarResetRepl = 0x010C;
        public const uint cmdidToolBarReplClearScreen = 0x010D;
        public const uint menuIdReplToolbar = 0x2000;

        public const uint comboIdReplScopes = 0x3000;
        public const uint comboIdReplScopesGetList = 0x3001;
    };
}

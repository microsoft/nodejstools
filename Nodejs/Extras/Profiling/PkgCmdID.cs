// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// MUST match PkgCmdID.h

namespace Microsoft.NodejsTools.Profiling
{
    internal static class PkgCmdIDList
    {
        public const uint cmdidStartNodeProfiling = 0x100;
        public const uint cmdidPerfExplorer = 0x101;
        public const uint cmdidAddPerfSession = 0x102;
        public const uint cmdidStartProfiling = 0x103;

        public const uint cmdidPerfCtxStartProfiling = 0x104;
        public const uint cmdidPerfCtxSetAsCurrent = 0x105;
        public const uint cmdidReportsCompareReports = 0x106;
        public const uint cmdidReportsAddReport = 0x107;
        public const uint cmdidOpenReport = 0x108;
        public const uint cmdidStopProfiling = 0x109;
        public const uint cmdidStartPerformanceAnalysis = 0x10A;

        public const uint menuIdPerfToolbar = 0x2000;
        public const uint menuIdPerfContext = 0x2001;
        public const uint menuIdPerfReportsContext = 0x2002;
        public const uint menuIdPerfSingleReportContext = 0x2003;
    };
}


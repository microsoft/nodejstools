// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// Guids.cs
// MUST match guids.h

using System;

namespace Microsoft.NodejsTools.Profiling
{
    internal static class ProfilingGuids
    {
        // Profiling guids
        public const string NodejsProfilingPkgString = "B515653F-FB69-4B64-9D3F-F1FCF8421DD0";
        public const string NodejsProfilingCmdSetString = "3F2BC93C-CA2D-450B-9BFC-0C96288F1ED6";
        public const string ProfilingEditorFactoryString = "3585dc22-81a0-409e-85ae-cae5d02d99cd";

        public static readonly Guid NodejsProfilingPkg = new Guid(NodejsProfilingPkgString);
        public static readonly Guid NodejsProfilingCmdSet = new Guid(NodejsProfilingCmdSetString);
        public static readonly Guid ProfilingEditorFactory = new Guid(ProfilingEditorFactoryString);
    }
}


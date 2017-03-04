// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// MUST match guids.h

using System;

namespace Microsoft.TestSccPackage
{
    internal static class Guids
    {
        public const string guidSccPackagePkgString = "394d1b85-f4a7-4af2-9078-e4aab7673b22";
        public const string guidSccPackageCmdSetString = "045cf08e-e640-42c4-af80-0251d6f553a1";

        public static readonly Guid guidSccPackageCmdSet = new Guid(guidSccPackageCmdSetString);
    };
}


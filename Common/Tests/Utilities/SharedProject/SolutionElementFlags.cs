// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace TestUtilities.SharedProject
{
    [Flags]
    public enum SolutionElementFlags
    {
        None,
        ExcludeFromSolution = 0x01,
        ExcludeFromConfiguration = 0x02
    }
}


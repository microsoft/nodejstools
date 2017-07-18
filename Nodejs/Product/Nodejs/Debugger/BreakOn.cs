// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    internal struct BreakOn
    {
        public readonly uint Count;
        public readonly BreakOnKind Kind;

        public BreakOn(BreakOnKind kind, uint count)
        {
            if (kind != BreakOnKind.Always && count < 1)
            {
                throw new ArgumentException("Invalid BreakOn count");
            }

            this.Kind = kind;
            this.Count = count;
        }
    }
}

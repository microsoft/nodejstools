// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl
{
    internal sealed class ColoredSpan
    {
        public readonly Span Span;
        public readonly ConsoleColor Color;

        public ColoredSpan(Span span, ConsoleColor color)
        {
            Span = span;
            Color = color;
        }
    }
}

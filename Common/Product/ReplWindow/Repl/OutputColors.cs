// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl
{
    internal sealed class ColoredSpan
    {
        public readonly Span Span;
        public readonly InteractiveWindowColor Color;

        public ColoredSpan(Span span, InteractiveWindowColor color)
        {
            Span = span;
            Color = color;
        }
    }
}

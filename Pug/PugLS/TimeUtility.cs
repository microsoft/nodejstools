// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudio.Pug
{
    internal static class TimeUtility
    {
        public static int MillisecondsSince(DateTime since)
        {
            var diff = DateTime.Now - since;
            return (int)diff.TotalMilliseconds;
        }
    }
}


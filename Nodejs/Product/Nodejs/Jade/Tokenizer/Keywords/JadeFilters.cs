// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Jade
{
    internal static class JadeFilters
    {
        public static bool IsFilter(string candidate)
        {
            var lower = candidate.ToLowerInvariant();
            return Array.BinarySearch<string>(_filters, lower) >= 0;
        }

        // must be sorted
        private static string[] _filters = new string[] {
            "cdata",
            "coffeescript",
            "less",
            "markdown",
            "sass",
            "stylus",
        };
    }
}

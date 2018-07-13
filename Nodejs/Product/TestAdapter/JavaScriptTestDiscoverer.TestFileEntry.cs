// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.TestAdapter
{
    public partial class JavaScriptTestDiscoverer
    {
        internal sealed class TestFileEntry
        {
            public readonly string FullPath;
            public readonly bool IsTypeScriptTest;

            public TestFileEntry(string fullPath, bool isTypeScriptTest)
            {
                this.FullPath = fullPath;
                this.IsTypeScriptTest = isTypeScriptTest;
            }
        }

        internal sealed class TestFileEntryComparer : IEqualityComparer<TestFileEntry>
        {
            public static readonly IEqualityComparer<TestFileEntry> Instance = new TestFileEntryComparer();

            private TestFileEntryComparer() { }

            bool IEqualityComparer<TestFileEntry>.Equals(TestFileEntry x, TestFileEntry y) => StringComparer.OrdinalIgnoreCase.Equals(x?.FullPath, y?.FullPath);

            int IEqualityComparer<TestFileEntry>.GetHashCode(TestFileEntry obj) => obj?.FullPath?.GetHashCode() ?? 0;
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.TestAdapter
{
    public partial class JavaScriptTestDiscoverer
    {
        private sealed class TestFileEntry
        {
            public readonly string File;
            public readonly bool IsTypeScriptTest;

            public TestFileEntry(string file, bool isTypeScriptTest)
            {
                this.File = file;
                this.IsTypeScriptTest = isTypeScriptTest;
            }
        }

        private struct TestFileEntryComparer : IEqualityComparer<TestFileEntry>
        {
            public bool Equals(TestFileEntry x, TestFileEntry y) => StringComparer.OrdinalIgnoreCase.Equals(x?.File, y?.File);

            public int GetHashCode(TestFileEntry obj) => obj?.File?.GetHashCode() ?? 0;
        }
    }
}

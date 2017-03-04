// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using Microsoft.NodejsTools;

namespace NodejsTests.Mocks
{
    internal class MockTextBuffer : TestUtilities.Mocks.MockTextBuffer
    {
        public MockTextBuffer(string content) :
            base(content: content, contentType: NodejsConstants.Nodejs)
        { }

        public MockTextBuffer(string content, string contentType, string filename = null) :
            base(content: content, contentType: contentType, filename: GetRandomFileNameIfNull(filename))
        { }

        private static string GetRandomFileNameIfNull(string filename)
        {
            return filename ?? Path.Combine(TestUtilities.TestData.GetTempPath(), Path.GetRandomFileName(), "file.js");
        }
    }
}


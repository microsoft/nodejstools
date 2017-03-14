// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.FileNameMapping
{
    [TestClass]
    public class LocalFileNameMapperTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetLocalFileNameTests()
        {
            // Arrange
            const string remoteFileName = "remoteFileName";
            var fileNameMapper = new LocalFileNameMapper();

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.AreEqual(remoteFileName, fileName);
        }
    }
}


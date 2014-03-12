/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.FileNameMapping {
    [TestClass]
    public class FuzzyLogicFileNameMapperTests {
        [TestMethod, Priority(0)]
        public void GetLocalFileNameForBuiltInModuleTests() {
            // Arrange
            const string remoteFileName = "node.js";
            var localFiles = new List<string>();
            var fileNameMapper = new FuzzyLogicFileNameMapper(localFiles);

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.AreEqual(remoteFileName, fileName);
        }

        [TestMethod, Priority(0)]
        public void GetLocalFileNameForRemoteModuleTests() {
            // Arrange
            const string remoteFileName = "/root/other/project/path/app.js";
            const string localFileName = @"c:\path\to\project\app.js";
            var localFiles = new List<string> { localFileName };
            var fileNameMapper = new FuzzyLogicFileNameMapper(localFiles);

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.AreEqual(localFileName, fileName);
        }

        [TestMethod, Priority(0)]
        public void GetLocalFileNameIfProjectContainsDuplicatesTests() {
            // Arrange
            const string remoteFileName = "/root/other/project/path/app.js";
            const string localFileName1 = @"c:\path\To\project\app.js";
            const string localFileName2 = @"c:\Path\to\project\app.js";
            var localFiles = new List<string> { localFileName1, localFileName2 };
            var fileNameMapper = new FuzzyLogicFileNameMapper(localFiles);

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.IsTrue(string.Equals(localFileName1, fileName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
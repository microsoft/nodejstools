// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools;
using System.IO;

namespace NodejsTests
{
    [TestClass]
    public class NodePathResolutionTests
    {
        [TestMethod, Priority(0)]
        public void CheckNodeExeEnvironmentResolution()
        {
            Assert.IsTrue(File.Exists(Nodejs.NodeExePath));
        }

        [TestMethod, Priority(0)]
        public void CheckRelativeNodeExePathResolution()
        {
            var path = @"C:\mynodepath\node.exe";
            var dir = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);

            Assert.AreEqual(path, Nodejs.GetAbsoluteNodeExePath(dir, filename),
                "Resolution failed when relative filename is surrounded by quotes");
            Assert.AreEqual(path, Nodejs.GetAbsoluteNodeExePath(dir, "\"" + filename + "\""),
                "Resolution failed when relative filename is surrounded by quotes");
            Assert.AreEqual(path, Nodejs.GetAbsoluteNodeExePath(dir, "./" + filename),
                "Resolution failed when relative filename begins with ./");
            Assert.AreEqual(path, Nodejs.GetAbsoluteNodeExePath(dir, "../" + Path.GetFileName(dir) + "/" + filename),
                "Resolution failed when relative filename begins with ../");
            Assert.AreEqual(".", Nodejs.GetAbsoluteNodeExePath(null, "."),
                "Resolution should return relative path on failure");
            Assert.AreEqual(Nodejs.NodeExePath, Nodejs.GetAbsoluteNodeExePath(dir, null),
                "Resolution should fall back to environment path if no relative path is specified");
        }

        [TestMethod, Priority(0)]
        public void CheckAbsoluteNodeExePathResolution()
        {
            var path = @"C:\Program Files\node.exe";
            var dir = Path.GetDirectoryName(path);

            Assert.AreEqual(path, Nodejs.GetAbsoluteNodeExePath(@"C:\myprojectpath", path),
                "Resolution should use absolute path if specified");
        }
    }
}


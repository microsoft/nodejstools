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
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class ChangeLiveCommandTests {
        [TestMethod]
        public void CreateChangeLiveCommand() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const string fileName = "fileName.js";
            const string source = "source";
            string wrappedSource = string.Format("{0}{1}{2}", NodeConstants.ScriptWrapBegin, source, NodeConstants.ScriptWrapEnd.Replace("\n",@"\n"));
            var module = new NodeModule(moduleId, fileName, fileName) { Source = source };

            // Act
            var changeLiveCommand = new ChangeLiveCommand(commandId, module);

            // Assert
            Assert.AreEqual(commandId, changeLiveCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"changelive\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"script_id\":{1},\"new_source\":\"{2}\",\"preview_only\":false}}}}",
                    commandId, moduleId, wrappedSource),
                changeLiveCommand.ToString());
        }

        [TestMethod]
        public void ProcessChangeLiveResponse() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const string fileName = "fileName.js";
            var module = new NodeModule(moduleId, fileName, fileName);
            var changeLiveCommand = new ChangeLiveCommand(commandId, module);

            // Act
            changeLiveCommand.ProcessResponse(SerializationTestData.GetChangeLiveResponse());

            // Assert
            Assert.AreEqual(commandId, changeLiveCommand.Id);
            Assert.IsTrue(changeLiveCommand.Updated);
            Assert.IsTrue(changeLiveCommand.NeedStepIn);
            Assert.IsTrue(changeLiveCommand.StackModified);
        }
    }
}
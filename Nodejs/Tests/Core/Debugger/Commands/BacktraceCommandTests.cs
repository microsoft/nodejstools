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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NodejsTests.Mocks;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class BacktraceCommandTests {
        [TestMethod]
        public void CreateBacktraceCommand() {
            // Arrange
            const int commandId = 3;
            const int fromFrame = 0;
            const int toFrame = 7;

            // Act
            var backtraceCommand = new BacktraceCommand(commandId, new MockEvaluationResultFactory(), fromFrame, toFrame);

            // Assert
            Assert.AreEqual(commandId, backtraceCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"backtrace\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"fromFrame\":{1},\"toFrame\":{2},\"inlineRefs\":true}}}}",
                    commandId, fromFrame, toFrame),
                backtraceCommand.ToString());
        }

        [TestMethod]
        public void ProcessBacktraceForКCallstackDepth() {
            // Arrange
            const int commandId = 3;
            const int fromFrame = 0;
            const int toFrame = 7;
            var backtraceCommand = new BacktraceCommand(commandId, new MockEvaluationResultFactory(), fromFrame, toFrame);
            JObject backtraceMessage = SerializationTestData.GetBacktraceResponse();

            // Act
            backtraceCommand.ProcessResponse(backtraceMessage);

            // Assert
            Assert.AreEqual(7, backtraceCommand.CallstackDepth);
            Assert.AreEqual(0, backtraceCommand.StackFrames.Count);
        }

        [TestMethod]
        public void ProcessBacktraceForStackFrames() {
            // Arrange
            const int commandId = 3;
            const int fromFrame = 0;
            const int toFrame = 7;
            var thread = new NodeThread(new NodeDebugger("localhost", 5858, 1), 0, false);
            var resultFactory = new MockEvaluationResultFactory();
            var backtraceCommand = new BacktraceCommand(commandId, resultFactory, fromFrame, toFrame, thread);
            JObject backtraceMessage = SerializationTestData.GetBacktraceResponse();

            // Act
            backtraceCommand.ProcessResponse(backtraceMessage);

            // Assert
            Assert.AreEqual(7, backtraceCommand.StackFrames.Count);
            NodeStackFrame firstFrame = backtraceCommand.StackFrames[0];
            Assert.AreEqual("Anonymous function", firstFrame.FunctionName);
            Assert.AreEqual(@"C:\Users\Tester\documents\visual studio 2012\Projects\NodejsApp1\NodejsApp1\server.js", firstFrame.FileName);
            Assert.AreEqual(23, firstFrame.LineNo);
            Assert.AreEqual(15, firstFrame.Locals.Count);
            Assert.AreEqual(5, firstFrame.Parameters.Count);
        }
    }
}
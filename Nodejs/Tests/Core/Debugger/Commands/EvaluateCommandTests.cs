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
using NodejsTests.Mocks;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class EvaluateCommandTests {
        [TestMethod]
        public void CreateEvaluateCommand() {
            // Arrange
            const int commandId = 3;
            var resultFactory = new MockEvaluationResultFactory();
            const string expression = "expression";

            // Act
            var evaluateCommand = new EvaluateCommand(commandId, resultFactory, expression);

            // Assert
            Assert.AreEqual(commandId, evaluateCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"evaluate\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"expression\":\"{1}\",\"frame\":0,\"global\":false,\"disable_break\":true,\"maxStringLength\":-1}}}}",
                    commandId, expression),
                evaluateCommand.ToString());
        }

        [TestMethod]
        public void ProcessEvaluateResponse() {
            // Arrange
            const int commandId = 3;
            var resultFactory = new MockEvaluationResultFactory();
            const string expression = "expression";
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);
            var evaluateCommand = new EvaluateCommand(commandId, resultFactory, expression, stackFrame);

            // Act
            evaluateCommand.ProcessResponse(SerializationTestData.GetEvaluateResponse());

            // Assert
            Assert.AreEqual(commandId, evaluateCommand.Id);
            Assert.IsNotNull(evaluateCommand.Result);
        }
    }
}
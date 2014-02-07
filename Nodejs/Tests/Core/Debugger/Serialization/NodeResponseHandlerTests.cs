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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class NodeResponseHandlerTests {
        //[TestMethod]
        //public void ExecuteBacktraceProcessing() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var thread = new NodeThread(null, 0, true);
        //    var json = SerializationTestData.GetBacktraceResponse();

        //    // Act
        //    NodeStackFrame[] frames = null;
        //    handler.ProcessBacktrace(
        //        thread,
        //        null,
        //        json,
        //        successHandler:
        //            backtraceFrames => {
        //                frames = backtraceFrames;
        //            }
        //    );

        //    // Assert
        //    Assert.IsNotNull(frames);
        //    Assert.AreEqual(7, frames.Length);
        //}

        //[TestMethod]
        //public void ExecuteBacktraceProcessingWithNullThread() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    JsonValue json = SerializationTestData.GetBacktraceResponse();
        //    NodeStackFrame[] frames = null;
        //    Exception exception = null;

        //    // Act
        //    try {
        //        handler.ProcessBacktrace(
        //            null,
        //            null,
        //            json,
        //            successHandler:
        //                backtraceFrames => {
        //                    frames = backtraceFrames;
        //                }
        //        );
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(frames);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}

        //[TestMethod]
        //public void ExecuteBacktraceProcessingWithNullJson() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var thread = new NodeThread(null, 0, true);
        //    NodeStackFrame[] frames = null;
        //    Exception exception = null;

        //    // Act
        //    try {
        //        handler.ProcessBacktrace(
        //            thread,
        //            null,
        //            null,
        //            successHandler:
        //                backtraceFrames => {
        //                    frames = backtraceFrames;
        //                }
        //        );
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(frames);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}

        //[TestMethod]
        //public void ExecuteEvaluateProcessing() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);
        //    JsonValue json = SerializationTestData.GetEvaluateResponse();
        //    const string name = "name";

        //    // Act
        //    NodeEvaluationResult result = handler.ProcessEvaluate(stackFrame, name, json);

        //    // Assert
        //    Assert.IsNotNull(result);
        //}

        //[TestMethod]
        //public void ExecuteEvaluateProcessingWithNullFrame() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    JsonValue json = SerializationTestData.GetEvaluateResponse();
        //    const string name = "name";
        //    NodeEvaluationResult result = null;
        //    Exception exception = null;

        //    // Act
        //    try {
        //        result = handler.ProcessEvaluate(null, name, json);
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(result);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}

        //[TestMethod]
        //public void ExecuteEvaluateProcessingWithNullName() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);
        //    JsonValue json = SerializationTestData.GetEvaluateResponse();
        //    NodeEvaluationResult result = null;
        //    Exception exception = null;

        //    // Act
        //    try {
        //        result = handler.ProcessEvaluate(stackFrame, null, json);
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(result);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}

        //[TestMethod]
        //public void ExecuteEvaluateProcessingWithNullJson() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);
        //    const string name = "name";
        //    NodeEvaluationResult result = null;
        //    Exception exception = null;

        //    // Act
        //    try {
        //        result = handler.ProcessEvaluate(stackFrame, name, null);
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(result);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}

        //[TestMethod]
        //public void ExecuteLookupProcessing() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var parent = new NodeEvaluationResult(25, null, null, null, null, null, NodeExpressionType.None, null);
        //    JsonValue json = SerializationTestData.GetLookupResponse();

        //    // Act
        //    NodeEvaluationResult[] results = handler.ProcessLookup(parent, json);

        //    // Assert
        //    Assert.IsNotNull(results);
        //    Assert.AreEqual(3, results.Length);
        //}

        //[TestMethod]
        //public void ExecuteLookupProcessingWithNullParent() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    JsonValue json = SerializationTestData.GetLookupResponse();
        //    Exception exception = null;
        //    NodeEvaluationResult[] results = null;

        //    // Act
        //    try {
        //        results = handler.ProcessLookup(null, json);
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(results);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}

        //[TestMethod]
        //public void ExecuteLookupProcessingWithNullJson() {
        //    // Arrange
        //    var factory = new MockEvaluationResultFactory();
        //    var handler = new NodeResponseHandler(factory);
        //    var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
        //    Exception exception = null;
        //    NodeEvaluationResult[] results = null;

        //    // Act
        //    try {
        //        results = handler.ProcessLookup(parent, null);
        //    }
        //    catch (Exception e) {
        //        exception = e;
        //    }

        //    // Assert
        //    Assert.IsNull(results);
        //    Assert.IsNotNull(exception);
        //    Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        //}
    }
}
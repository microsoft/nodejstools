// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NodejsTests.Debugger.Communication
{
    [TestClass]
    public class DebuggerClientTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateDebuggerClientWithNullConnection()
        {
            // Arrange
            Exception exception = null;
            DebuggerClient client = null;

            // Act
            try
            {
                client = new DebuggerClient(null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            // Assert
            Assert.IsNull(client);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ArgumentNullException));
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public async Task SendMessageViaDebuggerClient()
        {
            // Arrange
            var connectionMock = new Mock<IDebuggerConnection>();
            var messageEventArgs = new MessageEventArgs(Resources.NodeDisconnectResponse);
            connectionMock.Setup(p => p.SendMessage(It.IsAny<string>()))
                .Raises(f => f.OutputMessage += null, messageEventArgs);
            var client = new DebuggerClient(connectionMock.Object);
            var disconnectCommand = new DisconnectCommand(10);

            // Act
            await client.SendRequestAsync(disconnectCommand);

            // Assert
            Assert.IsTrue(disconnectCommand.Running);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void RaiseCompileScriptEventViaDebuggerClient()
        {
            // Arrange
            var connectionMock = new Mock<IDebuggerConnection>();
            var messageEventArgs = new MessageEventArgs(Resources.NodeCompileScriptResponse);
            var client = new DebuggerClient(connectionMock.Object);
            object sender = null;
            CompileScriptEventArgs args = null;

            // Act
            client.CompileScriptEvent += (s, a) =>
            {
                sender = s;
                args = a;
            };
            connectionMock.Raise(f => f.OutputMessage += null, messageEventArgs);

            // Assert
            Assert.AreEqual(client, sender);
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.CompileScriptEvent);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void RaiseBreakpointEventViaDebuggerClient()
        {
            // Arrange
            var connectionMock = new Mock<IDebuggerConnection>();
            var messageEventArgs = new MessageEventArgs(Resources.NodeBreakpointResponse);
            var client = new DebuggerClient(connectionMock.Object);
            object sender = null;
            BreakpointEventArgs args = null;

            // Act
            client.BreakpointEvent += (s, a) =>
            {
                sender = s;
                args = a;
            };
            connectionMock.Raise(f => f.OutputMessage += null, messageEventArgs);

            // Assert
            Assert.AreEqual(client, sender);
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.BreakpointEvent);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void RaiseExceptionEventViaDebuggerClient()
        {
            // Arrange
            var connectionMock = new Mock<IDebuggerConnection>();
            var messageEventArgs = new MessageEventArgs(Resources.NodeExceptionResponse);
            var client = new DebuggerClient(connectionMock.Object);
            object sender = null;
            ExceptionEventArgs args = null;

            // Act
            client.ExceptionEvent += (s, a) =>
            {
                sender = s;
                args = a;
            };
            connectionMock.Raise(f => f.OutputMessage += null, messageEventArgs);

            // Assert
            Assert.AreEqual(client, sender);
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.ExceptionEvent);
        }
    }
}


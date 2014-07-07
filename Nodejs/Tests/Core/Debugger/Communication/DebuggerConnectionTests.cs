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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NodejsTests.Debugger.Communication {
    [TestClass]
    public class DebuggerConnectionTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateDebuggerConnectionWithNullNetworkClientFactory() {
            // Arrange
            Exception exception = null;
            DebuggerConnection connection = null;

            // Act
            try {
                connection = new DebuggerConnection(null);
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(connection);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public async Task RaiseConnectionClosedEvent() {
            // Arrange
            var memoryStream = new MemoryStream();

            var tcpClientMock = new Mock<INetworkClient>();
            tcpClientMock.Setup(p => p.GetStream()).Returns(() => memoryStream);
            tcpClientMock.SetupGet(p => p.Connected).Returns(() => true);

            var tcpClientFactoryMock = new Mock<INetworkClientFactory>();
            tcpClientFactoryMock.Setup(p => p.CreateNetworkClient(It.IsAny<Uri>())).Returns(() => tcpClientMock.Object);

            var debuggerConnection = new DebuggerConnection(tcpClientFactoryMock.Object);
            object sender = null;
            EventArgs args = null;

            // Act
            debuggerConnection.ConnectionClosed += (s, a) => {
                sender = s;
                args = a;
            };

            debuggerConnection.Connect(new Uri("tcp://localhost:5858"));
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            memoryStream.Close();
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Assert
            Assert.IsNotNull(sender);
            Assert.AreEqual(debuggerConnection, sender);
            Assert.IsNotNull(args);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public async Task RaiseOutputMessageEvent() {
            // Arrange
            const string message = "Hello node.js!";
            string formattedMessage = string.Format("Content-Length: {0}{1}{1}{2}", Encoding.UTF8.GetByteCount(message), Environment.NewLine, message);
            byte[] rawMessage = Encoding.UTF8.GetBytes(formattedMessage);

            var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(rawMessage, 0, rawMessage.Length);
            await memoryStream.FlushAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);

            var tcpClientMock = new Mock<INetworkClient>();
            tcpClientMock.Setup(p => p.GetStream()).Returns(() => memoryStream);
            tcpClientMock.SetupGet(p => p.Connected).Returns(() => true);

            var tcpClientFactoryMock = new Mock<INetworkClientFactory>();
            tcpClientFactoryMock.Setup(p => p.CreateNetworkClient(It.IsAny<Uri>())).Returns(() => tcpClientMock.Object);

            var debuggerConnection = new DebuggerConnection(tcpClientFactoryMock.Object);
            object sender = null;
            MessageEventArgs args = null;
            bool inital = debuggerConnection.Connected;

            // Act
            debuggerConnection.OutputMessage += (s, a) => {
                sender = s;
                args = a;
            };
            debuggerConnection.Connect(new Uri("tcp://localhost:5858"));
            bool afterConnection = debuggerConnection.Connected;

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            memoryStream.Close();

            // Assert
            Assert.IsNotNull(sender);
            Assert.AreEqual(debuggerConnection, sender);
            Assert.IsNotNull(args);
            Assert.AreEqual(message, args.Message);
            Assert.IsFalse(inital);
            Assert.IsTrue(afterConnection);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public async Task SendMessageAsync() {
            // Arrange
            const string message = "Hello node.js!";
            var sourceStream = new MemoryStream();
            bool[] connected = { true };

            var tcpClientMock = new Mock<INetworkClient>();
            tcpClientMock.Setup(p => p.GetStream()).Returns(() => sourceStream);
            tcpClientMock.SetupGet(p => p.Connected).Returns(() => connected[0]);

            var tcpClientFactoryMock = new Mock<INetworkClientFactory>();
            tcpClientFactoryMock.Setup(p => p.CreateNetworkClient(It.IsAny<Uri>())).Returns(() => tcpClientMock.Object);

            var debuggerConnection = new DebuggerConnection(tcpClientFactoryMock.Object);

            // Act
            debuggerConnection.Connect(new Uri("tcp://localhost:5858"));
            debuggerConnection.SendMessage(message);

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            connected[0] = false;

            byte[] buffer = sourceStream.GetBuffer();
            string result = Encoding.UTF8.GetString(buffer, 0, (int)sourceStream.Length);

            sourceStream.Close();

            // Assert
            Assert.AreEqual(string.Format("Content-Length: {0}{1}{1}{2}", Encoding.UTF8.GetByteCount(message), Environment.NewLine, message), result);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public async Task RetrieveNodeVersion() {
            // Arrange
            const string version = "0.10.25";
            string message = string.Format("Embedding-Host: node v{0}\r\n\r\n", version);
            byte[] rawMessage = Encoding.UTF8.GetBytes(message);

            var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(rawMessage, 0, rawMessage.Length);
            await memoryStream.FlushAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);

            var tcpClientMock = new Mock<INetworkClient>();
            tcpClientMock.Setup(p => p.GetStream()).Returns(() => memoryStream);
            tcpClientMock.SetupGet(p => p.Connected).Returns(() => true);

            var tcpClientFactoryMock = new Mock<INetworkClientFactory>();
            tcpClientFactoryMock.Setup(p => p.CreateNetworkClient(It.IsAny<Uri>())).Returns(() => tcpClientMock.Object);

            var debuggerConnection = new DebuggerConnection(tcpClientFactoryMock.Object);

            // Act
            debuggerConnection.Connect(new Uri("tcp://localhost:5858"));
            for (int i = 0; i < 10 && debuggerConnection.NodeVersion == null; ++i) {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            memoryStream.Close();

            // Assert
            Assert.IsNotNull(debuggerConnection.NodeVersion);
            Assert.AreEqual(version, debuggerConnection.NodeVersion.ToString());
       }
    }
}
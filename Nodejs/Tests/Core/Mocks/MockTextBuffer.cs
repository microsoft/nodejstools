using Microsoft.NodejsTools;

namespace NodejsTests.Mocks {
    class MockTextBuffer : TestUtilities.Mocks.MockTextBuffer {
		public MockTextBuffer(string content) : base(content: content, contentType: NodejsConstants.Nodejs, filename: "file.js") {
        }
    }
}

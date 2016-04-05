using System.IO;
using Microsoft.NodejsTools;

namespace NodejsTests.Mocks {
    class MockTextBuffer : TestUtilities.Mocks.MockTextBuffer {
		public MockTextBuffer(string content) : base(content: content, contentType: NodejsConstants.Nodejs) {
        }

        private static string GetRandomFileNameIfNull(string filename) {
            if (filename == null) {
                filename = Path.Combine(TestUtilities.TestData.GetTempPath(), Path.GetRandomFileName(), "file.js");
            }
            return filename;
        }

        public MockTextBuffer(string content, string contentType, string filename = null) : base(content: content,
            contentType: contentType, filename: MockTextBuffer.GetRandomFileNameIfNull(filename)) {
        }
    }
}

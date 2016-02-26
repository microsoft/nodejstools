//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    internal static class SemverVersionTestHelper {
        public static void AssertVersionsEqual(
            ulong expectedMajor,
            ulong expectedMinor,
            ulong expectedPatch,
            string expectedPreRelease,
            string expectedBuildMetadata,
            SemverVersion actual) {
            Assert.AreEqual(expectedMajor, actual.Major);
            Assert.AreEqual(expectedMinor, actual.Minor, "Mismatched minor version.");
            Assert.AreEqual(expectedPatch, actual.Patch, "Mismatched patch version.");
            Assert.AreEqual(
                null != expectedPreRelease,
                actual.HasPreReleaseVersion,
                "Pre-release version info presence mismatch.");
            Assert.AreEqual(expectedPreRelease, actual.PreReleaseVersion, "Pre-release version info mismatch.");
            Assert.AreEqual(null != expectedBuildMetadata, actual.HasBuildMetadata, "Build metadata presence mismatch.");
            Assert.AreEqual(expectedBuildMetadata, actual.BuildMetadata, "Build metadata mismatch.");

            var expected = new StringBuilder(string.Format("{0}.{1}.{2}", expectedMajor, expectedMinor, expectedPatch));
            if (null != expectedPreRelease) {
                expected.Append('-');
                expected.Append(expectedPreRelease);
            }

            if (null != expectedBuildMetadata) {
                expected.Append('+');
                expected.Append(expectedBuildMetadata);
            }

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }
    }
}
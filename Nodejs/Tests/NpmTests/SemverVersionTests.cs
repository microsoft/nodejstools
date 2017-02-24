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

using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    /// <summary>
    /// Tests for semantic version parsing/output. Only exists because users may
    /// need to specify a pre-release version or build metadata when authoring their
    /// own packages.
    /// </summary>
    [TestClass]
    public class SemverVersionTests
    {
        [TestMethod, Priority(0)]
        public void BasicMajorMinorPatchVersion()
        {
            SemverVersionTestHelper.AssertVersionsEqual(
                1,
                2,
                3,
                null,
                null,
                SemverVersion.Parse("1.2.3"));
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow negative major version.")]
        public void NegativeMajorVersionFails()
        {
            SemverVersion.Parse("-1.2.3");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow negative minor version.")]
        public void NegativeMinorVersionFails()
        {
            SemverVersion.Parse("1.-2.3");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow negative patch version.")]
        public void NegativePatchVersionFails()
        {
            SemverVersion.Parse("1.2.-3");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow non-numeric major version.")]
        public void NonNumericMajorVersionFails()
        {
            SemverVersion.Parse("a.2.3");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow non-numeric minor version.")]
        public void NonNumericMinorVersionFails()
        {
            SemverVersion.Parse("1.b.3");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow non-numeric patch version.")]
        public void NonNumericPatchVersionFails()
        {
            SemverVersion.Parse("1.2.c");
        }

        [TestMethod, Priority(0)]
        public void AlphaPreRelease()
        {
            SemverVersionTestHelper.AssertVersionsEqual(1, 2, 3, "alpha", null, SemverVersion.Parse("1.2.3-alpha"));
        }

        [TestMethod, Priority(0)]
        public void NumericPreRelease()
        {
            SemverVersionTestHelper.AssertVersionsEqual(1, 2, 3, "4.5.6", null, SemverVersion.Parse("1.2.3-4.5.6"));
        }

        [TestMethod, Priority(0)]
        public void PreReleaseHyphenatedIdentifier()
        {
            SemverVersionTestHelper.AssertVersionsEqual(
                1,
                2,
                3,
                "alpha-2.1",
                null,
                SemverVersion.Parse("1.2.3-alpha-2.1"));
        }

        [TestMethod, Priority(0)]
        public void PreReleaseHyphenatedIdentifierWithoutVersion()
        {
            // This version code is crazy, and for that reason it fail when
            // regional settings is set to Turkish.
            SemverVersionTestHelper.AssertVersionsEqual(
                0,
                1,
                4,
                "DEPRECATED-USE-cfenv-INSTEAD",
                null,
                SemverVersion.Parse("0.1.4-DEPRECATED-USE-cfenv-INSTEAD"));
        }

        [TestMethod, Priority(0)]
        public void PreReleaseAndBuildMetadata()
        {
            // 1.0.0-alpha+001, 1.0.0+20130313144700, 1.0.0-beta+exp.sha.5114f85
            SemverVersionTestHelper.AssertVersionsEqual(1, 0, 0, "alpha", "001", SemverVersion.Parse("1.0.0-alpha+001"));
            SemverVersionTestHelper.AssertVersionsEqual(
                1,
                0,
                0,
                "beta",
                "exp.sha.5114f85",
                SemverVersion.Parse("1.0.0-beta+exp.sha.5114f85"));
        }

        [TestMethod, Priority(0)]
        public void BuildMetadataOnly()
        {
            SemverVersionTestHelper.AssertVersionsEqual(
                1,
                0,
                0,
                null,
                "20130313144700",
                SemverVersion.Parse("1.0.0+20130313144700"));
        }
    }
}
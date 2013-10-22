using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    internal static class SemverVersionTestHelper
    {
        public static void AssertVersionsEqual(
            int expectedMajor,
            int expectedMinor,
            int expectedPatch,
            string expectedPreRelease,
            string expectedBuildMetadata,
            SemverVersion actual)
        {
            Assert.AreEqual(expectedMajor, actual.Major);
            Assert.AreEqual(expectedMinor, actual.Minor, "Mismatched minor version.");
            Assert.AreEqual(expectedPatch, actual.Patch, "Mismatched patch version.");
            Assert.AreEqual(null != expectedPreRelease, actual.HasPreReleaseVersion, "Pre-release version info presence mismatch.");
            Assert.AreEqual(expectedPreRelease, actual.PreReleaseVersion, "Pre-release version info mismatch.");
            Assert.AreEqual(null != expectedBuildMetadata, actual.HasBuildMetadata, "Build metadata presence mismatch.");
            Assert.AreEqual(expectedBuildMetadata, actual.BuildMetadata, "Build metadata mismatch.");

            var expected = new StringBuilder(string.Format("{0}.{1}.{2}", expectedMajor, expectedMinor, expectedPatch));
            if (null != expectedPreRelease)
            {
                expected.Append('-');
                expected.Append(expectedPreRelease);
            }

            if (null != expectedBuildMetadata)
            {
                expected.Append('+');
                expected.Append(expectedBuildMetadata);
            }

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }


    }
}

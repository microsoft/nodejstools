using System;
using System.Runtime.InteropServices;
using System.Text;
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

        private static void AssertVersionsEqual(
            int expectedMajor,
            int expectedMinor,
            int expectedPatch,
            string expectedPreRelease,
            string expectedBuildMetadata,
            SemverVersion actual )
        {
            Assert.AreEqual( expectedMajor, actual.Major );
            Assert.AreEqual( expectedMinor, actual.Minor, "Mismatched minor version.");
            Assert.AreEqual( expectedPatch, actual.Patch, "Mismatched patch version.");
            Assert.AreEqual( null != expectedPreRelease, actual.HasPreReleaseVersion, "Pre-release version info presence mismatch.");
            Assert.AreEqual( expectedPreRelease, actual.PreReleaseVersion, "Pre-release version info mismatch.");
            Assert.AreEqual( null != expectedBuildMetadata, actual.HasBuildMetadata, "Build metadata presence mismatch.");
            Assert.AreEqual( expectedBuildMetadata, actual.BuildMetadata, "Build metadata mismatch.");

            var   expected    = new StringBuilder( string.Format( "{0}.{1}.{2}", expectedMajor, expectedMinor, expectedPatch ) );
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

        [TestMethod]
        public void TestBasicMajorMinorPatchVersion()
        {
            AssertVersionsEqual(
                1,
                2,
                3,
                null,
                null,
                SemverVersion.Parse( "1.2.3" ) );
        }

        [ TestMethod ]
        [ ExpectedException( typeof( SemverVersionFormatException ), "Should not allow negative major version." ) ]
        public void TestNegativeMajorVersionFails()
        {
            SemverVersion.Parse("-1.2.3");
        }

        [TestMethod]
        [ExpectedException(typeof( SemverVersionFormatException ), "Should not allow negative minor version.")]
        public void TestNegativeMinorVersionFails()
        {
            SemverVersion.Parse("1.-2.3");
        }

        [TestMethod]
        [ExpectedException( typeof( SemverVersionFormatException ), "Should not allow negative patch version.")]
        public void TestNegativePatchVersionFails()
        {
            SemverVersion.Parse("1.2.-3");
        }

        [ TestMethod ]
        [ ExpectedException( typeof ( SemverVersionFormatException ), "Should not allow non-numeric major version." ) ]
        public void TestNonNumericMajorVersionFails()
        {
            SemverVersion.Parse( "a.2.3" );
        }

        [TestMethod]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow non-numeric minor version.")]
        public void TestNonNumericMinorVersionFails()
        {
            SemverVersion.Parse("1.b.3");
        }

        [TestMethod]
        [ExpectedException(typeof(SemverVersionFormatException), "Should not allow non-numeric patch version.")]
        public void TestNonNumericPatchVersionFails()
        {
            SemverVersion.Parse("1.2.c");
        }

        [ TestMethod ]
        public void TestAlphaPreRelease()
        {
            AssertVersionsEqual( 1, 2, 3, "alpha", null, SemverVersion.Parse( "1.2.3-alpha" ) );
        }

        [ TestMethod ]
        public void TestNumericPreRelease()
        {
            AssertVersionsEqual(1, 2, 3, "4.5.6", null, SemverVersion.Parse("1.2.3-4.5.6") );
        }

        [ TestMethod ]
        public void TestPreReleaseHypenatedIdentifier()
        {
            AssertVersionsEqual(1, 2, 3, "alpha-2.1", null, SemverVersion.Parse("1.2.3-alpha-2.1"));
        }
    }
}

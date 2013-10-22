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

        [TestMethod]
        public void TestBasicMajorMinorPatchVersion()
        {
            SemverVersionTestHelper.AssertVersionsEqual(
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
            SemverVersionTestHelper.AssertVersionsEqual(1, 2, 3, "alpha", null, SemverVersion.Parse("1.2.3-alpha"));
        }

        [ TestMethod ]
        public void TestNumericPreRelease()
        {
            SemverVersionTestHelper.AssertVersionsEqual(1, 2, 3, "4.5.6", null, SemverVersion.Parse("1.2.3-4.5.6"));
        }

        [ TestMethod ]
        public void TestPreReleaseHypenatedIdentifier()
        {
            SemverVersionTestHelper.AssertVersionsEqual(1, 2, 3, "alpha-2.1", null, SemverVersion.Parse("1.2.3-alpha-2.1"));
        }

        [TestMethod]
        public void TestPreReleaseAndBuildMetadata()
        {
            // 1.0.0-alpha+001, 1.0.0+20130313144700, 1.0.0-beta+exp.sha.5114f85
            SemverVersionTestHelper.AssertVersionsEqual(1, 0, 0, "alpha", "001", SemverVersion.Parse("1.0.0-alpha+001" ));
            SemverVersionTestHelper.AssertVersionsEqual(1, 0, 0, "beta", "exp.sha.5114f85", SemverVersion.Parse("1.0.0-beta+exp.sha.5114f85"));
        }

        [TestMethod]
        public void TestBuildMetadataOnly()
        {
            SemverVersionTestHelper.AssertVersionsEqual(1, 0, 0, null, "20130313144700", SemverVersion.Parse("1.0.0+20130313144700"));
        }
    }
}

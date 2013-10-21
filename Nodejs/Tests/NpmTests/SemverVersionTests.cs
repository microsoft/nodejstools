using System;
using System.Runtime.InteropServices;
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
            var   version = SemverVersion.Parse( "1.2.3" );
            Assert.AreEqual( 1, version.Major );
            Assert.AreEqual( 2, version.Minor, "Mismatched minor version." );
            Assert.AreEqual( 3, version.Patch, "Mismatched patch version." );
            Assert.AreEqual( false, version.HasPreReleaseVersion, "Should not have pre-release version info." );
            Assert.AreEqual( null, version.PreReleaseVersion, "Pre-release version info should be null." );
            Assert.AreEqual( false, version.HasBuildMetadata, "Should not have build metadata." );
            Assert.AreEqual( null, version.BuildMetadata, "Build metadata should be null or empty.");
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
            var version = SemverVersion.Parse( "1.2.3-alpha" );
            Assert.AreEqual( 1, version.Major, "Major version mismatch." );
            Assert.AreEqual( 2, version.Minor, "Minor version mismatch." );
            Assert.AreEqual( 3, version.Patch, "Patch version mismatch." );
            Assert.IsTrue( version.HasPreReleaseVersion, "Should have pre-release version." );
            Assert.AreEqual( "alpha", version.PreReleaseVersion );
            Assert.IsFalse( version.HasBuildMetadata, "Should not have build metadata." );
        }

        [ TestMethod ]
        public void TestNumericPreRelease()
        {
            var version = SemverVersion.Parse("1.2.3-4.5.6");
            Assert.AreEqual(1, version.Major, "Major version mismatch.");
            Assert.AreEqual(2, version.Minor, "Minor version mismatch.");
            Assert.AreEqual(3, version.Patch, "Patch version mismatch.");
            Assert.IsTrue(version.HasPreReleaseVersion, "Should have pre-release version.");
            Assert.AreEqual("4.5.6", version.PreReleaseVersion);
            Assert.IsFalse(version.HasBuildMetadata, "Should not have build metadata.");
        }

        [ TestMethod ]
        public void TestPreReleaseHypenatedIdentifier()
        {
            var version = SemverVersion.Parse("1.2.3-alpha-2.1");
            Assert.AreEqual(1, version.Major, "Major version mismatch.");
            Assert.AreEqual(2, version.Minor, "Minor version mismatch.");
            Assert.AreEqual(3, version.Patch, "Patch version mismatch.");
            Assert.IsTrue(version.HasPreReleaseVersion, "Should have pre-release version.");
            Assert.AreEqual("alpha-2.1", version.PreReleaseVersion);
            Assert.IsFalse(version.HasBuildMetadata, "Should not have build metadata.");
        }
    }
}

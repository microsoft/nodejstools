using System;
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
            SemverVersion   version = new SemverVersion( "1.2.3" );
            Assert.AreEqual( 1, version.Major );
            Assert.AreEqual( 2, version.Minor, "Mismatched minor version." );
            Assert.AreEqual( 3, version.Patch, "Mismatched patch version." );
            Assert.AreEqual( false, version.HasPreReleaseVersion, "Should not have pre-release version info." );
            Assert.AreEqual( null, version.PreReleaseVersion, "Pre-release version info should be null." );
            Assert.AreEqual( false, version.HasBuildMetadata, "Should not have build metadata." );
            Assert.AreEqual( null, version.BuildMetadata, "Build metadata should be null or empty.");
        }
    }
}

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
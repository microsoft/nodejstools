// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HomepageValidationTests
{
    /// <summary>
    /// Tests for the homepage URL validation logic used in NpmPackageInstallViewModel.OpenHomepage()
    /// and NpmWorker.AddHomepage(). These tests verify the fix for ADO#2982591 (NTVS-002, CWE-749).
    ///
    /// The validation logic is:
    ///   Uri.TryCreate(homepage, UriKind.Absolute, out var uri)
    ///     && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
    ///
    /// BEFORE the fix: only checked !string.IsNullOrEmpty(homepage)
    /// AFTER the fix: requires valid absolute URI with http or https scheme
    /// </summary>
    [TestClass]
    public class HomepageUrlValidationTests
    {
        /// <summary>
        /// Simulates the ORIGINAL (vulnerable) validation logic.
        /// Only checks for null/empty — allows anything through.
        /// </summary>
        private static bool OriginalCanOpenHomepage(string homepage)
        {
            return !string.IsNullOrEmpty(homepage);
        }

        /// <summary>
        /// Simulates the FIXED validation logic.
        /// Requires a valid absolute URI with http or https scheme.
        /// </summary>
        private static bool FixedCanOpenHomepage(string homepage)
        {
            return Uri.TryCreate(homepage, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        // ===================================================================
        // LEGITIMATE URLs - should be ALLOWED by both old and new logic
        // ===================================================================

        [TestMethod]
        public void Https_Url_Allowed_ByOriginal()
        {
            Assert.IsTrue(OriginalCanOpenHomepage("https://lodash.com/"));
        }

        [TestMethod]
        public void Https_Url_Allowed_ByFix()
        {
            Assert.IsTrue(FixedCanOpenHomepage("https://lodash.com/"));
        }

        [TestMethod]
        public void Http_Url_Allowed_ByOriginal()
        {
            Assert.IsTrue(OriginalCanOpenHomepage("http://example.com"));
        }

        [TestMethod]
        public void Http_Url_Allowed_ByFix()
        {
            Assert.IsTrue(FixedCanOpenHomepage("http://example.com"));
        }

        [TestMethod]
        public void Https_WithPath_Allowed_ByFix()
        {
            Assert.IsTrue(FixedCanOpenHomepage("https://github.com/lodash/lodash/issues"));
        }

        [TestMethod]
        public void Https_WithPort_Allowed_ByFix()
        {
            Assert.IsTrue(FixedCanOpenHomepage("https://example.com:8080/page"));
        }

        // ===================================================================
        // MALICIOUS URLs - should be BLOCKED by fix but ALLOWED by original
        // These demonstrate the vulnerability in the original code.
        // ===================================================================

        [TestMethod]
        public void UNC_Path_Allowed_ByOriginal_VULNERABLE()
        {
            // VULNERABILITY: Original allows UNC paths → SMB hash leak
            Assert.IsTrue(OriginalCanOpenHomepage("\\\\attacker.example.com\\share"));
        }

        [TestMethod]
        public void UNC_Path_Blocked_ByFix()
        {
            // FIX: UNC path is not a valid http/https URI → blocked
            Assert.IsFalse(FixedCanOpenHomepage("\\\\attacker.example.com\\share"));
        }

        [TestMethod]
        public void LocalExe_Allowed_ByOriginal_VULNERABLE()
        {
            // VULNERABILITY: Original allows local file paths → arbitrary process launch
            Assert.IsTrue(OriginalCanOpenHomepage("C:\\Windows\\System32\\calc.exe"));
        }

        [TestMethod]
        public void LocalExe_Blocked_ByFix()
        {
            // FIX: file:// scheme is not http/https → blocked
            Assert.IsFalse(FixedCanOpenHomepage("C:\\Windows\\System32\\calc.exe"));
        }

        [TestMethod]
        public void ProtocolHandler_Allowed_ByOriginal_VULNERABLE()
        {
            // VULNERABILITY: Original allows protocol handlers → arbitrary app launch
            Assert.IsTrue(OriginalCanOpenHomepage("ms-msdt:/id PCWDiagnostic"));
        }

        [TestMethod]
        public void ProtocolHandler_Blocked_ByFix()
        {
            // FIX: ms-msdt scheme is not http/https → blocked
            Assert.IsFalse(FixedCanOpenHomepage("ms-msdt:/id PCWDiagnostic"));
        }

        [TestMethod]
        public void FileScheme_Allowed_ByOriginal_VULNERABLE()
        {
            // VULNERABILITY: Original allows file:// URIs
            Assert.IsTrue(OriginalCanOpenHomepage("file:///C:/Windows/System32/calc.exe"));
        }

        [TestMethod]
        public void FileScheme_Blocked_ByFix()
        {
            // FIX: file:// scheme is not http/https → blocked
            Assert.IsFalse(FixedCanOpenHomepage("file:///C:/Windows/System32/calc.exe"));
        }

        [TestMethod]
        public void JavascriptScheme_Allowed_ByOriginal_VULNERABLE()
        {
            // VULNERABILITY: Original allows javascript: URIs
            Assert.IsTrue(OriginalCanOpenHomepage("javascript:alert(1)"));
        }

        [TestMethod]
        public void JavascriptScheme_Blocked_ByFix()
        {
            // FIX: javascript scheme is not http/https → blocked
            Assert.IsFalse(FixedCanOpenHomepage("javascript:alert(1)"));
        }

        // ===================================================================
        // EDGE CASES - null/empty should be blocked by both
        // ===================================================================

        [TestMethod]
        public void Null_Blocked_ByOriginal()
        {
            Assert.IsFalse(OriginalCanOpenHomepage(null));
        }

        [TestMethod]
        public void Null_Blocked_ByFix()
        {
            Assert.IsFalse(FixedCanOpenHomepage(null));
        }

        [TestMethod]
        public void Empty_Blocked_ByOriginal()
        {
            Assert.IsFalse(OriginalCanOpenHomepage(""));
        }

        [TestMethod]
        public void Empty_Blocked_ByFix()
        {
            Assert.IsFalse(FixedCanOpenHomepage(""));
        }

        [TestMethod]
        public void Whitespace_Blocked_ByFix()
        {
            Assert.IsFalse(FixedCanOpenHomepage("   "));
        }

        [TestMethod]
        public void RelativePath_Blocked_ByFix()
        {
            // Relative paths are not absolute URIs → blocked
            Assert.IsFalse(FixedCanOpenHomepage("/some/path"));
        }

        [TestMethod]
        public void RandomGarbage_Blocked_ByFix()
        {
            Assert.IsFalse(FixedCanOpenHomepage("not a url at all!!!"));
        }
    }
}

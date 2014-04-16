using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class Mocha : ITestFramework {
        private const string UNITTEST_FILE_TOKEN = "#TEST-FILE#";
        private const string TESTCASE_LIST_FILE_TOKEN = "#TEST-LIST#";
        private string _discoverScript =
            @"var fs = require('fs');" +
            @"var mocha = require('mocha');" +
            @"var testDiscover = new mocha({});" +
            @"var g_testList;" +
            @"" +
            @"testDiscover.addFile('" + UNITTEST_FILE_TOKEN + @"');" +
            @"testDiscover.loadFiles();" +
            @"getTestList(testDiscover.suite);" +
            @"" +
            @"if (g_testList){" +
                @"var fd = fs.openSync('" + TESTCASE_LIST_FILE_TOKEN + @"', 'w');" +
                @"fs.writeSync(fd, g_testList);" +
                @"fs.closeSync(fd);" +
            @"}" +
            @"" +
            @"function getTestList(suite){" +
                @"if (suite) {" +
                    @"if (suite.tests){" +
                        @"suite.tests.forEach(function(test, index, tests){" +
                            @"g_testList = g_testList ? g_testList + '\r\n' + suite.title : suite.title;" +
                        @"});" +
                    @"}" +
                    @"if (suite.suites){" +
                        @"suite.suites.forEach(function(s, i, ss) {" +
                            @"getTestList(s);" +
                        @"});" +
                    @"}" +
                @"}" +
            @"}";

        public string Name {
            get {
                return "mocha";
            }
        }
        public string DiscoverScript (string testFile, string discoverResultFile) {
            string script = _discoverScript
                .Replace(UNITTEST_FILE_TOKEN, testFile.Replace("\\", "\\\\"))
                .Replace(TESTCASE_LIST_FILE_TOKEN, discoverResultFile.Replace("\\", "\\\\"));
            return script;
        }

        public void RunTest(string testName) {

        }

    }
}

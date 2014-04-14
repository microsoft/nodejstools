using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Threading;

namespace Microsoft.NodejsTools.TestAdapter
{
    class MochaDiscover
    {
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
                @"});"+
            @"}";

        private Action<string> _jsEvaluator;
        private string _unittestFile;
        public MochaDiscover(string unittestFile, Action<string> jsEvaluator) {
            _jsEvaluator = jsEvaluator;
            _unittestFile = unittestFile;
        }

        public string Discover() {
            string tempFile = Path.GetTempFileName();
            string testCases = string.Empty;
            string discoverScript = _discoverScript
                            .Replace(UNITTEST_FILE_TOKEN, _unittestFile.Replace("\\", "\\\\"))
                            .Replace(TESTCASE_LIST_FILE_TOKEN, tempFile.Replace("\\", "\\\\"));
            try {
                _jsEvaluator(discoverScript);
                for (int i = 0; i < 4; i++) {
                    try {
                        testCases = File.ReadAllText(tempFile);
                        break;
                    }
                    catch (IOException) {
                        //We took an error processing the file.  Wait a few and try again
                        Thread.Sleep(500);
                    }
                }
            }
            finally {
                try {
                    File.Delete(tempFile);
                }
                catch (Exception) { //
                    //Unable to delete for some reason
                    //  We leave the file behind in this case, its in TEMP so eventually OS will clean up
                }
            }
            return testCases;
        }
    }
}

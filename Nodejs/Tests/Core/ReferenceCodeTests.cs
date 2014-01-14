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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.Project;

namespace NodejsTests {
    [TestClass]
    public class ReferenceCodeTests {
        private const string _refCode = @"
path = new function() {
    this.relative = function(from, to) {
" + ReferenceCode.PathRelativeBody + @"
    }

    this.join = function() {
" + ReferenceCode.PathJoinBody + @"
    }

    this.normalize = function(p) {
" + ReferenceCode.PathNormalizeBody  + @"
    }

    this.resolve = function(to) {
" + ReferenceCode.PathResolveBody + @"
    }
}
";

        [TestMethod, Priority(0)]
        public void TestPathRelative() {
            string tempPath = Path.GetTempPath().Replace('\\', '/');
            string tempPathNoFirstSlash = tempPath.Substring(0, 2) + tempPath.Substring(3);
            string tempPathNoDrive = tempPath.Substring(2);
            string tempPathOtherDrive = "D" + tempPath.Substring(1);
            string tempPathOtherDriveNoSlash = "D:" + tempPath.Substring(2);

            var paths = new[] { tempPath, tempPathNoFirstSlash, tempPathNoDrive, tempPathOtherDrive, tempPathOtherDriveNoSlash };

            StringBuilder testCode = new StringBuilder();
            foreach (var fromPath in paths) {
                foreach (var toPath in paths) {
                    testCode.AppendFormat("t('{0}', '{1}');\r\n", fromPath, toPath);
                    testCode.AppendFormat("t('{0}', '{1}email/');\r\n", fromPath, toPath);
                }
            }

            TestReferenceFunction(testCode.ToString(), "path", "relative");
        }

        [TestMethod, Priority(0)]
        public void TestPathNormalize() {
            TestReferenceFunction("t('/foo/../bar', '/bar');", "path", "normalize");
        }

        [TestMethod, Priority(0)]
        public void TestPathResolve() {
            TestReferenceFunction("t('foo/bar', '/tmp/file/', '..', 'a/../subfile')", "path", "resolve");
        }

        [TestMethod, Priority(0)]
        public void TestPathJoin() {
            TestReferenceFunction("t('/foo', 'bar')", "path", "join");
            TestReferenceFunction("t('/foo', '/bar')", "path", "join");
            TestReferenceFunction("t('foo', '/bar')", "path", "join");
            TestReferenceFunction("t('foo', 'bar')", "path", "join");
        }

        private void TestReferenceFunction(string testCases, string module, string functionName) {
            StringBuilder testCase = new StringBuilder();

            
            testCase.Append(_refCode);
            testCase.AppendFormat(
                @"
function t() {{
var func = require('{0}').{1};
console.log('cwd ' + process.cwd());
var expected = func.apply(require('{0}'), arguments);
var actual = {0}.{1}.apply({0}, arguments);
require('assert').equal(expected, actual);
console.log('got ' + actual + ', expected ' + expected);
}}

{2}",
    module,
    functionName,
    testCases
            );
            var filename = Path.GetTempFileName();
            Console.WriteLine(filename);
            try {
                File.WriteAllText(filename, testCase.ToString());
                var res = ProcessOutput.Run(
                    Nodejs.NodeExePath,
                    new[] { filename },
                    Path.GetTempPath(),
                    new [] { new KeyValuePair<string, string>("=C:", Path.GetTempPath()) }, // Node.js uses this magic env var which we're apparently improperly inheriting ...  
                    false,
                    null
                );
                res.Wait(new TimeSpan(0, 10, 0));
                System.Threading.Thread.Sleep(1000);
                Assert.AreEqual(
                    0,
                    res.ExitCode,
                    "Expected success, got std out:\r\n" + string.Join("\r\n", res.StandardOutputLines) + "\r\nstd err:\r\n" + string.Join("\r\n", res.StandardErrorLines));
                Console.WriteLine(string.Join("\r\n", res.StandardOutputLines));
            } finally {
                File.Delete(filename);
            }
        }
    }
}


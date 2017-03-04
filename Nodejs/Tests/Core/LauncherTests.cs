// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests
{
    [TestClass]
    public class LauncherTests
    {
        [TestMethod, Priority(0)]
        public void LaunchWebBrowserUriTests()
        {
            var testCases = new[] {
                new { Url = "/fob", Port = 0, Expected = "http://localhost:0/fob" },
                new { Url = "http://localhost:9999/fob", Port = 9999, Expected = "http://localhost:9999/fob" },
                new { Url = "http://localhost/fob", Port = 9999, Expected = "http://localhost:9999/fob" },
                new { Url = "fob", Port = 9999, Expected = "http://localhost:9999/fob" },
                new { Url = "/hello/world", Port = 367, Expected = "http://localhost:367/hello/world" },
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine("{0} {1} == {2}", testCase.Url, testCase.Port, testCase.Expected);

                Assert.AreEqual(
                    NodejsProjectLauncher.GetFullUrl(testCase.Url, testCase.Port),
                    testCase.Expected
                );
            }
        }
    }
}


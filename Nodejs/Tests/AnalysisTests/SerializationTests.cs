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

using System.IO;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisTests {
    [TestClass]
    public class SerializationTests {
        [TestMethod, Priority(0)]
        public void BasicTest() {
            var analyzer = new JsAnalyzer();
            RoundTrip(analyzer);
        }

        public static T RoundTrip<T>(T value) {
            MemoryStream ms = new MemoryStream();
            new AnalysisSerializer().Serialize(ms, value);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)new AnalysisSerializer().Deserialize(ms);
        }

        [TestMethod, Priority(0)]
        public void RequireTest() {
            var entries = RoundTrip(
                Analysis.Analyze(
                    new AnalysisFile("mod.js", @"var x = require('mymod').value;"),
                    AnalysisFile.PackageJson("node_modules\\mymod\\package.json", "./lib/mymod"),
                    new AnalysisFile("node_modules\\mymod\\lib\\mymod.js", @"exports.value = 42;"),
                    new AnalysisFile("node_modules\\mymod\\lib\\mymod\\foo.js", @"exports.value = 'abc';")
                )
            );

            AssertUtil.ContainsExactly(
                entries["mod.js"].Analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }
    }
}

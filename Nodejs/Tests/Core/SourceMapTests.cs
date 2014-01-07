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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project.ImportWizard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NodejsTests {
    [TestClass]
    public class SourceMapTests {
        /// <summary>
        /// Sample code for mapping between:
        /// 
        /// class Greeter {
        ///     constructor(public greeting: string) { }
        ///     greet() {
        ///         return "<h1>" + this.greeting + "</h1>";
        ///     }
        /// };
        /// 
        /// to:
        /// 
        /// var Greeter = (function () {
        ///    function Greeter(greeting) {
        ///        this.greeting = greeting;
        ///    }
        ///    Greeter.prototype.greet = function () {
        ///        return "<h1>" + this.greeting + "</h1>";
        ///    };
        ///    return Greeter;
        ///})();
        ///;
        ///# sourceMappingURL=test.js.map
        /// </summary>
        private const string _sample = @"{'version':3,'file':'test.js','sourceRoot':'','sources':['test.ts'],'names':['Greeter','Greeter.constructor','Greeter.greet'],'mappings':'AAAA;IACIA,iBAAYA,QAAuBA;QAAvBC,aAAeA,GAARA,QAAQA;AAAQA,IAAIA,CAACA;IACxCD,0BAAAA;QACIE,OAAOA,MAAMA,GAAGA,IAAIA,CAACA,QAAQA,GAAGA,OAAOA;IAC3CA,CAACA;IACLF,eAACA;AAADA,CAACA,IAAA;AAAA,CAAC'}";

        [TestMethod, Priority(0)]
        public void TestBadMappings() {
            // empty segment
            AssertUtil.Throws<InvalidOperationException>(
                () => new SourceMap(new StringReader("{version:3, mappings:','}"))
            );

            // invalid VLQ segment
            AssertUtil.Throws<InvalidOperationException>(
                () => new SourceMap(new StringReader("{version:3, mappings:'$'}"))
            );

            // continued value doesn't continue
            AssertUtil.Throws<InvalidOperationException>(
                () => new SourceMap(new StringReader("{version:3, mappings:'9'}"))
            );

            // too big segment
            AssertUtil.Throws<InvalidOperationException>(
                () => new SourceMap(new StringReader("{version:3, mappings:'999999999999999999999999999999999999'}"))
            );
        }


        [TestMethod, Priority(0)]
        public void TestMappingLine() {
            var map = new SourceMap(new StringReader(_sample));
            var testCases = new[] { 
                new { Line = 0, Name = "Greeter", Filename = "test.ts" },
                new { Line = 1, Name = "Greeter", Filename = "test.ts" },
                new { Line = 1, Name = "Greeter.constructor", Filename = "test.ts" },
                new { Line = 1, Name = "Greeter.constructor", Filename = "test.ts" },
                new { Line = 2, Name = "Greeter", Filename = "test.ts" },
                new { Line = 3, Name = "Greeter.greet", Filename = "test.ts" },
                new { Line = 4, Name = "Greeter.greet", Filename = "test.ts" },
                new { Line = 5, Name = "Greeter", Filename = "test.ts" },
                new { Line = 5, Name = "Greeter", Filename = "test.ts" },
                new { Line = 5, Name = "Greeter", Filename = "test.ts" },
                new { Line = -1, Name = "", Filename = "" },
            };
            for (int i = 0; i < testCases.Length; i++) {
                SourceMapping mapping;
                if (map.TryMapLine(i, out mapping)) {
                    Assert.AreEqual(testCases[i].Filename, mapping.Filename);
                    Assert.AreEqual(testCases[i].Name, mapping.Name);
                    Assert.AreEqual(testCases[i].Line, mapping.Line);
                } else {
                    Assert.AreEqual(-1, testCases[i].Line);
                }
            }
        }

        [TestMethod, Priority(0)]
        public void TestMappingLineAndColumn() {
            var map = new SourceMap(new StringReader(_sample));
            var testCases = new[] { 
                new { InLine = 0, InColumn = 0, ExpectedLine = 0, ExpectedColumn = 0, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 1, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 1, InColumn = 4, ExpectedLine = 1, ExpectedColumn = 4, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 2, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter.constructor", Filename = "test.ts" },
                new { InLine = 2, InColumn = 4, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter.constructor", Filename = "test.ts" },
                new { InLine = 2, InColumn = 8, ExpectedLine = 1, ExpectedColumn = 16, Name = "Greeter.constructor", Filename = "test.ts" },
                new { InLine = 3, InColumn = 0, ExpectedLine = 1, ExpectedColumn = 39, Name = "Greeter.constructor", Filename = "test.ts" },
                new { InLine = 4, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 5, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter.greet", Filename = "test.ts" },
                new { InLine = 5, InColumn = 4, ExpectedLine = -1, ExpectedColumn = 4, Name = "Greeter.greet", Filename = "test.ts" },
                new { InLine = 5, InColumn = 32, ExpectedLine = 3, ExpectedColumn = 29, Name = "Greeter.greet", Filename = "test.ts" },
                new { InLine = 6, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter.greet", Filename = "test.ts" },
                new { InLine = 7, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 8, InColumn = 0, ExpectedLine = 5, ExpectedColumn = 0, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 9, InColumn = 0, ExpectedLine = 5, ExpectedColumn = 1, Name = "Greeter", Filename = "test.ts" },
                new { InLine = 10, InColumn = 0, ExpectedLine = -1, ExpectedColumn = 0, Name = "", Filename = "" },
            };
            for (int i = 0; i < testCases.Length; i++) {
                Console.WriteLine("{0} {1}", testCases[i].InLine, testCases[i].InColumn);
                SourceMapping mapping;
                if (map.TryMapPoint(testCases[i].InLine, testCases[i].InColumn, out mapping)) {
                    Assert.AreEqual(testCases[i].Filename, mapping.Filename);
                    Assert.AreEqual(testCases[i].Name, mapping.Name);
                    Assert.AreEqual(testCases[i].ExpectedLine, mapping.Line);
                    Assert.AreEqual(testCases[i].ExpectedColumn, mapping.Column);
                } else {
                    Assert.AreEqual(-1, testCases[i].ExpectedLine);
                }
            }
        }

        [TestMethod, Priority(0)]
        public void TestVersion() {
            try {
                new SourceMap(new StringReader("{}"));
                Assert.Fail("Exception not thrown on empty map");
            } catch (NotSupportedException ex) {
                Assert.IsTrue(ex.Message.Contains("V3"));
            }
            try {
                new SourceMap(new StringReader("{version:1}"));
                Assert.Fail("Exception not thrown on V1 map");
            } catch (NotSupportedException ex) {
                Assert.IsTrue(ex.Message.Contains("V3"));
            }
            try {
                new SourceMap(new StringReader("{version:4}"));
                Assert.Fail("Exception not thrown on V3 map");
            } catch (NotSupportedException ex) {
                Assert.IsTrue(ex.Message.Contains("V3"));
            }
        }

        [TestMethod, Priority(0)]
        public void TestNames() {
            var map = new SourceMap(new StringReader("{version:3, names:['foo']}"));
            Assert.AreEqual(map.Names[0], "foo");
        }


        [TestMethod, Priority(0)]
        public void TestSources() {
            var map = new SourceMap(new StringReader("{version:3, sources:['test.ts']}"));
            Assert.AreEqual(map.Sources[0], "test.ts");

            map = new SourceMap(new StringReader("{version:3, sources:['test.ts'], sourceRoot:'root_path\\\\'}"));
            Assert.AreEqual(map.Sources[0], "root_path\\test.ts");
        }

        [TestMethod, Priority(0)]
        public void TestFile() {
            var map = new SourceMap(new StringReader("{version:3, file:'test.js'}"));
            Assert.AreEqual(map.File, "test.js");
        }

        [TestMethod, Priority(0)]
        public void TestInvalidJson() {
            try {
                var map = new SourceMap(new StringReader("{'test.js\\'}"));
                Assert.Fail("Argument exception not raised");
            } catch (ArgumentException) {
            }
        }
    }
}

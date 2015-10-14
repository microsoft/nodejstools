//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Formatting;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisTests {
    [TestClass]
    public class FormatterTests {
#if FALSE
        [TestMethod, Priority(0)]
        public void ReformatDirectory() {
            ReformatDirectory("C:\\Source\\Express\\node_modules\\", "C:\\Source\\Express\\formatted\\");
        }

        private void ReformatDirectory(string inp, string output) {
            FormattingOptions options = new FormattingOptions() { SpacesPerIndent = 2, SpaceAfterFunctionInAnonymousFunctions = false };
            foreach (var file in Directory.GetFiles(inp, "*.js", SearchOption.AllDirectories)) {
                var newCode = FormatCode(File.ReadAllText(file), options);
                var outFile = Path.Combine(output, file.Substring(inp.Length));
                Directory.CreateDirectory(Path.GetDirectoryName(outFile));

                File.WriteAllText(outFile, newCode);
            }
        }
#endif

        [ClassInitialize]
        public static void ClassInititalize(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0)]
        public void TestAnonymousFunction() {
            TestCode(
@"a('hello', 15, function(err, res) {
console.log(err);
  b(res, 55, function(err, res2) {
console.log(err);
  });
});",
    @"a('hello', 15, function (err, res) {
  console.log(err);
  b(res, 55, function (err, res2) {
    console.log(err);
  });
});", new FormattingOptions() { SpacesPerIndent = 2 });

        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1351
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNestedFunctionAndArrayLiteral() {
            TestCode(
@"var a = require('a');
a.mount('/', [
pipe.static({ root: 'b' }),  
pipe.proxy({
'a': 1
})
]);",
    @"var a = require('a');
a.mount('/', [
    pipe.static({ root: 'b' }),  
    pipe.proxy({
        'a': 1
    })
]);");

            TestCode(
@"switch (abc) {
    case foo: function a() {
    x = 42; y = 100;[1,
        2, 3]
    }
}",
    @"switch (abc) {
    case foo: function a() {
        x = 42; y = 100; [1,
            2, 3]
    }
}");
        }

        [TestMethod, Priority(0)]
        public void TestJsonArray() {
            TestCode(
@"(function (seedData) {
    seedData.initialNotes = [{
            name: ""category"",
            notes: [{
                    note: ""note1"",
                    author: ""author1"", 
            color: ""blue""
                },
    {
                    note: ""note1"",
                    author: ""author2"",
        color: ""green""
                }]
        }];
})(module.exports);",
@"(function (seedData) {
    seedData.initialNotes = [{
            name: ""category"",
            notes: [{
                    note: ""note1"",
                    author: ""author1"", 
                    color: ""blue""
                },
                {
                    note: ""note1"",
                    author: ""author2"",
                    color: ""green""
                }]
        }];
})(module.exports);"

                );
        }

        [TestMethod, Priority(0)]
        public void TestFormatAfterInvalidKey() {
            Assert.AreEqual(0, Formatter.GetEditsAfterKeystroke("function f  () { }", 0, ':').Length);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestSemicolonEndOfDocument() {
            var testCode = new[] { 
                // https://nodejstools.codeplex.com/workitem/1102
                new { Before = "x=1\r\nconsole.log( 'hi');", After = "x=1\r\nconsole.log('hi');" },
                // https://nodejstools.codeplex.com/workitem/1075
                new { Before = "function hello () {\r\n    return;", After = "function hello() {\r\n    return;" }
            };

            foreach (var test in testCode) {
                var code = test.Before;
                Console.WriteLine(code);

                TestCode(code.IndexOf(';') + 1, ';', code, test.After);
            }
        }

        [TestMethod, Priority(0)]
        public void TestFormatAfterEnter() {
            var testCode = new[] { 
                // https://nodejstools.codeplex.com/workitem/1078
                new { Before = "function f() {\r\n    if(true)!\r\n}", After = "function f() {\r\n    if (true)\r\n\r\n}" },
                // https://nodejstools.codeplex.com/workitem/1075
                new { Before = "function hello() {!", After = "function hello() {\r\n" },
                // https://nodejstools.codeplex.com/workitem/1136
                new { Before = ";\r\n{\r\n    /*!\r\n}", After = ";\r\n{\r\n    /*\r\n\r\n}" },
                // https://nodejstools.codeplex.com/workitem/1133
                new { Before = ";\r\nvar mailOptions = {\r\n    from: 'blahblah',!", After = ";\r\nvar mailOptions = {\r\n    from: 'blahblah',\r\n" },
                // https://nodejstools.codeplex.com/workitem/1175
                new { Before = ";\r\nif(true)\r\n    if (true)!", After = ";\r\nif (true)\r\n    if (true)\r\n" },
            };

            foreach (var test in testCode) {
                Console.WriteLine(test.Before);
                string code = test.Before.Replace("!", "\r\n");
                int enterIndex = code.IndexOf(';') + 2;
                var lines = code.Split(new[] { "\r\n" }, StringSplitOptions.None);
                int startIndex = 0, endIndex = lines[0].Length + lines[1].Length + ("\r\n".Length * 2);
                for (int i = 1; i < lines.Length; i++) {
                    if (enterIndex < endIndex) {
                        break;
                    }

                    startIndex += lines[i - 1].Length + "\r\n".Length;
                    endIndex += lines[i].Length + "\r\n".Length;
                }

                TestEnter(startIndex, endIndex, code, test.After);
            }
        }

        [TestMethod, Priority(0)]
        public void TestFormatAfterSemiColon() {
            string surroundingCode = "x=1";
            var testCode = new[] { 
                new { Before = "function f() {    \r\n    return   42;\r\n}", After = "function f() {    \r\n    return 42;\r\n}" },
                new { Before = "function f() {    \r\n    return   42 ;\r\n}", After = "function f() {    \r\n    return 42;\r\n}" },
                new { Before = "function f() {    \r\n    return   ;\r\n}", After = "function f() {    \r\n    return;\r\n}" },
                new { Before = "throw   42;", After = "throw 42;" },
                new { Before = "throw ;", After = "throw;" },
                new { Before = "x=42;", After = "x = 42;" },
                new { Before = "x=42;", After = "x = 42;" },
                new { Before = "debugger ;", After = "debugger;" },
                new { Before = "while (true) {    \r\n    break ;\r\n}", After = "while (true) {    \r\n    break;\r\n}" },
                new { Before = "while (true) {    \r\n    continue ;\r\n}", After = "while (true) {    \r\n    continue;\r\n}" },
                new { Before = "var x=1,y=2;", After = "var x = 1, y = 2;" },
                // https://nodejstools.codeplex.com/workitem/1346
                new { Before = "while(true){\r\nconsole.log('hello';)\r\n}", After = "while(true){\r\nconsole.log('hello';)\r\n}"}
            };

            foreach (var test in testCode) {
                Console.WriteLine(test.Before);
                string code = surroundingCode + "\r\n" + test.Before + "\r\n" + surroundingCode;
                string expected = surroundingCode + "\r\n" + test.After + "\r\n" + surroundingCode;
                TestCode(
                    code.IndexOf(';') + 1,
                    ';',
                    code,
                    expected
                );
            }
        }

        [TestMethod, Priority(0)]
        public void TestSemicolonOnOwnLine() {
            // https://nodejstools.codeplex.com/workitem/1473
            TestCode(
    @"function f() {
    console.log('hi')
;
}",
    @"function f() {
    console.log('hi')
    ;
}");

            TestCode(
        @"function f() {
    console.log('hi') // comment
    ;
}",
        @"function f() {
    console.log('hi') // comment
    ;
}");
        }

        [TestMethod, Priority(0)]
        public void TestFormattingFunctionAsArgumentToFunction() {
            // Format dedenting first argument too much
            // https://nodejstools.codeplex.com/workitem/1463
            TestCode(
    @"g(
    function f(a, b, c) 
    {
        console.log('hi');
        console.log('bar')
    }
)",
    @"g(
    function f(a, b, c) {
        console.log('hi');
        console.log('bar')
    }
)"
  );
            // Format indenting second argument function too much
            // https://nodejstools.codeplex.com/workitem/1459
            TestCode(
    @"g(function () {
    console.log('hi')
}, function () {
        console.log('toofar')
    });",
    @"g(function () {
    console.log('hi')
}, function () {
    console.log('toofar')
});");

            TestCode(
@"g(
    function () {
        console.log('hi')
    }, function () {
        console.log('hi2')
    });",
@"g(
    function () {
        console.log('hi')
    }, function () {
        console.log('hi2')
    });");
        }

        [TestMethod, Priority(0)]
        public void TestArrayLiteral() {
            var testCode = new[] { 
                // https://nodejstools.codeplex.com/workitem/1474
                new { Before = "function f() {\r\n    console.log('hi')\r\n    x = [1,2,3]\r\n}",
                      After  = "function f() {\r\n    console.log('hi')\r\n    x = [1, 2, 3]\r\n}"},
                new { Before = "function f() {\r\n    console.log('hi')\r\n    [1,2,3]\r\n}",
                      After  = "function f() {\r\n    console.log('hi')\r\n    [1, 2, 3]\r\n}"},
                new { Before = "g(\r\n    function f(a, b, c)\r\n    {\r\n        console.log('hi');\r\n        [1,2,3]\r\n    }\r\n)",
                      After  = "g(\r\n    function f(a, b, c) {\r\n        console.log('hi');\r\n        [1, 2, 3]\r\n    }\r\n)"},
                new { Before = "var x =[\r\n1, 2, 3\r\n    ]",
                      After  = "var x = [\r\n    1, 2, 3\r\n]"},
                // Test multiple lines stay aligned, but individual single line arrays fixed
                new { Before = "function f() {\r\n    var x = [[1],\r\n             [2,   3],\r\n             [3,4,5]]\r\n}",
                      After  = "function f() {\r\n    var x = [[1],\r\n        [2, 3],\r\n        [3, 4, 5]]\r\n}"},
                // https://nodejstools.codeplex.com/workitem/1494 We shouldn't push the 3 & 4 together
                new { Before = "var x = [1,2,3 4]",
                      After  = "var x = [1, 2, 3 4]"},
            };

            foreach (var test in testCode) {
                TestCode(test.Before, test.After);
            }
        }

        [TestMethod, Priority(0)]
        public void TestFormatAfterBadFor() {
            TestCode(@"function g() { 
 for(int i = 0; i<1000000; i++) { 
 }",
   @"function g() {
    for (int i = 0; i < 1000000; i++) { 
    }");

            // https://nodejstools.codeplex.com/workitem/1475 
            TestCode(@"for {
    x = 2}",
   @"for {
    x = 2}");
        }

        [TestMethod, Priority(0)]
        public void TestIndexingOnFollowingLine() {
            // https://nodejstools.codeplex.com/workitem/1465
            TestCode(
    @"g()
[]",
    @"g()
[]");
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1204
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestShebang() {
            TestCode("#!/usr/env/node\r\n function f() {\r\n}", "#!/usr/env/node\r\nfunction f() {\r\n}");
        }

        [TestMethod, Priority(0)]
        public void TestInvalidTrailingQuote() {
            TestCode("var x = foo()'", "var x = foo() '");
            TestCode("return 42'", "return 42 '");
            TestCode("continue'", "continue '");

            TestCode("break'", "break '");
            TestCode("throw 42'", "throw 42 '");
        }

        [TestMethod, Priority(0)]
        public void TestMember() {
            TestCode(" a.b", "a.b");
            TestCode("a .b", "a.b");
            TestCode("a. b", "a.b");
            TestCode(" a . b", "a.b");
        }

        [TestMethod, Priority(0)]
        public void TestInvalidMember() {
            TestCode("x.42", "x.42");
            TestCode("x3.42", "x3.42");
            TestCode("x_.23", "x_.23");
            TestCode("x23.42", "x23.42");
            TestCode("x23.42m", "x23.42m");
            TestCode("x23.\"hello\"", "x23.\"hello\"");
        }

        [TestMethod, Priority(0)]
        public void TestCall() {
            TestCode("a()", "a()");
            TestCode("a ()", "a()");
            TestCode("a( b)", "a(b)");
            TestCode("a(b )", "a(b)");
            TestCode("a( b )", "a(b)");
            TestCode("a(b,c)", "a(b, c)");
            TestCode("a(b,  c)", "a(b, c)");
            TestCode("a(b, c )", "a(b, c)");
            //https://nodejstools.codeplex.com/workitem/1525
            TestCode(
@"socket.on(""disconnect"", function () {
    var a = 2;
}).on('message', function () {
    var b = 1;
});",
@"socket.on(""disconnect"", function () {
    var a = 2;
}).on('message', function () {
    var b = 1;
});");
        }

        [TestMethod, Priority(0)]
        public void TestUnaryOperator() {
            TestCode("typeof  x", "typeof x");
            TestCode("delete  x", "delete x");
            TestCode("void  x", "void x");
            TestCode("++ x", "++x");
        }

        [TestMethod, Priority(0)]
        public void TestEmpty() {
            TestCode("if (true);", "if (true);");
            TestCode("if (true) ;", "if (true);");

            // https://github.com/Microsoft/nodejstools/issues/24
            TestCode(
@"var x = function () {
    if (a) {
        123455
    };
};",
@"var x = function () {
    if (a) {
        123455
    };
};");

            // Shouldn't insert additional space between semicolons.
            TestCode(
@"function a() {
    var a;;
}",
@"function a() {
    var a;;
}");
        }

        [TestMethod, Priority(0)]
        public void TestNew() {
            TestCode("var x = new  Blah();", "var x = new Blah();");

        }

        [TestMethod, Priority(0)]
        public void TestFormatRange() {
            string surroundingCode = "x=1";
            var testCode = new[] { 
                new { Before = "function f() {    \r\n    return   42;\r\n}", After = "function f() {\r\n    return 42;\r\n}" },
                new { Before = "throw   42;", After = "throw 42;" },
                new { Before = "throw ;", After = "throw;" },
                new { Before = "x=42;", After = "x = 42;" },
                new { Before = "x=42;", After = "x = 42;" },
                new { Before = "debugger ;", After = "debugger;" },
                new { Before = "while (true) {    \r\n    break ;\r\n}", After = "while (true) {\r\n    break;\r\n}" },
                new { Before = "while (true) {    \r\n    continue ;\r\n}", After = "while (true) {\r\n    continue;\r\n}" },
                new { Before = "var x=1,y=2;", After = "var x = 1, y = 2;" },
                new { Before = "var x=1,y=2 ;", After = "var x = 1, y = 2;" },
            };

            foreach (var test in testCode) {
                Console.WriteLine(test.Before);
                string code = surroundingCode + "\r\n" + test.Before + "\r\n" + surroundingCode;
                string expected = surroundingCode + "\r\n" + test.After + "\r\n" + surroundingCode;

                // also check range
                TestCode(
                    surroundingCode.Length,
                    code.Length - surroundingCode.Length,
                    code,
                    expected
                );
            }
        }

        [TestMethod, Priority(0)]
        public void TestFormatAfterCloseBrace() {
            string surroundingCode = "x=1";
            var testCode = new[] { 
                new { Before = "while(true) {\r\nblah\r\n!", After = "while (true) {\r\n    blah\r\n}" },
                new { Before = "with(true) {\r\nblah\r\n!", After = "with (true) {\r\n    blah\r\n}" },
                new { Before = "for(var i=0;i<10;i++) {\r\nblah\r\n!", After = "for (var i = 0; i < 10; i++) {\r\n    blah\r\n}" },
                new { Before = "for(var x  in  []) {\r\nblah\r\n!", After = "for (var x in []) {\r\n    blah\r\n}" },
                new { Before = "{\r\nblah\r\n!", After = "{\r\n    blah\r\n}" },
                new { Before = "switch(abc){\r\ncase 42: return null;\r\n!", After = "switch (abc) {\r\n    case 42: return null;\r\n}" },
                new { Before = "try {\r\nabc\r\n!", After = "try {\r\n    abc\r\n}" },
                new { Before = "try {\r\nabc\r\n}catch(abc){\r\nabc\r\n!", After = "try {\r\n    abc\r\n} catch (abc) {\r\n    abc\r\n}" },
                new { Before = "try {\r\nabc\r\n}finally{\r\nabc\r\n!", After = "try {\r\n    abc\r\n} finally {\r\n    abc\r\n}" },
                new { Before = "{\r\n    break;\r\n!", After = "{\r\n    break;\r\n}" },
                new { Before = "{\r\n    break ;\r\n!", After = "{\r\n    break;\r\n}" },
                // https://nodejstools.codeplex.com/workitem/1346
                new { Before = "module.exports = {\r\n    f: function () { console!\r\n}", After = "module.exports = {\r\n    f: function () { console }\r\n}" },
            };

            foreach (var test in testCode) {
                Console.WriteLine(test.Before);
                string indexCode = surroundingCode + "\r\n" + test.Before + "\r\n" + surroundingCode;
                string code = surroundingCode + "\r\n" + test.Before.Replace('!', '}') + "\r\n" + surroundingCode;
                string expected = surroundingCode + "\r\n" + test.After + "\r\n" + surroundingCode;
                TestCode(
                    indexCode.IndexOf('!') + 1,
                    '}',
                    code,
                    expected
                );
            }
        }

        [TestMethod, Priority(0)]
        public void TestLabeledStatement() {
            TestCode(@"foo: {
    42;
}",
 @"foo: {
    42;
}");

        }

        [TestMethod, Priority(0)]
        public void TestContinue() {
            TestCode(
@"while (true) {
    continue
}",
@"while (true) {
    continue
}"
);

            TestCode(
@"while (true) {
    continue  
}",
@"while (true) {
    continue
}"
);
            TestCode(
@"while (true) {
label:
    while (true) {
        continue   label
    }
}",
@"while (true) {
    label:
        while (true) {
            continue label
        }
}"
);
        }

        [TestMethod, Priority(0)]
        public void TestBlock() {
            TestCode(
                "{\nvar b;\n}",
                "{\n    var b;\n}",
                new FormattingOptions() {
                    NewLine = "\n"
                });

            TestCode(
                "{\rvar b;\r}",
                "{\r    var b;\r}",
                new FormattingOptions() {
                    NewLine = "\r"
                }
                );

            TestCode(
                "{\r\nvar b;\r\n}",
                "{\r\n    var b;\r\n}"
                );
        }

        [TestMethod, Priority(0)]
        public void TestBreak() {
            TestCode(
@"while (true) {
    break
}",
@"while (true) {
    break
}"
);

            TestCode(
@"while (true) {
    break  
}",
@"while (true) {
    break
}"
);
            TestCode(
@"while (true) {
    break}",
@"while (true) {
    break
}"
);

            TestCode(
@"while (true) {
label:
    while (true) {
        break   label
    }
}",
@"while (true) {
    label:
        while (true) {
            break label
        }
}"
);
        }

        [TestMethod, Priority(0)]
        public void TestFunction() {
            TestCode(
@"function f () {
}",
@"function f() {
}");
            // https://nodejstools.codeplex.com/workitem/1740
            TestCode(
@"exports.hugues = function(req,res){    res.render('hugues', { title: 'Hugues', year: new Date().getFullYear(), message: 'Your hugues page.' });
};",
@"exports.hugues = function (req, res) {
    res.render('hugues', { title: 'Hugues', year: new Date().getFullYear(), message: 'Your hugues page.' });
};");
        }

        [TestMethod, Priority(0)]
        public void TestReturn() {
            TestCode(
@"function f() {
    return
}",
@"function f() {
    return
}"
);

            TestCode(
@"function f() {
    return  
}",
@"function f() {
    return
}"
);
            TestCode(
@"function f() {
    return   42
}",
@"function f() {
    return 42
}"
);

            TestCode(
@"function f() {
    return   42;
}",
@"function f() {
    return 42;
}"
);
        }

        [TestMethod, Priority(0)]
        public void TestYield() {
            TestCode(
@"function *f() {
    yield
}",
@"function* f() {
    yield
}"
);

            TestCode(
@"function *f() {
    yield  
}",
@"function* f() {
    yield
}"
);
            TestCode(
@"function *f() {
    yield   42
}",
@"function* f() {
    yield 42
}"
);

            TestCode(
@"function *f() {
    yield   42;
}",
@"function* f() {
    yield 42;
}"
);

            TestCode(
@"function * f() {
    yield   42;
}",
@"function* f() {
    yield 42;
}"
);

            TestCode(
@"function * f() {
    yield  *  42;
}",
@"function* f() {
    yield* 42;
}"
);
        }

        [TestMethod, Priority(0)]
        public void TestThrow() {
            TestCode(
@"function f() {
    throw
}",
@"function f() {
    throw
}"
);

            TestCode(
@"function f() {
    throw  
}",
@"function f() {
    throw
}"
);
            TestCode(
@"function f() {
    throw   42
}",
@"function f() {
    throw 42
}"
);

            TestCode(
@"function f() {
    throw   42;
}",
@"function f() {
    throw 42;
}"
);
        }

        [TestMethod, Priority(0)]
        public void TestObjectLiteral() {
            TestCode(
@"x = { get   foo() { }, set   foo(value) { } }",
@"x = { get foo() { }, set foo(value) { } }"
);


            TestCode(
@"x = {  }",
@"x = {}"
);

            TestCode(
@"x = {
}",
@"x = {
}"
);

            TestCode(
@"x = {
a: 42,
b: 100}",
@"x = {
    a: 42,
    b: 100
}"
            );

            TestCode(
@"x = {
a: 42, b: 100,
c: 42, d: 100}",
@"x = {
    a: 42, b: 100,
    c: 42, d: 100
}"
            );
            TestCode(
@"x = {a:42, b:100}",
@"x = { a: 42, b: 100 }"
            );

            //https://nodejstools.codeplex.com/workitem/1525
            TestCode(
@"var a = function (test) {
    return {
    }
}",
@"var a = function (test) {
    return {
    }
}"
);

            //https://nodejstools.codeplex.com/workitem/1560
            TestCode(
@"var a = function (test) {
    return {
        
    }
}",
@"var a = function (test) {
    return {
        
    }
}"
);
        }

        [TestMethod, Priority(0)]
        public void TestIndentObjectLiteralWithoutComma() {
            // https://nodejstools.codeplex.com/workitem/1782
            TestCode(@"Main.Test.prototype = {
    testFunc: function () {

    },
    testFunc3: function () {}
    testFunc2: function () {

    }
}",
@"Main.Test.prototype = {
    testFunc: function () {

    },
    testFunc3: function () { }
    testFunc2: function () {

    }
}");

            TestCode(@"Main.Test.prototype = {
    testFunc: function () {

    },
    testFunc3: function () {
}
    testFunc2: function () {

    }
}",
@"Main.Test.prototype = {
    testFunc: function () {

    },
    testFunc3: function () {
    }
    testFunc2: function () {

    }
}");
        }

        [TestMethod, Priority(0)]
        public void TestDoWhile() {
            TestCode(@"do
    { var a
}   while (1)",
              @"do {
    var a
} while (1)");


        }

        [TestMethod, Priority(0)]
        public void TestControlFlowBraceCombo() {
            var options = new FormattingOptions() { OpenBracesOnNewLineForControl = false, SpaceAfterKeywordsInControlFlowStatements = false };

            TestCode(
@"do {
} while (true);",
@"do {
} while (true);",
                options
            );

            TestCode(
@"try {
} finally {
}",
@"try {
} finally {
}",
                options
            );
        }

        [TestMethod, Priority(0)]
        public void TestSpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis() {
            var options = new FormattingOptions() { SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis = true };

            TestCode(
@"for (var x in abc) {
}",
@"for ( var x in abc ) {
}",
                options
            );


            TestCode(
@"for (var i = 0; i < 10; i++) {
}",
@"for ( var i = 0; i < 10; i++ ) {
}",
                options
            );

            TestCode(
@"if (true) {
}",
@"if ( true ) {
}",
                options
            );

            TestCode(
@"while (true) {
}",
@"while ( true ) {
}",
                options
            );

            TestCode(
@"do {
} while (true);",
@"do {
} while ( true );",
                options
            );

            TestCode(
@"try {
} catch (foo) {
}",
@"try {
} catch ( foo ) {
}",
                options
            );

            TestCode(
@"function () {
}",
@"function () {
}",
                options
            );

            TestCode(
@"function (  ) {
}",
@"function () {
}",
                options
            );

            TestCode(
@"function (a) {
}",
@"function ( a ) {
}",
                options
            );

            TestCode(
@"function (a, b) {
}",
@"function ( a, b ) {
}",
        options
    );

            TestCode(
@"(a)",
@"( a )",
        options
    );

            TestCode(
@"f(a)",
@"f( a )",
        options
    );

            TestCode(
@"f(a, b)",
@"f( a, b )",
        options
    );

            TestCode(
@"new f(a)",
@"new f( a )",
       options
   );

            TestCode(
@"new f(a, b)",
@"new f( a, b )",
        options
    );

            TestCode(
@"f[a]",
@"f[a]",
        options
    );

            TestCode(
@"f[a, b]",
@"f[a, b]",
        options
    );

            TestCode(
@"switch (abc) {
    case 42: break;
}",
@"switch ( abc ) {
    case 42: break;
}",
        options
    );

            TestCode(
@"(x)",
@"( x )",
        options
    );
        }

        [TestMethod, Priority(0)]
        public void TestSpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis2() {
            var options = new FormattingOptions() { SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis = false };

            TestCode(
@"for ( var x in abc ) {
}",
@"for (var x in abc) {
}",
                options
            );


            TestCode(
@"for ( var i = 0; i < 10; i++ ) {
}",
@"for (var i = 0; i < 10; i++) {
}",
                options
            );

            TestCode(
@"if ( true ) {
}",
@"if (true) {
}",
                options
            );

            TestCode(
@"while ( true ) {
}",
@"while (true) {
}",
                options
            );

            TestCode(
@"do {
} while ( true );",
@"do {
} while (true);",
                options
            );

            TestCode(
@"try {
} catch ( foo ) {
}",
@"try {
} catch (foo) {
}",
                options
            );

            TestCode(
@"function () {
}",
@"function () {
}",
                options
            );


            TestCode(
@"function ( a ) {
}",
@"function (a) {
}",
                options
            );

            TestCode(
@"function ( a, b ) {
}",
@"function (a, b) {
}",
        options
    );

            TestCode(
@"( a )",
@"(a)",
        options
    );

            TestCode(
@"f( a )",
@"f(a)",
        options
    );

            TestCode(
@"f( a, b )",
@"f(a, b)",
        options
    );

            TestCode(
@"new f( a )",
@"new f(a)",
       options
   );

            TestCode(
@"new f( a, b )",
@"new f(a, b)",
        options
    );

            TestCode(
@"f[a]",
@"f[a]",
        options
    );

            TestCode(
@"f[a, b]",
@"f[a, b]",
        options
    );

            TestCode(
@"switch ( abc ) {
    case 42: break;
}",
@"switch (abc) {
    case 42: break;
}",
        options
    );

            TestCode(
@"( x )",
@"(x)",
        options
    );
        }

        [TestMethod, Priority(0)]
        public void TestArithmetic() {
            TestCode("a+.0", "a + .0");
        }

        [TestMethod, Priority(0)]
        public void TestVariableDecl() {
            TestCode(@"var i = 0, j = 1;", @"var i = 0, j = 1;");
            TestCode(@"var i=0, j=1;", @"var i = 0, j = 1;");
            TestCode(@"var    i   =   0    ,   j  =  1;", @"var i = 0, j = 1;");

            TestCode(@"var i = 0, j = 1;", @"var i=0, j=1;", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = false });
        }

        [TestMethod, Priority(0)]
        public void TestLexcialDecl() {
            TestCode(@"i=1", @"i = 1");
        }

        [TestMethod, Priority(0)]
        public void TestForIn() {
            TestCode(
@"for(    var    x     in    abc) {
}",
@"for (var x in abc) {
}");
        }

        [TestMethod, Priority(0)]
        public void TestFor() {
            var options = new FormattingOptions() { SpaceAfterSemiColonInFor = true };
            TestCode(
@"for (var i = 0;i < 10;i++) {
}",
@"for (var i = 0; i < 10; i++) {
}",
  options);

            options = new FormattingOptions() { SpaceAfterSemiColonInFor = false };
            TestCode(
@"for (var i = 0; i < 10; i++) {
}",
@"for (var i = 0;i < 10;i++) {
}",
  options);
        }

        [TestMethod, Priority(0)]
        public void TestSpaceAfterComma() {
            var options = new FormattingOptions() { SpaceAfterComma = true };
            TestCode(
@"
1,2,3
x(1,2,3)
function x(a,b,c) {
}",
@"
1, 2, 3
x(1, 2, 3)
function x(a, b, c) {
}",
  options);

            options = new FormattingOptions() { SpaceAfterComma = false };
            TestCode(
@"
1, 2, 3
x(1, 2, 3)
function x(a, b, c) {
}",
@"
1,2,3
x(1,2,3)
function x(a,b,c) {
}",
                options);
        }

        [TestMethod, Priority(0)]
        public void TestSpaceAfterFunctionInAnonymousFunctions() {
            var options = new FormattingOptions() { SpaceAfterFunctionInAnonymousFunctions = true };
            TestCode(
@"
x = function() {
}",
@"
x = function () {
}",
  options);

            options = new FormattingOptions() { SpaceAfterFunctionInAnonymousFunctions = false };
            TestCode(
@"
x = function () {
}",
@"
x = function() {
}",

                options);
        }

        [TestMethod, Priority(0)]
        public void TestNestedSwitch() {
            TestCode("switch (a){\r\n    case 1: x += 2;\r\n case   2  : \r\n     for (var i=0;i<10;i++)\r\ni  --;\r\n}\r\n",
            @"switch (a) {
    case 1: x += 2;
    case 2:
        for (var i = 0; i < 10; i++)
            i--;
}
");
        }

        [TestMethod, Priority(0)]
        public void TestNestedIfs() {
            TestCode(@"if(1)if(1)if(1)if(1) {
    x += 2
}",
@"if (1) if (1) if (1) if (1) {
    x += 2
}");

            TestCode(@"if(1)if(1)if(1)if(1) {x += 2
}",
@"if (1) if (1) if (1) if (1) {
    x += 2
}");
        }

        [TestMethod, Priority(0)]
        public void TestSwitch() {
            TestCode("switch (a){\r\ncase   1   :   x+=2 ;    break;\r\n    case 2:{\r\n    }\r\n}\r\n",
                     "switch (a) {\r\n    case 1: x += 2; break;\r\n    case 2: {\r\n    }\r\n}\r\n");

            TestCode(
    "switch (x)\r\n     { case 1:   { var a }\r\n}",
    "switch (x) {\r\n    case 1: { var a }\r\n}"
);

            TestCode(@"switch(abc) {
    case 1:   x;
break;
}",
@"switch (abc) {
    case 1: x;
        break;
}");

            TestCode(@"switch(abc) {
    case 1:   x;   y;
break;
}",
@"switch (abc) {
    case 1: x; y;
        break;
}");

            TestCode(@"switch(abc) {
    case 1:   x;   y;
z;   zz;
break;
}",
@"switch (abc) {
    case 1: x; y;
        z; zz;
        break;
}");

            TestCode(
@"switch(abc) {
    case 42: break;
}",
@"switch (abc) {
    case 42: break;
}"
    );

            TestCode(
@"switch(abc) {
case 42: break;
}",
@"switch (abc) {
    case 42: break;
}"
    );

            TestCode(@"switch(abc) {
    case 1:
x
break;
}",
@"switch (abc) {
    case 1:
        x
        break;
}");


        }

        [TestMethod, Priority(0)]
        public void TestNewLineBracesForFunctions() {
            var options = new FormattingOptions() { OpenBracesOnNewLineForFunctions = true };

            TestCode(
@"function x() {
}",
@"function x()
{
}",
               options);

            options = new FormattingOptions() { OpenBracesOnNewLineForFunctions = false };
            TestCode(
@"function x()
{
}",
@"function x() {
}",
               options);
        }

        [TestMethod, Priority(0)]
        public void TestInsertTabs() {
            var options = new FormattingOptions() { SpacesPerIndent = null };
            TestCode(
@"switch (abc) {
    case 42: break;
}",
"switch (abc) {\r\n\tcase 42: break;\r\n}",
                options
            );

            TestCode(
                "switch (abc) {\r\n\tcase 42: break;\r\n}",
                "switch (abc) {\r\n\tcase 42: break;\r\n}",
                options
            );

            TestCode(
@"switch (abc) {
  case 42: break;
}",
"switch (abc) {\r\n\tcase 42: break;\r\n}",
                options
            );
        }

        [TestMethod, Priority(0)]
        public void TestComments() {
            // comments in weird spots can result in some slightly odd 
            // insertions or missing insertions.  These aren't set in stone
            // necessarily but these test cases make sure we're not doing
            // anything particularly horrible.  The current behavior is
            // mostly driven by whether or not we're scanning forwards or
            // backwards to replace a particular piece of white space.
            var options = new FormattingOptions() { SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis = true };
            TestCode(
@"if (/*comment*/true/*comment*/) {
}",
@"if (/*comment*/true /*comment*/) {
}",
  options);

            TestCode(
@"switch (abc) /*comment*/ {
    case 'abc': break;
}",
@"switch (abc) /*comment*/ {
    case 'abc': break;
}");

            TestCode(
@"switch (abc) /*comment*/
{
    case 'abc': break;
}",
@"switch (abc) /*comment*/ {
    case 'abc': break;
}");

            TestCode(
@"var x = 1, /* comment */
    y = 2;",
@"var x = 1, /* comment */
    y = 2;"
);

            TestCode(
@"var x = 1, /* comment */y = 2;",
@"var x = 1, /* comment */ y = 2;"
);

            TestCode(
@"x = a/*comment*/+/*comment*/b;",
@"x = a /*comment*/+/*comment*/ b;"
);

            TestCode(
@"x = a/*comment*/+/*comment*/
      b;",
@"x = a /*comment*/+/*comment*/
      b;"
);

            TestCode(
@"x = a/*comment*/+
      /*comment*/b;",
@"x = a /*comment*/+
      /*comment*/ b;"
);
        }

        [TestMethod, Priority(0)]
        public void TestSingleLineComments() {
            TestCode(
@"var x = {'abc':42,
         'bar':100 // foo
}",
@"var x = {
    'abc': 42,
    'bar': 100 // foo
}");

            TestCode(@"if(true)
// test
test;",
@"if (true)
    // test
    test;");

            TestCode(@"var x=function () {
//comment
return 1;
}", @"var x = function () {
    //comment
    return 1;
}");

            TestCode(
@"if(foo) // bar
    abc",
@"if (foo) // bar
    abc");

            TestCode(
@"switch (foo) // foo
{
}",
@"switch (foo) // foo
{
}");

            TestCode(
@"if (foo) // foo
{
}",
@"if (foo) // foo
{
}");

            TestCode(
@"if (foo) { // foo
}",
@"if (foo) { // foo
}");

            TestCode(
@"if(foo) /*bar*/ // foo
{
}",
@"if (foo) /*bar*/ // foo
{
}");

            TestCode(
@"if(foo/*comment*/) // foo
{
}",
@"if (foo/*comment*/) // foo
{
}");



            TestCode(
@"if(foo) // foo
{
} else // bar
{
}",
@"if (foo) // foo
{
} else // bar
{
}");

            TestCode(
@"if(foo)
{
} else // bar
{
}",
@"if (foo) {
} else // bar
{
}");

            TestCode(
@"var x = {'abc':42,
         'ba':100, // foo
         'quox':99
}",
@"var x = {
    'abc': 42,
    'ba': 100, // foo
    'quox': 99
}");

            TestCode(
@"for(var x in []) // abc
{
}",
@"for (var x in []) // abc
{
}");

            TestCode(
@"try {
} catch(abc) // comment
{
}",
@"try {
} catch (abc) // comment
{
}");

            TestCode(
@"try {
} finally // comment
{
}",
@"try {
} finally // comment
{
}");

            TestCode(
@"while(foo) // comment
{
}",
@"while (foo) // comment
{
}");

            TestCode(
@"with(foo) // comment
{
}",
@"with (foo) // comment
{
}");

            TestCode(
@"for(var i = 0; i < 10; i++) // comment
{
}",
@"for (var i = 0; i < 10; i++) // comment
{
}");

            TestCode(
@"for(; ;) // comment
{
}",
@"for (; ;) // comment
{
}");

            TestCode(
@"function f() // comment
{
}",
@"function f() // comment
{
}");

            TestCode(
@"switch (true) { // comment

}",
@"switch (true) { // comment

}");

            TestCode(
@"switch (true) // comment
{
}",
@"switch (true) // comment
{
}");

            // https://nodejstools.codeplex.com/workitem/1571
            TestCode(
@"e(p, function (ep) { // encode, then write results to engine
writeToEngine(ep);
});",
@"e(p, function (ep) { // encode, then write results to engine
    writeToEngine(ep);
});");
        }

        [TestMethod, Priority(0)]
        public void TestInsertSpaces() {
            var options = new FormattingOptions() { SpacesPerIndent = 2 };
            TestCode(
"switch (abc) {\r\n\tcase 42: break;\r\n}",
"switch (abc) {\r\n  case 42: break;\r\n}",
                options
            );

            TestCode(
                "switch (abc) {\r\n\t\tcase 42: break;\r\n}",
                "switch (abc) {\r\n  case 42: break;\r\n}",
                options
            );

            options = new FormattingOptions() { SpacesPerIndent = 6 };
            TestCode(
                "switch (abc) {\r\n\tcase 42: break;\r\n}",
                "switch (abc) {\r\n      case 42: break;\r\n}",
                options
            );

            TestCode(
                "switch (abc) {\r\n    case 42: break;\r\n}",
                "switch (abc) {\r\n      case 42: break;\r\n}",
                options
            );
        }

        [TestMethod, Priority(0)]
        public void TestNewLineBracesForFlowControl() {
            var options = new FormattingOptions() { OpenBracesOnNewLineForControl = true };
            TestCode(
@"switch (abc) {
    case 42: break;
}",
@"switch (abc)
{
    case 42: break;
}",
        options);

            TestCode(
@"do {
} while(true);",
@"do
{
} while(true);",
               options);

            TestCode(
@"while (true) {
}",
@"while (true)
{
}",
               options);

            TestCode(
@"with (true) {
}",
@"with (true)
{
}",
               options);

            TestCode(
@"for (var i = 0; i < 10; i++) {
}",
@"for (var i = 0; i < 10; i++)
{
}",
               options);

            TestCode(
@"for (var x in []) {
}",
@"for (var x in [])
{
}",
                options);

            TestCode(
@"if (true) {
}",
@"if (true)
{
}",
        options);

            TestCode(
@"if (true) {
} else {
}",
@"if (true)
{
} else
{
}",
        options);

            TestCode(
@"try {
} finally {
}",
@"try
{
} finally
{
}",
        options);

            TestCode(
@"try {
} catch(abc) {
}",
@"try
{
} catch (abc)
{
}",
        options);
        }

        [TestMethod, Priority(0)]
        public void TestNewLineBracesForFlowControl2() {
            var options = new FormattingOptions() { OpenBracesOnNewLineForControl = false };
            TestCode(
            @"switch (abc)
{
    case 42: break;
}",
            @"switch (abc) {
    case 42: break;
}",
                    options);

            TestCode(
@"do
{
} while(true);",
@"do {
} while(true);",
               options);

            TestCode(
@"while (true)
{
}",
@"while (true) {
}",
               options);

            TestCode(
@"with (true)
{
}",
@"with (true) {
}",
               options);

            TestCode(
@"for (var i = 0; i < 10; i++)
{
}",
@"for (var i = 0; i < 10; i++) {
}",
               options);

            TestCode(
@"for (var x in [])
{
}",
@"for (var x in []) {
}",
                options);

            TestCode(
@"if (true)
{
}",
@"if (true) {
}",
        options);

            TestCode(
@"if (true)
{
} else
{
}",
@"if (true) {
} else {
}",
        options);

            TestCode(
@"try
{
} finally
{
}",
@"try {
} finally {
}",
        options);

            TestCode(
@"try
{
} catch(abc)
{
}",
@"try {
} catch (abc) {
}",
        options);
        }

        [TestMethod, Priority(0)]
        public void TestIf() {
            // https://nodejstools.codeplex.com/workitem/1175
            TestCode(
@"if (true){
}else{
}",
@"if (true) {
} else {
}");

            TestCode(
@"if (true){
}
else{
}",
@"if (true) {
}
else {
}");

            TestCode(
@"if(true) {
if (true){
}
else{
}
}",
@"if (true) {
    if (true) {
    }
    else {
    }
}");

            TestCode(
@"if(true) {
    if (true){
    }
 else{
    }
}",
@"if (true) {
    if (true) {
    }
    else {
    }
}");

        }

        [TestMethod, Priority(0)]
        public void TestNestedBlock() {
            TestCode(
@"do {
if (true) {
aa
} else {
bb
}
} while(true);",
@"do {
    if (true) {
        aa
    } else {
        bb
    }
} while(true);");

            TestCode(@"if(true) {
    aa;  bb;
    cc;  dd
}",
@"if (true) {
    aa; bb;
    cc; dd
}");

            TestCode(
@"do {
for (var i = 0; i < 10; i++) {
}
} while(true);",
@"do {
    for (var i = 0; i < 10; i++) {
    }
} while(true);");


            TestCode(
@"do {
while (true) {
}
} while(true);",
@"do {
    while (true) {
    }
} while(true);");

            TestCode(
@"do {
with (true) {
}
} while(true);",
@"do {
    with (true) {
    }
} while(true);");

            TestCode(
@"do {
for (var x in []) {
}
} while(true);",
@"do {
    for (var x in []) {
    }
} while(true);");

            TestCode(
@"do {
do {
} while (true);
} while(true);",
@"do {
    do {
    } while (true);
} while(true);");

            TestCode(
@"do {
try {
} finally {
}
} while(true);",
@"do {
    try {
    } finally {
    }
} while(true);");

            TestCode(
@"do {
try {
} catch(arg) {
}
} while(true);",
@"do {
    try {
    } catch (arg) {
    }
} while(true);");

            TestCode(
@"do {
switch (abc) {
case 42: break;
}
} while(true);",
@"do {
    switch (abc) {
        case 42: break;
    }
} while(true);");
        }

        [TestMethod, Priority(0)]
        public void TestSpacesAroundBinaryOperator() {
            TestCode("x+y", "x + y", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = true });
            TestCode("x+y", "x+y", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = false });
            TestCode("x + y", "x + y", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = true });
            TestCode("x + y", "x+y", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = false });
            TestCode("x+y+z", "x + y + z", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = true });
            TestCode("x+y+z", "x+y+z", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = false });
            TestCode("x + y + z", "x + y + z", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = true });
            TestCode("x + y + z", "x+y+z", new FormattingOptions() { SpaceBeforeAndAfterBinaryOperator = false });
        }

        [TestMethod, Priority(0)]
        public void TestSpaceAfterKeywordsInControlFlowStatements() {
            TestCode(
@"if(true) {
}",
@"if (true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = true });

            TestCode(
@"if(true) {
}",
@"if(true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = false });

            TestCode(
@"if (true) {
}",
@"if(true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = false });

            TestCode(
@"with(true) {
}",
@"with (true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = true });

            TestCode(
@"with(true) {
}",
@"with(true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = false });

            TestCode(
@"with (true) {
}",
@"with(true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = false });

            TestCode(
@"while(true) {
}",
@"while (true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = true });

            TestCode(
@"while(true) {
}",
@"while(true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = false });

            TestCode(
@"while (true) {
}",
@"while(true) {
}", new FormattingOptions() { SpaceAfterKeywordsInControlFlowStatements = false });
        }

        [TestMethod, Priority(0)]
        public void TestSimple() {
            TestCode(
@"do {
x
} while(true);",
@"do {
    x
} while(true);");

            TestCode(
@"do {
break
} while(true);",
@"do {
    break
} while(true);");

            TestCode(
@"do {
continue
} while(true);",
@"do {
    continue
} while(true);");

            TestCode(
@"do {
;
} while(true);",
@"do {
    ;
} while(true);");

            TestCode(
@"do {
debugger
} while(true);",
@"do {
    debugger
} while(true);");

            TestCode(
@"do {
var x = 100;
} while(true);",
@"do {
    var x = 100;
} while(true);");

            TestCode(
@"do {
return 42;
} while(true);",
@"do {
    return 42;
} while(true);");

            TestCode(
@"do {
throw null;
} while(true);",
@"do {
    throw null;
} while(true);");
        }

        [TestMethod, Priority(0)]
        public void TestFormatterNotReplacingAggressively() {
            var code =
@"function f() {
    function g() {
     
}
}";

            var edits = Formatter.GetEditsForDocument(code, null);
            Assert.AreEqual(1, edits.Length);
            Assert.AreEqual(43, edits[0].Start);
            Assert.AreEqual("    ", edits[0].Text);
            Assert.AreEqual(0, edits[0].Length);
        }

        private static void TestCode(string code, string expected, FormattingOptions options = null) {
            var firstFormat = FormatCode(code, options);
            Assert.AreEqual(expected, firstFormat);

            // TODO: We should reenable this once we get this to work.  At the time of removing this TestInvalidTrailingQuote 
            // failed due to this...

            // a second call to format on a formatted code should have no changes
            //var secondFormat = FormatCode(firstFormat, options);
            //Assert.AreEqual(firstFormat, secondFormat, "First and Second call to format had different results...");

        }

        private static void TestCode(int position, char ch, string code, string expected, FormattingOptions options = null) {
            Assert.AreEqual(expected, FormatCode(code, position, ch, options));
        }

        private static void TestCode(int start, int end, string code, string expected, FormattingOptions options = null) {
            Assert.AreEqual(expected, FormatCode(code, start, end, options));
        }

        private static void TestEnter(int start, int end, string code, string expected, FormattingOptions options = null) {
            Assert.AreEqual(expected, FormatEnter(code, start, end, options));
        }

        private static string FormatCode(string code, FormattingOptions options) {
            var ast = new JSParser(code).Parse(new CodeSettings());
            var edits = Formatter.GetEditsForDocument(code, options);
            return ApplyEdits(code, edits);
        }

        private static string FormatCode(string code, int position, char ch, FormattingOptions options) {
            var ast = new JSParser(code).Parse(new CodeSettings());
            var edits = Formatter.GetEditsAfterKeystroke(code, position, ch, options);
            return ApplyEdits(code, edits);
        }

        private static string FormatCode(string code, int start, int end, FormattingOptions options) {
            var ast = new JSParser(code).Parse(new CodeSettings());
            var edits = Formatter.GetEditsForRange(code, start, end, options);
            return ApplyEdits(code, edits);
        }

        private static string FormatEnter(string code, int start, int end, FormattingOptions options) {
            var ast = new JSParser(code).Parse(new CodeSettings());
            var edits = Formatter.GetEditsAfterEnter(code, start, end, options);
            return ApplyEdits(code, edits);
        }

        private static string ApplyEdits(string code, Edit[] edits) {
            StringBuilder newCode = new StringBuilder(code);
            int delta = 0;
            foreach (var edit in edits) {
                newCode.Remove(edit.Start + delta, edit.Length);
                newCode.Insert(edit.Start + delta, edit.Text);
                delta -= edit.Length;
                delta += edit.Text.Length;
            }
            return newCode.ToString();
        }
    }
}

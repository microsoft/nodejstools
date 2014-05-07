using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisTests {
    [TestClass]
    public class AnalysisTests {
        [TestMethod]
        public void TestPrimitiveMembers() {
            string code = @"
var b = true;
var n = 42.0;
var s = 'abc';
function f() {
}
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                GetMemberNames(analysis, "b"), 
                "valueOf"
            );
            AssertUtil.ContainsAtLeast(
                GetMemberNames(analysis, "n"),
                "toFixed", "toExponential"
            );
            AssertUtil.ContainsAtLeast(
                GetMemberNames(analysis, "s"),
                "anchor", "big", "indexOf"
            );
            AssertUtil.ContainsAtLeast(
                GetMemberNames(analysis, "f"),
                "apply"
            );
            
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("b.valueOf", 0),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("n.toFixed", 0),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("s.big", 0),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("f.apply", 0),
                BuiltinTypeId.Function
            ); 
        }

        [TestMethod]
        public void TestArrayForEach() {
            string code = @"
var arr = [1,2,3];
function f(a) {
    // here
}
arr.forEach(f);
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("// here")),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestThisFlows() {
            string code = @"
function f() {
    this.func();
}
f.prototype.func = function() {
    this.value = 42;
}

var x = new f().value;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            ); 
        }

        [TestMethod]
        public void TestDefinitiveAssignmentScoping() {
            string code = @"
var x = {abc:42};
x = x.abc;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }


        [TestMethod]
        public void TestDefinitiveAssignmentMerged() {
            string code = @"
if(true) {
    y = 100;
}else{
    y = 'abc';
}
x = y;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number,
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestDefinitiveAssignment() {
            string code = @"
a = 100;
// int value
a = 'abc';
// string value
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("int value")),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("string value")),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestDefinitiveAssignmentNested() {
            string code = @"
a = 100;
// a int value
b = 200;
// b int value
a = 'abc';
// a string value
b = 'abc';
// b string value
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("b int value")),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("a int value")),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("a string value")),
                BuiltinTypeId.String
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("b", code.IndexOf("b int value")),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("b", code.IndexOf("b string value")),
                BuiltinTypeId.String
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("b string value")),
                BuiltinTypeId.String
            );
        }

        /// <summary>
        /// Tests the internal [[Contruct]] method and makes sure
        /// we return the value if the function returns an object.
        /// </summary>
        [TestMethod]
        public void TestConstruct() {
            var code = @"function f() {
     return {abc:42};
}
var x = new f();
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Object
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", 0),
                BuiltinTypeId.Number
            );

            code = @"function f() {
     return 42;
     return {abc:42};
}
var x = new f();
";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Object
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", 0),
                BuiltinTypeId.Number
            );

            code = @"function f() {
     return 42;
}
var x = new f();
";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Object
            );
        }

        [TestMethod]
        public void TestConstructReturnFunction() {
            var code = @"function f() {
     function inner() {
     }
     inner.abc = 42;
     return inner;
}
var x = new f();
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", 0),
                BuiltinTypeId.Number
            );
        }

        private static IEnumerable<string> GetMemberNames(ModuleAnalysis analysis, string name) {
            return analysis.GetMembersByIndex(name, 0).Select(x => x.Name);
        }



        [TestMethod]
        public void TestExports() {
            string code = @"
exports.num = 42;
module.exports.str = 'abc'
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("exports.num", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("exports.str", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestAccessorDescriptor() {
            string code = @"
function f() {
}
Object.defineProperty(f, 'foo', {get: function() { return 42; }})
x = f.foo;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestDefineProperties() {
            string code = @"
function f() {
}
Object.defineProperties(f, {abc:{get: function() { return 42; } } })
x = f.abc;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestFunctionExpression() {
            string code = @"
var abc = {abc: function abcdefg() { } };
var x = abcdefg;
";
            var analysis = ProcessText(code);
            Assert.AreEqual(0, analysis.GetTypeIdsByIndex("x", 0).Count());
        }

        [TestMethod]
        public void TestGlobal() {
            var analysis = ProcessText(";");

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("Number", 0),
                BuiltinTypeId.Function
            );
            /*
            analysis = ProcessText("var x = 42;");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );*/
        }

        [TestMethod]
        public void TestNumberFunction() {
            var analysis = ProcessText(";");

            foreach (var numberValue in new[] { "length", "MAX_VALUE", "MIN_VALUE", "NaN", "NEGATIVE_INFINITY", "POSITIVE_INFINITY" }) {
                AssertUtil.ContainsExactly(
                    analysis.GetTypeIdsByIndex("Number." + numberValue, 0),
                    BuiltinTypeId.Number
                );
            }

            foreach (var nullValue in new[] { "arguments", "caller" }) {
                AssertUtil.ContainsExactly(
                    analysis.GetTypeIdsByIndex("Number." + nullValue, 0),
                    BuiltinTypeId.Null
                );
            }

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("Number.prototype", 0),
                BuiltinTypeId.Object
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("Number.name", 0),
                BuiltinTypeId.String
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("Number.isNaN(42)", 0),
                BuiltinTypeId.Boolean
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("Number.isFinite(42)", 0),
                BuiltinTypeId.Boolean
            );
        }

        [TestMethod]
        public void TestSimpleClosure() {
            var code = @" function f()
{
 var x = 42;
 function g() { return x }
 return g;
}";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("f()()", 0),
                BuiltinTypeId.Number
            );
        }


        //[TestMethod]
        //public void ReproTestCase() {
        //    var analysis = ProcessText(File.ReadAllText(@"C:\Source\ExpressApp29\ExpressApp29\node_modules\jade\node_modules\with\node_modules\uglify-js\lib\scope.js"));
        //    Console.WriteLine("Done processing");
        //}

        [TestMethod]
        public void TestNumber() {
            string code = "x = 42;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
            /*
            analysis = ProcessText("var x = 42;");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );*/
        }

        [TestMethod]
        public void TestVariableLookup() {
            string code = "x = 42; y = x;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestVariableDeclaration() {
            var analysis = ProcessText("var x = 42;");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestStringLiteral() {
            string code = "x = 'abc';";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestArrayLiteral() {
            string code = "x = [1,2,3];";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x[0]", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestTrueFalse() {
            string code = "x = true;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Boolean
            );

            code = "x = false;";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Boolean
            );
        }

        [TestMethod]
        public void TestObjectLiteral() {
            string code = "x = {abc:42};";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.Length),
                BuiltinTypeId.Number
            );

            code = "x = {42:42};";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x[42]", code.Length),
                BuiltinTypeId.Number
            );

            code = "x = {abc:42};";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x['abc']", code.Length),
                BuiltinTypeId.Number
            );

            code = "x = {}; x['abc'] = 42;";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.Length),
                BuiltinTypeId.Number
            );

            code = "x = {abc:42};";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x[0, 'abc']", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestForLoop() {
            var analysis = ProcessText("for(var x in [1,2,3]) { }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestFunctionCreation() {
            var analysis = ProcessText(@"function f() {
}
f.prototype.abc = 42

var x = new f().abc;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );

            analysis = ProcessText(@"function f() {
}
f.abc = 42

var x = f.prototype.constructor.abc;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );

        }

        [TestMethod]
        public void TestPrototypeNoMerge() {
            var analysis = ProcessText(@"
var abc = Function.prototype;
abc.bar = 42;
function f() {
}
var x = new f().bar;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0)
            );
        }

        [TestMethod]
        public void TestMergeSpecialization() {
            var analysis = ProcessText(@"function merge(a, b){
  if (a && b) {
    for (var key in b) {
      a[key] = b[key];
    }
  }
  return a;
};


function f() {
}

f.abc = 42

function g() {
}

merge(g, f)
var x = g.abc;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestCopySpecialization() {
            var analysis = ProcessText(@"function copy (obj) {
  var o = {}
  Object.keys(obj).forEach(function (i) {
    o[i] = obj[i]
  })
  return o
}

var x = {abc:42};
var y = {abc:'abc'};
var c1 = copy(x);
var c2 = copy(y);
var abc1 = c1.abc;
var abc2 = c2.abc;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc1", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc2", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestCreateSpecialization() {
            var analysis = ProcessText(@"function F() {
}

create = function create(o) {
    F.prototype = o;
    return new F();
}

abc1 = create({abc:42}).abc
abc2 = create({abc:'abc'}).abc
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc1", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc2", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestFunctionParametersMultipleCallers() {
            var analysis = ProcessText(@"function f(a) {
    return a;
}

var x = f(42);
var y = f('abc');
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestFunctionParameters() {
            var analysis = ProcessText(@"function x(a) { return a; }
var y = x(42);");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestFunctionReturn() {
            var analysis = ProcessText("function x() { return 42; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestFunction() {
            var analysis = ProcessText("function x() { }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Function
            );            

            analysis = ProcessText("function x() { return 'abc'; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.String
            );

            analysis = ProcessText("function x() { return null; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Null
            );

            analysis = ProcessText("function x() { return undefined; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Undefined
            );

            analysis = ProcessText("function x() { return true; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Boolean
            );

            analysis = ProcessText("function x() { return function() { }; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Function
            );

            analysis = ProcessText("function x() { return { }; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Object
            );
        }

        [TestMethod]
        public void TestNewFunction() {
            string code = "function y() { return 42; }\r\nx = new y();";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Object
            );

            code = "function f() { this.abc = 42; }\r\nx = new f();";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestNewFunctionInstanceVariable() {
            string code = "function f() { this.abc = 42; }\r\nx = new f();";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.Length),
                BuiltinTypeId.Number
            );

            code = @"
function x() { this.abc = 42; }
function y() { this.abc = 'abc'; }
abcx = new x();
abcy = new y();";
            analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abcx.abc", code.Length),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abcy.abc", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestSimplePrototype() {
            string code = @"
function f() {
}
f.abc = 42;

function j() {
}
j.prototype = f;

x = new j();
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestTypeOf() {
            string code = "x = typeof 42;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestVariableShadowing() {
            var code = @"var a = 'abc';
function f() {
console.log(a);
var a = 100;
}
f();";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("console.log")),
                BuiltinTypeId.Number
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.IndexOf("f();")),
                BuiltinTypeId.String
            );
        }

        [TestMethod]
        public void TestFunctionExpressionNameScoping() {
            var code = @"var x = function abc() {
    // here
    var x = 42;
    return abc.bar;
}
x.bar = 42;
";
            var analysis = ProcessText(code);
            
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc", code.IndexOf("// here")),
                BuiltinTypeId.Function
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", code.Length),
                BuiltinTypeId.Number
            );
        }


        [TestMethod]
        public void TestFunctionExpressionNameScopingNested() {
            var code = @"
function f() {
    var x = function abc() {
        // here
        var x = 42;
        return abc.bar;
    }
    x.bar = 42;
    var inner = x();
}
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc", code.IndexOf("// here")),
                BuiltinTypeId.Function
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("inner", code.IndexOf("var inner")),
                BuiltinTypeId.Number
            );
        }
        
        [TestMethod]
        public void TestFunctionClosureCall() {
            var code = @"function abc() {
    function g(x) {
        function inner() {
        }
        return x;
    }
    return g;
}
var x = abc()(42);
var y = abc()('abc');
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", code.Length),
                BuiltinTypeId.String
            );
        }


        [TestMethod]
        public void TestTypes() {
            string code = "x = {undefined:undefined, number:42, string:'str', null:null, boolean:true, function: function() {}, object: {}}";
            var analysis = ProcessText(code);
            var testCases = new[] { 
                new {Name="number", TypeId = BuiltinTypeId.Number},
                new {Name="string", TypeId = BuiltinTypeId.String},
                new {Name="null", TypeId = BuiltinTypeId.Null},
                new {Name="boolean", TypeId = BuiltinTypeId.Boolean},
                new {Name="object", TypeId = BuiltinTypeId.Object},
                new {Name="function", TypeId = BuiltinTypeId.Function},
                new {Name="undefined", TypeId = BuiltinTypeId.Undefined},
            };

            foreach (var testCase in testCases) {
                AssertUtil.ContainsExactly(
                    analysis.GetTypeIdsByIndex("x." + testCase.Name, code.Length),
                    testCase.TypeId
                );
                AssertUtil.ContainsExactly(
                    analysis.GetTypeIdsByIndex("typeof x." + testCase.Name, code.Length),
                    BuiltinTypeId.String
                );
                AssertUtil.ContainsExactly(
                    analysis.GetTypeIdsByIndex("x[typeof x." + testCase.Name + "]", code.Length),
                    testCase.TypeId
                );
            }
        }

        public static ModuleAnalysis ProcessText(string text) {
            var sourceUnit = GetSourceUnit(text);
            var state = new JsAnalyzer();
            var entry = state.AddModule("fob.js", null);
            Prepare(entry, sourceUnit);
            entry.Analyze(CancellationToken.None);

            return entry.Analysis;
        }

        public static void Prepare(IJsProjectEntry entry, TextReader sourceUnit) {
            var parser = new JSParser(sourceUnit.ReadToEnd());
            var ast = parser.Parse(new CodeSettings());
            entry.UpdateTree(ast, null);
        }

        public static TextReader GetSourceUnit(string text) {
            return new StringReader(text);
        }
    }

    static class AnalysisTestExtensions {
        public static IEnumerable<BuiltinTypeId> GetTypeIdsByIndex(this ModuleAnalysis analysis, string exprText, int index) {
            return analysis.GetValuesByIndex(exprText, index).Select(m => {
                return m.TypeId;
            });
        }
    }
}

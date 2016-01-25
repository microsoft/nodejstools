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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace AnalysisTests {
    [TestClass]
    public class AnalysisTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            NodejsTestData.Deploy();
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/945
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestFunctionApply() {
            string code = @"
function g() {
    this.abc = 42;
}

function f() {
    g.apply(this);
}

var x = new f().abc;
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/945
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestToString() {
            string code = @"
var x = new Object();
var y = x.toString();
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0),
                BuiltinTypeId.String
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/965
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestBuiltinDoc() {
            string code = @"
var x = 'abc'
";
            var analysis = ProcessText(code);

            var members = analysis.GetMembersByIndex("x", code.Length);
            Assert.AreNotEqual(
                -1,
                members.Where(x => x.Name == "substring").First().Documentation.IndexOf("Returns the substring at")
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/935
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestAllMembers() {
            string code = @"
function f() {
    if(true) {
        return {abc:42};
    }else{
        return {def:'abc'};
    }
}
var x = f();
";
            var analysis = ProcessText(code);

            var members = analysis.GetMembersByIndex("x", code.Length).Select(x => x.Completion);
            AssertUtil.ContainsAtLeast(
                members,
                "abc",
                "def"
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/950
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestReturnValues() {
            string code = @"
function f(x) {
    return x;
}
f({abc:42});
f({def:'abc'});
";
            var analysis = ProcessText(code);

            var members = analysis.GetMembersByIndex("f(42)", code.Length).Select(x => x.Completion);
            AssertUtil.ContainsAtLeast(
                members,
                "abc",
                "def"
            );
        }

        [TestMethod, Priority(0)]
        public void TestArguments() {
            string code = @"
function f() {
    return arguments[0];
}

function g() {
    return arguments[1];
}

var x = f(0);
var y = f(0, 'abc');
var z = g('abc', 0);
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("z", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestAssignmentScope() {
            string code = @"
var rjs;
function f() {
    rjs = 42;
}

function g() {
    return rjs;
}

x = g();";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );

        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/987
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestMembersListsInvalidIdentifier() {
            string code = @"
var x = {};
x['./foo/quox.js'] = 42;
var z = x['./foo/quox.js'];
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("z", code.Length),
                BuiltinTypeId.Number
            );

            AssertUtil.DoesntContain(
                analysis.GetMembersByIndex("x", code.Length).Select(x => x.Completion),
                "./foo/quox.js"
            );
        }


        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestArrayPush() {
            string code = @"
var arr = [];
arr.push(42);
var x = arr[0];
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestArrayPop() {
            string code = @"
var arr = [];
arr.push(42);
var x = arr.pop();

var arr2 = ['abc'];
var y = arr2.pop();

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

        [TestMethod, Priority(0)]
        public void TestArraySetIndex() {
            string code = @"
var arr = [];
arr[0] = 42;
var x = arr[0];

var arr2 = [undefined, 42];
arr2[0] = 'abc';
var y = arr2[0];

";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", code.Length),
                BuiltinTypeId.String,
                BuiltinTypeId.Undefined
            );
        }

        [TestMethod, Priority(0)]
        public void TestArraySlice() {
            string code = @"
var arr = [1,2,3];
var x = arr.slice(1,2)[0];
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestArrayConcat() {
            string code = @"
var arr = ['abc', 'def'];
x = {};
arr.concat('all').forEach(function f(a) {
    x[a] = 42;
}
// here
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.IndexOf("// here")),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.def", code.IndexOf("// here")),
                BuiltinTypeId.Number
            );
            // Some day this would be nice...
            //AssertUtil.ContainsExactly(
            //    analysis.GetTypeIdsByIndex("x.all", code.IndexOf("// here")),
            //    BuiltinTypeId.Number
            //);
        }

        [TestMethod, Priority(0)]
        public void TestPrototypeGetter() {
            string code = @"
function MyObject() {
}

MyObject.prototype.__defineGetter__('myprop', function(){  
	return this.abc;
});

var x = new MyObject();
x.abc = 42;
var y = x.myprop;";

            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunctionDefineGetter() {
            string code = @"function f() {
}

f.__defineGetter__('abc', function() { return 42; })

var x = f.abc;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunctionPrototypeLookup() {
            string code = @"function f() {
}

Object.prototype.xyz = function() { return this.prop; }
f.prop = 42
var y = f.xyz;
var x = f.xyz();
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", code.Length),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestProtoSetCorrectly() {
            string code = @"function f() { }
f.prototype.abc = 42
var x = (new f()).__proto__.abc;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestInfiniteDescriptors() {
            string code = @"x = {abc:42}

function f(x) {
    a = Object.getOwnPropertyDescriptor(x, 'abc')
    Object.defineProperty(x, 'abc', a)
    f(a)
}

f(x)
";
            var analysis = ProcessText(code);
        }

        [TestMethod, Priority(0)]
        public void TestCopyDescriptor() {
            string code = @"x = {abc:42}

desc = Object.getOwnPropertyDescriptor(x, 'abc')
y = {}
Object.defineProperty(y, 'abc', desc)
z = y.abc;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("z", 0),
                BuiltinTypeId.Number
            ); 
        }

        /// <summary>
        /// Currently failing because object literal {value: copied[propName]} gets merged resulting in shared types
        /// </summary>
        [Ignore]
        [TestMethod, Priority(0)]
        public void FailingTestDefinePropertyLoop() {
            string code = @"
var o = {};
var copied = {num:42, str:'100'};
for(var propName in copied) {
    Object.defineProperty(o, propName, {value: copied[propName]});
}
";

            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("o.num", code.Length),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("o.str", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
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

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/931
        /// 
        /// This needs to flow when our function call analysis exceeds our limits.
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestThisFlows2() {
            string code = @"
var app = {};
app.init = function() {
    this.defaultInit();
}

app.defaultInit = function() {
    this.foo = 42;
}

app.init()

var x = app.foo;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }
        
        [TestMethod, Priority(0)]
        public void TestThisPassedAndReturned() {
            string code = @"function X(csv) {
    abc = 100;
    return csv;
}

function Y() {
    abc = 42;
    this.abc = X(this);
}

xyz = new Y();";

            var analysis = ProcessText(code);
            
            AssertUtil.ContainsExactly(
                analysis.GetValuesByIndex("xyz.abc", code.Length),
                analysis.GetValuesByIndex("new Y()", code.Length)
            );
        }

        [Ignore]
        [TestMethod, Priority(2)]
        public void Failing_TestThis() {
            var code = @"function SimpleTest(x, y) {
    this._name = 'SimpleTest';
    this._number = 1;
    this._y = y;
}

SimpleTest.prototype.performRequest = function (webResource, outputData, options, callback) {
  this._performRequest(webResource, { outputData: outputData }, options, callback);
};

SimpleTest.prototype._performRequest = function (webResource, body, options, callback, chunkedCallback) {
  var self = this;
  // here
};";

            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetTypeIdsByIndex("self", code.IndexOf("// here")),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("self._name", code.IndexOf("// here")),
                BuiltinTypeId.String
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("self._number", code.IndexOf("// here")),
                BuiltinTypeId.Number
            );

        }

        [TestMethod, Priority(0)]
        public void TestPropertyGotoDef() {
            string code = @"
Object.defineProperty(Object.prototype, 'should', { set: function() { }, get: function() { 42 } });
var x = {};
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetVariablesByIndex("x.should", code.Length)
                    .Select(x => x.Location.Line + ", " + x.Location.Column + ", " + x.Type),
                "2, 79, Definition"
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1097
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestAttributeGotoDef() {
            string code = @"
var x = {}; 
x.abc = 42; 
x.abc
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetVariablesByIndex("x.abc", code.Length)
                    .Select(x => x.Location.Line + ", " + x.Location.Column + ", " + x.Type),
                "3, 1, Definition",
                "4, 1, Reference"
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1464
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestBuiltinRequireGotoDef() {
            string code = @"
var assert = require('assert'); 
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetVariablesByIndex("assert", code.Length)
                    .Select(x => x.Location.Line + ", " + x.Location.Column + ", " + x.Type),
                "2, 5, Definition", 
                "2, 5, Reference"
            );
        }

        [TestMethod, Priority(0)]
        public void TestGlobals() {
            string code = @"
var x = 42;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetAllAvailableMembersByIndex(code.Length).Select(x => x.Name),
                "require",
                "setTimeout"
            );
        }

        [TestMethod, Priority(0)]
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

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/949
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestDefinitiveAssignmentScoping2() {
            string code = @"
a = b = c = 2;
a
b
c
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("a", code.Length),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("b", code.Length),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("c", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestSignatureHelp() {
            string code = @"
function foo(x, y) {}
foo(123, 'abc');
foo('abc', 123);

function abc(f) {
}

abc(abc);
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                GetSignatures(analysis, "foo", code.Length),
                "x number or string, y number or string"
            );
            AssertUtil.ContainsExactly(
                GetSignatures(analysis, "abc", code.Length),
                "f function"
            );
        }

        private static string[] GetSignatures(ModuleAnalysis analysis, string name, int index) {
            List<string> sigs = new List<string>();

            foreach (var sig in analysis.GetSignaturesByIndex(name, index)) {
                sigs.Add(
                    String.Join(
                        ", ",
                        sig.Parameters.Select(x => x.Name + " " + x.Type)
                    )
                );
            }
            return sigs.ToArray();
        }

        [Ignore]
        [TestMethod, Priority(0)]
        public void TestJSDoc() {
            string code = @"
/** Documentation for f.
  * 
  * Just a paragraph. It shouldn't show up anywhere.
  *
  * @param  a   Documentation for a.
  * 
  * Another paragraph that won't show up anywhere. This one has a {@link}.
  *
  * @arg    b   Documentation for b.
  *             It spans multiple lines.
  * @param  c   Documentation for c. It has a {@link} in it.
  * @arg   [d]  Documentation for d. It is an optional parameter.
  * @argument [e=123]
  * Documentation for e. It has a default value.
  *
  * @see Not a parameter!
  */
function f(a, b, c, d, e) {}

/** Documentation for g. */
var g = function() {}

var h;
/** Documentation for h. */
h = function() {}
";
            var analysis = ProcessText(code);

            var f = analysis.GetSignaturesByIndex("f", code.Length).Single();
            string fExpected =
                @"Documentation for f.

Just a paragraph. It shouldn't show up anywhere.

@param  a   Documentation for a.

Another paragraph that won't show up anywhere. This one has a {@link}.

@arg    b   Documentation for b. It spans multiple lines.
@param  c   Documentation for c. It has a {@link} in it.
@arg   [d]  Documentation for d. It is an optional parameter.
@argument [e=123] Documentation for e. It has a default value.

@see Not a parameter!";
            Assert.AreEqual(fExpected, f.Documentation);

            var g = analysis.GetSignaturesByIndex("g", code.Length).Single();
            Assert.AreEqual("Documentation for g.", g.Documentation);

            var h = analysis.GetSignaturesByIndex("h", code.Length).Single();
            Assert.AreEqual("Documentation for h.", h.Documentation);
        }

        [TestMethod, Priority(0)]
        public void TestStringMembers() {
            string code = @"
var x = 'abc';
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("x", code.Length).Select(x => x.Name),
                "length"
            );
        }

        [TestMethod, Priority(0)]
        public void TestBadIndexes() {
            string code = @"
var x = {};
x[] = 42;
var y = x[];
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetMembersByIndex("y", code.Length)
            );
        }

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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
        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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



        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1197
        /// 
        /// defineProperties needs to take a dependency on the properties
        /// object it's reading from, and when a new property is created
        /// we need to enqueue the callers.
        /// </summary>
        [Ignore]
        [TestMethod, Priority(0)]
        public void TestDefinePropertiesDependencies() {
            string code = @"
var x =  {};
var properties = function() {
    var ret = {};
    ret['one'] = 1;
    ret['two'] = 2;
    ret['three'] = 3;

    return ret;
}();

function createProperties() {
    var ret = {};
    
    Object.keys(properties).forEach(
        function(item) {
            ret[item] = { get: function() { return item; } }
        }
    );

    return ret;
}

Object.defineProperties(x, createProperties());

";

            var analysis = ProcessText(code);

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("x", code.Length).Select(x => x.Name),
                "one",
                "two",
                "three"
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunctionExpression() {
            string code = @"
var abc = {abc: function abcdefg() { } };
var x = abcdefg;
";
            var analysis = ProcessText(code);
            Assert.AreEqual(0, analysis.GetTypeIdsByIndex("x", 0).Count());
        }

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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


        //[TestMethod, Priority(0)]
        //public void ReproTestCase() {
        //    var analysis = ProcessText(File.ReadAllText(@"C:\Source\ExpressApp29\ExpressApp29\node_modules\jade\node_modules\with\node_modules\uglify-js\lib\scope.js"));
        //    Console.WriteLine("Done processing");
        //}

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestVariableLookup() {
            string code = "x = 42; y = x;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestVariableDeclaration() {
            var analysis = ProcessText("var x = 42;");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestStringLiteral() {
            string code = "x = 'abc';";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
        public void TestArrayLiteral() {
            string code = "x = [1,2,3];";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x[0]", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestSpecializedExtend() {
            var code = @"function extend(obj) {
    if (!_.isObject(obj)) return obj;
    var source, prop;
    for (var i = 1, length = arguments.length; i < length; i++) {
      source = arguments[i];
      for (prop in source) {
        if (hasOwnProperty.call(source, prop)) {
            obj[prop] = source[prop];
        }
      }
    }
    return obj;
  };


var x = {};
extend(x, {abc:42});
";


            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.abc", code.Length),
                BuiltinTypeId.Number
            );

        }

        [TestMethod, Priority(0)]
        public void TestBackboneExtend() {
            var code = @"function extend(protoProps, staticProps) {
    var parent = this;
    var child;
                           
    if (protoProps && _.has(protoProps, 'constructor')) {
        child = protoProps.constructor;
    } else {
        child = function(){ return parent.apply(this, arguments); };
    }

    _.extend(child, parent, staticProps);
                           
    var Surrogate = function(){ this.constructor = child; };
    Surrogate.prototype = parent.prototype;
    child.prototype = new Surrogate;

    if (protoProps) _.extend(child.prototype, protoProps);

    child.__super__ = parent.prototype;

    return child;
    };

function Model() {
    this.modelAttr = 'foo';
}

Model.extend = extend
var myclass = Model.extend({extAttr:function() { return 42; }});
var myinst = new myclass();
";


            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("myinst.extAttr", code.Length),
                BuiltinTypeId.Function
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("myinst.modelAttr", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
        public void TestLodashAssign() {
            var code = @"
var assign = function(object, source, guard) {
     var index, iterable = object, result = iterable;
     if (!iterable) return result;
     var args = arguments,
         argsIndex = 0,
         argsLength = typeof guard == 'number' ? 2 : args.length;
     if (argsLength > 3 && typeof args[argsLength - 2] == 'function') {
       var callback = baseCreateCallback(args[--argsLength - 1], args[argsLength--], 2);
     } else if (argsLength > 2 && typeof args[argsLength - 1] == 'function') {
       callback = args[--argsLength];
     }
     while (++argsIndex < argsLength) {
       iterable = args[argsIndex];
       if (iterable && objectTypes[typeof iterable]) {
         var ownIndex = -1,
             ownProps = objectTypes[typeof iterable] && keys(iterable),
             length = ownProps ? ownProps.length : 0;
         
         while (++ownIndex < length) {
           index = ownProps[ownIndex];
           result[index] = callback ? callback(result[index], iterable[index]) : iterable[index];
         }
       }
     }
     return result
   };


var x = {abc:42}
var y = {}
assign(y, x);
var z = y.abc;

";

            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("z", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestObjectKeys() {
            var code = @"
var foo = {};

var attrs = {
    num: 42,
    str: 'abc'
};
Object.keys(attrs).forEach(function(key) {
    var attr = foo[key] = {};
    attr.value = attrs[key];
});

var num = foo.num.value;
var str = foo.str.value;
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("num", code.Length),
                BuiltinTypeId.Number,
                BuiltinTypeId.String
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("str", code.Length),
                BuiltinTypeId.Number,
                BuiltinTypeId.String
            );
        }


        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(1)]
        public void TestConstructor() {
            var code = @"function f() {
	this.abc = 42;
}

var inst = new f()
var x = inst.abc
var y = new inst.constructor()
var z = y.abc";

            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("z", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(2)]
        public void TestForInLoop() {
            var analysis = ProcessText("for(var x in ['a','b','c']) { }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        [Ignore]
        [TestMethod, Priority(2)]
        public void FailingTestForInLoop2() {
            var analysis = ProcessText("for(x in ['a','b','c']) { }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }


        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        /// <summary>
        /// This shouldn't stack overflow...
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestRecursiveForEach() {
            var analysis = ProcessText(@"
var x = [null];
x[0] = x.forEach;
x.forEach(x.forEach);
");
        }

        /// <summary>
        /// Array instances created via array constructor should be unique
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNewArrayUnique() {
            var analysis = ProcessText(@"
var arr1 = new Array(5);
var arr2 = new Array(10);
arr1.foo = 42;
arr2.foo = 'abc';
");

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("arr1.foo", 0),
                BuiltinTypeId.Number
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("arr2.foo", 0),
                BuiltinTypeId.String
            );
        }

        /// <summary>
        /// Array instances created via array constructor should be unique
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNewObjectUnique() {
            var analysis = ProcessText(@"
var obj1 = new Object(5);
var obj2 = new Object(10);
obj1.foo = 42;
obj2.foo = 'abc';
");

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("obj1.foo", 0),
                BuiltinTypeId.Number
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("obj2.foo", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
        public void TestObjectInstanceNoPrototype() {
            var analysis = ProcessText(@"
var x = new Object().prototype;
var y = {}.prototype;
");

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0)
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0)
            );
        }

        [TestMethod, Priority(0)]
        public void TestKeysSpecialization() {
            var analysis = ProcessText(@"
function keys(o) {
  var a = []
  for (var i in o) if (o.hasOwnProperty(i)) a.push(i)
  return a
}        

var x = keys({'abc':42});
");
            AssertUtil.ContainsExactly(
                analysis.GetDescriptionByIndex("x", 0),
                "Array object"
            );
        }

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestMergeDescriptorsSpecialization() {
            var analysis = ProcessText(@"var merge = function exports(dest, src) {
    Object.getOwnPropertyNames(src).forEach(function (name) {
        var descriptor = Object.getOwnPropertyDescriptor(src, name)
        Object.defineProperty(dest, name, descriptor)
    })
    return dest
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

        [TestMethod, Priority(0)]
        public void TestToObjectSpecialization() {
            var analysis = ProcessText(@"function toObject(value) {
      return isObject(value) ? value : Object(value);
    }

var x = toObject('abc');
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
        public void TestMergeSpecialization2() {
            var analysis = ProcessText(@"function merge (to, from) {
  var keys = Object.keys(from)
    , i = keys.length
    , key

  while (i--) {
    key = keys[i];
    if ('undefined' === typeof to[key]) {
      to[key] = from[key];
    } else {
      if (exports.isObject(from[key])) {
        merge(to[key], from[key]);
      } else {
        to[key] = from[key];
      }
    }
  }
}


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

        [TestMethod, Priority(0)]
        public void TestMergeCloneSpecialization2() {
            var analysis = ProcessText(@"function mergeClone (to, from) {
  var keys = Object.keys(from)
    , i = keys.length
    , key

  while (i--) {
    key = keys[i];
    if ('undefined' === typeof to[key]) {
      // make sure to retain key order here because of a bug handling the $each
      // operator in mongodb 2.4.4
      to[key] = clone(from[key], { retainKeyOrder : 1});
    } else {
      if (exports.isObject(from[key])) {
        mergeClone(to[key], from[key]);
      } else {
        // make sure to retain key order here because of a bug handling the
        // $each operator in mongodb 2.4.4
        to[key] = clone(from[key], { retainKeyOrder : 1});
      }
    }
  }
}


function f() {
}

f.abc = 42

function g() {
}

mergeClone(g, f)
var x = g.abc;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }


        [TestMethod, Priority(0)]
        public void TestCloneSpecialization() {
            var analysis = ProcessText(@"function clone (obj) {
  if (obj === undefined || obj === null)
    return obj;

  return null;
}

var x = clone({abc:42})
var y = x.abc;
");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestWrapFunctionSpecialization() {
            var code = @"function wrapfunction(fn, message) {
  if (typeof fn !== 'function') {
    throw new TypeError('argument fn must be a function')
  }

  var args = createArgumentsString(fn.length)
  var deprecate = this
  var stack = getStack()
  var site = callSiteLocation(stack[1])

  site.name = fn.name

  var deprecatedfn = eval('(function (' + args + ') {\n'
    + '""use strict""\n'
    + 'log.call(deprecate, message, site)\n'
    + 'return fn.apply(this, arguments)\n'
    + '})')

  return deprecatedfn
}

function f(a) {
    return a;
}

wrapped = wrapfunction(f, 'hello');
var res = wrapped(42);
";

            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("res", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestStringCasing() {
            string code = @"
var x = {};
['upper'].forEach(function f(attr) { x[attr.toUpperCase()] = 42; });
['LOWER'].forEach(function f(attr) { x[attr.toLowerCase()] = 'abc'; })
";

            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.UPPER", code.Length),
                BuiltinTypeId.Number
            );
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x.lower", code.Length),
                BuiltinTypeId.String
            );
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("x.upper", code.Length));
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("x.LOWER", code.Length));
        }

        [TestMethod, Priority(0)]
        public void TestBuiltinFunctionDocumentation() {
            string code = @"
var func = Object.defineProperties;
";

            var analysis = ProcessText(code);
            var func = analysis.GetValuesByIndex("func", code.Length).First();
            Assert.AreEqual(func.ShortDescription, "function");
        }

        /// <summary>
        /// We don't allow assignment to most built-in members that
        /// are defined by JavaScript
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestImmutableObjects() {
            string code = @"
global.Infinity = 'abc'
var x = global.Infinity";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1039
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNodejsCallbacks() {
            string code = @"
var http = require('http');
var https = require('https');
var net = require('net');
http.createServer(function (req, res) {
    // 1
});
http.get({}, function (res) {
    // 2
});
http.request({}, function (res) {
    // 3
});
https.createServer(function (req, res) {
    // 4
});
https.get({}, function (res) {
    // 5
});
https.request({}, function (res) {
    // 6
});
net.createServer(function(c) {
    // 7
});
net.connect('', function(c) {
    // 8
});
net.connect('', function(c) {
    // 9
});
";

            var analysis = ProcessText(code);

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("req", code.IndexOf("// 1")).Select(x => x.Completion), 
                "socket",
                "statusCode",
                "url"
            );
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("res", code.IndexOf("// 1")).Select(x => x.Completion),
                "writeHead",
                "end",
                "removeHeader",
                "addTrailers",
                "writeContinue"
            );

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("res", code.IndexOf("// 2")).Select(x => x.Completion),
                "httpVersion",
                "headers",
                "trailers",
                "method"
            );
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("res", code.IndexOf("// 3")).Select(x => x.Completion),
                "httpVersion",
                "headers",
                "trailers",
                "method"
            );

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("req", code.IndexOf("// 4")).Select(x => x.Completion),
                "abort",
                "setTimeout",
                "setNoDelay"
            );
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("res", code.IndexOf("// 4")).Select(x => x.Completion),
                "writeHead",
                "end",
                "removeHeader",
                "addTrailers",
                "writeContinue"
            );

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("res", code.IndexOf("// 5")).Select(x => x.Completion),
                "httpVersion",
                "headers",
                "trailers",
                "method"
            );
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("res", code.IndexOf("// 6")).Select(x => x.Completion),
                "httpVersion",
                "headers",
                "trailers",
                "method"
            );

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("c", code.IndexOf("// 7")).Select(x => x.Completion),
                "connect",
                "bufferSize"
            );
            AssertUtil.ContainsAtLeast(
               analysis.GetMembersByIndex("c", code.IndexOf("// 8")).Select(x => x.Completion),
               "connect",
               "bufferSize"
           );
            AssertUtil.ContainsAtLeast(
               analysis.GetMembersByIndex("c", code.IndexOf("// 9")).Select(x => x.Completion),
               "connect",
               "bufferSize"
           );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/914
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNodejsSpecializations() {
            string code = @"
var pid = process.pid;
var platform = process.platform;
var maxTickDepth = process.maxTickDepth;
var title = process.title;
";

            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("pid", code.Length), BuiltinTypeId.Number);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("platform", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("maxTickDepth", code.Length), BuiltinTypeId.Number);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("title", code.Length), BuiltinTypeId.String); 
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1077
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNodejsPathMembers() {
            string code = @"
var path = require('path');
var join = path.join('a', 'b');
var normalize = path.normalize('/foo/quox/..');
var resolve = path.resolve('/foo/quox', '/tmp/file');
var relative = path.relative('/foo/quox', '/foo/q2');
var dirname = path.dirname('/tmp/foo.txt');
var basename = path.basename('/tmp/foo.txt');
var extname = path.extname('/tmp/foo.txt');
";

            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("join", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("normalize", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("resolve", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("relative", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("dirname", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("basename", code.Length), BuiltinTypeId.String);
            AssertUtil.ContainsExactly(analysis.GetTypeIdsByIndex("extname", code.Length), BuiltinTypeId.String);
        }


        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1077
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestNodejsBufferInstanceMembers() {
            string code = @"
var buf1 = new require('buffer').Buffer(42);
var buf2 = require('buffer').Buffer(42);";
            var analysis = ProcessText(code);

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("buf1", code.Length).Select(x => x.Completion),
                "write",
                "copy",
                "toJSON",
                "slice"
            );
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("buf2", code.Length).Select(x => x.Completion),
                "write",
                "copy",
                "toJSON",
                "slice"
            );
        }

        [TestMethod, Priority(0)]
        public void TestNodejsCreateFunctions() {
            var code = @"var x = require('http').createServer(null, null);
var y = require('net').createServer(null, null);
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("x", code.Length).Select(x => x.Name),
                "listen"
            );
            analysis = ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("y", code.Length).Select(x => x.Name),
                "listen"
            );
        }


        [TestMethod, Priority(0)]
        public void TestSetPrototypeSpecialization() {
            StringBuilder code = new StringBuilder(@"function setProto(obj, proto) {
  if (typeof Object.setPrototypeOf === 'function')
    return Object.setPrototypeOf(obj, proto)
  else
    obj.__proto__ = proto
}

");

            const int varCount = 25;
            for (int i = 0; i < varCount; i++) {
                code.AppendLine(String.Format("var v{0} = {{p{0}:42}};", i));
            }
            for (int i = 1; i < varCount; i++) {
                code.AppendLine(String.Format("setProto(v{0}, v{1});", i, i - 1));
            }


            for (int i = 0; i < varCount; i++) {
                code.AppendLine(String.Format("i{0} = v{0}.p{0};", i));
            }

            var analysis = ProcessText(code.ToString());
            // we should have our own value...
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("v0.p0", 0),
                BuiltinTypeId.Number
            );
            // we shouldn't have merged and shouldn't have other values
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("v0.p1", 0)
            );

            // and we should be able to follow the prototype chain from the
            // last value back to the 1st value
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("v" + (varCount - 1) + ".p0", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestCopySpecialization() {
            var analysis = ProcessText(@"function copy (obj) {
  var o = {}
  Object.keys(obj).forEach(function (i) {
    o[i] = obj[i]
  })
  return o
}

var x = {abc:42};
var y = {bar:'abc'};
var c1 = copy(x);
var c2 = copy(y);
var abc1 = c1.abc;
var abc2 = c2.bar;
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

        [TestMethod, Priority(0)]
        public void TestCreateSpecialization() {
            var analysis = ProcessText(@"function F() {
}

create = function create(o) {
    F.prototype = o;
    return new F();
}

abc1 = create({abc:42}).abc
abc2 = create({bar:'abc'}).bar
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestFunctionParameters() {
            var analysis = ProcessText(@"function x(a) { return a; }
var y = x(42);");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("y", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunctionReturn() {
            var analysis = ProcessText("function x() { return 42; }");
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x()", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestTypeOf() {
            string code = "x = typeof 42;";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
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


        [TestMethod, Priority(0)]
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
        
        [TestMethod, Priority(0)]
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


        [TestMethod, Priority(0)]
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

        [TestMethod, Priority(0)]
        public void TestDateTime() {
            var code = @"var d = Date;
";

            var analysis = ProcessText(code);

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("d", code.Length).Select(x => x.Completion),
                "UTC",
                "now",
                "parse"
            );

            AssertUtil.ContainsAtLeast(
                GetSignatures(analysis, "d", code.Length),
                "milliseconds object",
                "dateString object",
                "year object, month object, day object, hours object, minutes object, seconds object, milliseconds object"
            );
        }

        [TestMethod, Priority(0)]
        public void TestDescriptions() {
            var code = @"var n = 1;
var b = true;
var s = 'str';
var f = function() { }
var f1 = function f() { }
var f2 = function f(a, b) { }
var f3 = function f() { return 42; }
var f4 = function f() { return 42; return true; }
var bf = Math.max;
var nu = null;
var o = {};
var o1 = {abc:42};
var undef = undefined;
";

            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("n", code.Length), "number");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("b", code.Length), "boolean");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("s", code.Length), "string");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("f", code.Length), "function f()");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("f1", code.Length), "function f()");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("f2", code.Length), "function f(a, b)");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("f3", code.Length), "function f() -> number");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("f4", code.Length), "function f() -> number, boolean");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("nu", code.Length), "null");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("bf", code.Length), "built-in function max");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("o", code.Length), "object");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("o1", code.Length), "object\r\nContains: abc");
            AssertUtil.ContainsExactly(analysis.GetDescriptionByIndex("undef", code.Length), "undefined");                
        }

        [TestMethod, Priority(0)]
        public void TestUtilInherits() {
            var code = @"util = require('util');

function f() {
}
function g() {
}
g.prototype.abc = 42;

util.inherits(f, g);
var super_ = f.super_;
var abc = new f().abc;
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("super_", code.Length),
                BuiltinTypeId.Function
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc", code.Length),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestUtilInherits2() {
            var code = @"util = require('util');

function f() {
}
function g() {
}

util.inherits(f, g);
var super_ = f.super_;
f.prototype.abc = 'abc';
var abc = new g().abc;
";
            var analysis = ProcessText(code);
            var util = analysis.GetTypeIdsByIndex("util", code.Length);
            var inherits = analysis.GetTypeIdsByIndex("util.inherits", code.Length);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("super_", code.Length),
                BuiltinTypeId.Function
            );

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("abc", code.Length)
            );
        }

        /// <summary>
        /// Assigning to __proto__ should result in apparent update
        /// to the internal [[Prototype]] property.
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestDunderProto() {
            // shouldn't crash
            var code = @"
function f() { }
function g() { }
g.prototype.abc = 42
f.prototype.__proto__ = g.prototype
var x = new f().abc;
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.Number
            );

            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("new f()", code.Length).Select(x => x.Name),
                "abc"
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/974
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestInvalidGrouping() {
            // shouldn't crash
            var code = @"var x = ();
";
            var analysis = ProcessText(code);
        }

        [TestMethod, Priority(0)]
        public void UncalledPrototypeMethod() {
            var code = @"
function Class() {
	this.abc = 42;
}

Class.prototype.foo = function(fn) {
	var x = this.abc;
}
";
            var analysis = ProcessText(code);

            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.IndexOf("var x")),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestEventEmitter() {
            var code = @"
var events = require('events')
ee = new events.EventEmitter();
function f(args) {
    // here
}
ee.on('myevent', f)
ee.emit('myevent', 42)
";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("args", code.IndexOf("// here")),
                BuiltinTypeId.Number
            );
        }

        [TestMethod, Priority(0)]
        public void TestAnalysisWrapperLifetime() {
                var server1 = @"var mymod = require('./mymod.js');
mymod.value = {abc:42}";
                var server2 = @"var mymod = require('./mymod.js');
mymod.value = {abc:'abc'}";

            var entries = Analysis.Analyze(
                new AnalysisFile("server.js", server1),
                new AnalysisFile("mymod.js", @"function f() {
    return exports.value;
}
exports.f = f;
")
            );
            
            Analysis.Prepare(entries["server.js"], server2);

            entries["server.js"].Analyze(default(CancellationToken));

            AssertUtil.ContainsExactly(
                entries["mymod.js"].Analysis.GetTypeIdsByIndex("f().abc", 0),
                BuiltinTypeId.String
            );
        }

        [TestMethod, Priority(0)]
        public void TestExportsLocation() {
            var server = @"var mymod = require('./mymod.js');";
            var entries = Analysis.Analyze(
                new AnalysisFile("server.js", server),
                new AnalysisFile("mymod.js", @"")
            );

            entries["server.js"].Analyze(default(CancellationToken));

            AssertUtil.ContainsExactly(
                entries["server.js"].Analysis.GetVariablesByIndex("mymod", server.Length).Select(x => x.Type + x.Location.FilePath),
                "Definition" + "server.js",
                "Value" + "mymod.js",
                "Reference" + "server.js"
            );
        }

        [TestMethod, Priority(0)]
        public void TestBadString() {
            var code = "var x = '\uD83D';";
            var analysis = ProcessText(code);
            AssertUtil.ContainsExactly(
                analysis.GetTypeIdsByIndex("x", code.Length),
                BuiltinTypeId.String
            );
        }

#if FALSE
        [TestMethod, Priority(0)]
        public void AnalyzeX() {
            var limits = new AnalysisLimits() {
                ReturnTypes = 1,
                AssignedTypes = 1,
                DictKeyTypes = 1,
                DictValueTypes = 1,
                IndexTypes = 1,
                InstanceMembers = 1
            };
            var sw = new Stopwatch();
            sw.Start();
            Analysis.Analyze("C:\\Source\\azure-sdk-tools-xplat", limits);
            Console.WriteLine("Time: {0}", sw.Elapsed);
        }

        [TestMethod, Priority(0)]
        public void AnalyzeExpress() {
            File.WriteAllText("C:\\Source\\Express\\express.txt", Analyzer.DumpAnalysis("C:\\Source\\Express"));
        }
#endif


        internal virtual ModuleAnalysis ProcessText(string text) {
            return ProcessOneText(text);
        }

        internal static ModuleAnalysis ProcessOneText(string text) {
            var sourceUnit = Analysis.GetSourceUnit(text);
            var state = new JsAnalyzer();
            var entry = state.AddModule("fob.js", null);
            Analysis.Prepare(entry, sourceUnit);
            entry.Analyze(CancellationToken.None);

            return entry.Analysis;
        }

    }

    static class AnalysisTestExtensions {
        public static IEnumerable<BuiltinTypeId> GetTypeIdsByIndex(this ModuleAnalysis analysis, string exprText, int index) {
            return analysis.GetValuesByIndex(exprText, index).Select(m => {
                return m.TypeId;
            });
        }
        public static IEnumerable<string> GetDescriptionByIndex(this ModuleAnalysis analysis, string exprText, int index) {
            return analysis.GetValuesByIndex(exprText, index).Select(m => {
                return m.Description;
            });
        }
    }
}

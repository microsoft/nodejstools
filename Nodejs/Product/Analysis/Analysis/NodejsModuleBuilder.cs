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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using Microsoft.NodejsTools.Analysis.Values;

namespace Microsoft.NodejsTools.Analysis {

    partial class NodejsModuleBuilder {
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly dynamic _all;
        //private readonly string _filename;
        private readonly JsAnalyzer _analyzer;
        private readonly LazyPropertyFunctionValue _readableStream, _writableStream;
        private Dictionary<string, string> _moduleDocs = new Dictionary<string, string>();

        public NodejsModuleBuilder(string filename, JsAnalyzer analyzer) {
            _all = _serializer.DeserializeObject(File.ReadAllText(filename));
            _analyzer = analyzer;
            _readableStream = new LazyPropertyFunctionValue(_analyzer._builtinEntry, "readable", "stream");
            _writableStream = new LazyPropertyFunctionValue(_analyzer._builtinEntry, "writable", "stream");
        }

        public static void Build(string filename, JsAnalyzer analyzer) {
            new NodejsModuleBuilder(filename, analyzer).Build();
        }

        public void Build() {

            Dictionary<string, ExportsValue> exportsTable = new Dictionary<string, ExportsValue>();
            // run through and initialize all of the modules.
            foreach (var module in _all["modules"]) {
                var moduleName = FixModuleName((string)module["name"]);
                var entry = new ProjectEntry(_analyzer, moduleName, null, true);
                var documentation = ParseDocumentation(module["desc"]);
                _analyzer.Modules.AddModule(moduleName, entry);

                exportsTable[moduleName] = entry.InitNodejsVariables(documentation);
            }

            // next create all of the classes
            foreach (var module in _all["modules"]) {
                var moduleName = FixModuleName((string)module["name"]);
                var exports = exportsTable[moduleName];

                if (module.ContainsKey("classes")) {
                    foreach (var klass in module["classes"]) {
                        GenerateClass(exports, klass);
                    }
                }

                if (module.ContainsKey("properties")) {
                    foreach (var property in module["properties"]) {
                        if (property.ContainsKey("methods")) {
                            GenerateClass(exports, property);
                        }
                    }
                }
            }

            foreach (var module in _all["modules"]) {
                var moduleName = FixModuleName((string)module["name"]);
                var exports = exportsTable[moduleName];
                Dictionary<string, FunctionSpecializer> specialMethods;
                _moduleSpecializations.TryGetValue(moduleName, out specialMethods);

                if (module.ContainsKey("methods")) {
                    foreach (var method in module["methods"]) {
                        GenerateMethod(exports, specialMethods, method);
                    }
                }

            }

            foreach (var misc in _all["miscs"]) {
                if (misc["name"] == "Global Objects") {
                    GenerateGlobals(misc["globals"]);
                    break;
                }
            }

        }

        private void GenerateMethod(ExpandoValue exports, Dictionary<string, FunctionSpecializer> specialMethods, dynamic method) {
            string methodName = (string)method["name"];

            ObjectValue returnValue = null;
            if (methodName.StartsWith("create") && methodName.Length > 6) {
                string klassName = methodName.Substring(6);
                PropertyDescriptorValue propDesc;
                if (exports.Descriptors.TryGetValue(klassName, out propDesc) && propDesc.Values != null) {
                    var types = propDesc.Values.Types;
                    if (types != null && types.Count == 1) {
                        var type = types.First().Value;
                        if (type is FunctionValue) {
                            returnValue = ((FunctionValue)type)._instance;
                        }
                    }
                }
            }

            foreach (var sig in method["signatures"]) {
                BuiltinFunctionValue function;
                FunctionSpecializer specialMethod;
                if (specialMethods != null &&
                    specialMethods.TryGetValue(methodName, out specialMethod)) {
                    function = specialMethod.Specialize(
                        exports.ProjectEntry,
                        methodName,
                        ParseDocumentation((string)method["desc"]),
                        returnValue,
                        GetParameters(sig["params"])
                    );
                } else if (returnValue != null) {
                    function = new ReturningFunctionValue(
                        exports.ProjectEntry,
                        methodName,
                        returnValue.SelfSet,
                        ParseDocumentation((string)method["desc"]),
                        GetParameters(sig["params"])
                    );
                } else {
                    function = new BuiltinFunctionValue(
                        exports.ProjectEntry,
                        methodName,
                        ParseDocumentation((string)method["desc"]),
                        null,
                        GetParameters(sig["params"])
                    );
                }

                exports.Add(function);
            }
        }

        private void GenerateClass(ObjectValue exports, dynamic klass) {
            string className = (string)klass["name"];

            List<OverloadResult> overloads = new List<OverloadResult>();
            var fixedClassName = FixClassName(className);
            dynamic signatures;
            if (klass.TryGetValue("signatures", out signatures)) {
                foreach (var sig in signatures) {
                    var parameters = GetParameters(sig["params"]);
                    var doc = ParseDocumentation((string)sig["desc"]);

                    overloads.Add(new SimpleOverloadResult(fixedClassName, doc, parameters));
                }
            }
            BuiltinFunctionValue klassValue = new ClassBuiltinFunctionValue(
                exports.ProjectEntry,
                fixedClassName,
                overloads.ToArray(),
                ParseDocumentation((string)klass["desc"])
            );

            exports.Add(klassValue);

            if (klass.ContainsKey("methods")) {
                var prototype = (PrototypeValue)klassValue.Descriptors["prototype"].Values.Types.First().Value;
                Dictionary<string, FunctionSpecializer> classSpecializations;
                _classSpecializations.TryGetValue(className, out classSpecializations);
                foreach (var method in klass["methods"]) {
                    GenerateMethod(
                        prototype,
                        classSpecializations,
                        method
                    );
                }
            }
            if (klass.ContainsKey("classMethods")) {
                foreach (var method in klass["methods"]) {
                    GenerateMethod(
                        klassValue,
                        null,
                        method
                    );
                }
            }

            if (klass.ContainsKey("properties")) {
                var prototype = (PrototypeValue)klassValue.Descriptors["prototype"].Values.Types.First().Value;

                GenerateProperties(klass, prototype, null);
            }
        }

        private void GenerateGlobal(ObjectValue exports, dynamic klass) {
            string name = FixClassName((string)klass["name"]);
            ObjectValue value = new BuiltinObjectValue(
                exports.ProjectEntry,
                ParseDocumentation((string)klass["desc"])
            );

            exports.Add(name, value.Proxy);

            if (klass.ContainsKey("methods")) {
                foreach (var method in klass["methods"]) {
                    GenerateMethod(
                        value,
                        null,
                        method
                    );
                }
            }

            Dictionary<string, PropertySpecializer> specializers;
            _propertySpecializations.TryGetValue(name, out specializers);

            GenerateProperties(klass, value, specializers);
        }

        private void GenerateProperties(dynamic klass, ObjectValue value, Dictionary<string, PropertySpecializer> specializers) {
            if (klass.ContainsKey("properties")) {
                foreach (var prop in klass["properties"]) {
                    string propName = prop["name"];
                    string desc = ParseDocumentation(prop["desc"]);

                    string textRaw = "";
                    if (prop.ContainsKey("textRaw")) {
                        textRaw = prop["textRaw"];
                    }

                    PropertySpecializer specializer;
                    AnalysisValue propValue = null;
                    if (specializers != null &&
                        specializers.TryGetValue(propName, out specializer)) {
                        propValue = specializer.Specialize(value.ProjectEntry, propName);
                    } else if (desc.IndexOf("<code>Boolean</code>") != -1) {
                        propValue = value.ProjectEntry.Analyzer._trueInst;
                    } else if (desc.IndexOf("<code>Number</code>") != -1) {
                        propValue = value.ProjectEntry.Analyzer._zeroIntValue;
                    } else if (desc.IndexOf("<code>Readable Stream</code>") != -1) {
                        propValue = _readableStream;
                    } else if (desc.IndexOf("<code>Writable Stream</code>") != -1 || textRaw == "process.stderr") {
                        propValue = _writableStream;
                    } else if (!String.IsNullOrWhiteSpace(textRaw)) {
                        int start, end;
                        if ((start = textRaw.IndexOf('{')) != -1 && (end = textRaw.IndexOf('}')) != -1 &&
                            start < end) {
                            string typeName = textRaw.Substring(start, end - start);
                            switch (typeName) {
                                case "Boolean":
                                    propValue = value.ProjectEntry.Analyzer._trueInst;
                                    break;
                                case "Number":
                                    propValue = value.ProjectEntry.Analyzer._zeroIntValue;
                                    break;
                            }
                        }
                    }

                    if (propValue == null) {
                        propValue = new BuiltinObjectValue(value.ProjectEntry);
                    }

                    value.Add(
                        new MemberAddInfo(
                            propName,
                            propValue,
                            desc,
                            true
                        )
                    );
                }
            }
        }

        private void GenerateGlobals(dynamic globals) {
            foreach (var global in globals) {
                if (global.ContainsKey("events") ||
                    global.ContainsKey("methods") ||
                    global.ContainsKey("properties") ||
                    global.ContainsKey("classes")) {
                    GenerateGlobal(_analyzer._globalObject, global);
                }
            }
        }

        private ParameterResult[] GetParameters(dynamic parameters) {
            List<ParameterResult> res = new List<ParameterResult>();
            foreach (var param in parameters) {
                if (param["name"] == "...") {
                    break;
                }
                res.Add(new ParameterResult(FixModuleName(param["name"])));
            }
            return res.ToArray();
        }


        private void GenerateModuleDocs() {
            foreach (var module in _all["modules"]) {
                var desc = (string)module["desc"];
                var output = ParseDocumentation(desc).ToString();

                _moduleDocs[FixModuleName(module["name"])] = output;
            }
        }

        private static char[] _newlines = new[] { '\r', '\n' };

        private static string ParseDocumentation(string desc) {
            StringBuilder output = new StringBuilder();
            var reader = XmlReader.Create(
                new StringReader(
                    "<html>" + desc + "</html>"
                )
            );

            string text = null;
            while (reader.Read()) {
                switch (reader.NodeType) {
                    case XmlNodeType.Element:
                        switch (reader.Name) {
                            case "li":
                                output.Append("* ");
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        text = reader.Value;
                        output.Append(text);
                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name) {
                            case "p":
                            case "li":
                            case "h1":
                            case "h2":
                            case "h3":
                            case "h4":
                            case "h5":
                            case "pre":
                                output.Append("\r\n");
                                break;
                            case "code":
                                if (text != null && text.IndexOfAny(_newlines) != -1) {
                                    output.Append("\r\n");
                                }
                                break;
                        }
                        break;

                }
            }

            // trim escaped newlines...
            while (output.ToString().EndsWith("\\r\\n")) {
                output.Length -= 4;
            }
            return output.ToString();
        }

#if FALSE
        private string GenerateJavaScript() {
            _output.AppendLine("global = {};");

            GenerateRequire(_output, _all);

            foreach (var misc in _all["miscs"]) {
                if (misc["name"] == "Global Objects") {
                    GenerateGlobals(misc["globals"]);
                    break;
                }
            }

            return _output.ToString();
        }

        private void GenerateGlobals(dynamic globals) {
            foreach (var global in globals) {
                if (global.ContainsKey("events") ||
                    global.ContainsKey("methods") ||
                    global.ContainsKey("properties") ||
                    global.ContainsKey("classes")) {


                    _output.AppendFormat("var {0} = new ", global["name"]);
                    GenerateModuleWorker(global, 0, "__" + global["name"]);
                    _output.Append(";");
                }
            }
        }

        private void GenerateModule(dynamic module, int indentation = 0) {
            string modName = FixModuleName(module["name"]);

            GenerateModuleWorker(module, indentation, modName);
        }

        private void GenerateEvents(dynamic events, int indentation) {
            StringBuilder eventsDoc = new StringBuilder();
            eventsDoc.AppendLine();
            eventsDoc.Append(' ', indentation * 4);
            eventsDoc.AppendLine("/// <summary>");
            eventsDoc.Append(' ', indentation * 4);
            eventsDoc.AppendLine("/// Supported events: &#10;");
            StringBuilder eventNames = new StringBuilder();
            foreach (var ev in events) {
                if (ev["name"].IndexOf(' ') != -1) {
                    continue;
                }
                if (eventNames.Length != 0) {
                    eventNames.Append(", ");
                }
                eventNames.Append(ev["name"]);
                
                eventsDoc.Append(' ', indentation * 4);
                eventsDoc.Append("/// ");
                eventsDoc.Append(ev["name"]);                
                if (ev.ContainsKey("desc")) {
                    eventsDoc.Append(": ");
                    eventsDoc.Append(LimitDescription(ev["desc"]));
                }
                eventsDoc.Append("&#10;");
                eventsDoc.AppendLine();
            }
            eventsDoc.Append(' ', indentation * 4);
            eventsDoc.AppendLine("/// </summary>");

            string supportedEvents = String.Format("/// <summary>Supported Events: {0}</summary>", eventNames.ToString());
            _output.AppendLine("this.addListener = function(event, listener) {");
            _output.AppendLine(eventsDoc.ToString());
            _output.AppendLine("}");

            _output.AppendLine("this.once = function(event, listener) {");
            _output.AppendLine(eventsDoc.ToString());
            _output.AppendLine("}");

            _output.AppendLine("this.removeListener = function(event, listener) {");
            _output.AppendLine(supportedEvents);
            _output.AppendLine("}");
            
            _output.AppendLine("this.removeAllListeners = function(event) {");
            _output.AppendLine(supportedEvents);
            _output.AppendLine("}");

            _output.AppendLine("this.setMaxListeners = function(n) { }");
            
            _output.AppendLine("this.listeners = function(event) {");
            _output.AppendLine(supportedEvents);
            _output.AppendLine("}");

            _output.AppendLine("this.emit = function(event, arguments) {");
            _output.AppendLine(eventsDoc.ToString());
            _output.AppendLine("}");

            _output.AppendLine("this.on = function(event, listener) {");
            _output.AppendLine(eventsDoc.ToString());
            _output.AppendLine("}");
        }

        private void GenerateMethod(string fullName, dynamic method, int indentation, string body = null) {
            _output.Append(' ', indentation * 4);
            _output.AppendFormat("this.{0} = function(", method["name"]);
            var signature = method["signatures"][0];
            bool first = true;
            foreach (var param in signature["params"]) {
                if (param["name"] == "...") {
                    break;
                }
                if (!first) {
                    _output.Append(", ");
                }

                _output.Append(FixModuleName(param["name"]));
                first = false;

                // TODO: Optional?
            }

            _output.AppendLine(") {");

            if (method.ContainsKey("desc")) {
                _output.Append(' ', (indentation + 1) * 4);
                _output.AppendFormat("/// <summary>{0}</summary>", FixDescription(method["desc"]));
                _output.AppendLine();
            }
            foreach (var curSig in method["signatures"]) {
                if (curSig["params"].Length > 0) {
                    _output.Append(' ', (indentation + 1) * 4);
                    _output.AppendLine("/// <signature>");

                    foreach (var param in curSig["params"]) {
                        _output.Append(' ', (indentation + 1) * 4);
                        _output.AppendFormat("/// <param name=\"{0}\"", param["name"]);

                        if (param.ContainsKey("type")) {
                            _output.AppendFormat(" type=\"{0}\"", FixModuleName(param["type"]));
                        }
                        _output.Append('>');

                        if (param.ContainsKey("desc")) {
                            _output.Append(param["desc"]);
                        }
                        _output.AppendLine("</param>");
                    }

                    if (curSig.ContainsKey("return")) {
                        object returnType = null, returnDesc = null;
                        if (curSig["return"].ContainsKey("type")) {
                            returnType = FixModuleName(curSig["return"]["type"]);
                        }

                        if (curSig["return"].ContainsKey("desc")) {
                            returnDesc = curSig["return"]["desc"];
                        }

                        if (returnType != null || returnDesc != null) {
                            _output.Append(' ', (indentation + 1) * 4);
                            _output.Append("/// <returns");

                            if (returnType != null) {
                                _output.AppendFormat(" type=\"{0}\">", returnType);
                            } else {
                                _output.Append('>');
                            }

                            if (returnDesc != null) {
                                _output.Append(returnDesc);
                            }
                            _output.AppendLine("</returns>");
                        }
                    }
                    _output.Append(' ', (indentation + 1) * 4);
                    _output.AppendLine("/// </signature>");
                }
            }

            if (body != null) {
                _output.AppendLine(body);
            } else if (method["name"].StartsWith("create") && method["name"].Length > 6) {
                _output.Append(' ', indentation * 4);
                _output.AppendFormat("return new this.{1}();", fullName, method["name"].Substring(6));
                _output.AppendLine();
            }
            _output.Append(' ', indentation * 4);
            _output.AppendLine("}");
        }
#endif

        private static string FixModuleName(string module) {
            if (module == "tls_(ssl)") {
                return "tls";
            } else if (module == "Events") {
                return "events";
            }

            for (int i = 0; i < module.Length; i++) {
                if (!(Char.IsLetterOrDigit(module[i]) || module[i] == '_')) {
                    return module.Substring(0, i);
                }
            }
            return module;
        }

        private static string FixDescription(string desc) {
            return desc.Replace("\n", "&#10;");
        }

        private static string LimitDescription(string desc) {
            int newLine;
            if ((newLine = desc.IndexOf('\n')) != -1) {
                return desc.Substring(0, newLine).Replace("<p>", "").Replace("</p>", "") + " ...";
            }
            return desc.Replace("<p>", "").Replace("</p>", "");
        }

        private string FixClassName(string name) {
            name = name.Replace(" ", "");
            int dot;
            if ((dot = name.IndexOf('.')) != -1) {
                return name.Substring(dot + 1);
            }
            return name;
        }

    }
}

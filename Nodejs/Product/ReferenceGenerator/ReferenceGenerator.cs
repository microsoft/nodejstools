using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace NodeReferenceGenerator {

    class NodeReferenceGenerator {
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly StringBuilder _output = new StringBuilder();
        private readonly dynamic _all;

        public NodeReferenceGenerator() {
            _all = _serializer.DeserializeObject(File.ReadAllText("all.json"));
        }

        static void Main(string[] args) {
            var generator = new NodeReferenceGenerator();
            var js = generator.GenerateJavaScript();
            
            File.WriteAllText("all.js", File.ReadAllText("IntellisenseHeader.js") + js);

            var cs = generator.GenerateCSharp();
            File.WriteAllText("modules.cs", cs);
        }

        private string GenerateCSharp() {
            StringBuilder res = new StringBuilder();
            res.Append(@"/* ****************************************************************************
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

using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.NodejsTools.Intellisense {
    sealed partial class CompletionSource : ICompletionSource {
");
            res.AppendLine("        private static Dictionary<string, string> _nodejsModules = new Dictionary<string, string>() {");
            foreach (var module in _all["modules"]) {
                var desc = (string)module["desc"];
                StringBuilder output = new StringBuilder();
                var reader = XmlReader.Create(
                    new StringReader(
                        "<html>" + desc+ "</html>"
                    )
                );

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
                            var text = reader.Value
                                .Replace("\\", "\\\\")
                                .Replace("\"", "\\\"")
                                .Replace("\r\n", "\\r\\n")
                                .Replace("\n", "\\r\\n");

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
                                    output.Append("\\r\\n");
                                    break;
                            }
                            break;

                    }
                }

                // trim escaped newlines...
                while (output.ToString().EndsWith("\\r\\n")) {
                    output.Length -= 4;
                }

                res.AppendFormat("            {{\"{0}\", \"{1}\" }},",
                    FixModuleName(module["name"]),
                    output.ToString()
                );
                res.AppendLine();
            }
            res.AppendLine("        };");
            res.AppendLine(@"    }
}");
            return res.ToString();
        }

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

        private void GenerateModuleWorker(dynamic module, int indentation, string name) {
            _output.Append(' ', indentation * 4);
            _output.AppendFormat("function {0}() {{", name);
            _output.AppendLine();

            if (module.ContainsKey("desc")) {
                _output.Append(' ', (indentation + 1) * 4);
                _output.AppendFormat("/// <summary>{0}</summary>", FixDescription(module["desc"]));
                _output.AppendLine();
            }

            if (module.ContainsKey("methods")) {
                foreach (var method in module["methods"]) {
                    GenerateMethod(name, method, indentation + 1);
                }
            }

            if (module.ContainsKey("events")) {
                GenerateEvents(module["events"], indentation + 1);
            }

            if (module.ContainsKey("classes")) {
                foreach (var klass in module["classes"]) {
                    GenerateClass(name, klass, indentation + 1);
                }
            }

            if (module.ContainsKey("properties")) {
                Func<string, string> specializer;
                PropertySpecializations.TryGetValue(name, out specializer);
                GenerateProperties(module["properties"], indentation + 1, specializer);
            }

            _output.AppendFormat("}}", name);
        }

        private Dictionary<string, Func<string, string>> PropertySpecializations = MakePropertySpecializations();

        private static Dictionary<string, Func<string, string>> MakePropertySpecializations() {
            return new Dictionary<string, Func<string, string>>() {
                { "__process", ProcessPropertySpecialization }
            };
        }

        private static string ProcessPropertySpecialization(string propertyName) {
            switch (propertyName) {
                case "env":
                    return "{}";
                case "versions":
                    return "{node: '0.10.0', v8: '3.14.5.8'}";
                case "pid":
                    return "0";
                case "title":
                    return "''";
                case "platform":
                    return "'win32'";
                case "maxTickDepth":
                    return "1000";
                case "argv":
                    return "[ 'node.exe' ]";

            }
            return null;
        }

        private void GenerateClass(string modName, dynamic klass, int indentation) {
            string className = FixClassName(klass["name"]);
            _output.Append(' ', indentation * 4);
            _output.AppendFormat("this.{0} = function() {{", className);
            _output.AppendLine();

            if (klass.ContainsKey("methods")) {
                foreach (var method in klass["methods"]) {
                    GenerateMethod(modName + "." + className, method, indentation + 1);
                }
            }

            if (klass.ContainsKey("events")) {
                GenerateEvents(klass["events"], indentation + 1);
            }

            if (klass.ContainsKey("properties")) {
                GenerateProperties(klass["properties"], indentation + 1, null);
            }

            _output.Append(' ', indentation * 4);
            _output.AppendLine("}");
        }

        private void GenerateProperties(dynamic properties, int indentation, Func<string, string> specializer) {
            foreach (var prop in properties) {
                string desc = "";
                if (prop.ContainsKey("desc")) {
                    desc = prop["desc"];

                    _output.Append(' ', indentation * 4);
                    _output.AppendFormat("/// <field name='{0}'>{1}</field>",
                        prop["name"],
                        FixDescription(desc));
                    _output.AppendLine();
                }

                _output.Append(' ', indentation * 4);
                string value = null;
                if (desc.IndexOf("<code>Boolean</code>") != -1) {
                    value = "true";
                } else if (desc.IndexOf("<code>Number</code>") != -1) {
                    value = "0";
                } else if (prop.ContainsKey("textRaw")) {
                    string textRaw = prop["textRaw"];
                    int start, end;
                    if ((start = textRaw.IndexOf('{')) != -1 && (end = textRaw.IndexOf('}')) != -1 &&
                        start < end) {
                        string typeName = textRaw.Substring(start, end - start);
                        switch (typeName) {
                            case "Boolean":
                                value = "true";
                                break;
                            case "Number":
                                value = "0";
                                break;
                        }
                    }
                }

                if (value == null) {
                    if (specializer != null) {
                        value = specializer(prop["name"]) ?? "undefined";
                    }

                    if (value == null) {
                        value = "undefined";
                    }
                }
                _output.AppendFormat("this.{0} = {1};", prop["name"], value);
                _output.AppendLine();
            }
        }

        private void GenerateEvents(dynamic events, int indentation) {
            _output.AppendLine("emitter = new Events().EventEmitter;");

            foreach (var ev in events) {
                if (ev["name"].IndexOf(' ') != -1) {
                    continue;
                }
                if (ev.ContainsKey("desc")) {
                    _output.Append(' ', indentation * 4);
                    _output.AppendFormat("/// <field name='{0}'>{1}</field>", ev["name"], FixDescription(ev["desc"]));
                    _output.AppendLine();

                }
                _output.Append(' ', indentation * 4);
                _output.AppendFormat("this.{0} = new emitter();", ev["name"]);
                _output.AppendLine();
            }
        }

        private void GenerateMethod(string fullName, dynamic method, int indentation = 1) {
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

            if (method["name"].StartsWith("create") && method["name"].Length > 6) {
                _output.Append(' ', indentation * 4);
                _output.AppendFormat("return new this.{1}();", fullName, method["name"].Substring(6));
                _output.AppendLine();
            }
            _output.Append(' ', indentation * 4);
            _output.AppendLine("}");
        }

        private void GenerateRequire(StringBuilder res, dynamic all) {
            res.AppendLine("require = function () {");
            res.AppendLine("    var require_count = 0;");
            res.AppendLine("    var cache = {");

            foreach (var module in all["modules"]) {
                res.AppendFormat("        \"{0}\": null,", FixModuleName(module["name"]));
                res.AppendLine();
            }

            res.AppendLine("    }");
            res.AppendLine("    function make_module(module_name) {");
            res.AppendLine("        switch(module_name) { ");
            foreach (var module in all["modules"]) {
                res.AppendFormat("            case \"{0}\": return new ", FixModuleName(module["name"]));
                GenerateModule(module, 1);
                res.AppendLine(";");
            }
            res.AppendLine("        }");
            res.AppendLine("    }");

            res.Append(@"    var f = function(module) { 
        if(require_count++ >= 50) {
            require_count = 0;
            intellisense.progress();
        }
        var result = cache[module];
        if(typeof result !== 'undefined') {
            if(result === null) {
                // we lazily create only the modules which are actually used
                cache[module] = result = make_module(module);
            }
            return result;
        }
        // value not cached, see if we can look it up
        try { 
            var __prevFilename = __filename;
            var __prevDirname = __dirname;
            // **NTVS** INSERT USER MODULE SWITCH HERE **NTVS**
        } finally {
            __filename = __prevFilename;
            __dirname = __prevDirname;
        }
    }
    o.__proto__ = f.__proto__;
    f.__proto__ = o;
    return f;
}()");
        }


        private static dynamic FixModuleName(string module) {
            if (module == "tls_(ssl)") {
                return "tls";
            }

            for (int i = 0; i < module.Length; i++) {
                if (!(Char.IsLetterOrDigit(module[i]) || module[i] == '_')) {
                    return module.Substring(0, i);
                }
            }
            return module;
        }

        private static string FixDescription(string desc) {
            return desc.Replace("\n", " ");
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

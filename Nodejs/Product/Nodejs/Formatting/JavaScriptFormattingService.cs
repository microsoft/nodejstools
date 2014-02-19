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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.NodejsTools.Formatting {
    class JavaScriptFormattingService : IActiveScriptSite {
        private readonly TypeScriptServiceHost _host;
        private readonly TypeScriptLanguageService _langSvc;
        private static readonly Lazy<JavaScriptFormattingService> _instance = new Lazy<JavaScriptFormattingService>(() => new JavaScriptFormattingService());
        private static readonly Guid JScriptEngine = new Guid("F414C260-6AC0-11CF-B6D1-00AA00BBBB58");

        public static JavaScriptFormattingService Instance {
            get { return _instance.Value; }
        }

        private JavaScriptFormattingService() {
            var t = Type.GetTypeFromCLSID(JScriptEngine);
            var scriptParse = Activator.CreateInstance(t) as IActiveScript;            
            scriptParse.SetScriptSite(this);
            var parse = scriptParse as IActiveScriptParse;
            _host = new TypeScriptServiceHost();

            ErrorHandler.ThrowOnFailure(
                scriptParse.AddNamedItem(
                    "__ntvsLangServiceHost", 
                    AddNamedItemFlags.IsVisible | AddNamedItemFlags.GlobalMembers
                )
            );

            dynamic dispatchObject;
            ErrorHandler.ThrowOnFailure(scriptParse.GetScriptDispatch(null, out dispatchObject));

            ParseTypeScriptLanguageService(parse);
            CreateHelperFunctions(parse);

            _host.ArrayMaker = dispatchObject.__ntvsMakeArray;

            CreateLanguageServiceInstance(parse);

            _langSvc = new TypeScriptLanguageService(dispatchObject.__ntvsLangService);
        }

        public TextEdit[] GetFormattingEditsForDocument(string fileName, int minChar, int limChar, FormatCodeOptions options) {
            return _langSvc.GetFormattingEditsForDocument(fileName, minChar, limChar, options);
        }

        public TextEdit[] GetFormattingEditsForRange(string fileName, int minChar, int limChar, FormatCodeOptions options) {
            return _langSvc.GetFormattingEditsForRange(fileName, minChar, limChar, options);
        }

        public TextEdit[] GetFormattingEditsOnPaste(string fileName, int minChar, int limChar, FormatCodeOptions options) {
            return _langSvc.GetFormattingEditsOnPaste(fileName, minChar, limChar, options);
        }

        public TextEdit[] GetFormattingEditsAfterKeystroke(string fileName, int position, string key, FormatCodeOptions options) {
            return _langSvc.GetFormattingEditsAfterKeystroke(fileName, position, key, options);
        }

        public void AddDocument(string path, ITextBuffer buffer) {
            _host.AddDocument(path, buffer);
        }

        public void RemoveDocument(string path) {
            _host.RemoveDocument(path);
        }

        /// <summary>
        /// Creates a new instance of the TypeScript language service which uses our host object.
        /// </summary>
        private static void CreateLanguageServiceInstance(IActiveScriptParse parse) {
            object res;
            ErrorHandler.ThrowOnFailure(
                parse.ParseScriptText(
                    @"
__ntvsLangService =  new TypeScript.Services.TypeScriptServicesFactory().createPullLanguageService(__ntvsLangServiceHost)",
                    null,
                    null,
                    null,
                    0,
                    1,
                    ParseScriptTextFlags.IsVisible,
                    out res,
                    IntPtr.Zero
                )
            );
        }

        /// <summary>
        /// Creates a new function which we use to create JavaScript arrays from
        /// our ArrayHelper class.
        /// </summary>
        private static void CreateHelperFunctions(IActiveScriptParse parse) {
            object res;
            ErrorHandler.ThrowOnFailure(
                parse.ParseScriptText(
                @"
function __ntvsMakeArray(arg) {
    var res = Array();
    for(var i = 0; i<arg.length(); i++) {
        res[i] = arg.item(i);
    }
    return res;
}
",
                null,
                null,
                null,
                0,
                1,
                ParseScriptTextFlags.IsVisible,
                out res,
                IntPtr.Zero)
            );
        }

        /// <summary>
        /// Loads the TypeScript services script into the script engine
        /// </summary>
        private static void ParseTypeScriptLanguageService(IActiveScriptParse parse) {
            var tsServicePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "typescriptServices.js"
            );

            object res;
            ErrorHandler.ThrowOnFailure(
                parse.ParseScriptText(
                    File.ReadAllText(tsServicePath),
                    null,
                    null,
                    null,
                    0,
                    1,
                    ParseScriptTextFlags.IsVisible,
                    out res,
                    IntPtr.Zero
                )
            );
        }

        public int GetLCID(out uint plcid) {
            plcid = (uint)System.Globalization.CultureInfo.CurrentCulture.LCID;
            return 0;
        }

        public int GetItemInfo(string pstrName, GetItemInfoFlags dwReturnMask, out object ppiunkItem, IntPtr ppti) {
            switch (pstrName) {
                case "__ntvsLangServiceHost":
                    ppiunkItem = _host;
                    return 0;
            }

            throw new NotImplementedException();
        }

        public int GetDocVersionString(out string pbstrVersion) {
            pbstrVersion = null;
            return VSConstants.E_NOTIMPL;
        }

        public int OnScriptTerminate(object pvarResult, ref EXCEPINFO pexcepinfo) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnStateChange(ScriptState ssScriptState) {
            return 0;
        }

        public int OnScriptError(IActiveScriptError error) {
#if DEBUG
            EXCEPINFO ehInfo;
            error.GetExceptionInfo(out ehInfo);
            Debug.Fail(String.Format("OnScriptError {0} from {1}", ehInfo.bstrDescription, ehInfo.bstrSource));
#endif
            return 0;
        }

        public int OnEnterScript() {
            return 0;
        }

        public int OnLeaveScript() {
            return 0;
        }
    }
}

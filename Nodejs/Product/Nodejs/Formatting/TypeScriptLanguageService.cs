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

using System.Collections;

namespace Microsoft.NodejsTools.Formatting {

    /// <summary>
    /// Provides a wrapper around the TypeScript language service which is hosted
    /// inside of a JavaScript engine.
    /// </summary>
    class TypeScriptLanguageService {
        private readonly dynamic _jsObject;

        public TypeScriptLanguageService(dynamic jsObject) {
            _jsObject = jsObject;
        }

        public void Refresh() {
            _jsObject.refresh();
        }

        public TextEdit[] GetFormattingEditsForDocument(string fileName, int minChar, int limChar, FormatCodeOptions options) {
            return GetTextEdits(_jsObject.getFormattingEditsForDocument(fileName, minChar, limChar, options));
        }

        public TextEdit[] GetFormattingEditsForRange(string fileName, int minChar, int limChar, FormatCodeOptions options) {
            return GetTextEdits(_jsObject.getFormattingEditsForRange(fileName, minChar, limChar, options));
        }

        public TextEdit[] GetFormattingEditsOnPaste(string fileName, int minChar, int limChar, FormatCodeOptions options) {
            return GetTextEdits(_jsObject.getFormattingEditsOnPaste(fileName, minChar, limChar, options));
        }

        public TextEdit[] GetFormattingEditsAfterKeystroke(string fileName, int position, string key, FormatCodeOptions options) {
            return GetTextEdits(_jsObject.getFormattingEditsAfterKeystroke(fileName, position, key, options));
        }

        /// <summary>
        /// Converts the JavaScript text edit objects into .NET text edit objects
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private static TextEdit[] GetTextEdits(dynamic res) {
            TextEdit[] arr = new TextEdit[res.length];

            // Enumeration is weird because JavaScript arrays are weird...
            IEnumerator enumerator = (res as IEnumerable).GetEnumerator();
            int length = res.length;

            for (int i = 0; i < length && enumerator.MoveNext(); i++) {
                dynamic obj = enumerator.Current;
                arr[i] = new TextEdit(obj.minChar, obj.limChar, obj.text);
            }
            return arr;
        }

        // Additional APIs which could be useful in the future:
        /*
        cleanupSemanticCache(): void;
        getSyntacticDiagnostics(fileName: string): TypeScript.Diagnostic[];
        getSemanticDiagnostics(fileName: string): TypeScript.Diagnostic[];
        getCompilerOptionsDiagnostics(): TypeScript.Diagnostic[];
        getCompletionsAtPosition(fileName: string, position: number, isMemberCompletion: boolean): CompletionInfo;
        getCompletionEntryDetails(fileName: string, position: number, entryName: string): CompletionEntryDetails;
        getTypeAtPosition(fileName: string, position: number): TypeInfo;
        getNameOrDottedNameSpan(fileName: string, startPos: number, endPos: number): SpanInfo;
        getBreakpointStatementAtPosition(fileName: string, position: number): SpanInfo;
        getSignatureAtPosition(fileName: string, position: number): SignatureInfo;
        getDefinitionAtPosition(fileName: string, position: number): DefinitionInfo[];
        getReferencesAtPosition(fileName: string, position: number): ReferenceEntry[];
        getOccurrencesAtPosition(fileName: string, position: number): ReferenceEntry[];
        getImplementorsAtPosition(fileName: string, position: number): ReferenceEntry[];
        getNavigateToItems(searchValue: string): NavigateToItem[];
        getScriptLexicalStructure(fileName: string): NavigateToItem[];
        getOutliningRegions(fileName: string): TypeScript.TextSpan[];
        getBraceMatchingAtPosition(fileName: string, position: number): TypeScript.TextSpan[];
        getIndentationAtPosition(fileName: string, position: number, options: EditorOptions): number;
        getEmitOutput(fileName: string): TypeScript.EmitOutput;
        getSyntaxTree(fileName: string): TypeScript.SyntaxTree;*/
    }
}

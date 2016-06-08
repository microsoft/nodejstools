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

namespace Microsoft.NodejsTools.Options {
    /// <summary>
    /// Copied from  TypeScript\VS\LanguageService\TypeScriptLanguageService\ToolsOptions\Constants.cs
    /// </summary>
    public static class TypeScriptRegistrySwitches {
        public const string TypeScriptLanguageServiceSubKey = "TypeScriptLanguageService";

        public const string TerminateProcessOnException = "TerminateProcessOnException";
        public const string SimulateCrashOnCompletionRequest = "SimulateCrashOnCompletionRequest";
        public const string EnableDevMode = "EnableDevMode";
        public const string CustomTypeScriptServicesFileLocation = "CustomTypeScriptServicesFileLocation";
        public const string CustomDefaultLibraryLocation = "CustomDefaultLibraryLocation";
        public const string CustomDefaultES6LibraryLocation = "CustomDefaultES6LibraryLocation";

        public const string FormatCompletedLineOnEnter = "FormatCompletedLineOnEnter_TEMP"; // TODO: Remove "TEMP" when option is supported
        public const string FormatCompletedStatementOnSemicolon = "FormatCompletedStatementOnSemicolon";
        public const string FormatCompletedBlockOnRightCurlyBrace = "FormatCompletedBlockOnRightCurlyBrace";
        public const string FormatOnPaste = "FormatOnPaste_TEMP"; // TODO: Remove "TEMP" when option is supported

        public const string PlaceOpenBraceOnNewLineForFunctions = "PlaceOpenBraceOnNewLineForFunctions";
        public const string PlaceOpenBraceOnNewLineForControlBlocks = "PlaceOpenBraceOnNewLineForControlBlocks";

        public const string InsertSpaceAfterCommaDelimiter = "InsertSpaceAfterCommaDelimiter";
        public const string InsertSpaceAfterSemicolonInForStatements = "InsertSpaceAfterSemicolonInForStatements";
        public const string InsertSpaceBeforeAndAfterBinaryOperators = "InsertSpaceBeforeAndAfterBinaryOperators";
        public const string InsertSpaceAfterKeywordsInControlFlowStatements = "InsertSpaceAfterKeywordsInControlFlowStatements";
        public const string InsertSpaceAfterFunctionKeywordForAnonymousFunctions = "InsertSpaceAfterFunctionKeywordForAnonymousFunctions";
        public const string InsertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis = "InsertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis";

        public const string ShowVirtualProjectsInSolutionExplorerWhenNoSolution = "ShowVirtualProjectsInSolutionExplorerWhenNoSolution";
        public const string ShowVirtualProjectsInSolutionExplorerWhenSolutionOpen = "ShowVirtualProjectsInSolutionExplorerWhenSolutionOpen";

        public const string AutomaticallyCompileTypeScriptFilesWhenSavedWhenNoSolution = "AutomaticallyCompileTypeScriptFilesWhenSavedWhenNoSolution";
        public const string UseAMDCodeGenerationForModulesThatAreNotPartOfAProject = "UseAMDCodeGenerationForModulesThatAreNotPartOfAProject";
        public const string UseCommonJSCodeGenerationForModulesThatAreNotPartOfAProject = "UseCommonJSCodeGenerationForModulesThatAreNotPartOfAProject";
        public const string UseSystemCodeGenerationForModulesThatAreNotPartOfAProject = "UseSystemCodeGenerationForModulesThatAreNotPartOfAProject";
        public const string UseUMDCodeGenerationForModulesThatAreNotPartOfAProject = "UseUMDCodeGenerationForModulesThatAreNotPartOfAProject";
        public const string UseES2015CodeGenerationForModulesThatAreNotPartOfAProject = "UseES2015CodeGenerationForModulesThatAreNotPartOfAProject";

        public const string UseTypeScriptExperimental = "UseTypeScriptExperimental";

        public const string UseJsxReactForFilesThatAreNotPartOfAProject = "UseJsxReactForFilesThatAreNotPartOfAProject";
        public const string UseJsxPreserveForFilesThatAreNotPartOfAProject = "UseJsxPreserveForFilesThatAreNotPartOfAProject";

        public const string ECMAScriptForFilesThatAreNotPartOfAProject = "ECMAScriptForFilesThatAreNotPartOfAProject";
        public const string ShowGruntGulpDialogForAspNet = "ShowGruntGulpDialogForAspNet";

        public const string CompletionChars = "CompletionChars";
    }
}
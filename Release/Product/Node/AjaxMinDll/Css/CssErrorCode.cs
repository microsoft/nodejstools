// CssStringMgr.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Microsoft.Ajax.Utilities
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    public enum CssErrorCode
    {
        NoError = 0,
        UnknownError = 1000,
        UnterminatedComment,
        UnterminatedString,
        UnnecessaryUnits,
        UnexpectedNumberCharacter,
        ExpectedOpenParenthesis,
        InvalidLowSurrogate,
        HighSurrogateNoLow,
        UnderscoreNotValid,
        UnexpectedEscape,
        UnexpectedStringCharacter,
        DecimalNoDigit,
        EquivalentNumbers,
        ScannerSubsystem,
        FallbackEncodingFailed,
        UnknownCharacterEncoding,
        ParserSubsystem,
        ExpectedCharset,
        ExpectedSemicolon,
        UnexpectedToken,
        UnexpectedAtKeyword,
        ExpectedNamespace,
        ExpectedImport,
        ExpectedCommaOrSemicolon,
        ExpectedMediaIdentifier,
        ExpectedCommaOrOpenBrace,
        ExpectedOpenBrace,
        ExpectedSemicolonOrOpenBrace,
        DeclarationIgnoredFormat,
        DeclarationIgnored,
        ExpectedIdentifier,
        ExpectedSelector,
        ExpectedIdentifierOrString,
        ExpectedClosingBracket,
        ExpectedClosingParenthesis,
        ExpectedColon,
        ExpectedExpression,
        HashAfterUnaryNotAllowed,
        ExpectedHexColor,
        TokenAfterUnaryNotAllowed,
        UnexpectedDimension,
        ExpectedProgId,
        ExpectedFunction,
        ProgIdIEOnly,
        ExpectedEqualSign,
        ExpectedTerm,
        ExpectedComma,
        ExpectedRgbNumberOrPercentage,
        ColorCanBeCollapsed,
        HackGeneratesInvalidCss,
        ExpectedEndOfFile,
        DuplicateNamespaceDeclaration,
        UndeclaredNamespace,
        InvalidUnicodeRange,
        ExpressionError,
        ExpectedMediaQueryExpression,
        ExpectedMediaFeature,
        ExpectedMediaQuery,
        MediaQueryRequiresSpace,
        PossibleInvalidClassName,
        ExpectedClosingBrace,
        ExpectedPercentageFromOrTo,
        ExpectedSemicolonOrClosingBrace,
        ExpectedUnit,
        ExpectedProduct,
        ExpectedSum,
        ExpectedMinMax,
        UnexpectedEndOfFile,
        ExpectedNumber,
        UnexpectedCharset,
        PossibleCharsetError,
    };
}
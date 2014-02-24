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

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade {
    internal class JadeClassificationNameProvider : IClassificationContextNameProvider<JadeToken> {
        private readonly IClassificationTypeRegistryService _classReg;
        private const string 
            HtmlComment = "HTML Comment",
            HtmlElementName = "HTML Element Name",
            HtmlAttributeName = "HTML Attribute Name",
            HtmlAttributeValue = "HTML Attribute Value",
            HtmlOperator = "HTML Operator",
            HtmlEntity = "HTML Entity",
            CssPropertyName = "CSS Property Name",
            CssPropertyValue = "CSS Property Value",
            CssSelector = "CSS Selector";

        public JadeClassificationNameProvider(IClassificationTypeRegistryService ClassificationRegistryService) {
            _classReg = ClassificationRegistryService;
        }

        public IClassificationType GetClassificationType(JadeToken token) {
            if (token.Classification != null) {
                return token.Classification;
            }

            return _classReg.GetClassificationType(GetClassificationName(token));
        }

        private static string GetClassificationName(JadeToken token) {
            switch (token.TokenType) {
                case JadeTokenType.TagName:
                    return HtmlElementName;
                case JadeTokenType.ClassLiteral:
                    return JadeClassificationTypes.ClassLiteral;
                case JadeTokenType.IdLiteral:
                    return JadeClassificationTypes.IdLiteral;
                case JadeTokenType.Filter:
                    return JadeClassificationTypes.Filter;
                case JadeTokenType.TagKeyword:
                    return JadeClassificationTypes.Keyword;
                case JadeTokenType.Variable:
                    return JadeClassificationTypes.Variable;
                case JadeTokenType.Comment:
                    return HtmlComment;
                case JadeTokenType.String:
                    return HtmlElementName;
                case JadeTokenType.AttributeName:
                    return HtmlAttributeName;
                case JadeTokenType.AttributeValue:
                    return HtmlAttributeValue;
                case JadeTokenType.Operator:
                    return HtmlOperator;
                case JadeTokenType.AngleBracket:
                    return HtmlOperator;
                case JadeTokenType.Entity:
                    return HtmlEntity;
                case JadeTokenType.CodeKeyword:
                    return PredefinedClassificationTypeNames.Keyword;
                case JadeTokenType.Number:
                    return PredefinedClassificationTypeNames.Number;
                case JadeTokenType.Punctuator:
                    return PredefinedClassificationTypeNames.Operator;
                case JadeTokenType.CssPropertyName:
                    return CssPropertyName;
                case JadeTokenType.CssPropertyValue:
                    return CssPropertyValue;
                case JadeTokenType.CssSelector:
                    return CssSelector;

                default:
                    return "Default";
            }
        }
    }
}

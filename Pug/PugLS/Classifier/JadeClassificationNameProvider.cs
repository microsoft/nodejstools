// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.VisualStudio.Pug
{
    internal class JadeClassificationNameProvider : IClassificationContextNameProvider<JadeToken>
    {
        private readonly IClassificationTypeRegistryService classReg;
        private const string HtmlComment = "HTML Comment";
        private const string HtmlElementName = "HTML Element Name";
        private const string HtmlAttributeName = "HTML Attribute Name";
        private const string HtmlAttributeValue = "HTML Attribute Value";
        private const string HtmlOperator = "HTML Operator";
        private const string HtmlEntity = "HTML Entity";
        private const string CssPropertyName = "CSS Property Name";
        private const string CssPropertyValue = "CSS Property Value";
        private const string CssSelector = "CSS Selector";

        public JadeClassificationNameProvider(IClassificationTypeRegistryService ClassificationRegistryService)
        {
            this.classReg = ClassificationRegistryService;
        }

        public IClassificationType GetClassificationType(JadeToken token)
        {
            if (token.Classification != null)
            {
                return token.Classification;
            }

            return this.classReg.GetClassificationType(GetClassificationName(token));
        }

        private static string GetClassificationName(JadeToken token)
        {
            switch (token.TokenType)
            {
                case JadeTokenType.TagName:
                    return HtmlElementName;
                case JadeTokenType.ClassLiteral:
                    return PugClassificationTypes.ClassLiteral;
                case JadeTokenType.IdLiteral:
                    return PugClassificationTypes.IdLiteral;
                case JadeTokenType.Filter:
                    return PugClassificationTypes.Filter;
                case JadeTokenType.TagKeyword:
                    return PugClassificationTypes.Keyword;
                case JadeTokenType.Variable:
                    return PugClassificationTypes.Variable;
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

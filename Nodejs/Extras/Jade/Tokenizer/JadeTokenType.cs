// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    internal enum JadeTokenType
    {
        None,
        Comment,
        String,
        TagName,
        Operator,
        Number,
        AttributeName,
        AttributeValue,
        Filter,
        CodeKeyword,     // if, each, else, ...
        TagKeyword,      // stylesheets:, javascripts:, as:
        IdLiteral,      // a#foo    <a id="foo"></a> or #foo <div id="foo"></div>
        ClassLiteral,   // a.button <a class="button"></a>
        CssSelector,        // CSS selector
        CssPropertyName,    // CSS property name
        CssPropertyValue,   // CSS property value
        Punctuator,
        Variable,
        AngleBracket,
        Entity
    }
}

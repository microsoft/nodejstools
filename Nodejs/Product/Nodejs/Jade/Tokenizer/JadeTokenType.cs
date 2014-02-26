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


namespace Microsoft.NodejsTools.Jade {
    enum JadeTokenType {
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

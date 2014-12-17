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

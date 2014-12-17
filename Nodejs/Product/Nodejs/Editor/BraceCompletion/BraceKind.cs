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

namespace Microsoft.NodejsTools.Editor.BraceCompletion {
    internal static class BraceKind {
        public static class Parentheses {
            public const char Open = '(';
            public const char Close = ')';
        }

        public static class SquareBrackets {
            public const char Open = '[';
            public const char Close = ']';
        }

        public static class CurlyBrackets {
            public const char Open = '{';
            public const char Close = '}';
        }

        public static class SingleQuotes {
            public const char Open = '\'';
            public const char Close = '\'';
        }

        public static class DoubleQuotes {
            public const char Open = '"';
            public const char Close = '"';
        }
    }
}
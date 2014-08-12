// context.cs
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


namespace Microsoft.NodejsTools.Parsing {
    /// <summary>
    /// Represents a JavaScript token with a span.
    /// </summary>
    public class TokenWithSpan {
        private readonly int _start, _end;
        private readonly JSToken _token;
        private readonly LocationResolver _indexResolver;

        internal TokenWithSpan() {
        }

        internal TokenWithSpan(LocationResolver indexResolver, int start, int end, JSToken token) {
            _token = token;
            _start = start;
            _end = end;
            _indexResolver = indexResolver;
        }

        public TokenWithSpan(TokenWithSpan currentToken, JSToken newToken) {
            _indexResolver = currentToken._indexResolver;
            _start = currentToken._start;
            _end = currentToken._end;
            _token = newToken;
        }

        /// <summary>
        /// Gets the 0 based starting position
        /// </summary>
        public int Start { get { return _start; } }

        /// <summary>
        /// Gets the 0 based end position
        /// </summary>
        public int End { get { return _end; } }

        /// <summary>
        /// Gets the token
        /// </summary>
        public JSToken Token { get { return _token; } }

        public IndexSpan Span { get { return IndexSpan.FromBounds(_start, _end); } }

        /// <summary>
        /// Gets the 1 based starting line number
        /// </summary>
        public int StartLine {
            get {
                if (_indexResolver == null) {
                    return 1;
                }
                return _indexResolver.IndexToLocation(Start).Line;
            }
        }

        /// <summary>
        /// Gets the 1 based ending line number
        /// </summary>
        public int EndLine {
            get {
                if (_indexResolver == null) {
                    return 1;
                }
                return _indexResolver.IndexToLocation(End).Line;
            }
        }

        /// <summary>
        /// 1 based start column
        /// </summary>
        public int StartColumn {
            get {
                if (_indexResolver == null) {
                    return 1;
                }
                return _indexResolver.IndexToLocation(Start).Column;
            }
        }

        /// <summary>
        /// 1 based end column
        /// </summary>
        public int EndColumn {
            get {
                if (_indexResolver == null) {
                    return 1;
                }
                return _indexResolver.IndexToLocation(End).Column;
            }
        }

        public override string ToString() {
            return Token.ToString();
        }
    }
}
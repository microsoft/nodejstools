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
        private int _startPosition, _endPosition;
        internal JSToken _token;    // TODO: EOF handling prevents this from being readonly
        private readonly IndexResolver _indexResolver;

        internal TokenWithSpan() {
        }

        internal TokenWithSpan(IndexResolver indexResolver, int startPosition, int endPosition, JSToken token) {
            _token = token;
            _startPosition = startPosition;
            _endPosition = endPosition;
            _indexResolver = indexResolver;
        }

        /// <summary>
        /// Gets the 0 based starting position
        /// </summary>
        public int StartPosition { get { return _startPosition; } }
        /// <summary>
        /// Gets the 0 based end position
        /// </summary>
        public int EndPosition { get { return _endPosition; } }
        /// <summary>
        /// Gets the token
        /// </summary>
        public JSToken Token { get { return _token; } }

        /// <summary>
        /// Gets the 1 based starting line number
        /// </summary>
        public int StartLineNumber {
            get {
                if (_indexResolver == null) {
                    return 1;
                }
                return _indexResolver.IndexToLocation(StartPosition).Line;
            }
        }

        /// <summary>
        /// Gets the 1 based ending line number
        /// </summary>
        public int EndLineNumber {
            get {
                if (_indexResolver == null) {
                    return 1;
                }
                return _indexResolver.IndexToLocation(EndPosition).Line;
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
                return _indexResolver.IndexToLocation(StartPosition).Column;
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
                return _indexResolver.IndexToLocation(EndPosition).Column;
            }
        }

        public TokenWithSpan Clone() {
            return new TokenWithSpan(
                _indexResolver,
                StartPosition,
                EndPosition,
                Token
            );
        }

        public TokenWithSpan FlattenToStart() {
            return new TokenWithSpan(
                _indexResolver,
                StartPosition,
                StartPosition,
                Token
            );
        }

        public TokenWithSpan FlattenToEnd() {
            return new TokenWithSpan(
                _indexResolver,
                EndPosition,
                EndPosition,
                Token
            );
        }

        public TokenWithSpan CombineWith(TokenWithSpan other) {
            if (other == null) {
                return Clone();
            }

            return new TokenWithSpan(
                _indexResolver,
                StartPosition,
                other.EndPosition,
                Token
            );
        }

        /// <summary>
        /// TODO: This method prevents making _startPosition and _endPostion readonly
        /// </summary>
        public TokenWithSpan UpdateWith(TokenWithSpan other) {
            if (other != null) {
                if (other.StartPosition < this.StartPosition) {
                    _startPosition = other.StartPosition;
                }

                if (other.EndPosition > this.EndPosition) {
                    _endPosition = other.EndPosition;
                }
            }

            return this;
        }

        public override string ToString() {
            return Token.ToString();
        }
    }
}
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

using System;

namespace Microsoft.Ajax.Utilities
{
    public class Context
    {
        public DocumentContext Document { get; private set; }

        public int StartLineNumber { get; internal set; }
        public int StartLinePosition { get; internal set; }
        public int StartPosition { get; internal set; }
        public int EndLineNumber { get; internal set; }
        public int EndLinePosition { get; internal set; }
        public int EndPosition { get; internal set; }
        public int SourceOffsetStart { get; internal set; }
        public int SourceOffsetEnd { get; internal set; }

        public JSToken Token { get; internal set; }

        public Context(JSParser parser)
            : this(new DocumentContext(parser))
        {
        }

        public Context(DocumentContext document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            Document = document;

            StartLineNumber = 1;
            EndLineNumber = 1;
            EndPosition = Document.Source.IfNotNull(s => s.Length);

            Token = JSToken.None;
        }

        public Context(DocumentContext document, int startLineNumber, int startLinePosition, int startPosition, int endLineNumber, int endLinePosition, int endPosition, JSToken token)
            : this(document)
        {
            StartLineNumber = startLineNumber;
            StartLinePosition = startLinePosition;
            StartPosition = startPosition;
            EndLineNumber = endLineNumber;
            EndLinePosition = endLinePosition;
            EndPosition = endPosition;
            Token = token;
        }

        public Context Clone()
        {
            return new Context(this.Document)
            {
                StartLineNumber = this.StartLineNumber, 
                StartLinePosition = this.StartLinePosition, 
                StartPosition = this.StartPosition,
                EndLineNumber = this.EndLineNumber, 
                EndLinePosition = this.EndLinePosition, 
                EndPosition = this.EndPosition,
                SourceOffsetStart = this.SourceOffsetStart,
                SourceOffsetEnd = this.SourceOffsetEnd,
                Token = this.Token,
            };
        }

        public Context FlattenToStart()
        {
            // clone the context and flatten the end to be the start position
            var clone = Clone();
            clone.EndLineNumber = clone.StartLineNumber;
            clone.EndLinePosition = clone.StartLinePosition;
            clone.EndPosition = clone.StartPosition;
            return clone;
        }

        public Context FlattenToEnd()
        {
            // clone the context and flatten the start to the end position
            var clone = Clone();
            clone.StartLineNumber = clone.EndLineNumber;
            clone.StartLinePosition = clone.EndLinePosition;
            clone.StartPosition = clone.EndPosition;
            return clone;
        }

        public Context CombineWith(Context other)
        {
            return other == null
                ? this.Clone()
                : new Context(Document)
                    {
                        StartLineNumber = this.StartLineNumber,
                        StartLinePosition = this.StartLinePosition,
                        StartPosition = this.StartPosition,
                        EndLineNumber = other.EndLineNumber,
                        EndLinePosition = other.EndLinePosition,
                        EndPosition = other.EndPosition,
                        SourceOffsetStart = this.SourceOffsetStart,
                        SourceOffsetEnd = other.SourceOffsetEnd,
                        Token = this.Token
                    };
        }

        public int StartColumn
        {
            get
            {
                return StartPosition - StartLinePosition;
            }
        }

        public int EndColumn
        {
            get
            {
                return EndPosition - EndLinePosition;
            }
        }

        public bool HasCode
        {
            get
            {
                return !Document.IsGenerated 
                    && EndPosition > StartPosition 
                    && EndPosition <= Document.Source.Length
                    && EndPosition != StartPosition;
            }
        }

        public String Code
        {
            get
            {
                return (!Document.IsGenerated && EndPosition > StartPosition && EndPosition <= Document.Source.Length)
                  ? Document.Source.Substring(StartPosition, EndPosition - StartPosition)
                  : null;
            }
        }

        internal void ReportUndefined(Lookup lookup)
        {
            UndefinedReferenceException ex = new UndefinedReferenceException(lookup, this);
            Document.ReportUndefined(ex);
        }

        internal void ChangeFileContext(string fileContext)
        {
            // if the file context is the same, then there's nothing to change
            if (string.Compare(Document.FileContext, fileContext, StringComparison.OrdinalIgnoreCase) != 0)
            {
                // different source. Need to create a clone of the current document context but
                // with the new file context
                Document = Document.DifferentFileContext(fileContext);
            }
        }

        internal void HandleError(JSError errorId)
        {
            HandleError(errorId, false);
        }

        internal void HandleError(JSError errorId, bool forceToError)
        {
            if ((errorId != JSError.UndeclaredVariable && errorId != JSError.UndeclaredFunction) || !Document.HasAlreadySeenErrorFor(Code))
            {
                var error = new JScriptException(errorId, this);

                if (forceToError)
                {
                    error.IsError = true;
                }
                else
                {
                    error.IsError = error.Severity < 2;
                }

                Document.HandleError(error);
            }
        }

        public Context UpdateWith(Context other)
        {
            if (other != null)
            {
                if (other.StartPosition < this.StartPosition)
                {
                    this.StartPosition = other.StartPosition;
                    this.StartLineNumber = other.StartLineNumber;
                    this.StartLinePosition = other.StartLinePosition;
                    this.SourceOffsetStart = other.SourceOffsetStart;
                }

                if (other.EndPosition > this.EndPosition)
                {
                    this.EndPosition = other.EndPosition;
                    this.EndLineNumber = other.EndLineNumber;
                    this.EndLinePosition = other.EndLinePosition;
                    this.SourceOffsetEnd = other.SourceOffsetEnd;
                }
            }

            return this;
        }

        public bool IsBefore(Context other)
        {
            // this context is BEFORE the other context if it starts on an earlier line,
            // OR if it starts on the same line but at an earlier column
            // (or if the other context is null)
            return other == null
                || StartLineNumber < other.StartLineNumber
                || (StartLineNumber == other.StartLineNumber && StartColumn < other.StartColumn);
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
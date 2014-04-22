// jstoken.cs
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

namespace Microsoft.NodejsTools.Parsing
{

    public enum JSToken : int
    {
        None = -1,
        EndOfFile,

        // main statement switch
        Semicolon,                      // ;
        RightCurly,                     // }
        LeftCurly,                      // {
        Debugger,
        Var,
        If,
        For,
        Do,
        While,
        Continue,
        Break,
        Return,
        With,
        Switch,
        Throw,
        Try,
        Function,
        Else,

        // used by both statement and expression switches

        // main expression switch
        Null,
        True,
        False,
        This,
        Identifier,
        StringLiteral,
        IntegerLiteral,
        NumericLiteral,

        LeftParenthesis,                // (
        LeftBracket,                    // [
        AccessField,                    // .

        // operators
        FirstOperator,
        // unary ops
        Void = FirstOperator,
        TypeOf,
        Delete,
        Increment,                      // ++
        Decrement,                      // --
        LogicalNot,     // !
        BitwiseNot,                     // ~

        FirstBinaryOperator,
        // binary ops
        Plus = FirstBinaryOperator,     // +
        Minus,                          // -
        Multiply,                       // *
        Divide,                         // /
        Modulo,                         // %
        BitwiseAnd,                     // &
        BitwiseOr,                      // |
        BitwiseXor,                     // ^
        LeftShift,                      // <<
        RightShift,                     // >>
        UnsignedRightShift,             // >>>

        Equal,                          // ==
        NotEqual,                       // !=
        StrictEqual,                    // ===
        StrictNotEqual,                 // !==
        LessThan,                       // <
        LessThanEqual,                  // <=
        GreaterThan,                    // >
        GreaterThanEqual,               // >=

        LogicalAnd,                     // &&
        LogicalOr,                      // ||

        InstanceOf,
        In,
        Comma,                          // ,

        Assign,                         // =
        PlusAssign,                     // +=
        MinusAssign,                    // -=
        MultiplyAssign,                 // *=
        DivideAssign,                   // /=
        ModuloAssign,                   // %=
        BitwiseAndAssign,               // &=
        BitwiseOrAssign,                // |=
        BitwiseXorAssign,               // ^=
        LeftShiftAssign,                // <<=
        RightShiftAssign,               // >>=
        UnsignedRightShiftAssign,       // >>>=
        LastAssign = UnsignedRightShiftAssign,

        ConditionalIf,                  // ? // MUST FOLLOW LastBinaryOp
        Colon,                          // :
        LastOperator = Colon,

        // context specific keywords
        Case,
        Catch,
        Default,
        Finally,
        New,
        RightParenthesis,               // )
        RightBracket,                   // ]
        SingleLineComment,              // for authoring
        MultipleLineComment,            // for authoring
        UnterminatedComment,            // for authoring

        // reserved words
        Class,
        Const,
        Enum,
        Export,
        Extends,
        Import,
        Super,

        // ECMA strict reserved words
        Implements,
        Interface,
        Let,
        Package,
        Private,
        Protected,
        Public,
        Static,
        Yield,

        // always okay for identifiers
        Get,
        Set,

        EndOfLine, // only returned if the RawTokens flag is set on the scanner, but also used in error-recovery
        WhiteSpace, // only returned if the RawTokens flag is set on the scanner
        Error, // only returned if the RawTokens flag is set on the scanner
        RegularExpression, // only returned if the RawTokens flag is set on the scanner

    }
}

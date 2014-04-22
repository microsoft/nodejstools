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

using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
#if FALSE
    class NumericInstanceInfo : BuiltinInstanceInfo {
        public NumericInstanceInfo(BuiltinClassInfo klass)
            : base(klass) {
        }

        public override IAnalysisSet BinaryOperation(Node node, AnalysisUnit unit, JSToken operation, IAnalysisSet rhs) {
            switch (operation) {
                case JSToken.GreaterThan:
                case JSToken.LessThan:
                case JSToken.LessThanEqual:
                case JSToken.GreaterThanEqual:
                case JSToken.Equal:
                case JSToken.NotEqual:
                    return ProjectState.ClassInfos[BuiltinTypeId.Boolean].Instance;
                case JSToken.Plus:
                case JSToken.Minus:
                case JSToken.Multiply:
                case JSToken.Divide:
                case JSToken.Modulo:
                case JSToken.BitwiseAnd:
                case JSToken.BitwiseOr:
                case JSToken.BitwiseXor:
                case JSToken.LeftShift:
                case JSToken.RightShift:
                    return ConstantInfo.NumericOp(node, this, unit, operation, rhs) ?? CallReverseBinaryOp(node, unit, operation, rhs);
            }
            return CallReverseBinaryOp(node, unit, operation, rhs);
        }
    }
#endif
}

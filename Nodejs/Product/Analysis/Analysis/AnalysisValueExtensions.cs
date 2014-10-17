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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Analysis.Values {
    static class AnalysisValueExtensions {
        public static string GetStringValue(this AnalysisValue value) {
            StringValue str = value as StringValue;
            if (str != null) {
                return str._value;
            }
            return null;
        }

        public static double? GetNumberValue(this AnalysisValue value) {
            NumberValue number = value as NumberValue;
            if (number != null) {
                return number._value;
            }
            return null;
        }
    }
}

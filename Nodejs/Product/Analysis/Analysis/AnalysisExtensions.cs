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

namespace Microsoft.NodejsTools.Interpreter {
    public static class AnalysisExtensions {
        /// <summary>
        /// Removes all trailing white space including new lines, tabs, and form feeds.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string TrimDocumentation(this string self) {
            if (self != null) {
                return self.TrimEnd('\n', '\r', ' ', '\f', '\t');
            } 
            return self;
        }
    }
}

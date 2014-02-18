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

using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Formatting {
    /// <summary>
    /// Helper class to marshal .NET array to JavaScript where we can
    /// convert it into a JavaScript array.
    /// </summary>
    [ComVisible(true)]
    public class ArrayHelper {
        public readonly object[] _data;

        internal ArrayHelper(object[] data) {
            _data = data;
        }

        public int length() {
            return _data.Length;
        }

        public object item(int index) {
            return _data[index];
        }
    }
}

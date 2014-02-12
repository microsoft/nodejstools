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

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// A collection of outline regions than match specific text buffer version
    /// </summary>
    class OutlineRegionCollection : TextRangeCollection<OutlineRegion>, ICloneable {
        public int TextBufferVersion { get; internal set; }

        public OutlineRegionCollection(int textBufferVersion) {
            TextBufferVersion = textBufferVersion;
        }

        #region ICloneable
        public virtual object Clone() {
            var clone = new OutlineRegionCollection(TextBufferVersion);

            foreach (var item in this)
                clone.Add(item.Clone() as OutlineRegion);

            return clone;
        }
        #endregion
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// A collection of outline regions than match specific text buffer version
    /// </summary>
    internal class OutlineRegionCollection : TextRangeCollection<OutlineRegion>, ICloneable
    {
        public int TextBufferVersion { get; internal set; }

        public OutlineRegionCollection(int textBufferVersion)
        {
            this.TextBufferVersion = textBufferVersion;
        }

        #region ICloneable
        public virtual object Clone()
        {
            var clone = new OutlineRegionCollection(this.TextBufferVersion);

            foreach (var item in this)
            {
                clone.Add(item.Clone() as OutlineRegion);
            }

            return clone;
        }
        #endregion
    }
}

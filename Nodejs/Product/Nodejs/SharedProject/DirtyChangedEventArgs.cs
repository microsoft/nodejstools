// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudioTools.Project
{
    public sealed class DirtyChangedEventArgs : EventArgs
    {
        private readonly bool _isDirty;
        public static readonly DirtyChangedEventArgs DirtyValue = new DirtyChangedEventArgs(true);
        public static readonly DirtyChangedEventArgs SavedValue = new DirtyChangedEventArgs(false);

        public DirtyChangedEventArgs(bool isDirty)
        {
            this._isDirty = isDirty;
        }

        public bool IsDirty => this._isDirty;
    }
}

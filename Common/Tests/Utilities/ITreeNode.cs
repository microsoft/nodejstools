// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


using System.Windows.Input;

namespace TestUtilities
{
    public interface ITreeNode
    {
        void Select();
        void AddToSelection();

        void DragOntoThis(params ITreeNode[] source);
        void DragOntoThis(Key modifier, params ITreeNode[] source);
    }
}


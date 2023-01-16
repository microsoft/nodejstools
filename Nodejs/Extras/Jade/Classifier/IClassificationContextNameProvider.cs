// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade
{
    internal interface IClassificationContextNameProvider<T>
    {
        IClassificationType GetClassificationType(T t);
    }
}

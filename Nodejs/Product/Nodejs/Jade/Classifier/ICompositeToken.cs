// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace Microsoft.NodejsTools.Jade
{
    internal interface ICompositeToken<TTokenType>
    {
        ReadOnlyCollection<TTokenType> TokenList
        {
            get;
        }
    }
}

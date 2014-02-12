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

namespace Microsoft.NodejsTools {
    /// <summary>
    /// Just used for our MEF import to get the metadata in a strongly
    /// typed way.
    /// </summary>
    sealed class TaggerProviderMetadata {
        public readonly IEnumerable<string> ContentTypes;
        public readonly IEnumerable<Type> TagTypes;

        public TaggerProviderMetadata(IDictionary<string, object> values) {
            ContentTypes = (IEnumerable<string>)values["ContentTypes"];
            TagTypes = (IEnumerable<Type>)values["TagTypes"];
        }
    }
}

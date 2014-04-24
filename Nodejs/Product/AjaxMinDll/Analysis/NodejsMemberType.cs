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

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Indicates the type of a variable result lookup.
    /// 
    /// These types are generally closely tied to the types which exist
    /// in the JavaScript type system, but we can introduce new types 
    /// which may onto higher level concepts.  For example we have a 
    /// concept of a module type which we use for indicating that
    /// the object is a Node.js module.  It can also include concepts
    /// such as keywords which we include in completions.
    /// </summary>
    public enum JsMemberType {
        Unknown,
        Object,
        Undefined,
        Null,
        Boolean,
        Number,
        String,
        Function,

        Module,
        Multiple,
        Keyword,
    }
}

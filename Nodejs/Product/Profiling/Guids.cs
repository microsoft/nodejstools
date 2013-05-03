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

// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.NodejsTools.Profiling
{
    static class GuidList
    {
        public const string guidNodeProfilingPkgString = "9C34161A-379E-4933-A0DC-871FE64D34F1";
        public const string guidNodeProfilingCmdSetString = "3F2BC93C-CA2D-450B-9BFC-0C96288F1ED6";
        public const string guidEditorFactoryString = "3585dc22-81a0-409e-85ae-cae5d02d99cd";

        public static readonly Guid guidNodeProfilingCmdSet = new Guid(guidNodeProfilingCmdSetString);

        public static readonly Guid VsUIHierarchyWindow_guid = new Guid("{7D960B07-7AF8-11D0-8E5E-00A0C911005A}");
        public static readonly Guid guidEditorFactory = new Guid(guidEditorFactoryString);

        public static readonly Guid GuidPerfPkg = new Guid("{F4A63B2A-49AB-4b2d-AA59-A10F01026C89}");
    };
}
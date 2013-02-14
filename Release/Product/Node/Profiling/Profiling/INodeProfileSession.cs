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
using System.Runtime.InteropServices;

namespace Microsoft.NodeTools.Profiling {
    [Guid("7C711031-50B4-4263-901E-9EF86DD6DC57")]
    public interface INodeProfileSession {
        string Name {
            get;
        }

        string Filename {
            get;
        }

        INodePerformanceReport GetReport(object item);

        void Save(string filename = null);

        void Launch(bool openReport = false);

        bool IsSaved {
            get;
        }
    }
}

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

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackageJson{
        string Name { get; }
        SemverVersion Version { get; }
        IScripts Scripts { get; }
        IPerson Author { get; }
        string Description { get; }
        IKeywords Keywords { get; }
        string Homepage { get; }
        IBugs Bugs { get; }
        ILicenses Licenses { get; }
        IFiles Files { get; }
        IMan Man { get; }
        IDependencies Dependencies { get; }
        IDependencies DevDependencies { get; }
        IBundledDependencies BundledDependencies { get; }
        IDependencies OptionalDependencies { get; }
        IDependencies AllDependencies { get; }
    }
}
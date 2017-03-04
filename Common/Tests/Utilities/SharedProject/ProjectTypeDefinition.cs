// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Defines a project type definition, an instance of this gets exported:
    /// 
    /// [Export]
    /// [ProjectExtension(".njsproj")]                            // required
    /// [ProjectTypeGuid("577B58BB-F149-4B31-B005-FC17C8F4CE7C")] // required
    /// [CodeExtension(".js")]                                    // required
    /// [SampleCode("console.log('hi');")]                        // optional
    /// internal static ProjectTypeDefinition ProjectTypeDefinition = new ProjectTypeDefinition();
    /// </summary>
    public sealed class ProjectTypeDefinition
    {
    }
}


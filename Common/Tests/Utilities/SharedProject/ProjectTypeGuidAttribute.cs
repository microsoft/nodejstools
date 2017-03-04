// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Registers the project type guid for a project.  See ProjectTypeDefinition
    /// for how this is used.  This attribute is required.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ProjectTypeGuidAttribute : Attribute
    {
        public readonly string _projectTypeGuid;

        public ProjectTypeGuidAttribute(string projectTypeGuid)
        {
            _projectTypeGuid = projectTypeGuid;
        }

        public string ProjectTypeGuid
        {
            get
            {
                return _projectTypeGuid;
            }
        }
    }
}


// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using TestUtilities.SharedProject;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities
{
    public sealed class SolutionFolder : ISolutionElement
    {
        private readonly string _name;
        private static Guid _solutionFolderGuid = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        public SolutionFolder(string name)
        {
            _name = name;
        }
        public MSBuild.Project Save(MSBuild.ProjectCollection collection, string location)
        {
            Directory.CreateDirectory(Path.Combine(location, _name));
            return null;
        }

        public Guid TypeGuid
        {
            get { return _solutionFolderGuid; }
        }

        public SolutionElementFlags Flags
        {
            get { return SolutionElementFlags.ExcludeFromConfiguration; }
        }

        public string Name { get { return _name; } }
    }
}


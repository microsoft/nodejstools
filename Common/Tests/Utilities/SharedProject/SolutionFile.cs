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
using System.IO;
using System.Text;
using Microsoft.VisualStudioTools;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject {
    /// <summary>
    /// Represents a generic solution which can be generated for shared project tests based upon
    /// the language which is being tested.
    /// 
    /// Call Solution.Generate to write the solution out to disk and return an IDisposable object
    /// which when disposed will clean up the solution.
    /// 
    /// You can also get a SolutionFile by calling ProjectDefinition.Generate which will create
    /// a single project SolutionFile.
    /// </summary>
    public sealed class SolutionFile : IDisposable {
        public readonly string Filename;

        private SolutionFile(string slnFilename) {
            Filename = slnFilename;
        }

        public static SolutionFile Generate(string solutionName, params ProjectDefinition[] toGenerate) {
            List<MSBuild.Project> projects = new List<MSBuild.Project>();
            var location = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(location);

            foreach (var project in toGenerate) {
                projects.Add(project.ProjectType.Generate(location, project.Name, project.Items));
            }

#if DEV10
            StringBuilder slnFile = new StringBuilder(@"
Microsoft Visual Studio Solution File, Format Version 11.00
\u0023 Visual Studio 2010
");
#elif DEV11
            StringBuilder slnFile = new StringBuilder(@"
Microsoft Visual Studio Solution File, Format Version 12.00
\u0023 Visual Studio 2012
");
#elif DEV12
            StringBuilder slnFile = new StringBuilder(@"
Microsoft Visual Studio Solution File, Format Version 12.00
\u0023 Visual Studio 2013
VisualStudioVersion = 12.0.20827.3
MinimumVisualStudioVersion = 10.0.40219.1
");
#else
#error Unsupported VS version
#endif
            for (int i = 0; i < projects.Count; i++) {
                var project = projects[i];
                var kind = toGenerate[i].ProjectType;

                slnFile.AppendFormat(@"Project(""{0:B}"") = ""{1}"", ""{2}"", ""{3:B}""
EndProject
", kind.ProjectTypeGuid,
 Path.GetFileNameWithoutExtension(project.FullPath),
 CommonUtils.GetRelativeFilePath(location, project.FullPath),
 Guid.Parse(project.GetProperty("ProjectGuid").EvaluatedValue));
            }
            slnFile.Append(@"Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
");
            foreach (var project in projects) {
                slnFile.AppendFormat(@"		{0:B}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{0:B}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{0:B}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{0:B}.Release|Any CPU.Build.0 = Release|Any CPU
", Guid.Parse(project.GetProperty("ProjectGuid").EvaluatedValue));
            }

            slnFile.Append(@"	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

            var slnFilename = Path.Combine(location, solutionName + ".sln");
            File.WriteAllText(slnFilename, slnFile.ToString(), Encoding.UTF8);
            return new SolutionFile(slnFilename);
        }

        #region IDisposable Members

        public void Dispose() {
            try {
                Directory.Delete(Path.GetDirectoryName(Filename), true);
            } catch {
            }
        }

        #endregion
    }
}

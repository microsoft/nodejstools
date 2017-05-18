// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockVsBuildManagerAccessor : IVsBuildManagerAccessor
    {
        public int BeginDesignTimeBuild()
        {
            BuildParameters buildParameters = new BuildParameters(MSBuild.ProjectCollection.GlobalProjectCollection);
            BuildManager.DefaultBuildManager.BeginBuild(buildParameters);
            return VSConstants.S_OK;
        }

        public int ClaimUIThreadForBuild()
        {
            return VSConstants.S_OK;
        }

        public int EndDesignTimeBuild()
        {
            BuildManager.DefaultBuildManager.EndBuild();
            return VSConstants.S_OK;
        }

        public int Escape(string pwszUnescapedValue, out string pbstrEscapedValue)
        {
            throw new NotImplementedException();
        }

        public int GetCurrentBatchBuildId(out uint pBatchId)
        {
            throw new NotImplementedException();
        }

        public int GetSolutionConfiguration(object punkRootProject, out string pbstrXmlFragment)
        {
            throw new NotImplementedException();
        }

        public int RegisterLogger(int submissionId, object punkLogger)
        {
            return VSConstants.S_OK;
        }

        public int ReleaseUIThreadForBuild()
        {
            return VSConstants.S_OK;
        }

        public int Unescape(string pwszEscapedValue, out string pbstrUnescapedValue)
        {
            throw new NotImplementedException();
        }

        public int UnregisterLoggers(int submissionId)
        {
            return VSConstants.S_OK;
        }
    }
}


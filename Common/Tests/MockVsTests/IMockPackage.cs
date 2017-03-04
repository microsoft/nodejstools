// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


using System;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    /// <summary>
    /// Performs initialization of a mock VS package.
    /// 
    /// Initializing a real MPF Package class inside of MockVs is not actually possible  
    /// 
    /// Despite using siting, MPF actually goes off to global service providers for various
    /// activities.  For example it uses the ActivityLog class which does not get properly
    /// sited.  
    /// 
    /// To use MockVs packages should abstract most of the code from their package into an
    /// independent service and have their package publish (and promote) their service.  Mock
    /// packages can then do the same thing.
    /// </summary>
    public interface IMockPackage : IDisposable
    {
        void Initialize();
    }
}


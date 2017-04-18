// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;

// If you get compiler errors CS0579, "Duplicate '<attributename>' attribute", check your
// Properties\AssemblyInfo.cs file and remove any lines duplicating the ones below.
// (See also AssemblyInfoCommon.cs in this same directory.)

#if !SUPPRESS_COMMON_ASSEMBLY_VERSION 
[assembly: AssemblyVersion(AssemblyVersionInfo.StableVersion)]
#endif
[assembly: AssemblyFileVersion(AssemblyVersionInfo.Version)]

internal class AssemblyVersionInfo
{
    // This version string (and the comment for StableVersion) should be
    // updated manually between major releases (e.g. from 1.0 to 2.0).
    // Servicing branches and minor releases should retain the value.
    public const string ReleaseVersion = "1.0";

    // This version string (and the comment for Version) should be updated
    // manually between minor releases (e.g. from 1.0 to 1.1).
    // Servicing branches and prereleases should retain the value.
    public const string FileVersion = "1.4";

    // This version should never change from "4100.00"; BuildRelease.ps1
    // will replace it with a generated value.
    public const string BuildNumber = "4100.00";

    public const string VSMajorVersion = "15";
    private const string VSVersionSuffix = "2017";

    public const string VSVersion = VSMajorVersion + ".0";

    // Defaults to "1.0.0.(2012|2013|2015)"
    public const string StableVersion = ReleaseVersion + ".0." + VSVersionSuffix;

    // Defaults to "1.3.4100.00"
    public const string Version = FileVersion + "." + BuildNumber;
}


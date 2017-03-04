// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// The following assembly information is common to all VisualStudioTools Test assemblies.
// If you get compiler errors CS0579, "Duplicate '<attributename>' attribute", check your 
// Properties\AssemblyInfo.cs file and remove any lines duplicating the ones below.
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Tools for Visual Studio")]
[assembly: AssemblyCopyright("Copyright \u00A9 Microsoft")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion(AssemblyVersionInfo.StableVersion)]
[assembly: AssemblyFileVersion(AssemblyVersionInfo.Version)]

internal class AssemblyVersionInfo
{
    // This version string (and the comments for StableVersion and Version)
    // should be updated manually between major releases.
    // Servicing branches should retain the value
    public const string ReleaseVersion = "1.0";
    // This version string (and the comment for StableVersion) should be
    // updated manually between minor releases.
    // Servicing branches should retain the value
    public const string MinorVersion = "0";

    public const string BuildNumber = "0.00";

    public const string VSMajorVersion = "15";
    private const string VSVersionSuffix = "2017";

    public const string VSVersion = VSMajorVersion + ".0";

    // Defaults to "1.0.0.(2010|2012|2013)"
    public const string StableVersion = ReleaseVersion + "." + MinorVersion + "." + VSVersionSuffix;

    // Defaults to "1.0.0.00"
    public const string Version = ReleaseVersion + "." + BuildNumber;
}
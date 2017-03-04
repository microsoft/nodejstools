// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.NodejsTools.Npm
{
    public static class NpmArgumentBuilder
    {
        public static string GetNpmInstallArguments(string packageName,
            string versionRange,
            DependencyType type,
            bool global = false,
            bool saveToPackageJson = true,
            string otherArguments = "")
        {
            string dependencyArguments = "";
            if (global)
            {
                dependencyArguments = "-g";
            }
            else if (saveToPackageJson)
            {
                switch (type)
                {
                    case DependencyType.Standard:
                        dependencyArguments = "--save";
                        break;
                    case DependencyType.Development:
                        dependencyArguments = "--save-dev";
                        break;
                    case DependencyType.Optional:
                        dependencyArguments = "--save-optional";
                        break;
                }
            }

            otherArguments = otherArguments.TrimStart(' ', '\t');
            if (otherArguments.StartsWith("@", StringComparison.Ordinal))
            {
                return string.Format(CultureInfo.InvariantCulture, "install {0}{1} {2}", packageName, otherArguments, dependencyArguments);
            }
            else if (!string.IsNullOrEmpty(versionRange))
            {
                return string.Format(CultureInfo.InvariantCulture, "install {0}@\"{1}\" {2} {3}", packageName, versionRange, dependencyArguments, otherArguments);
            }

            return string.Format(CultureInfo.InvariantCulture, "install {0} {1} {2}", packageName, dependencyArguments, otherArguments);
        }
    }
}


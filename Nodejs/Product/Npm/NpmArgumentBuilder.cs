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

namespace Microsoft.NodejsTools.Npm {
    public static class NpmArgumentBuilder {
        public static string GetNpmInstallArguments(string packageName,
            string versionRange,
            DependencyType type,
            bool global = false,
            bool saveToPackageJson = true,
            string otherArguments = "") 
        {
            string dependencyArguments = "";
            if (global) {
                dependencyArguments = "-g";
            } else if (saveToPackageJson) {
                switch(type) {
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
            if (otherArguments.StartsWith("@")) {
                return string.Format("install {0}{1} {2}", packageName, otherArguments, dependencyArguments);
            } else if (!string.IsNullOrEmpty(versionRange)) {
                return string.Format("install {0}@\"{1}\" {2} {3}", packageName, versionRange, dependencyArguments, otherArguments);
            }

            return string.Format("install {0} {1} {2}", packageName, dependencyArguments, otherArguments);
        }
    }
}

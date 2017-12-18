// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.TypeScript
{
    // Copied from VS/LanguageService/TypeScriptLanguageService/Versions/LanguageServiceVersionLocator.cs
    // in the TypeScript repo. 
    // TODO: figure out a better way to share functionality between the TypeScript repo and our components

    internal static class TypeScriptCompilerLocator
    {
        private static Lazy<IReadOnlyDictionary<Version, string>> orderedAvailableVersions = new Lazy<IReadOnlyDictionary<Version, string>>(() =>
        {
            return GetAvailableTypeScriptVersionsFromDisk();
        });

        public static string GetDefaultVersion()
        {
            var latest = orderedAvailableVersions.Value.Keys.Max();

            return orderedAvailableVersions.Value[latest];
        }

        public static bool TryGetPreferredVersion(Version version, out string path)
        {
            return orderedAvailableVersions.Value.TryGetValue(version, out path);
        }

        private static IReadOnlyDictionary<Version, string> GetAvailableTypeScriptVersionsFromDisk()
        {
            var directories = Directory.GetDirectories(GetSdkRoot());

            var dictionary = new Dictionary<Version, string>();
            foreach (var dir in directories)
            {
                if (Version.TryParse(Path.GetFileName(dir), out Version version))
                {
                    dictionary.Add(version, dir);
                }
            }

            return dictionary;
        }

        private static string GetSdkRoot()
        {
            return Path.Combine(GenerateProgramFiles32(), @"Microsoft SDKs\TypeScript");
        }

        private static string GenerateProgramFiles32()
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(programFilesX86))
            {
                programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            return programFilesX86;
        }
    }
}

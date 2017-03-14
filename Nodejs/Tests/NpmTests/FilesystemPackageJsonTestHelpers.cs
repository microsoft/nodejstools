// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;

namespace NpmTests
{
    public static class FilesystemPackageJsonTestHelpers
    {
        public static void CreatePackageJson(string filename, string json)
        {
            using (var fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(fout))
                {
                    writer.Write(json);
                }
            }
        }

        public static string CreateRootPackageDir(TemporaryFileManager manager)
        {
            return manager.GetNewTempDirectory().FullName;
        }

        public static string CreateRootPackage(TemporaryFileManager manager, string json)
        {
            var dir = CreateRootPackageDir(manager);
            var path = Path.Combine(dir, "package.json");
            CreatePackageJson(path, json);
            return dir;
        }
    }
}


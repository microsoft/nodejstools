// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
#if !NO_WINDOWS
using System.Windows.Forms;
#endif
using Microsoft.Win32;
#if !NO_WINDOWS
using Microsoft.NodejsTools.Project;
#endif
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools
{
    public static class Nodejs
    {
        private const string NodejsRegPath = "Software\\Node.js";
        private const string InstallPath = "InstallPath";

        public static string NodeExePath => GetPathToNodeExecutableFromEnvironment();

        public static string GetAbsoluteNodeExePath(string root, string relativePath)
        {
            var overridePath = CommonUtils.UnquotePath(relativePath ?? string.Empty);
            if (!String.IsNullOrWhiteSpace(overridePath))
            {
                if (string.IsNullOrWhiteSpace(root))
                {
                    return relativePath;
                }
                try
                {
                    return CommonUtils.GetAbsoluteFilePath(root, overridePath);
                }
                catch (InvalidOperationException)
                {
                    return relativePath;
                }
            }
            return NodeExePath;
        }

        public static string GetPathToNodeExecutableFromEnvironment(string executable = "node.exe")
        {
            // Attempt to find Node.js/NPM in the Registry. (Currrent User)
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            using (var node = baseKey.OpenSubKey(NodejsRegPath))
            {
                if (node != null)
                {
                    var key = (node.GetValue(InstallPath) as string) ?? string.Empty;
                    var execPath = Path.Combine(key, executable);
                    if (File.Exists(execPath))
                    {
                        return execPath;
                    }
                }
            }

            // Attempt to find Node.js/NPM in the Registry. (Local Machine x64)
            if (Environment.Is64BitOperatingSystem)
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var node64 = baseKey.OpenSubKey(NodejsRegPath))
                {
                    if (node64 != null)
                    {
                        var key = (node64.GetValue(InstallPath) as string) ?? string.Empty;
                        var execPath = Path.Combine(key, executable);
                        if (File.Exists(execPath))
                        {
                            return execPath;
                        }
                    }
                }
            }

            // Attempt to find Node.js/NPM in the Registry. (Local Machine x86)
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var node = baseKey.OpenSubKey(NodejsRegPath))
            {
                if (node != null)
                {
                    var key = (node.GetValue(InstallPath) as string) ?? string.Empty;
                    var execPath = Path.Combine(key, executable);
                    if (File.Exists(execPath))
                    {
                        return execPath;
                    }
                }
            }

            // If we didn't find node.js in the registry we should look at the user's path.
            foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
            {
                try
                {
                    var execPath = Path.Combine(dir, executable);
                    if (File.Exists(execPath))
                    {
                        return execPath;
                    }
                }
                catch (ArgumentException) { /*noop*/ }
            }

            // It wasn't in the users path.  Check Program Files for the nodejs folder.
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", executable);
            if (File.Exists(path))
            {
                return path;
            }

            // It wasn't in the users path.  Check Program Files x86 for the nodejs folder.
            var x86path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!String.IsNullOrEmpty(x86path))
            {
                path = Path.Combine(x86path, "nodejs", executable);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // we didn't find the path.
            return null;
        }

#if !NO_WINDOWS
        public static void ShowNodejsNotInstalled()
        {
            MessageBox.Show(
                SR.GetString(SR.NodejsNotInstalled),
                SR.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        public static void ShowNodejsPathNotFound(string path)
        {
            MessageBox.Show(
                SR.GetString(SR.NodeExeDoesntExist, path),
                SR.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
#endif
    }
}


// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.Win32;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudioTools
{
    internal static class SettingsManagerCreator
    {
        private const string VSVersion = "16.0";

        public static SettingsManager GetSettingsManager(DTE dte)
        {
            return GetSettingsManager(new ServiceProvider(((IOleServiceProvider)dte)));
        }

        public static SettingsManager GetSettingsManager(IServiceProvider provider)
        {
            SettingsManager settings = null;
            string devenvPath = null;
            if (provider == null)
            {
                provider = ServiceProvider.GlobalProvider;
            }

            if (provider != null)
            {
                try
                {
                    settings = new ShellSettingsManager(provider);
                }
                catch (NotSupportedException)
                {
                    var dte = (DTE)provider.GetService(typeof(DTE));
                    if (dte != null)
                    {
                        devenvPath = dte.FullName;
                    }
                }
            }

            if (settings == null)
            {
                if (!File.Exists(devenvPath))
                {
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    using (var key = root.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"Software\Microsoft\VisualStudio\{0}\Setup\VS", VSVersion)))
                    {
                        if (key == null)
                        {
                            throw new InvalidOperationException("Cannot find settings store for Visual Studio " + VSVersion);
                        }
                        devenvPath = key.GetValue("EnvironmentPath") as string;
                    }
                }
                if (!File.Exists(devenvPath))
                {
                    throw new InvalidOperationException("Cannot find settings store for Visual Studio " + VSVersion);
                }
#if DEBUG
                settings = ExternalSettingsManager.CreateForApplication(devenvPath, "Exp");
#else
                settings = ExternalSettingsManager.CreateForApplication(devenvPath);
#endif
            }

            return settings;
        }
    }
}

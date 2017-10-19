// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Options
{
    /// <summary>
    /// Base class used for saving/loading of settings.  The settings are stored in VSRegistryRoot\NodejsTools\Options\Category\SettingName
    /// where Category is provided in the constructor and SettingName is provided to each call of the Save*/Load* APIs.
    /// x = 42
    /// 
    /// The primary purpose of this class is so that we can be in control of providing reasonable default values.
    /// </summary>
    [ComVisible(true)]
    public class NodejsDialogPage : DialogPage
    {
        private readonly string _category;
        private const string _optionsKey = "Options";

        internal NodejsDialogPage(string category)
        {
            this._category = category;
        }

        internal virtual void SaveBool(string name, bool value)
        {
            SaveString(name, value.ToString());
        }

        internal void SaveInt(string name, int value)
        {
            SaveString(name, value.ToString());
        }

        internal void SaveString(string name, string value)
        {
            SaveString(name, value, this._category);
        }

        internal static void SaveString(string name, string value, string cat)
        {
            using (var pythonKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(NodejsConstants.BaseRegistryKey))
            {
                using (var optionsKey = pythonKey.CreateSubKey(_optionsKey))
                {
                    using (var categoryKey = optionsKey.CreateSubKey(cat))
                    {
                        categoryKey.SetValue(name, value, Win32.RegistryValueKind.String);
                    }
                }
            }
        }

        internal void SaveEnum<T>(string name, T value) where T : struct
        {
            SaveString(name, value.ToString());
        }

        internal void SaveDateTime(string name, DateTime value)
        {
            SaveString(name, value.ToString(CultureInfo.InvariantCulture));
        }

        internal int? LoadInt(string name)
        {
            var res = LoadString(name);
            if (res == null)
            {
                return null;
            }

            if (int.TryParse(res, out var val))
            {
                return val;
            }
            return null;
        }

        internal virtual bool? LoadBool(string name)
        {
            var res = LoadString(name);
            if (res == null)
            {
                return null;
            }

            if (bool.TryParse(res, out var val))
            {
                return val;
            }
            return null;
        }

        internal string LoadString(string name)
        {
            return LoadString(name, this._category);
        }

        internal static string LoadString(string name, string cat)
        {
            using (var nodeKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(NodejsConstants.BaseRegistryKey))
            {
                using (var optionsKey = nodeKey.CreateSubKey(_optionsKey))
                {
                    using (var categoryKey = optionsKey.CreateSubKey(cat))
                    {
                        return categoryKey.GetValue(name) as string;
                    }
                }
            }
        }

        internal T? LoadEnum<T>(string name) where T : struct
        {
            var res = LoadString(name);
            if (res == null)
            {
                return null;
            }

            if (Enum.TryParse<T>(res, out var enumRes))
            {
                return enumRes;
            }
            return null;
        }

        internal DateTime? LoadDateTime(string name)
        {
            var res = LoadString(name);
            if (res == null)
            {
                return null;
            }

            if (DateTime.TryParse(res, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateRes))
            {
                return dateRes;
            }
            return null;
        }
    }
}

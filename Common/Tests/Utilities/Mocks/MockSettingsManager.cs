// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestUtilities.Mocks
{
    [ComVisible(true)]
    public class MockSettingsManager : IVsSettingsManager
    {
        public readonly MockSettingsStore Store = new MockSettingsStore();

        public int GetApplicationDataFolder(uint folder, out string folderPath)
        {
            throw new NotImplementedException();
        }

        public int GetCollectionScopes(string collectionPath, out uint scopes)
        {
            throw new NotImplementedException();
        }

        public int GetCommonExtensionsSearchPaths(uint paths, string[] commonExtensionsPaths, out uint actualPaths)
        {
            throw new NotImplementedException();
        }

        public int GetPropertyScopes(string collectionPath, string propertyName, out uint scopes)
        {
            throw new NotImplementedException();
        }

        public int GetReadOnlySettingsStore(uint scope, out IVsSettingsStore store)
        {
            store = Store;
            return VSConstants.S_OK;
        }

        public int GetWritableSettingsStore(uint scope, out IVsWritableSettingsStore writableStore)
        {
            writableStore = Store;
            return VSConstants.S_OK;
        }
    }
}


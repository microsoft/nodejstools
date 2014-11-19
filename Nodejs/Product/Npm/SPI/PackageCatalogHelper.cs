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

using System.Collections.Generic;
using Microsoft.NodejsTools.Npm.SQLiteTables;
using Microsoft.Win32;
using Newtonsoft.Json;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal static class PackageCatalogHelper {
        public static void CreateCatalogTableIfNotExists(this SQLiteConnection db) {
            db.CreateTable<DbVersion>();
            db.CreateTable<RegistryFileMapping>();
        }

        public static void CreateRegistryTableIfNotExists(this SQLiteConnection db) {
            // Create virtual table for FTS
            db.Execute("CREATE VIRTUAL TABLE IF NOT EXISTS CatalogEntry USING FTS4(Name, Description, Keywords, Homepage, Version, AvailableVersions, Author, PublishDateTimeString)");

            db.CreateTable<RegistryInfo>();
        }

        public static IPackage ToPackage(this CatalogEntry entry) {
            return new PackageProxy() {
                Name = entry.Name,
                Description = entry.Description,
                Author = JsonConvert.DeserializeObject<Person>(entry.Author),
                Keywords = JsonConvert.DeserializeObject<IEnumerable<string>>(entry.Keywords) ?? new List<string>(),
                Version = JsonConvert.DeserializeObject<SemverVersion>(entry.Version),
                AvailableVersions = JsonConvert.DeserializeObject<IEnumerable<SemverVersion>>(entry.AvailableVersions),
                Homepages = JsonConvert.DeserializeObject<IEnumerable<string>>(entry.Homepage) ?? new List<string>(),
                PublishDateTimeString = entry.PublishDateTimeString
            };
        }
    }
}

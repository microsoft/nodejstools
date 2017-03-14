// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.NodejsTools.Npm.SQLiteTables;
using Newtonsoft.Json;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal static class PackageCatalogHelper
    {
        public static void CreateCatalogTableIfNotExists(this SQLiteConnection db)
        {
            db.CreateTable<DbVersion>();
            db.CreateTable<RegistryFileMapping>();
        }

        public static void CreateRegistryTableIfNotExists(this SQLiteConnection db)
        {
            // Create virtual table for FTS
            db.Execute("CREATE VIRTUAL TABLE IF NOT EXISTS CatalogEntry USING FTS4(Name, Description, Keywords, Homepage, Version, AvailableVersions, Author, PublishDateTimeString)");

            db.CreateTable<RegistryInfo>();
        }

        public static IPackage ToPackage(this CatalogEntry entry)
        {
            return new PackageProxy()
            {
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


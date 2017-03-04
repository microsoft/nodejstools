// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SQLiteTables
{
    public class CatalogEntry
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Homepage { get; set; }

        public string Version { get; set; }

        public string AvailableVersions { get; set; }

        public string Author { get; set; }

        public string Keywords { get; set; }

        public string PublishDateTimeString { get; set; }
    }

    public class DbVersion
    {
        [PrimaryKey, NotNull]
        public int Id { get; set; }
    }

    public class RegistryFileMapping
    {
        [PrimaryKey, NotNull, Indexed]
        public string RegistryUrl { get; set; }

        [NotNull, Unique]
        public string DbFileLocation { get; set; }
    }

    public class RegistryInfo
    {
        [PrimaryKey, NotNull, Indexed]
        public string RegistryUrl { get; set; }

        [NotNull, Unique]
        public long Revision { get; set; }

        [NotNull]
        public DateTime UpdatedOn { get; set; }
    }
}


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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SQLiteTables {
    public class CatalogEntry {
        [PrimaryKey, NotNull, Indexed]
        public string Name { get; set; }

        [MaxLength(500)]
        public  string Description { get; set; }

        public  string Homepage { get; set; }

        public string Version { get; set; }

        public string Author { get; set; }

        public string Keywords { get; set; }

        public string PublishDateTimeString { get; set; }
    }

    public class DbVersion {
        [PrimaryKey]
        [NotNull]
        public int Id { get; set; }

        [NotNull, Unique]
        public long Revision { get; set; }

        [NotNull]
        public DateTime UpdatedOn { get; set; }
    } 
}

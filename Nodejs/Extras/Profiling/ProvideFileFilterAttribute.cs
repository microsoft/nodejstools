// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    internal class ProvideFileFilterAttribute : RegistrationAttribute
    {
        private readonly string _id, _name, _filter;
        private readonly int _sortPriority;

        public ProvideFileFilterAttribute(string projectGuid, string name, string filter, int sortPriority)
        {
            _name = name;
            _id = projectGuid;
            _filter = filter;
            _sortPriority = sortPriority;
        }

        public override void Register(RegistrationContext context)
        {
            using (var engineKey = context.CreateKey("Projects\\" + _id + "\\Filters\\" + _name))
            {
                engineKey.SetValue(String.Empty, _filter);
                engineKey.SetValue("SortPriority", _sortPriority);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}


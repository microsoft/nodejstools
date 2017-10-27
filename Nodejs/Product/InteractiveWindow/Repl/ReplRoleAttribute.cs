// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Represents an interactive window role.
    /// 
    /// This attribute is a MEF contract and can be used to associate a REPL provider with its commands.
    /// This is new in 1.5.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ReplRoleAttribute : Attribute
    {
        public readonly string Name;

        public ReplRoleAttribute(string name)
        {
            if( name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Contains(","))
            {
                throw new ArgumentException("ReplRoleAttribute name cannot contain any commas. Apply multiple attributes if you want to support multiple roles.", nameof(name));
            }

            this.Name = name;
        }
    }
}

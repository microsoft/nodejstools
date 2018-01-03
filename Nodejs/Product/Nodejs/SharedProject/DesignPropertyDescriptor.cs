// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// The purpose of DesignPropertyDescriptor is to allow us to customize the
    /// display name of the property in the property grid.  None of the CLR
    /// implementations of PropertyDescriptor allow you to change the DisplayName.
    /// </summary>
    public class DesignPropertyDescriptor : PropertyDescriptor
    {
        private string displayName; // Custom display name
        private PropertyDescriptor property; // Base property descriptor
        private Hashtable editors = new Hashtable(); // Type -> editor instance
        private TypeConverter converter;

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override string DisplayName => this.displayName;

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override Type ComponentType => this.property.ComponentType;

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override bool IsReadOnly => this.property.IsReadOnly;

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override Type PropertyType => this.property.PropertyType;

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override object GetEditor(Type editorBaseType)
        {
            var editor = this.editors[editorBaseType];
            if (editor == null)
            {
                for (var i = 0; i < this.Attributes.Count; i++)
                {
                    var attr = this.Attributes[i] as EditorAttribute;
                    if (attr == null)
                    {
                        continue;
                    }
                    var editorType = Type.GetType(attr.EditorBaseTypeName);
                    if (editorBaseType == editorType)
                    {
                        var type = GetTypeFromNameProperty(attr.EditorTypeName);
                        if (type != null)
                        {
                            editor = CreateInstance(type);
                            this.editors[type] = editor; // cache it
                            break;
                        }
                    }
                }
            }
            return editor;
        }

        /// <summary>
        /// Return type converter for property
        /// </summary>
        public override TypeConverter Converter
        {
            get
            {
                if (this.converter == null)
                {
                    this.converter = this.property.Converter;
                }
                return this.converter;
            }
        }

        /// <summary>
        /// Convert name to a Type object.
        /// </summary>
        public virtual Type GetTypeFromNameProperty(string typeName)
        {
            return Type.GetType(typeName);
        }

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override bool CanResetValue(object component)
        {
            var result = this.property.CanResetValue(component);
            return result;
        }

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override object GetValue(object component)
        {
            var value = this.property.GetValue(component);
            return value;
        }

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override void ResetValue(object component)
        {
            this.property.ResetValue(component);
        }

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override void SetValue(object component, object value)
        {
            this.property.SetValue(component, value);
        }

        /// <summary>
        /// Delegates to base.
        /// </summary>
        public override bool ShouldSerializeValue(object component)
        {
            // If the user has set the AlwaysSerializedAttribute, do not attempt to bold.
            if (this.property.ComponentType.GetProperty(this.property.Name).IsDefined(typeof(AlwaysSerializedAttribute)))
            {
                return false;
            }
            else
            {
                var result = this.property.ShouldSerializeValue(component);
                return result;
            }
        }

        /// <summary>
        /// Constructor.  Copy the base property descriptor and also hold a pointer
        /// to it for calling its overridden abstract methods.
        /// </summary>
        public DesignPropertyDescriptor(PropertyDescriptor prop)
            : base(prop)
        {
            Utilities.ArgumentNotNull(nameof(prop), prop);

            this.property = prop;

            var attr = prop.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;

            if (attr != null)
            {
                this.displayName = attr.DisplayName;
            }
            else
            {
                this.displayName = prop.Name;
            }
        }
    }
}

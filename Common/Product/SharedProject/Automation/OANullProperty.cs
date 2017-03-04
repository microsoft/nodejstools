// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// This object defines a so called null object that is returned as instead of null. This is because callers in VSCore usually crash if a null propery is returned for them.
    /// </summary>
    [ComVisible(true)]
    public class OANullProperty : EnvDTE.Property
    {
        #region fields
        private OAProperties parent;
        #endregion

        #region ctors

        public OANullProperty(OAProperties parent)
        {
            this.parent = parent;
        }
        #endregion

        #region EnvDTE.Property

        public object Application => String.Empty;
        public EnvDTE.Properties Collection =>
                //todo: EnvDTE.Property.Collection
                this.parent;

        public EnvDTE.DTE DTE => null;
        public object get_IndexedValue(object index1, object index2, object index3, object index4)
        {
            return String.Empty;
        }

        public void let_Value(object value)
        {
            //todo: let_Value
        }

        public string Name => String.Empty;
        public short NumIndices => 0;
        public object Object
        {
            get { return this.parent.Target; }
            set
            {
            }
        }

        public EnvDTE.Properties Parent => this.parent;
        public void set_IndexedValue(object index1, object index2, object index3, object index4, object value)
        {
        }

        public object Value
        {
            get { return String.Empty; }
            set { }
        }
        #endregion
    }
}


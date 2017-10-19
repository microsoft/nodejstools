// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal class HierarchyEventArgs : EventArgs
    {
        private uint _itemId;
        private string _fileName;
        private IVsTextLines _buffer;

        public HierarchyEventArgs(uint itemId, string canonicalName)
        {
            this._itemId = itemId;
            this._fileName = canonicalName;
        }
        public string CanonicalName => this._fileName; public uint ItemID => this._itemId; public IVsTextLines TextBuffer
        {
            get { return this._buffer; }
            set { this._buffer = value; }
        }
    }

    internal abstract partial class LibraryManager : IDisposable, IVsRunningDocTableEvents
    {
        internal class HierarchyListener : IVsHierarchyEvents, IDisposable
        {
            private IVsHierarchy _hierarchy;
            private uint _cookie;
            private LibraryManager _manager;

            public HierarchyListener(IVsHierarchy hierarchy, LibraryManager manager)
            {
                Utilities.ArgumentNotNull("hierarchy", hierarchy);
                Utilities.ArgumentNotNull("manager", manager);

                this._hierarchy = hierarchy;
                this._manager = manager;
            }

            protected IVsHierarchy Hierarchy => this._hierarchy;
            #region Public Methods
            public bool IsListening => (0 != this._cookie); public void StartListening(bool doInitialScan)
            {
                if (0 != this._cookie)
                {
                    return;
                }
                ErrorHandler.ThrowOnFailure(
                    this._hierarchy.AdviseHierarchyEvents(this, out this._cookie));
                if (doInitialScan)
                {
                    InternalScanHierarchy(VSConstants.VSITEMID_ROOT);
                }
            }
            public void StopListening()
            {
                InternalStopListening(true);
            }
            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                InternalStopListening(false);
                this._cookie = 0;
                this._hierarchy = null;
            }

            #endregion
            #region IVsHierarchyEvents Members

            public int OnInvalidateIcon(IntPtr hicon)
            {
                // Do Nothing.
                return VSConstants.S_OK;
            }

            public int OnInvalidateItems(uint itemidParent)
            {
                // TODO: Find out if this event is needed.
                return VSConstants.S_OK;
            }

            public int OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded)
            {
                // Check if the item is my language file.
                if (!IsAnalyzableSource(itemidAdded, out var name))
                {
                    return VSConstants.S_OK;
                }

                // This item is a my language file, so we can notify that it is added to the hierarchy.
                var args = new HierarchyEventArgs(itemidAdded, name);
                this._manager.OnNewFile(this._hierarchy, args);
                return VSConstants.S_OK;
            }

            public int OnItemDeleted(uint itemid)
            {
                // Notify that the item is deleted only if it is my language file.
                if (!IsAnalyzableSource(itemid, out var name))
                {
                    return VSConstants.S_OK;
                }
                var args = new HierarchyEventArgs(itemid, name);
                this._manager.OnDeleteFile(this._hierarchy, args);
                return VSConstants.S_OK;
            }

            public int OnItemsAppended(uint itemidParent)
            {
                // TODO: Find out what this event is about.
                return VSConstants.S_OK;
            }

            public int OnPropertyChanged(uint itemid, int propid, uint flags)
            {
                if ((null == this._hierarchy) || (0 == this._cookie))
                {
                    return VSConstants.S_OK;
                }
                if (!IsAnalyzableSource(itemid, out var name))
                {
                    return VSConstants.S_OK;
                }
                if (propid == (int)__VSHPROPID.VSHPROPID_IsNonMemberItem)
                {
                    this._manager.IsNonMemberItemChanged(this._hierarchy, new HierarchyEventArgs(itemid, name));
                }
                return VSConstants.S_OK;
            }
            #endregion

            private bool InternalStopListening(bool throwOnError)
            {
                if ((null == this._hierarchy) || (0 == this._cookie))
                {
                    return false;
                }
                var hr = this._hierarchy.UnadviseHierarchyEvents(this._cookie);
                if (throwOnError)
                {
                    ErrorHandler.ThrowOnFailure(hr);
                }
                this._cookie = 0;
                return ErrorHandler.Succeeded(hr);
            }

            /// <summary>
            /// Do a recursive walk on the hierarchy to find all this language files in it.
            /// It will generate an event for every file found.
            /// </summary>
            private void InternalScanHierarchy(uint itemId)
            {
                var currentItem = itemId;
                while (VSConstants.VSITEMID_NIL != currentItem)
                {
                    // If this item is a my language file, then send the add item event.
                    if (IsAnalyzableSource(currentItem, out var itemName))
                    {
                        var args = new HierarchyEventArgs(currentItem, itemName);
                        this._manager.OnNewFile(this._hierarchy, args);
                    }

                    // NOTE: At the moment we skip the nested hierarchies, so here  we look for the 
                    // children of this node.
                    // Before looking at the children we have to make sure that the enumeration has not
                    // side effects to avoid unexpected behavior.
                    var canScanSubitems = true;
                    var hr = this._hierarchy.GetProperty(currentItem, (int)__VSHPROPID.VSHPROPID_HasEnumerationSideEffects, out var propertyValue);
                    if ((VSConstants.S_OK == hr) && (propertyValue is bool))
                    {
                        canScanSubitems = !(bool)propertyValue;
                    }
                    // If it is allow to look at the sub-items of the current one, lets do it.
                    if (canScanSubitems)
                    {
                        hr = this._hierarchy.GetProperty(currentItem, (int)__VSHPROPID.VSHPROPID_FirstChild, out var child);
                        if (VSConstants.S_OK == hr)
                        {
                            // There is a sub-item, call this same function on it.
                            InternalScanHierarchy(GetItemId(child));
                        }
                    }

                    // Move the current item to its first visible sibling.
                    hr = this._hierarchy.GetProperty(currentItem, (int)__VSHPROPID.VSHPROPID_NextSibling, out var sibling);
                    if (VSConstants.S_OK != hr)
                    {
                        currentItem = VSConstants.VSITEMID_NIL;
                    }
                    else
                    {
                        currentItem = GetItemId(sibling);
                    }
                }
            }

            private bool IsAnalyzableSource(uint itemId, out string canonicalName)
            {
                // Find out if this item is a physical file.
                Guid typeGuid;
                canonicalName = null;
                int hr;
                try
                {
                    hr = this.Hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGuid);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    return false;
                }
                if (Microsoft.VisualStudio.ErrorHandler.Failed(hr) ||
                    VSConstants.GUID_ItemType_PhysicalFile != typeGuid)
                {
                    // It is not a file, we can exit now.
                    return false;
                }

                // This item is a file; find if current language can recognize it.
                hr = this.Hierarchy.GetCanonicalName(itemId, out canonicalName);
                if (ErrorHandler.Failed(hr))
                {
                    return false;
                }
                return (System.IO.Path.GetExtension(canonicalName).Equals(".xaml", StringComparison.OrdinalIgnoreCase)) ||
                    this._manager._package.IsRecognizedFile(canonicalName);
            }

            /// <summary>
            /// Gets the item id.
            /// </summary>
            /// <param name="variantValue">VARIANT holding an itemid.</param>
            /// <returns>Item Id of the concerned node</returns>
            private static uint GetItemId(object variantValue)
            {
                if (variantValue == null)
                    return VSConstants.VSITEMID_NIL;
                if (variantValue is int)
                    return (uint)(int)variantValue;
                if (variantValue is uint)
                    return (uint)variantValue;
                if (variantValue is short)
                    return (uint)(short)variantValue;
                if (variantValue is ushort)
                    return (uint)(ushort)variantValue;
                if (variantValue is long)
                    return (uint)(long)variantValue;
                return VSConstants.VSITEMID_NIL;
            }
        }
    }
}

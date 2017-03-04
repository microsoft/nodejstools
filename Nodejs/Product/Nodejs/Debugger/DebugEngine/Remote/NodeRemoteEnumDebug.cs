// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio;

namespace Microsoft.NodejsTools.Debugger.Remote
{
    internal class NodeRemoteEnumDebug<T>
        where T : class
    {
        private readonly T _elem;
        private bool _done;

        public NodeRemoteEnumDebug(T elem = null)
        {
            this._elem = elem;
            Reset();
        }

        protected T Element => this._elem;
        public int GetCount(out uint pcelt)
        {
            pcelt = (this._elem == null) ? 0u : 1u;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, T[] rgelt, ref uint pceltFetched)
        {
            if (this._done)
            {
                pceltFetched = 0;
                return VSConstants.S_FALSE;
            }
            else
            {
                pceltFetched = 1;
                rgelt[0] = this._elem;
                this._done = true;
                return VSConstants.S_OK;
            }
        }

        public int Reset()
        {
            this._done = (this._elem == null);
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            if (celt == 0)
            {
                return VSConstants.S_OK;
            }
            else if (this._done)
            {
                return VSConstants.S_FALSE;
            }
            else
            {
                this._done = true;
                return celt > 1 ? VSConstants.S_FALSE : VSConstants.S_OK;
            }
        }
    }
}


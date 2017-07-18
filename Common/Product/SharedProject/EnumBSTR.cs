// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal class EnumBSTR : IVsEnumBSTR
    {
        private readonly IEnumerable<string> _enumerable;
        private IEnumerator<string> _enumerator;

        public EnumBSTR(IEnumerable<string> items)
        {
            this._enumerable = items;
            this._enumerator = this._enumerable.GetEnumerator();
        }

        public int Clone(out IVsEnumBSTR ppenum)
        {
            ppenum = new EnumBSTR(this._enumerable);
            return VSConstants.S_OK;
        }

        public int GetCount(out uint pceltCount)
        {
            var coll = this._enumerable as ICollection<string>;
            if (coll != null)
            {
                pceltCount = checked((uint)coll.Count);
                return VSConstants.S_OK;
            }
            else
            {
                pceltCount = 0;
                return VSConstants.E_NOTIMPL;
            }
        }

        public int Next(uint celt, string[] rgelt, out uint pceltFetched)
        {
            pceltFetched = 0;

            var i = 0;
            for (; i < celt && this._enumerator.MoveNext(); ++i)
            {
                ++pceltFetched;
                rgelt[i] = this._enumerator.Current;
            }
            for (; i < celt; ++i)
            {
                rgelt[i] = null;
            }

            return pceltFetched > 0 ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        public int Reset()
        {
            this._enumerator = this._enumerable.GetEnumerator();
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            while (celt != 0 && this._enumerator.MoveNext())
            {
                celt--;
            }
            return VSConstants.S_OK;
        }
    }
}

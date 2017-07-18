// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

// These classes use a generic enumerator implementation to create the various enumerators required by the engine.
// They allow the enumeration of everything from programs to breakpoints.

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    #region Base Class
    internal class AD7Enum<T, I> where I : class
    {
        private readonly T[] m_data;
        private uint m_position;

        public AD7Enum(T[] data)
        {
            this.m_data = data;
            this.m_position = 0;
        }

        public int Clone(out I ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (uint)this.m_data.Length;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, T[] rgelt, out uint celtFetched)
        {
            return Move(celt, rgelt, out celtFetched);
        }

        public int Reset()
        {
            lock (this)
            {
                this.m_position = 0;

                return VSConstants.S_OK;
            }
        }

        public int Skip(uint celt)
        {
            uint celtFetched;

            return Move(celt, null, out celtFetched);
        }

        private int Move(uint celt, T[] rgelt, out uint celtFetched)
        {
            lock (this)
            {
                var hr = VSConstants.S_OK;
                celtFetched = (uint)this.m_data.Length - this.m_position;

                if (celt > celtFetched)
                {
                    hr = VSConstants.S_FALSE;
                }
                else if (celt < celtFetched)
                {
                    celtFetched = celt;
                }

                if (rgelt != null)
                {
                    for (var c = 0; c < celtFetched; c++)
                    {
                        rgelt[c] = this.m_data[this.m_position + c];
                    }
                }

                this.m_position += celtFetched;

                return hr;
            }
        }
    }
    #endregion Base Class

    internal class AD7FrameInfoEnum : AD7Enum<FRAMEINFO, IEnumDebugFrameInfo2>, IEnumDebugFrameInfo2
    {
        public AD7FrameInfoEnum(FRAMEINFO[] data)
            : base(data)
        {
        }

        public int Next(uint celt, FRAMEINFO[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class AD7PropertyInfoEnum : AD7Enum<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
    {
        public AD7PropertyInfoEnum(DEBUG_PROPERTY_INFO[] data)
            : base(data)
        {
        }
    }

    internal class AD7ThreadEnum : AD7Enum<IDebugThread2, IEnumDebugThreads2>, IEnumDebugThreads2
    {
        public AD7ThreadEnum(IDebugThread2[] threads)
            : base(threads)
        {
        }

        public int Next(uint celt, IDebugThread2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class AD7ModuleEnum : AD7Enum<IDebugModule2, IEnumDebugModules2>, IEnumDebugModules2
    {
        public AD7ModuleEnum(IDebugModule2[] modules)
            : base(modules)
        {
        }

        public int Next(uint celt, IDebugModule2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class AD7PropertyEnum : AD7Enum<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
    {
        public AD7PropertyEnum(DEBUG_PROPERTY_INFO[] properties)
            : base(properties)
        {
        }
    }

    internal class AD7CodeContextEnum : AD7Enum<IDebugCodeContext2, IEnumDebugCodeContexts2>, IEnumDebugCodeContexts2
    {
        public AD7CodeContextEnum(IDebugCodeContext2[] codeContexts)
            : base(codeContexts)
        {
        }

        public int Next(uint celt, IDebugCodeContext2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class AD7BoundBreakpointsEnum : AD7Enum<IDebugBoundBreakpoint2, IEnumDebugBoundBreakpoints2>, IEnumDebugBoundBreakpoints2
    {
        public AD7BoundBreakpointsEnum(IDebugBoundBreakpoint2[] breakpoints)
            : base(breakpoints)
        {
        }

        public int Next(uint celt, IDebugBoundBreakpoint2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class AD7ErrorBreakpointsEnum : AD7Enum<IDebugErrorBreakpoint2, IEnumDebugErrorBreakpoints2>, IEnumDebugErrorBreakpoints2
    {
        public AD7ErrorBreakpointsEnum(IDebugErrorBreakpoint2[] breakpoints)
            : base(breakpoints)
        {
        }

        public int Next(uint celt, IDebugErrorBreakpoint2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }
}

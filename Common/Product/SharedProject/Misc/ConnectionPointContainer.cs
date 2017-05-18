// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Class used to identify a source of events of type SinkType.
    /// </summary>
    [ComVisible(false)]
    internal interface IEventSource<SinkType>
        where SinkType : class
    {
        void OnSinkAdded(SinkType sink);
        void OnSinkRemoved(SinkType sink);
    }

    [ComVisible(true)]
    public class ConnectionPointContainer : IConnectionPointContainer
    {
        private Dictionary<Guid, IConnectionPoint> connectionPoints;
        internal ConnectionPointContainer()
        {
            this.connectionPoints = new Dictionary<Guid, IConnectionPoint>();
        }
        internal void AddEventSource<SinkType>(IEventSource<SinkType> source)
            where SinkType : class
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }
            if (this.connectionPoints.ContainsKey(typeof(SinkType).GUID))
            {
                throw new ArgumentException("EventSource guid already added to the list of connection points", "source");
            }
            this.connectionPoints.Add(typeof(SinkType).GUID, new ConnectionPoint<SinkType>(this, source));
        }

        #region IConnectionPointContainer Members
        void IConnectionPointContainer.EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
        {
            throw new NotImplementedException();
        }
        void IConnectionPointContainer.FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
        {
            ppCP = this.connectionPoints[riid];
        }
        #endregion
    }

    internal class ConnectionPoint<SinkType> : IConnectionPoint
        where SinkType : class
    {
        private Dictionary<uint, SinkType> sinks;
        private uint nextCookie;
        private ConnectionPointContainer container;
        private IEventSource<SinkType> source;
        internal ConnectionPoint(ConnectionPointContainer container, IEventSource<SinkType> source)
        {
            if (null == container)
            {
                throw new ArgumentNullException("container");
            }
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }
            this.container = container;
            this.source = source;
            this.sinks = new Dictionary<uint, SinkType>();
            this.nextCookie = 1;
        }
        #region IConnectionPoint Members
        public void Advise(object pUnkSink, out uint pdwCookie)
        {
            var sink = pUnkSink as SinkType;
            if (null == sink)
            {
                Marshal.ThrowExceptionForHR(VSConstants.E_NOINTERFACE);
            }
            this.sinks.Add(this.nextCookie, sink);
            pdwCookie = this.nextCookie;
            this.source.OnSinkAdded(sink);
            this.nextCookie += 1;
        }

        public void EnumConnections(out IEnumConnections ppEnum)
        {
            throw new NotImplementedException();
            ;
        }

        public void GetConnectionInterface(out Guid pIID)
        {
            pIID = typeof(SinkType).GUID;
        }

        public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
        {
            ppCPC = this.container;
        }

        public void Unadvise(uint dwCookie)
        {
            // This will throw if the cookie is not in the list.
            var sink = this.sinks[dwCookie];
            this.sinks.Remove(dwCookie);
            this.source.OnSinkRemoved(sink);
        }
        #endregion
    }
}


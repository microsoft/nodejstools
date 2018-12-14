// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Text;

namespace Microsoft.NodejsTools.Diagnostics
{
    internal sealed class NodejsToolsEventSource : EventSource
    {
        public static NodejsToolsEventSource Instance = new NodejsToolsEventSource();

        public static class Keywords
        {
            public const EventKeywords LogPossiblePII = (EventKeywords)0x0001;
            public const EventKeywords HierarchyEvent = (EventKeywords)0x0002;
            public const EventKeywords Errors = (EventKeywords)0x0080; // Matches the TypeScriptLanguageService event keywords
        }

        public const string SuppressedMsg = "<PII suppressed>";
        public const string NullMsg = "<null>";

        [Event(1, Message = "Hierarchy event started. OnItemAdded requested for ParentNode.HierarhcyId: {0}, ParentNode.ItemName: \"{1}\", ChildNode.HierarchyId: {2}, ChildNode.ItemName: \"{3}\", PreviousVisibleNode.HierarchyId: {4}, PreviousVisibleNode.ItemName: \"{5}\", isDiskNode: {6}", Keywords = Keywords.HierarchyEvent)]
        internal void HierarchyEventStart(uint parentNodeHierarchyId, string parentNodeName, uint childNodeHierarchyId, string childNodeName, uint? previousVisibleNodeHierarchyId, string previousVisibleNodeName, bool isDiskNode)
        {
            this.WriteEvent(
                1,
                parentNodeHierarchyId,
                this.GetSuppressedMessage(parentNodeName),
                childNodeHierarchyId,
                this.GetSuppressedMessage(childNodeName),
                previousVisibleNodeHierarchyId.HasValue ? previousVisibleNodeHierarchyId.Value.ToString() : NullMsg,
                this.GetSuppressedMessage(previousVisibleNodeName),
                isDiskNode);
        }

        [Event(2, Message = "Hierarchy event stopped. {0}", Keywords = Keywords.HierarchyEvent)]
        internal void HierarchyEventStop(string message)
        {
            this.WriteEvent(2, message);
        }

        [Event(3, Message = "Hierarchy event failed. \"{0}\". Result: {1}. Sink.OnItemAdded({2}, {3}, {4}).\r\nHierarchy\r\nItemId\tName\tParentId\r\n{5}", Keywords = Keywords.HierarchyEvent, Level = EventLevel.Error)]
        internal void HierarchyEventException(string message, int result, uint parentHierarchyId, uint prevId, uint childHierarchyId, IEnumerable<HierarchyItem> hierarchyItems)
        {
            var builder = new StringBuilder();
            foreach (var item in hierarchyItems)
            {
                builder.AppendLine($"{item.ItemId}\t{this.GetSuppressedMessage(item.Name)}\t{item.ParentId}");
            }

            this.WriteEvent(3, message, result, parentHierarchyId, prevId, childHierarchyId, builder.ToString());
        }

        private string GetSuppressedMessage(string msg)
        {
            return string.IsNullOrWhiteSpace(msg) || this.IsEnabled(EventLevel.LogAlways, Keywords.LogPossiblePII)
                ? msg ?? NullMsg
                : SuppressedMsg;
        }
    }
}

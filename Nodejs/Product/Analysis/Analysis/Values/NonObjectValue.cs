using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents a value which is not an object (number, string, bool)
    /// </summary>
    abstract class NonObjectValue : AnalysisValue, IReferenceableContainer {
        public abstract AnalysisValue Prototype {
            get;
        }

        public override Dictionary<string, IAnalysisSet> GetAllMembers() {
            return Prototype.GetAllMembers();
        }

        public override IAnalysisSet GetMember(Node node, AnalysisUnit unit, string name) {
            return Prototype.GetMember(node, unit, name);
        }

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            var proto = Prototype as IReferenceableContainer;
            if (proto != null) {
                return proto.GetDefinitions(name);
            }
            return new IReferenceable[0];
        }
    }
}

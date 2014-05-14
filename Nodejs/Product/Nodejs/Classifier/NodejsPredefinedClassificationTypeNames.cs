using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Classifier {
    class NodejsPredefinedClassificationTypeNames {
        /// <summary>
        /// Open grouping classification.  Used for (, [, {, ), ], and }...  A subtype of the Python
        /// operator grouping.
        /// </summary>
        public const string Grouping = "Node.js grouping";

        /// <summary>
        /// Classification used for comma characters when used outside of a literal, comment, etc...
        /// </summary>
        public const string Comma = "Node.js comma";

        /// <summary>
        /// Classification used for . characters when used outside of a literal, comment, etc...
        /// </summary>
        public const string Dot = "Node.js dot";

        /// <summary>
        /// Classification used for all other operators
        /// </summary>
        public const string Operator = "Node.js operator";
    }
}

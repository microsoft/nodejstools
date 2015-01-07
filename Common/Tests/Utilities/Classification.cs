//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Classification;

namespace TestUtilities {
    public class Classification {
        public readonly int Start, End;
        public readonly string Text;
        public readonly string ClassificationType;

        public Classification(string classificationType, int start, int end, string text) {
            ClassificationType = classificationType;
            Start = start;
            End = end;
            Text = text;
        }

        public static void Verify(IList<ClassificationSpan> spans, params Classification[] expected) {
            bool passed = false;
            try {
                for (int i = 0; i < spans.Count; i++) {
                    if (i >= expected.Length) {
                        Assert.Fail();
                        break;
                    }

                    var curSpan = spans[i];

                    int start = curSpan.Span.Start.Position;
                    int end = curSpan.Span.End.Position;

                    string spanInfo = string.Format("Span #{0}: {1}", i, expected[i].Text);
                    Assert.AreEqual(expected[i].Start, start, spanInfo);
                    Assert.AreEqual(expected[i].End, end, spanInfo);
                    Assert.AreEqual(expected[i].Text, curSpan.Span.GetText(), spanInfo);
                    Assert.IsTrue(curSpan.ClassificationType.IsOfType(expected[i].ClassificationType), 
                        "Classifier incorrect for '{0}'.  Expected:{1} Actual:{2}", expected[i].Text, expected[i].ClassificationType, curSpan.ClassificationType);
                }

                passed = true;
            } finally {
                if (!passed) {
                    // Output expected and actual results as Classification objects for easy diffing and copy-pasting.

                    Console.WriteLine("Expected:\r\n" +
                        String.Join(",\r\n",
                            expected.Select(cls =>
                                String.Format("new Classification(\"{0}\", {1}, {2}, \"{3}\")",
                                    cls.ClassificationType,
                                    cls.Start,
                                    cls.End,
                                    FormatString(cls.Text)
                                )
                            )
                        )
                    );

                    Console.WriteLine("Actual:\r\n" +
                        String.Join(",\r\n",
                            spans.Select(curSpan =>
                                String.Format("new Classification(\"{0}\", {1}, {2}, \"{3}\")",
                                    curSpan.ClassificationType.Classification,
                                    curSpan.Span.Start.Position,
                                    curSpan.Span.End.Position,
                                    FormatString(curSpan.Span.GetText())
                                )
                            )
                        )
                    );
                }
            }
        }

        public static string FormatString(string p) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < p.Length; i++) {
                switch (p[i]) {
                    case '\\': res.Append("\\\\"); break;
                    case '\n': res.Append("\\n"); break;
                    case '\r': res.Append("\\r"); break;
                    case '\t': res.Append("\\t"); break;
                    case '"': res.Append("\\\""); break;
                    default: res.Append(p[i]); break;
                }
            }
            return res.ToString();
        }

    }

}

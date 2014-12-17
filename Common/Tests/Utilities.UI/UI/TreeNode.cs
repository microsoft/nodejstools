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
using System.Linq;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class TreeNode : AutomationWrapper
    {
        public TreeNode(AutomationElement element)
            : base(element) {
        }

        public new void Select()
        {
            try
            {
                var parent = Element.GetSelectionItemPattern().Current.SelectionContainer;
                foreach (var item in parent.GetSelectionPattern().Current.GetSelection())
                {
                    item.GetSelectionItemPattern().RemoveFromSelection();
                }
                Element.GetSelectionItemPattern().AddToSelection();
            }
            catch (InvalidOperationException)
            {
                // Control does not support this pattern, so let's just click
                // on it.
                var point = Element.GetClickablePoint();
                point.Offset(0.0, 50.0);
                Mouse.MoveTo(point);
                System.Threading.Thread.Sleep(100);
                point.Offset(0.0, -50.0);
                Mouse.MoveTo(point);
                System.Threading.Thread.Sleep(100);
                Mouse.Click(System.Windows.Input.MouseButton.Left);
                System.Threading.Thread.Sleep(100);
            }
        }

        public void Deselect()
        {
            Element.GetSelectionItemPattern().RemoveFromSelection();
        }

        public string Value
        {
            get
            {
                return this.Element.Current.Name.ToString();
            }
        }

        public bool IsExpanded
        {
            get
            {
                switch (Element.GetExpandCollapsePattern().Current.ExpandCollapseState)
                {
                    case ExpandCollapseState.Collapsed:
                        return false;
                    case ExpandCollapseState.Expanded:
                        return true;
                    case ExpandCollapseState.LeafNode:
                        return true;
                    case ExpandCollapseState.PartiallyExpanded:
                        return false;
                    default:
                        return false;
                }
            }
            set
            {
                if (value)
                {
                    Element.GetExpandCollapsePattern().Expand();
                }
                else
                {
                    Element.GetExpandCollapsePattern().Collapse();
                }
            }
        }

        public List<TreeNode> Nodes
        {
            get
            {
                return Element.FindAll(
                    TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem)
                )
                    .OfType<AutomationElement>()
                    .Select(e => new TreeNode(e))
                    .ToList();
            }
        }

        public void ExpandCollapse()
        {
            try {
                var pattern = Element.GetExpandCollapsePattern();
                switch (pattern.Current.ExpandCollapseState)
                {
                    case ExpandCollapseState.Collapsed:
                        pattern.Expand();
                        break;
                    case ExpandCollapseState.Expanded:
                        pattern.Collapse();
                        break;
                    case ExpandCollapseState.LeafNode:
                        break;
                    case ExpandCollapseState.PartiallyExpanded:
                        pattern.Expand();
                        break;
                    default:
                        break;
                }
            } catch (InvalidOperationException) {
                Element.GetInvokePattern().Invoke();
            }
        }

        public void DoubleClick()
        {
            Element.GetInvokePattern().Invoke();
        }

        public void ShowContextMenu()
        {
            Select();
            System.Threading.Thread.Sleep(100);
            Mouse.Click(System.Windows.Input.MouseButton.Right);
            System.Threading.Thread.Sleep(100);
        }
    }
}

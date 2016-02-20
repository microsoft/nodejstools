using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;


namespace Microsoft.NodejsTools.Project.NewFileMenuGroup
{
    internal static class NewFileUtilities
    {
        public static string GetTemplateFile(string fileType)
        {
            string templateFileName = null;

            switch (fileType) {
                case NodejsConstants.JavaScript:
                    templateFileName = "EmptyJs.js";
                    break;
                case NodejsConstants.TypeScript:
                    templateFileName = "EmptyTs.ts";
                    break;
                case NodejsConstants.HTML:
                    templateFileName = "EmptyHTML.html";
                    break;
                case NodejsConstants.CSS:
                    templateFileName = "EmptyCSS.css";
                    break;
            }

            if (templateFileName == null) {
                Debug.Fail(String.Format("Invalid file type: {0}", fileType));
            }

            return NodejsToolsInstallPath.GetFile("FileTemplates\\NewItem\\" + templateFileName);
        }

        private static string GetInitialName(string fileType)
        {
            string name = null;

            switch (fileType) {
                case NodejsConstants.JavaScript:
                    name = "JavaScript.js";
                    break;
                case NodejsConstants.TypeScript:
                    name = "TypeScript.ts";
                    break;
                case NodejsConstants.HTML:
                    name = "HTML.html";
                    break;
                case NodejsConstants.CSS:
                    name = "CSS.css";
                    break;
            }

            if (name == null) {
                Debug.Fail(String.Format("Invalid file type: {0}", fileType));
            }

            return name;
        }

        private static void CreateNewFile(NodejsProjectNode projectNode, uint containerId, string fileType)
        {
            using (var dialog = new NewFileNameForm(GetInitialName(fileType))) {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    string itemName = dialog.TextBox.Text;

                    VSADDRESULT[] pResult = new VSADDRESULT[1];
                    projectNode.AddItem(
                        containerId,                                 // Identifier of the container folder. 
                        VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE,    // Indicate that we want to create this new file by cloning a template file.
                        itemName,
                        1,                                           // Number of templates in the next parameter. Must be 1 if using VSADDITEMOP_CLONEFILE.
                        new string[] { GetTemplateFile(fileType) },  // Array contains the template file path.
                        IntPtr.Zero,                                 // Handle to the Add Item dialog box. Must be Zero if using VSADDITEMOP_CLONEFILE.
                        pResult
                    );

                    // TODO: Do we need to check if result[0] = VSADDRESULT.ADDRESULT_Success here?
                }
            }
        }

        internal static void CreateNewJavaScriptFile(NodejsProjectNode projectNode, uint containerId)
        {
            CreateNewFile(projectNode, containerId, NodejsConstants.JavaScript);
        }

        internal static void CreateNewTypeScriptFile(NodejsProjectNode projectNode, uint containerId)
        {
            CreateNewFile(projectNode, containerId, NodejsConstants.TypeScript);
        }

        internal static void CreateNewHTMLFile(NodejsProjectNode projectNode, uint containerId)
        {
            CreateNewFile(projectNode, containerId, NodejsConstants.HTML);
        }

        internal static void CreateNewCSSFile(NodejsProjectNode projectNode, uint containerId)
        {
            CreateNewFile(projectNode, containerId, NodejsConstants.CSS);
        }
    }
}

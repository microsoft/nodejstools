// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Project.NewFileMenuGroup
{
    internal static class NewFileUtilities
    {
        internal static void CreateNewFile(NodejsProjectNode projectNode, uint containerId)
        {
            using (var dialog = new NewFileNameForm(""))
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var itemName = dialog.TextBox.Text;
                    if (string.IsNullOrWhiteSpace(itemName))
                    {
                        return;
                    }
                    itemName = itemName.Trim();

                    var pResult = new VSADDRESULT[1];
                    projectNode.AddItem(
                        containerId,                                 // Identifier of the container folder. 
                        VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE,    // Indicate that we want to create this new file by cloning a template file.
                        itemName,
                        1,                                           // Number of templates in the next parameter. Must be 1 if using VSADDITEMOP_CLONEFILE.
                        new string[] { Path.GetTempFileName() },     // Array contains the template file path.
                        IntPtr.Zero,                                 // Handle to the Add Item dialog box. Must be Zero if using VSADDITEMOP_CLONEFILE.
                        pResult);
                }
            }
        }
    }
}


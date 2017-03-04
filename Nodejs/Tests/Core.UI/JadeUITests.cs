// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI
{
    [TestClass]
    public class JadeUITests : NodejsProjectTest
    {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void InsertTabs()
        {
            using (new OptionHolder("TextEditor", "Jade", "InsertTabs", true))
            {
                using (var solution = Project("TabsSpaces", Content("quox.pug", "ul\r\n    li A\r\n    li B")).Generate().ToVs())
                {
                    var jadeFile = solution.OpenItem("TabsSpaces", "quox.pug");
                    jadeFile.MoveCaret(1, 1);
                    Keyboard.Type("\t");
                    Assert.AreEqual(jadeFile.Text, "\tul\r\n    li A\r\n    li B");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void InsertSpaces()
        {
            using (new OptionHolder("TextEditor", "Jade", "InsertTabs", false))
            {
                using (var solution = Project("TabsSpaces", Content("quox.pug", "ul\r\n    li A\r\n    li B")).Generate().ToVs())
                {
                    var jadeFile = solution.OpenItem("TabsSpaces", "quox.pug");
                    jadeFile.MoveCaret(1, 1);
                    Keyboard.Type("\t");
                    Assert.AreEqual(jadeFile.Text, "    ul\r\n    li A\r\n    li B");
                }
            }
        }
    }
}


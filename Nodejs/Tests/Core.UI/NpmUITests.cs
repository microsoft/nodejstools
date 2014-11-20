/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using Moq;
using NpmTests;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class NpmUITests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Npm UI")]
        [HostType("VSTestHost")]
        public void TestNpmUIInitialization() {
            using (var app = new VisualStudioApp()) {
                // Initialize call is required because NTVS does not autoload its package
                // We may not be on UI thread, but Dev11 and Dev12 know how to sort that out.
                UIThread.InitializeAndAlwaysInvokeToCurrentThread();
                UIThread.Invoke(() => {
                    NpmPackageInstallWindow npmWindow = OpenNpmWindowAndWaitForReady();

                    Assert.IsTrue(npmWindow.FilterTextBox.IsKeyboardFocused, "FilterTextBox should be keyboard focused");
                    Assert.AreEqual(0, npmWindow._packageList.SelectedIndex, "First item in package list should be selected");
                });
            }
        }

        [TestMethod, Priority(0), TestCategory("Npm UI")]
        [HostType("VSTestHost")]
        public void TestNpmUIArrowKeyBehavior() {
            using (var app = new VisualStudioApp()) {
                UIThread.InitializeAndAlwaysInvokeToCurrentThread();
                UIThread.Invoke(() => {
                    NpmPackageInstallWindow npmWindow = OpenNpmWindowAndWaitForReady();

                    System.Windows.Input.Keyboard.Focus(npmWindow.FilterTextBox);
                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Down);
                    WaitForUIInputIdle();

                    var selectedItem = GetSelectedPackageListItemContainer(npmWindow);
                    Assert.IsTrue(selectedItem.IsKeyboardFocused, "Focus should be on newly selected item");
                    Assert.AreEqual(0, npmWindow._packageList.SelectedIndex);

                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Down);
                    WaitForUIInputIdle();

                    Assert.AreEqual(1, npmWindow._packageList.SelectedIndex);

                    npmWindow.FilterTextBox.Focus();
                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Up);
                    WaitForUIInputIdle();

                    selectedItem = GetSelectedPackageListItemContainer(npmWindow);
                    Assert.IsTrue(npmWindow.FilterTextBox.IsKeyboardFocused, "Focus should remain on filter box");
                    Assert.AreEqual(1, npmWindow._packageList.SelectedIndex, "Pressing up while in filter box should maintain current selection");

                    selectedItem = GetSelectedPackageListItemContainer(npmWindow);
                    selectedItem.Focus();
                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Up);
                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Up);
                    WaitForUIInputIdle();

                    Assert.IsTrue(npmWindow.FilterTextBox.IsKeyboardFocused, "Focus should move to filter textbox after pressing up key while on topmost package is selected");

                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Up);
                    WaitForUIInputIdle();

                    Assert.IsTrue(npmWindow.FilterTextBox.IsKeyboardFocused, "Focus should remain on textbox while pressing up when topmost package is selected");
                });
            }
        }

        [TestMethod, Priority(0), TestCategory("Npm UI")]
        [HostType("VSTestHost")]
        public void TestNpmUITabKeyBehavior() {
            using (var app = new VisualStudioApp()) {
                UIThread.InitializeAndAlwaysInvokeToCurrentThread();
                UIThread.Invoke(() => {
                    NpmPackageInstallWindow npmWindow = OpenNpmWindowAndWaitForReady();

                    npmWindow.FilterTextBox.Focus();
                    WaitForUIInputIdle();

                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Tab);
                    WaitForUIInputIdle();

                    var selectedItem = GetSelectedPackageListItemContainer(npmWindow);
                    Assert.IsTrue(selectedItem.IsKeyboardFocused);

                    // Install button disabled, must key down to select "installable" package
                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Down);
                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Tab);
                    WaitForUIInputIdle();

                    Assert.IsTrue(npmWindow.DependencyComboBox.IsKeyboardFocused);

                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Tab);
                    WaitForUIInputIdle();

                    Assert.IsTrue(npmWindow.SaveToPackageJsonCheckbox.IsKeyboardFocused);

                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Tab);
                    WaitForUIInputIdle();

                    Assert.IsTrue(npmWindow.ArgumentsTextBox.IsKeyboardFocused);

                    TestUtilities.UI.Keyboard.PressAndRelease(Key.Tab);
                    WaitForUIInputIdle();

                    Assert.IsTrue(npmWindow.InstallButton.IsKeyboardFocused);
                });
            }
        }

        private NpmPackageInstallWindow OpenNpmWindowAndWaitForReady() {
            var npmControllerMock = GetNpmControllerMock();
            NpmPackageInstallWindow npmWindow = new NpmPackageInstallWindow(npmControllerMock.Object, new NpmOutputViewModel(npmControllerMock.Object));
            npmWindow.Show();
            WaitForPackageListItemsToAppear(npmWindow);
            return npmWindow;
        }

        private static ListViewItem GetSelectedPackageListItemContainer(NpmPackageInstallWindow npmWindow) {
            return (ListViewItem) npmWindow._packageList.ItemContainerGenerator.ContainerFromItem(npmWindow._packageList.SelectedItem);
        }

        private static Mock<INpmController> GetNpmControllerMock() {
            var npmControllerMock = new Mock<INpmController>();
            var packageMock = new Mock<IPackage>();
            packageMock.Setup(mock => mock.Name).Returns("mock package");

            var packageList = new List<IPackage>() {
                packageMock.Object,
                packageMock.Object,
                packageMock.Object
            };

            var globalPackages = new Mock<IGlobalPackages>();
            globalPackages.Setup(mock => mock.Modules).Returns((new Mock<INodeModules>()).Object);

            var rootPackage = new Mock<IRootPackage>();
            rootPackage.Setup(mock => mock.Modules).Returns((new Mock<INodeModules>()).Object);

            npmControllerMock.Setup(mock => mock.GetRepositoryCatalogAsync(It.IsAny<bool>(), null)).ReturnsAsync(new MockPackageCatalog(packageList));
            npmControllerMock.Setup(mock => mock.GlobalPackages).Returns(globalPackages.Object);
            npmControllerMock.Setup(mock => mock.RootPackage).Returns(rootPackage.Object);

            return npmControllerMock;
        }

        private void WaitForUIInputIdle() {
            Dispatcher.CurrentDispatcher.Invoke((Action)(() => { }),
                DispatcherPriority.ApplicationIdle);
        }

        private void WaitForPackageListItemsToAppear(NpmPackageInstallWindow npmWindow) {
            for (int i = 0; i < 100; i++) {
                if (npmWindow._packageList.Items.Count > 0) {
                    return;
                }

                Dispatcher.CurrentDispatcher.Invoke(() => { System.Threading.Thread.Sleep(100); },
                    DispatcherPriority.ApplicationIdle);
            }

            Assert.Fail("Package list items took too long to appear");
        }
    }
}

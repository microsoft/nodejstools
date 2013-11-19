using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    [TestClass]
    public class ProblematicPackageJsonTests : AbstractPackageJsonTests{
        [TestMethod]
        public void TestFreshPackageJsonParseFromResource(){
            var pkg = LoadFromResource("NpmTests.Resources.fresh_package.json");
            Assert.IsNotNull(pkg, "Fresh package should not be null.");
        }
    }
}

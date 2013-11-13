using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NpmTests
{

    [TestClass]
    public class NpmSearchTests
    {

        private TextReader GetCatalogueReader(){
            NodejsTestData.Deploy();
            return new StreamReader(TestData.GetPath(@"NpmSearchData\npmsearchfullcatalog.txt"));
        }

        [TestMethod]
        public void TestParseModuleCatalogue(){
            IList<IPackage> target = new List<IPackage>();
            IDictionary<string, IPackage> byName = new Dictionary<string, IPackage>();
            INpmSearchLexer lexer = NpmSearchParserFactory.CreateLexer();
            INpmSearchParser parser = NpmSearchParserFactory.CreateParser(lexer);
            parser.Package += (source, args) => {
                target.Add(args.Package);
                byName[args.Package.Name] = args.Package;
            };

            using (var reader = GetCatalogueReader()){
                lexer.Lex(reader);
            }

            Assert.AreEqual(47365, target.Count, "Unexpected package count in catalogue list.");
            Assert.AreEqual(target.Count, byName.Count, "Number of packages should be same in list and dictionary.");
        }
    }
}

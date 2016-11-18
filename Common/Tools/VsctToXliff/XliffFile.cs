/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of 
 * the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VsctToXliff
{
    internal sealed class XliffFile
    {
        private static readonly XNamespace XSI = @"http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace XLIFF = @"urn:oasis:names:tc:xliff:document:1.2";

        private const string XliffExt = ".xlf";

        public string XliffFolder { get; }
        public string FileNameWithoutExtension { get; }

        public XliffFile(string targetFolder, string fileNameWithoutExtension)
        {
            if (string.IsNullOrEmpty(targetFolder))
            {
                throw new ArgumentException($"{nameof(targetFolder)} should be set");
            }

            if (!Path.IsPathRooted(targetFolder))
            {
                throw new ArgumentException("Expected a rooted path");
            }

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                throw new ArgumentException($"{nameof(fileNameWithoutExtension)} should be set");

            }

            this.XliffFolder = targetFolder;
            this.FileNameWithoutExtension = fileNameWithoutExtension;
        }

        public void WriteTranslationFile(string originalName, IEnumerable<ITranslationUnit> transUnits, string targetLanguage)
        {
            var fileElement = new XElement(XLIFF + "file",
                        new XAttribute("original", originalName),
                        new XAttribute("source-language", "en"),
                        new XAttribute("datatype", "xml"),
                        new XElement(XLIFF + "body",
                            transUnits.Select(unit => GenerateTransElements(unit, targetLanguage))
                       )
                    );

            if (!StringComparer.OrdinalIgnoreCase.Equals(targetLanguage, "en"))
            {
                fileElement.Add(new XAttribute("target-language", targetLanguage));
            }

            var document = new XDocument(
                new XElement(XLIFF + "xliff",
                    new XAttribute("xmlns", "urn:oasis:names:tc:xliff:document:1.2"),
                    new XAttribute(XNamespace.Xmlns + "xsi", XSI),
                    new XAttribute(XSI + "schemaLocation", "urn:oasis:names:tc:xliff:document:1.2 xliff-core-1.2-transitional.xsd"),
                    new XAttribute("version", "1.2"),
                    fileElement
                )
            );

            Directory.CreateDirectory(Path.Combine(this.XliffFolder, targetLanguage));
            document.Save(Path.Combine(this.XliffFolder, targetLanguage, $"{this.FileNameWithoutExtension}{XliffExt}"));
        }

        private XElement GenerateTransElements(ITranslationUnit transUnits, string targetLanguage)
        {
            var element = new XElement(XLIFF + "trans-unit",
                                 new XAttribute("id", transUnits.Key),
                                 new XElement(XLIFF + "source",
                                     new XAttribute(XNamespace.Xml + "lang", "en"),
                                        transUnits.EnglishValue));

            if (!StringComparer.OrdinalIgnoreCase.Equals(targetLanguage, "en"))
            {
                element.Add(new XElement(XLIFF + "target",
                                     new XAttribute(XNamespace.Xml + "lang", targetLanguage),
                                     new XAttribute("state", "needs-translation"),
                                        transUnits.EnglishValue)
                              );
            }

            return element;
        }

        public IDictionary<string, string> LoadTranslatedElements(string targetLanguage)
        {
            var translations = new Dictionary<string, string>();

            var fileName = Path.Combine(this.XliffFolder, targetLanguage, $"{this.FileNameWithoutExtension}{XliffExt}");
            if (File.Exists(fileName))
            {
                var document = XDocument.Load(fileName);
                var elements = document.Descendants(XLIFF + "trans-unit");
                foreach (var element in elements)
                {
                    var id = element.Attribute("id").Value;
                    var translated = element.Element(XLIFF + "target").Value;
                    if (!translations.ContainsKey(id))
                    {
                        translations.Add(id, translated);
                    }
                }
            }

            return translations;
        }
    }
}

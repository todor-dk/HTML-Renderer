using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Scientia.HtmlRenderer.Html5.Parsing;
using Scientia.HtmlRenderer;
using Scientia.HtmlRenderer.Dom;
using HtmlRenderer.TestLib;

namespace HtmlRenderer.UnitTests
{
    internal static class DomTestUtils
    {
        private const string BasePath = @"C:\DEV_ATL\GitHub\HTML-Renderer\HTML-Renderer\Source\Testing\HtmlRenderer.ExperimentalApp\Data\Files";

        public static void TestDom(string keyword, string file)
        {
            string path = Path.Combine(BasePath, keyword, file);

            Assert.IsTrue(File.Exists(path + ".txt"), "Description file is missing");
            Assert.IsTrue(File.Exists(path + ".html"), "HTML file is missing");
            Assert.IsTrue(File.Exists(path + ".dom"), "DOM file is missing");

            string url;
            using (StreamReader reader = new StreamReader(path + ".txt"))
            {
                reader.ReadLine();
                url = reader.ReadLine().Substring(5);
            }

            Document document;
            using (FileStream fs = File.OpenRead(path + ".html"))
            {
                // Parse the HTML
                StreamHtmlStream stream = new StreamHtmlStream(fs);
                BrowsingContext browsingContext = new BrowsingContext();
                document = browsingContext.ParseDocument(stream, url);
            }
            
            Assert.IsNotNull(document);

            string dom = File.ReadAllText(path + ".dom");

            var root = TestLib.Dom.Persisting.TextReader.FromData(dom);
            Assert.IsNotNull(root);

            var cc = new CompareContext();

            var hr = cc.ValidateRecursive(document);
            Assert.IsTrue(hr == HierarchyResult.Valid, "Document Structure is Invalid. " + hr.ToString());

            hr = cc.ValidateRecursive(root as Document);
            Assert.IsTrue(hr == HierarchyResult.Valid, "Reference Structure is Invalid. " + hr.ToString());

            //cc.IgnoreDocumentOrigin = true;
            //cc.IgnoreBaseUriExceptForElementAndDocument = true;
            cc.IgnoredCompareResult = CompareResult.Node_BaseUri | CompareResult.Document_CharacterSet | CompareResult.Document_DocumentUri | CompareResult.Document_Url | CompareResult.Document_Origin;

            var eq = cc.CompareRecursive(root, document);
            Assert.IsTrue(eq == CompareResult.Equal, "Document and Reference are not equal. " + eq);
        }
    }
}

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
using HtmlRenderer.TestLib.Dom.Persisting;

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

            string html = File.ReadAllText(path + ".html");

            // Parse the HTML
            StringHtmlStream stream = new StringHtmlStream(html);
            BrowsingContext browsingContext = new BrowsingContext();
            Document document = browsingContext.ParseDocument(stream, url);

            Assert.IsNotNull(document);

            string dom = File.ReadAllText(path + ".dom");

            var root = TestLib.Dom.Persisting.TextReader.FromData(dom);
            Assert.IsNotNull(root);

            var cc = new TestLib.Dom.CompareContext();
            cc.IgnoreDocumentOrigin = true;
            cc.IgnoreBaseUriExceptForElementAndDocument = true;
            var eq = root.CompareWith(document, cc);
            Assert.IsTrue(eq);
        }
    }
}

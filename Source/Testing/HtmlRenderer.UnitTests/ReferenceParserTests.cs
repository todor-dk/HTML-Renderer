using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scientia.HtmlRenderer.Html5.Parsing;
using Scientia.HtmlRenderer;
using Scientia.HtmlRenderer.Dom;
using HtmlRenderer.TestLib.Dom.Persisting;

namespace HtmlRenderer.UnitTests
{
    [TestClass]
    public class ReferenceParserTests
    {
        [TestMethod]
        public void ParseAcid1()
        {
            StringHtmlStream stream = new StringHtmlStream(ReferenceParserTests.Acid1Html);
            BrowsingContext browsingContext = new BrowsingContext();
            Document document = browsingContext.ParseDocument(stream, "about:blank");
        }

        [TestMethod]
        public void ParseAcid1AndReferenceEquality()
        {
            StringHtmlStream stream = new StringHtmlStream(ReferenceParserTests.Acid1Html);
            BrowsingContext browsingContext = new BrowsingContext();
            Document document = browsingContext.ParseDocument(stream, "http://localhost/Test/acid1.htm", "windows-1252");
            
            Assert.IsNotNull(document);

            var root = TextReader.FromData(ReferenceTestTests.Acid1Dom);
            Assert.IsNotNull(root);

            var cc = new TestLib.CompareContext();
            //cc.IgnoreDocumentOrigin = true;
            var eq = cc.CompareRecursive(root, document);
            Assert.IsTrue((eq == TestLib.CompareResult.Equal) || (eq == TestLib.CompareResult.Document_Origin));
        }

        [TestMethod]
        public void TestCopyAsReference()
        {
            StringHtmlStream stream = new StringHtmlStream(ReferenceParserTests.Acid1Html);
            BrowsingContext browsingContext = new BrowsingContext();
            Document document = browsingContext.ParseDocument(stream, "http://localhost/Test/acid1.htm", "windows-1252");

            Assert.IsNotNull(document);

            System.IO.MemoryStream dataStream = new System.IO.MemoryStream();
            DomBinaryWriter.Save(document, dataStream);
            dataStream.Position = 0;

            var root = BinaryReader.FromStream(dataStream);
            Assert.IsNotNull(root);

            var cc = new TestLib.CompareContext();
            //cc.IgnoreDocumentOrigin = true;
            var eq = cc.CompareRecursive(root, document);
            Assert.IsTrue(eq == TestLib.CompareResult.Equal);
        }

        public const string Acid1Html = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0//EN\" \"http://www.w3.org/TR/REC-html40/strict.dtd\">\n<html>\n\t<head>\n\t\t<title>\n\t\t\t display/box/float/clear test \n\t\t</title>\n \t<style type=\"text/css\">\n/* last modified: 1 Dec 98 */ \t\n\nhtml {\nfont: 10px/1 Verdana, sans-serif;\nbackground-color: blue;\ncolor: white;\n}\n\nbody {\nmargin: 1.5em;\nborder: .5em solid black;\npadding: 0;\nwidth: 48em;\nbackground-color: white;\n}\n\ndl {\nmargin: 0;\nborder: 0;\npadding: .5em;\n}\n\ndt { \nbackground-color: rgb(204,0,0);\nmargin: 0; \npadding: 1em;\nwidth: 10.638%; /* refers to parent element's width of 47em. = 5em or 50px */\nheight: 28em;\nborder: .5em solid black;\nfloat: left;\n}\n\ndd {\nfloat: right;\nmargin: 0 0 0 1em;\nborder: 1em solid black;\npadding: 1em;\nwidth: 34em;\nheight: 27em;\n}\n\nul {\nmargin: 0;\nborder: 0;\npadding: 0;\n}\n\nli {\ndisplay: block; /* i.e., suppress marker */\ncolor: black;\nheight: 9em;\nwidth: 5em;\nmargin: 0;\nborder: .5em solid black;\npadding: 1em;\nfloat: left;\nbackground-color: #FC0;\n}\n\n#bar {\nbackground-color: black;\ncolor: white;\nwidth: 41.17%; /* = 14em */\nborder: 0;\nmargin: 0 1em;\n}\n\n#baz {\nmargin: 1em 0;\nborder: 0;\npadding: 1em;\nwidth: 10em;\nheight: 10em;\nbackground-color: black;\ncolor: white;\n}\n\nform { \nmargin: 0;\ndisplay: inline;\n}\n\np { \nmargin: 0;\n}\n\nform p {\nline-height: 1.9;\n}\n\nblockquote {\nmargin: 1em 1em 1em 2em;\nborder-width: 1em 1.5em 2em .5em;\nborder-style: solid;\nborder-color: black;\npadding: 1em 0;\nwidth: 5em;\nheight: 9em;\nfloat: left;\nbackground-color: #FC0;\ncolor: black;\n}\n\naddress {\nfont-style: normal;\n}\n\nh1 {\nbackground-color: black;\ncolor: white;\nfloat: left;\nmargin: 1em 0;\nborder: 0;\npadding: 1em;\nwidth: 10em;\nheight: 10em;\nfont-weight: normal;\nfont-size: 1em;\n}\n  </style>\n\t</head>\n\t<body>\n\t\t<dl>\n\t\t\t<dt>\n\t\t\t toggle \n\t\t\t</dt>\n\t\t\t<dd>\n\t\t\t<ul>\n\t\t\t\t<li>\n\t\t\t\t the way \n\t\t\t\t</li>\n\t\t\t\t<li id=\"bar\">\n\t\t\t\t<p>\n\t\t\t\t the world ends \n\t\t\t\t</p>\n\t\t\t\t<form action=\"./\" method=\"get\">\n\t\t\t\t\t<p>\n\t\t\t\t\t bang \n\t\t\t\t\t<input type=\"radio\" name=\"foo\" value=\"off\">\n\t\t\t\t\t</p>\n\t\t\t\t\t<p>\n\t\t\t\t\t whimper \n\t\t\t\t\t<input type=\"radio\" name=\"foo2\" value=\"on\">\n\t\t\t\t\t</p>\n\t\t\t\t</form>\n\t\t\t\t</li>\n\t\t\t\t<li>\n\t\t\t\t i grow old \n\t\t\t\t</li>\n\t\t\t\t<li id=\"baz\">\n\t\t\t\t pluot? \n\t\t\t\t</li>\n\t\t\t</ul>\n\t\t\t<blockquote>\n\t\t\t\t<address>\n\t\t\t\t\t bar maids, \n\t\t\t\t</address>\n\t\t\t</blockquote>\n\t\t\t<h1>\n\t\t\t\t sing to me, erbarme dich \n\t\t\t</h1>\n\t\t\t</dd>\n\t\t</dl>\n\t\t<p style=\"color: black; font-size: 1em; line-height: 1.3em; clear: both\">\n\t\t This is a nonsensical document, but syntactically valid HTML 4.0. All 100%-conformant CSS1 agents should be able to render the document elements above this paragraph indistinguishably (to the pixel) from this \n\t\t\t<a href=\"sec5526c.gif\">reference rendering,</a>\n\t\t (except font rasterization and form widgets). All discrepancies should be traceable to CSS1 implementation shortcomings. Once you have finished evaluating this test, you can return to the <A HREF=\"sec5526c.htm\">parent page</A>. \n\t\t</p>\n\t</body>\n</html>\n";
    }
}

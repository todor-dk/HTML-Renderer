using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlRenderer.TestLib.Dom.Persisting;
using TheArtOfDev.HtmlRenderer.Dom;
using TheArtOfDev.HtmlRenderer;

namespace HtmlRenderer.UnitTests
{
    [TestClass]
    public class ReferenceTestTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var root = TextReader.FromData(Acid1Dom);

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            BinaryWriter.Save(root, stream);

            stream.Position = 0;
            var root2 = BinaryReader.FromStream(stream);

            var eq = root.CompareWith(root, new TestLib.Dom.CompareContext());

            var eq2 = root.CompareWith(root2, new TestLib.Dom.CompareContext());

            var visitor = new TestLib.CodeGeneration.CodeGeneratorVisitor();
            root.Accept(visitor);
            var str = visitor.ToString();
            System.IO.File.WriteAllText(@"c:\temp\acid.txt", str);
        }

        [TestMethod]
        public void CreateAcidDomFromCode()
        {
            BrowsingContext context = new BrowsingContext();
            Document doc = CreateAcidDom(context);
        }

        public static Document CreateAcidDom(BrowsingContext context)
        {
            Document doc = context.CreateDocument("http://localhost:14873/DomAnalyzer.html");

            // DocType !!!
            DocumentType doctype = doc.DomImplementation.CreateDocumentType("html", String.Empty, String.Empty);
            doc.AppendChild(doctype);

            Element html = doc.CreateElement("html");
            doc.AppendChild(html);
            Element head = doc.CreateElement("head");
            html.AppendChild(head);
            head.AppendChild(doc.CreateTextNode("\n\t\t"));
            Element title = doc.CreateElement("title");
            head.AppendChild(title);
            title.AppendChild(doc.CreateTextNode("\n\t\t\t display/box/float/clear test \n\t\t"));
            head.AppendChild(doc.CreateTextNode("\n \t"));
            Element style = doc.CreateElement("style");
            head.AppendChild(style);
            style.SetAttribute("type", "text/css");
            style.AppendChild(doc.CreateTextNode("\n/* last modified: 1 Dec 98 */ \t\n\nhtml {\nfont: 10px / 1 Verdana, sans - serif;\nbackground - color: blue;\ncolor: white;\n}\n\nbody {\nmargin: 1.5em;\nborder: .5em solid black;\npadding: 0;\nwidth: 48em;\nbackground - color: white;\n}\n\ndl {\nmargin: 0;\nborder: 0;\npadding: .5em;\n}\n\ndt { \nbackground - color: rgb(204, 0, 0);\nmargin: 0; \npadding: 1em;\nwidth: 10.638 %; /* refers to parent element's width of 47em. = 5em or 50px */\nheight: 28em;\nborder: .5em solid black;\nfloat: left;\n}\n\ndd {\nfloat: right;\nmargin: 0 0 0 1em;\nborder: 1em solid black;\npadding: 1em;\nwidth: 34em;\nheight: 27em;\n}\n\nul {\nmargin: 0;\nborder: 0;\npadding: 0;\n}\n\nli {\ndisplay: block; /* i.e., suppress marker */\ncolor: black;\nheight: 9em;\nwidth: 5em;\nmargin: 0;\nborder: .5em solid black;\npadding: 1em;\nfloat: left;\nbackground - color: #FC0;\n}\n\n#bar {\nbackground-color: black;\ncolor: white;\nwidth: 41.17%; /* = 14em */\nborder: 0;\nmargin: 0 1em;\n}\n\n#baz {\nmargin: 1em 0;\nborder: 0;\npadding: 1em;\nwidth: 10em;\nheight: 10em;\nbackground-color: black;\ncolor: white;\n}\n\nform { \nmargin: 0;\ndisplay: inline;\n}\n\np { \nmargin: 0;\n}\n\nform p {\nline-height: 1.9;\n}\n\nblockquote {\nmargin: 1em 1em 1em 2em;\nborder-width: 1em 1.5em 2em .5em;\nborder-style: solid;\nborder-color: black;\npadding: 1em 0;\nwidth: 5em;\nheight: 9em;\nfloat: left;\nbackground-color: #FC0;\ncolor: black;\n}\n\naddress {\nfont-style: normal;\n}\n\nh1 {\nbackground-color: black;\ncolor: white;\nfloat: left;\nmargin: 1em 0;\nborder: 0;\npadding: 1em;\nwidth: 10em;\nheight: 10em;\nfont-weight: normal;\nfont-size: 1em;\n}\n  "));
            head.AppendChild(doc.CreateTextNode("\n\t"));
            html.AppendChild(doc.CreateTextNode("\n\t"));

            // BODY
            Element body = doc.CreateElement("body");
            body.AppendChild(doc.CreateTextNode("\n\t\t"));
            Element dl = doc.CreateElement("dl");
            body.AppendChild(dl);
            dl.AppendChild(doc.CreateTextNode("\n\t\t\t"));
            Element dt = doc.CreateElement("dt");
            dl.AppendChild(dt);
            dt.AppendChild(doc.CreateTextNode("\n\t\t\t toggle \n\t\t\t"));
            dl.AppendChild(doc.CreateTextNode("\n\t\t\t"));

            Element dd = doc.CreateElement("dd");
            dl.AppendChild(dd);
            dd.AppendChild(doc.CreateTextNode("\n\t\t\t"));
            Element ul = doc.CreateElement("ul");
            dd.AppendChild(ul);
            ul.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            Element li = doc.CreateElement("li");
            ul.AppendChild(li);
            li.AppendChild(doc.CreateTextNode("\n\t\t\t\t the way \n\t\t\t\t"));
            ul.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            li = doc.CreateElement("li");
            ul.AppendChild(li);
            li.SetAttribute("id", "bar");
            li.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            Element p = doc.CreateElement("p");
            li.AppendChild(p);
            p.AppendChild(doc.CreateTextNode("\n\t\t\t\t the world ends \n\t\t\t\t"));
            li.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));

            Element form = doc.CreateElement("form");
            li.AppendChild(form);
            form.SetAttribute("action", "./");
            form.SetAttribute("method", "get");
            form.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t"));
            p = doc.CreateElement("p");
            form.AppendChild(p);
            p.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t bang \n\t\t\t\t\t"));
            Element input = doc.CreateElement("input");
            p.AppendChild(input);
            input.SetAttribute("name", "foo");
            input.SetAttribute("type", "radio");
            input.SetAttribute("value", "off");
            p.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t"));
            form.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t"));
            p = doc.CreateElement("p");
            form.AppendChild(p);
            p.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t whimper \n\t\t\t\t\t"));
            input = doc.CreateElement("input");
            p.AppendChild(input);
            input.SetAttribute("name", "foo2");
            input.SetAttribute("type", "radio");
            input.SetAttribute("value", "on");
            p.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t"));
            form.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            li.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            ul.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));

            li = doc.CreateElement("li");
            ul.AppendChild(li);
            li.AppendChild(doc.CreateTextNode("\n\t\t\t\t i grow old \n\t\t\t\t"));
            ul.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            li = doc.CreateElement("li");
            ul.AppendChild(li);
            li.SetAttribute("id", "baz");
            li.AppendChild(doc.CreateTextNode("\n\t\t\t\t pluot? \n\t\t\t\t"));
            ul.AppendChild(doc.CreateTextNode("\n\t\t\t"));
            dd.AppendChild(doc.CreateTextNode("\n\t\t\t"));

            Element blockquote = doc.CreateElement("blockquote");
            dd.AppendChild(blockquote);
            blockquote.AppendChild(doc.CreateTextNode("\n\t\t\t\t"));
            Element address = doc.CreateElement("address");
            blockquote.AppendChild(address);
            address.AppendChild(doc.CreateTextNode("\n\t\t\t\t\t bar maids, \n\t\t\t\t"));
            blockquote.AppendChild(doc.CreateTextNode("\n\t\t\t"));
            dd.AppendChild(doc.CreateTextNode("\n\t\t\t"));
            Element h1 = doc.CreateElement("h1");
            dd.AppendChild(h1);
            h1.AppendChild(doc.CreateTextNode("\n\t\t\t\t sing to me, erbarme dich \n\t\t\t"));
            dd.AppendChild(doc.CreateTextNode("\n\t\t\t"));
            dl.AppendChild(doc.CreateTextNode("\n\t\t"));
            body.AppendChild(doc.CreateTextNode("\n\t\t"));

            p = doc.CreateElement("p");
            body.AppendChild(p);
            p.SetAttribute("style", "color: black; font-size: 1em; line-height: 1.3em; clear: both");
            p.AppendChild(doc.CreateTextNode("\n\t\t This is a nonsensical document, but syntactically valid HTML 4.0. All 100%-conformant CSS1 agents should be able to render the document elements above this paragraph indistinguishably (to the pixel) from this \n\t\t\t"));
            Element a = doc.CreateElement("a");
            p.AppendChild(a);
            a.SetAttribute("href", "sec5526c.gif");
            a.AppendChild(doc.CreateTextNode("reference rendering,"));
            p.AppendChild(doc.CreateTextNode("\n\t\t (except font rasterization and form widgets). All discrepancies should be traceable to CSS1 implementation shortcomings. Once you have finished evaluating this test, you can return to the "));
            a = doc.CreateElement("a");
            p.AppendChild(a);
            a.SetAttribute("href", "sec5526c.htm");
            a.AppendChild(doc.CreateTextNode("parent page"));
            p.AppendChild(doc.CreateTextNode(". \n\t\t"));
            body.AppendChild(doc.CreateTextNode("\n\t\n\n"));

            return doc;
        }

        // Taken from MS Edge
        public const string Acid1Dom = @"NodeId : 0
NodeType : 9
NodeName : #document
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : null 
ParentNode : null 
ParentElement : null 
FirstChild : 1
LastChild : 2
PreviousSibling : null 
NextSibling : null 
NodeValue : === null
TextContent : === null
ChildNodes : 2 : 1, 2
ChildElementCount : undefined
FirstElementChild : undefined 
LastElementChild : undefined 
Children : undefined 
URL : http://localhost:14873/DomAnalyzer.html
DocumentURI : === undefined
Origin : === undefined
CompatMode : CSS1Compat
CharacterSet : utf-8
ContentType : === undefined
DocType : 1
DocumentElement : 2

NodeId : 1
NodeType : 10
NodeName : html
BaseUri : === null
OwnerDocument : 0
ParentNode : 0
ParentElement : null 
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 2
NodeValue : === null
TextContent : === null
ChildNodes : 0 : 
Name : html
PublicId : === empty
SystemId : === empty

NodeId : 2
NodeType : 1
NodeName : HTML
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 0
ParentElement : null 
FirstChild : 3
LastChild : 12
PreviousSibling : 1
NextSibling : null 
NodeValue : === null
TextContent : == CgkJCgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQogCQovKiBsYXN0IG1vZGlmaWVkOiAxIERlYyA5OCAqLyAJCgpodG1sIHsKZm9udDogMTBweC8xIFZlcmRhbmEsIHNhbnMtc2VyaWY7CmJhY2tncm91bmQtY29sb3I6IGJsdWU7CmNvbG9yOiB3aGl0ZTsKfQoKYm9keSB7Cm1hcmdpbjogMS41ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKcGFkZGluZzogMDsKd2lkdGg6IDQ4ZW07CmJhY2tncm91bmQtY29sb3I6IHdoaXRlOwp9CgpkbCB7Cm1hcmdpbjogMDsKYm9yZGVyOiAwOwpwYWRkaW5nOiAuNWVtOwp9CgpkdCB7IApiYWNrZ3JvdW5kLWNvbG9yOiByZ2IoMjA0LDAsMCk7Cm1hcmdpbjogMDsgCnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwLjYzOCU7IC8qIHJlZmVycyB0byBwYXJlbnQgZWxlbWVudCdzIHdpZHRoIG9mIDQ3ZW0uID0gNWVtIG9yIDUwcHggKi8KaGVpZ2h0OiAyOGVtOwpib3JkZXI6IC41ZW0gc29saWQgYmxhY2s7CmZsb2F0OiBsZWZ0Owp9CgpkZCB7CmZsb2F0OiByaWdodDsKbWFyZ2luOiAwIDAgMCAxZW07CmJvcmRlcjogMWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CndpZHRoOiAzNGVtOwpoZWlnaHQ6IDI3ZW07Cn0KCnVsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDA7Cn0KCmxpIHsKZGlzcGxheTogYmxvY2s7IC8qIGkuZS4sIHN1cHByZXNzIG1hcmtlciAqLwpjb2xvcjogYmxhY2s7CmhlaWdodDogOWVtOwp3aWR0aDogNWVtOwptYXJnaW46IDA7CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKcGFkZGluZzogMWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKfQoKI2JhciB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CndpZHRoOiA0MS4xNyU7IC8qID0gMTRlbSAqLwpib3JkZXI6IDA7Cm1hcmdpbjogMCAxZW07Cn0KCiNiYXogewptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKfQoKZm9ybSB7IAptYXJnaW46IDA7CmRpc3BsYXk6IGlubGluZTsKfQoKcCB7IAptYXJnaW46IDA7Cn0KCmZvcm0gcCB7CmxpbmUtaGVpZ2h0OiAxLjk7Cn0KCmJsb2NrcXVvdGUgewptYXJnaW46IDFlbSAxZW0gMWVtIDJlbTsKYm9yZGVyLXdpZHRoOiAxZW0gMS41ZW0gMmVtIC41ZW07CmJvcmRlci1zdHlsZTogc29saWQ7CmJvcmRlci1jb2xvcjogYmxhY2s7CnBhZGRpbmc6IDFlbSAwOwp3aWR0aDogNWVtOwpoZWlnaHQ6IDllbTsKZmxvYXQ6IGxlZnQ7CmJhY2tncm91bmQtY29sb3I6ICNGQzA7CmNvbG9yOiBibGFjazsKfQoKYWRkcmVzcyB7CmZvbnQtc3R5bGU6IG5vcm1hbDsKfQoKaDEgewpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwpmbG9hdDogbGVmdDsKbWFyZ2luOiAxZW0gMDsKYm9yZGVyOiAwOwpwYWRkaW5nOiAxZW07CndpZHRoOiAxMGVtOwpoZWlnaHQ6IDEwZW07CmZvbnQtd2VpZ2h0OiBub3JtYWw7CmZvbnQtc2l6ZTogMWVtOwp9CiAgCgkKCQoJCQoJCQkKCQkJIHRvZ2dsZSAKCQkJCgkJCQoJCQkKCQkJCQoJCQkJIHRoZSB3YXkgCgkJCQkKCQkJCQoJCQkJCgkJCQkgdGhlIHdvcmxkIGVuZHMgCgkJCQkKCQkJCQoJCQkJCQoJCQkJCSBiYW5nIAoJCQkJCQoJCQkJCQoJCQkJCQoJCQkJCSB3aGltcGVyIAoJCQkJCQoJCQkJCQoJCQkJCgkJCQkKCQkJCQoJCQkJIGkgZ3JvdyBvbGQgCgkJCQkKCQkJCQoJCQkJIHBsdW90PyAKCQkJCQoJCQkKCQkJCgkJCQkKCQkJCQkgYmFyIG1haWRzLCAKCQkJCQoJCQkKCQkJCgkJCQkgc2luZyB0byBtZSwgZXJiYXJtZSBkaWNoIAoJCQkKCQkJCgkJCgkJCgkJIFRoaXMgaXMgYSBub25zZW5zaWNhbCBkb2N1bWVudCwgYnV0IHN5bnRhY3RpY2FsbHkgdmFsaWQgSFRNTCA0LjAuIEFsbCAxMDAlLWNvbmZvcm1hbnQgQ1NTMSBhZ2VudHMgc2hvdWxkIGJlIGFibGUgdG8gcmVuZGVyIHRoZSBkb2N1bWVudCBlbGVtZW50cyBhYm92ZSB0aGlzIHBhcmFncmFwaCBpbmRpc3Rpbmd1aXNoYWJseSAodG8gdGhlIHBpeGVsKSBmcm9tIHRoaXMgCgkJCXJlZmVyZW5jZSByZW5kZXJpbmcsCgkJIChleGNlcHQgZm9udCByYXN0ZXJpemF0aW9uIGFuZCBmb3JtIHdpZGdldHMpLiBBbGwgZGlzY3JlcGFuY2llcyBzaG91bGQgYmUgdHJhY2VhYmxlIHRvIENTUzEgaW1wbGVtZW50YXRpb24gc2hvcnRjb21pbmdzLiBPbmNlIHlvdSBoYXZlIGZpbmlzaGVkIGV2YWx1YXRpbmcgdGhpcyB0ZXN0LCB5b3UgY2FuIHJldHVybiB0byB0aGUgcGFyZW50IHBhZ2UuIAoJCQoJCgo=
ChildNodes : 3 : 3, 11, 12
ChildElementCount : 2
FirstElementChild : 3
LastElementChild : 12
Children : 2 : 3, 12
PreviousElementSibling : null 
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : html
TagName : HTML
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 3
NodeType : 1
NodeName : HEAD
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 2
ParentElement : 2
FirstChild : 4
LastChild : 10
PreviousSibling : null 
NextSibling : 11
NodeValue : === null
TextContent : == CgkJCgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQogCQovKiBsYXN0IG1vZGlmaWVkOiAxIERlYyA5OCAqLyAJCgpodG1sIHsKZm9udDogMTBweC8xIFZlcmRhbmEsIHNhbnMtc2VyaWY7CmJhY2tncm91bmQtY29sb3I6IGJsdWU7CmNvbG9yOiB3aGl0ZTsKfQoKYm9keSB7Cm1hcmdpbjogMS41ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKcGFkZGluZzogMDsKd2lkdGg6IDQ4ZW07CmJhY2tncm91bmQtY29sb3I6IHdoaXRlOwp9CgpkbCB7Cm1hcmdpbjogMDsKYm9yZGVyOiAwOwpwYWRkaW5nOiAuNWVtOwp9CgpkdCB7IApiYWNrZ3JvdW5kLWNvbG9yOiByZ2IoMjA0LDAsMCk7Cm1hcmdpbjogMDsgCnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwLjYzOCU7IC8qIHJlZmVycyB0byBwYXJlbnQgZWxlbWVudCdzIHdpZHRoIG9mIDQ3ZW0uID0gNWVtIG9yIDUwcHggKi8KaGVpZ2h0OiAyOGVtOwpib3JkZXI6IC41ZW0gc29saWQgYmxhY2s7CmZsb2F0OiBsZWZ0Owp9CgpkZCB7CmZsb2F0OiByaWdodDsKbWFyZ2luOiAwIDAgMCAxZW07CmJvcmRlcjogMWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CndpZHRoOiAzNGVtOwpoZWlnaHQ6IDI3ZW07Cn0KCnVsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDA7Cn0KCmxpIHsKZGlzcGxheTogYmxvY2s7IC8qIGkuZS4sIHN1cHByZXNzIG1hcmtlciAqLwpjb2xvcjogYmxhY2s7CmhlaWdodDogOWVtOwp3aWR0aDogNWVtOwptYXJnaW46IDA7CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKcGFkZGluZzogMWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKfQoKI2JhciB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CndpZHRoOiA0MS4xNyU7IC8qID0gMTRlbSAqLwpib3JkZXI6IDA7Cm1hcmdpbjogMCAxZW07Cn0KCiNiYXogewptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKfQoKZm9ybSB7IAptYXJnaW46IDA7CmRpc3BsYXk6IGlubGluZTsKfQoKcCB7IAptYXJnaW46IDA7Cn0KCmZvcm0gcCB7CmxpbmUtaGVpZ2h0OiAxLjk7Cn0KCmJsb2NrcXVvdGUgewptYXJnaW46IDFlbSAxZW0gMWVtIDJlbTsKYm9yZGVyLXdpZHRoOiAxZW0gMS41ZW0gMmVtIC41ZW07CmJvcmRlci1zdHlsZTogc29saWQ7CmJvcmRlci1jb2xvcjogYmxhY2s7CnBhZGRpbmc6IDFlbSAwOwp3aWR0aDogNWVtOwpoZWlnaHQ6IDllbTsKZmxvYXQ6IGxlZnQ7CmJhY2tncm91bmQtY29sb3I6ICNGQzA7CmNvbG9yOiBibGFjazsKfQoKYWRkcmVzcyB7CmZvbnQtc3R5bGU6IG5vcm1hbDsKfQoKaDEgewpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwpmbG9hdDogbGVmdDsKbWFyZ2luOiAxZW0gMDsKYm9yZGVyOiAwOwpwYWRkaW5nOiAxZW07CndpZHRoOiAxMGVtOwpoZWlnaHQ6IDEwZW07CmZvbnQtd2VpZ2h0OiBub3JtYWw7CmZvbnQtc2l6ZTogMWVtOwp9CiAgCgk=
ChildNodes : 5 : 4, 5, 7, 8, 10
ChildElementCount : 2
FirstElementChild : 5
LastElementChild : 8
Children : 2 : 5, 8
PreviousElementSibling : null 
NextElementSibling : 12
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : head
TagName : HEAD
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 4
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 3
ParentElement : 3
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 5
NodeValue : == CgkJ
TextContent : == CgkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJ
Length : 3
WholeText : == CgkJ

NodeId : 5
NodeType : 1
NodeName : TITLE
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 3
ParentElement : 3
FirstChild : 6
LastChild : 6
PreviousSibling : 4
NextSibling : 7
NodeValue : === null
TextContent : == CgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQ==
ChildNodes : 1 : 6
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : 8
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : title
TagName : TITLE
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 6
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 5
ParentElement : 5
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQ==
TextContent : == CgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQ==
Length : 37
WholeText : == CgkJCSBkaXNwbGF5L2JveC9mbG9hdC9jbGVhciB0ZXN0IAoJCQ==

NodeId : 7
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 3
ParentElement : 3
FirstChild : null 
LastChild : null 
PreviousSibling : 5
NextSibling : 8
NodeValue : == CiAJ
TextContent : == CiAJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CiAJ
Length : 3
WholeText : == CiAJ

NodeId : 8
NodeType : 1
NodeName : STYLE
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 3
ParentElement : 3
FirstChild : 9
LastChild : 9
PreviousSibling : 7
NextSibling : 10
NodeValue : === null
TextContent : == Ci8qIGxhc3QgbW9kaWZpZWQ6IDEgRGVjIDk4ICovIAkKCmh0bWwgewpmb250OiAxMHB4LzEgVmVyZGFuYSwgc2Fucy1zZXJpZjsKYmFja2dyb3VuZC1jb2xvcjogYmx1ZTsKY29sb3I6IHdoaXRlOwp9Cgpib2R5IHsKbWFyZ2luOiAxLjVlbTsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAwOwp3aWR0aDogNDhlbTsKYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7Cn0KCmRsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IC41ZW07Cn0KCmR0IHsgCmJhY2tncm91bmQtY29sb3I6IHJnYigyMDQsMCwwKTsKbWFyZ2luOiAwOyAKcGFkZGluZzogMWVtOwp3aWR0aDogMTAuNjM4JTsgLyogcmVmZXJzIHRvIHBhcmVudCBlbGVtZW50J3Mgd2lkdGggb2YgNDdlbS4gPSA1ZW0gb3IgNTBweCAqLwpoZWlnaHQ6IDI4ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKZmxvYXQ6IGxlZnQ7Cn0KCmRkIHsKZmxvYXQ6IHJpZ2h0OwptYXJnaW46IDAgMCAwIDFlbTsKYm9yZGVyOiAxZW0gc29saWQgYmxhY2s7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDM0ZW07CmhlaWdodDogMjdlbTsKfQoKdWwgewptYXJnaW46IDA7CmJvcmRlcjogMDsKcGFkZGluZzogMDsKfQoKbGkgewpkaXNwbGF5OiBibG9jazsgLyogaS5lLiwgc3VwcHJlc3MgbWFya2VyICovCmNvbG9yOiBibGFjazsKaGVpZ2h0OiA5ZW07CndpZHRoOiA1ZW07Cm1hcmdpbjogMDsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CmZsb2F0OiBsZWZ0OwpiYWNrZ3JvdW5kLWNvbG9yOiAjRkMwOwp9CgojYmFyIHsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKd2lkdGg6IDQxLjE3JTsgLyogPSAxNGVtICovCmJvcmRlcjogMDsKbWFyZ2luOiAwIDFlbTsKfQoKI2JheiB7Cm1hcmdpbjogMWVtIDA7CmJvcmRlcjogMDsKcGFkZGluZzogMWVtOwp3aWR0aDogMTBlbTsKaGVpZ2h0OiAxMGVtOwpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwp9Cgpmb3JtIHsgCm1hcmdpbjogMDsKZGlzcGxheTogaW5saW5lOwp9CgpwIHsgCm1hcmdpbjogMDsKfQoKZm9ybSBwIHsKbGluZS1oZWlnaHQ6IDEuOTsKfQoKYmxvY2txdW90ZSB7Cm1hcmdpbjogMWVtIDFlbSAxZW0gMmVtOwpib3JkZXItd2lkdGg6IDFlbSAxLjVlbSAyZW0gLjVlbTsKYm9yZGVyLXN0eWxlOiBzb2xpZDsKYm9yZGVyLWNvbG9yOiBibGFjazsKcGFkZGluZzogMWVtIDA7CndpZHRoOiA1ZW07CmhlaWdodDogOWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKY29sb3I6IGJsYWNrOwp9CgphZGRyZXNzIHsKZm9udC1zdHlsZTogbm9ybWFsOwp9CgpoMSB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CmZsb2F0OiBsZWZ0OwptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKZm9udC13ZWlnaHQ6IG5vcm1hbDsKZm9udC1zaXplOiAxZW07Cn0KICA=
ChildNodes : 1 : 9
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : 5
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : style
TagName : STYLE
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 1

NamespaceURI : === null
Prefix : === null
LocalName : type
Name : type
Value : text/css

NodeId : 9
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 8
ParentElement : 8
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == Ci8qIGxhc3QgbW9kaWZpZWQ6IDEgRGVjIDk4ICovIAkKCmh0bWwgewpmb250OiAxMHB4LzEgVmVyZGFuYSwgc2Fucy1zZXJpZjsKYmFja2dyb3VuZC1jb2xvcjogYmx1ZTsKY29sb3I6IHdoaXRlOwp9Cgpib2R5IHsKbWFyZ2luOiAxLjVlbTsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAwOwp3aWR0aDogNDhlbTsKYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7Cn0KCmRsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IC41ZW07Cn0KCmR0IHsgCmJhY2tncm91bmQtY29sb3I6IHJnYigyMDQsMCwwKTsKbWFyZ2luOiAwOyAKcGFkZGluZzogMWVtOwp3aWR0aDogMTAuNjM4JTsgLyogcmVmZXJzIHRvIHBhcmVudCBlbGVtZW50J3Mgd2lkdGggb2YgNDdlbS4gPSA1ZW0gb3IgNTBweCAqLwpoZWlnaHQ6IDI4ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKZmxvYXQ6IGxlZnQ7Cn0KCmRkIHsKZmxvYXQ6IHJpZ2h0OwptYXJnaW46IDAgMCAwIDFlbTsKYm9yZGVyOiAxZW0gc29saWQgYmxhY2s7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDM0ZW07CmhlaWdodDogMjdlbTsKfQoKdWwgewptYXJnaW46IDA7CmJvcmRlcjogMDsKcGFkZGluZzogMDsKfQoKbGkgewpkaXNwbGF5OiBibG9jazsgLyogaS5lLiwgc3VwcHJlc3MgbWFya2VyICovCmNvbG9yOiBibGFjazsKaGVpZ2h0OiA5ZW07CndpZHRoOiA1ZW07Cm1hcmdpbjogMDsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CmZsb2F0OiBsZWZ0OwpiYWNrZ3JvdW5kLWNvbG9yOiAjRkMwOwp9CgojYmFyIHsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKd2lkdGg6IDQxLjE3JTsgLyogPSAxNGVtICovCmJvcmRlcjogMDsKbWFyZ2luOiAwIDFlbTsKfQoKI2JheiB7Cm1hcmdpbjogMWVtIDA7CmJvcmRlcjogMDsKcGFkZGluZzogMWVtOwp3aWR0aDogMTBlbTsKaGVpZ2h0OiAxMGVtOwpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwp9Cgpmb3JtIHsgCm1hcmdpbjogMDsKZGlzcGxheTogaW5saW5lOwp9CgpwIHsgCm1hcmdpbjogMDsKfQoKZm9ybSBwIHsKbGluZS1oZWlnaHQ6IDEuOTsKfQoKYmxvY2txdW90ZSB7Cm1hcmdpbjogMWVtIDFlbSAxZW0gMmVtOwpib3JkZXItd2lkdGg6IDFlbSAxLjVlbSAyZW0gLjVlbTsKYm9yZGVyLXN0eWxlOiBzb2xpZDsKYm9yZGVyLWNvbG9yOiBibGFjazsKcGFkZGluZzogMWVtIDA7CndpZHRoOiA1ZW07CmhlaWdodDogOWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKY29sb3I6IGJsYWNrOwp9CgphZGRyZXNzIHsKZm9udC1zdHlsZTogbm9ybWFsOwp9CgpoMSB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CmZsb2F0OiBsZWZ0OwptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKZm9udC13ZWlnaHQ6IG5vcm1hbDsKZm9udC1zaXplOiAxZW07Cn0KICA=
TextContent : == Ci8qIGxhc3QgbW9kaWZpZWQ6IDEgRGVjIDk4ICovIAkKCmh0bWwgewpmb250OiAxMHB4LzEgVmVyZGFuYSwgc2Fucy1zZXJpZjsKYmFja2dyb3VuZC1jb2xvcjogYmx1ZTsKY29sb3I6IHdoaXRlOwp9Cgpib2R5IHsKbWFyZ2luOiAxLjVlbTsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAwOwp3aWR0aDogNDhlbTsKYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7Cn0KCmRsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IC41ZW07Cn0KCmR0IHsgCmJhY2tncm91bmQtY29sb3I6IHJnYigyMDQsMCwwKTsKbWFyZ2luOiAwOyAKcGFkZGluZzogMWVtOwp3aWR0aDogMTAuNjM4JTsgLyogcmVmZXJzIHRvIHBhcmVudCBlbGVtZW50J3Mgd2lkdGggb2YgNDdlbS4gPSA1ZW0gb3IgNTBweCAqLwpoZWlnaHQ6IDI4ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKZmxvYXQ6IGxlZnQ7Cn0KCmRkIHsKZmxvYXQ6IHJpZ2h0OwptYXJnaW46IDAgMCAwIDFlbTsKYm9yZGVyOiAxZW0gc29saWQgYmxhY2s7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDM0ZW07CmhlaWdodDogMjdlbTsKfQoKdWwgewptYXJnaW46IDA7CmJvcmRlcjogMDsKcGFkZGluZzogMDsKfQoKbGkgewpkaXNwbGF5OiBibG9jazsgLyogaS5lLiwgc3VwcHJlc3MgbWFya2VyICovCmNvbG9yOiBibGFjazsKaGVpZ2h0OiA5ZW07CndpZHRoOiA1ZW07Cm1hcmdpbjogMDsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CmZsb2F0OiBsZWZ0OwpiYWNrZ3JvdW5kLWNvbG9yOiAjRkMwOwp9CgojYmFyIHsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKd2lkdGg6IDQxLjE3JTsgLyogPSAxNGVtICovCmJvcmRlcjogMDsKbWFyZ2luOiAwIDFlbTsKfQoKI2JheiB7Cm1hcmdpbjogMWVtIDA7CmJvcmRlcjogMDsKcGFkZGluZzogMWVtOwp3aWR0aDogMTBlbTsKaGVpZ2h0OiAxMGVtOwpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwp9Cgpmb3JtIHsgCm1hcmdpbjogMDsKZGlzcGxheTogaW5saW5lOwp9CgpwIHsgCm1hcmdpbjogMDsKfQoKZm9ybSBwIHsKbGluZS1oZWlnaHQ6IDEuOTsKfQoKYmxvY2txdW90ZSB7Cm1hcmdpbjogMWVtIDFlbSAxZW0gMmVtOwpib3JkZXItd2lkdGg6IDFlbSAxLjVlbSAyZW0gLjVlbTsKYm9yZGVyLXN0eWxlOiBzb2xpZDsKYm9yZGVyLWNvbG9yOiBibGFjazsKcGFkZGluZzogMWVtIDA7CndpZHRoOiA1ZW07CmhlaWdodDogOWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKY29sb3I6IGJsYWNrOwp9CgphZGRyZXNzIHsKZm9udC1zdHlsZTogbm9ybWFsOwp9CgpoMSB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CmZsb2F0OiBsZWZ0OwptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKZm9udC13ZWlnaHQ6IG5vcm1hbDsKZm9udC1zaXplOiAxZW07Cn0KICA=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == Ci8qIGxhc3QgbW9kaWZpZWQ6IDEgRGVjIDk4ICovIAkKCmh0bWwgewpmb250OiAxMHB4LzEgVmVyZGFuYSwgc2Fucy1zZXJpZjsKYmFja2dyb3VuZC1jb2xvcjogYmx1ZTsKY29sb3I6IHdoaXRlOwp9Cgpib2R5IHsKbWFyZ2luOiAxLjVlbTsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAwOwp3aWR0aDogNDhlbTsKYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7Cn0KCmRsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IC41ZW07Cn0KCmR0IHsgCmJhY2tncm91bmQtY29sb3I6IHJnYigyMDQsMCwwKTsKbWFyZ2luOiAwOyAKcGFkZGluZzogMWVtOwp3aWR0aDogMTAuNjM4JTsgLyogcmVmZXJzIHRvIHBhcmVudCBlbGVtZW50J3Mgd2lkdGggb2YgNDdlbS4gPSA1ZW0gb3IgNTBweCAqLwpoZWlnaHQ6IDI4ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKZmxvYXQ6IGxlZnQ7Cn0KCmRkIHsKZmxvYXQ6IHJpZ2h0OwptYXJnaW46IDAgMCAwIDFlbTsKYm9yZGVyOiAxZW0gc29saWQgYmxhY2s7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDM0ZW07CmhlaWdodDogMjdlbTsKfQoKdWwgewptYXJnaW46IDA7CmJvcmRlcjogMDsKcGFkZGluZzogMDsKfQoKbGkgewpkaXNwbGF5OiBibG9jazsgLyogaS5lLiwgc3VwcHJlc3MgbWFya2VyICovCmNvbG9yOiBibGFjazsKaGVpZ2h0OiA5ZW07CndpZHRoOiA1ZW07Cm1hcmdpbjogMDsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CmZsb2F0OiBsZWZ0OwpiYWNrZ3JvdW5kLWNvbG9yOiAjRkMwOwp9CgojYmFyIHsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKd2lkdGg6IDQxLjE3JTsgLyogPSAxNGVtICovCmJvcmRlcjogMDsKbWFyZ2luOiAwIDFlbTsKfQoKI2JheiB7Cm1hcmdpbjogMWVtIDA7CmJvcmRlcjogMDsKcGFkZGluZzogMWVtOwp3aWR0aDogMTBlbTsKaGVpZ2h0OiAxMGVtOwpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwp9Cgpmb3JtIHsgCm1hcmdpbjogMDsKZGlzcGxheTogaW5saW5lOwp9CgpwIHsgCm1hcmdpbjogMDsKfQoKZm9ybSBwIHsKbGluZS1oZWlnaHQ6IDEuOTsKfQoKYmxvY2txdW90ZSB7Cm1hcmdpbjogMWVtIDFlbSAxZW0gMmVtOwpib3JkZXItd2lkdGg6IDFlbSAxLjVlbSAyZW0gLjVlbTsKYm9yZGVyLXN0eWxlOiBzb2xpZDsKYm9yZGVyLWNvbG9yOiBibGFjazsKcGFkZGluZzogMWVtIDA7CndpZHRoOiA1ZW07CmhlaWdodDogOWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKY29sb3I6IGJsYWNrOwp9CgphZGRyZXNzIHsKZm9udC1zdHlsZTogbm9ybWFsOwp9CgpoMSB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CmZsb2F0OiBsZWZ0OwptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKZm9udC13ZWlnaHQ6IG5vcm1hbDsKZm9udC1zaXplOiAxZW07Cn0KICA=
Length : 1502
WholeText : == Ci8qIGxhc3QgbW9kaWZpZWQ6IDEgRGVjIDk4ICovIAkKCmh0bWwgewpmb250OiAxMHB4LzEgVmVyZGFuYSwgc2Fucy1zZXJpZjsKYmFja2dyb3VuZC1jb2xvcjogYmx1ZTsKY29sb3I6IHdoaXRlOwp9Cgpib2R5IHsKbWFyZ2luOiAxLjVlbTsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAwOwp3aWR0aDogNDhlbTsKYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7Cn0KCmRsIHsKbWFyZ2luOiAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IC41ZW07Cn0KCmR0IHsgCmJhY2tncm91bmQtY29sb3I6IHJnYigyMDQsMCwwKTsKbWFyZ2luOiAwOyAKcGFkZGluZzogMWVtOwp3aWR0aDogMTAuNjM4JTsgLyogcmVmZXJzIHRvIHBhcmVudCBlbGVtZW50J3Mgd2lkdGggb2YgNDdlbS4gPSA1ZW0gb3IgNTBweCAqLwpoZWlnaHQ6IDI4ZW07CmJvcmRlcjogLjVlbSBzb2xpZCBibGFjazsKZmxvYXQ6IGxlZnQ7Cn0KCmRkIHsKZmxvYXQ6IHJpZ2h0OwptYXJnaW46IDAgMCAwIDFlbTsKYm9yZGVyOiAxZW0gc29saWQgYmxhY2s7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDM0ZW07CmhlaWdodDogMjdlbTsKfQoKdWwgewptYXJnaW46IDA7CmJvcmRlcjogMDsKcGFkZGluZzogMDsKfQoKbGkgewpkaXNwbGF5OiBibG9jazsgLyogaS5lLiwgc3VwcHJlc3MgbWFya2VyICovCmNvbG9yOiBibGFjazsKaGVpZ2h0OiA5ZW07CndpZHRoOiA1ZW07Cm1hcmdpbjogMDsKYm9yZGVyOiAuNWVtIHNvbGlkIGJsYWNrOwpwYWRkaW5nOiAxZW07CmZsb2F0OiBsZWZ0OwpiYWNrZ3JvdW5kLWNvbG9yOiAjRkMwOwp9CgojYmFyIHsKYmFja2dyb3VuZC1jb2xvcjogYmxhY2s7CmNvbG9yOiB3aGl0ZTsKd2lkdGg6IDQxLjE3JTsgLyogPSAxNGVtICovCmJvcmRlcjogMDsKbWFyZ2luOiAwIDFlbTsKfQoKI2JheiB7Cm1hcmdpbjogMWVtIDA7CmJvcmRlcjogMDsKcGFkZGluZzogMWVtOwp3aWR0aDogMTBlbTsKaGVpZ2h0OiAxMGVtOwpiYWNrZ3JvdW5kLWNvbG9yOiBibGFjazsKY29sb3I6IHdoaXRlOwp9Cgpmb3JtIHsgCm1hcmdpbjogMDsKZGlzcGxheTogaW5saW5lOwp9CgpwIHsgCm1hcmdpbjogMDsKfQoKZm9ybSBwIHsKbGluZS1oZWlnaHQ6IDEuOTsKfQoKYmxvY2txdW90ZSB7Cm1hcmdpbjogMWVtIDFlbSAxZW0gMmVtOwpib3JkZXItd2lkdGg6IDFlbSAxLjVlbSAyZW0gLjVlbTsKYm9yZGVyLXN0eWxlOiBzb2xpZDsKYm9yZGVyLWNvbG9yOiBibGFjazsKcGFkZGluZzogMWVtIDA7CndpZHRoOiA1ZW07CmhlaWdodDogOWVtOwpmbG9hdDogbGVmdDsKYmFja2dyb3VuZC1jb2xvcjogI0ZDMDsKY29sb3I6IGJsYWNrOwp9CgphZGRyZXNzIHsKZm9udC1zdHlsZTogbm9ybWFsOwp9CgpoMSB7CmJhY2tncm91bmQtY29sb3I6IGJsYWNrOwpjb2xvcjogd2hpdGU7CmZsb2F0OiBsZWZ0OwptYXJnaW46IDFlbSAwOwpib3JkZXI6IDA7CnBhZGRpbmc6IDFlbTsKd2lkdGg6IDEwZW07CmhlaWdodDogMTBlbTsKZm9udC13ZWlnaHQ6IG5vcm1hbDsKZm9udC1zaXplOiAxZW07Cn0KICA=

NodeId : 10
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 3
ParentElement : 3
FirstChild : null 
LastChild : null 
PreviousSibling : 8
NextSibling : null 
NodeValue : == Cgk=
TextContent : == Cgk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == Cgk=
Length : 2
WholeText : == Cgk=

NodeId : 11
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 2
ParentElement : 2
FirstChild : null 
LastChild : null 
PreviousSibling : 3
NextSibling : 12
NodeValue : == Cgk=
TextContent : == Cgk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == Cgk=
Length : 2
WholeText : == Cgk=

NodeId : 12
NodeType : 1
NodeName : BODY
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 2
ParentElement : 2
FirstChild : 13
LastChild : 71
PreviousSibling : 11
NextSibling : null 
NodeValue : === null
TextContent : == CgkJCgkJCQoJCQkgdG9nZ2xlIAoJCQkKCQkJCgkJCQoJCQkJCgkJCQkgdGhlIHdheSAKCQkJCQoJCQkJCgkJCQkKCQkJCSB0aGUgd29ybGQgZW5kcyAKCQkJCQoJCQkJCgkJCQkJCgkJCQkJIGJhbmcgCgkJCQkJCgkJCQkJCgkJCQkJCgkJCQkJIHdoaW1wZXIgCgkJCQkJCgkJCQkJCgkJCQkKCQkJCQoJCQkJCgkJCQkgaSBncm93IG9sZCAKCQkJCQoJCQkJCgkJCQkgcGx1b3Q/IAoJCQkJCgkJCQoJCQkKCQkJCQoJCQkJCSBiYXIgbWFpZHMsIAoJCQkJCgkJCQoJCQkKCQkJCSBzaW5nIHRvIG1lLCBlcmJhcm1lIGRpY2ggCgkJCQoJCQkKCQkKCQkKCQkgVGhpcyBpcyBhIG5vbnNlbnNpY2FsIGRvY3VtZW50LCBidXQgc3ludGFjdGljYWxseSB2YWxpZCBIVE1MIDQuMC4gQWxsIDEwMCUtY29uZm9ybWFudCBDU1MxIGFnZW50cyBzaG91bGQgYmUgYWJsZSB0byByZW5kZXIgdGhlIGRvY3VtZW50IGVsZW1lbnRzIGFib3ZlIHRoaXMgcGFyYWdyYXBoIGluZGlzdGluZ3Vpc2hhYmx5ICh0byB0aGUgcGl4ZWwpIGZyb20gdGhpcyAKCQkJcmVmZXJlbmNlIHJlbmRlcmluZywKCQkgKGV4Y2VwdCBmb250IHJhc3Rlcml6YXRpb24gYW5kIGZvcm0gd2lkZ2V0cykuIEFsbCBkaXNjcmVwYW5jaWVzIHNob3VsZCBiZSB0cmFjZWFibGUgdG8gQ1NTMSBpbXBsZW1lbnRhdGlvbiBzaG9ydGNvbWluZ3MuIE9uY2UgeW91IGhhdmUgZmluaXNoZWQgZXZhbHVhdGluZyB0aGlzIHRlc3QsIHlvdSBjYW4gcmV0dXJuIHRvIHRoZSBwYXJlbnQgcGFnZS4gCgkJCgkKCg==
ChildNodes : 5 : 13, 14, 62, 63, 71
ChildElementCount : 2
FirstElementChild : 14
LastElementChild : 63
Children : 2 : 14, 63
PreviousElementSibling : 3
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : body
TagName : BODY
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 13
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 12
ParentElement : 12
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 14
NodeValue : == CgkJ
TextContent : == CgkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJ
Length : 3
WholeText : == CgkJ

NodeId : 14
NodeType : 1
NodeName : DL
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 12
ParentElement : 12
FirstChild : 15
LastChild : 61
PreviousSibling : 13
NextSibling : 62
NodeValue : === null
TextContent : == CgkJCQoJCQkgdG9nZ2xlIAoJCQkKCQkJCgkJCQoJCQkJCgkJCQkgdGhlIHdheSAKCQkJCQoJCQkJCgkJCQkKCQkJCSB0aGUgd29ybGQgZW5kcyAKCQkJCQoJCQkJCgkJCQkJCgkJCQkJIGJhbmcgCgkJCQkJCgkJCQkJCgkJCQkJCgkJCQkJIHdoaW1wZXIgCgkJCQkJCgkJCQkJCgkJCQkKCQkJCQoJCQkJCgkJCQkgaSBncm93IG9sZCAKCQkJCQoJCQkJCgkJCQkgcGx1b3Q/IAoJCQkJCgkJCQoJCQkKCQkJCQoJCQkJCSBiYXIgbWFpZHMsIAoJCQkJCgkJCQoJCQkKCQkJCSBzaW5nIHRvIG1lLCBlcmJhcm1lIGRpY2ggCgkJCQoJCQkKCQk=
ChildNodes : 5 : 15, 16, 18, 19, 61
ChildElementCount : 2
FirstElementChild : 16
LastElementChild : 19
Children : 2 : 16, 19
PreviousElementSibling : null 
NextElementSibling : 63
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : dl
TagName : DL
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 15
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 14
ParentElement : 14
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 16
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 16
NodeType : 1
NodeName : DT
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 14
ParentElement : 14
FirstChild : 17
LastChild : 17
PreviousSibling : 15
NextSibling : 18
NodeValue : === null
TextContent : == CgkJCSB0b2dnbGUgCgkJCQ==
ChildNodes : 1 : 17
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : 19
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : dt
TagName : DT
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 17
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 16
ParentElement : 16
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCSB0b2dnbGUgCgkJCQ==
TextContent : == CgkJCSB0b2dnbGUgCgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCSB0b2dnbGUgCgkJCQ==
Length : 16
WholeText : == CgkJCSB0b2dnbGUgCgkJCQ==

NodeId : 18
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 14
ParentElement : 14
FirstChild : null 
LastChild : null 
PreviousSibling : 16
NextSibling : 19
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 19
NodeType : 1
NodeName : DD
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 14
ParentElement : 14
FirstChild : 20
LastChild : 60
PreviousSibling : 18
NextSibling : 61
NodeValue : === null
TextContent : == CgkJCQoJCQkJCgkJCQkgdGhlIHdheSAKCQkJCQoJCQkJCgkJCQkKCQkJCSB0aGUgd29ybGQgZW5kcyAKCQkJCQoJCQkJCgkJCQkJCgkJCQkJIGJhbmcgCgkJCQkJCgkJCQkJCgkJCQkJCgkJCQkJIHdoaW1wZXIgCgkJCQkJCgkJCQkJCgkJCQkKCQkJCQoJCQkJCgkJCQkgaSBncm93IG9sZCAKCQkJCQoJCQkJCgkJCQkgcGx1b3Q/IAoJCQkJCgkJCQoJCQkKCQkJCQoJCQkJCSBiYXIgbWFpZHMsIAoJCQkJCgkJCQoJCQkKCQkJCSBzaW5nIHRvIG1lLCBlcmJhcm1lIGRpY2ggCgkJCQoJCQk=
ChildNodes : 7 : 20, 21, 51, 52, 57, 58, 60
ChildElementCount : 3
FirstElementChild : 21
LastElementChild : 58
Children : 3 : 21, 52, 58
PreviousElementSibling : 16
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : dd
TagName : DD
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 20
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 21
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 21
NodeType : 1
NodeName : UL
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : 22
LastChild : 50
PreviousSibling : 20
NextSibling : 51
NodeValue : === null
TextContent : == CgkJCQkKCQkJCSB0aGUgd2F5IAoJCQkJCgkJCQkKCQkJCQoJCQkJIHRoZSB3b3JsZCBlbmRzIAoJCQkJCgkJCQkKCQkJCQkKCQkJCQkgYmFuZyAKCQkJCQkKCQkJCQkKCQkJCQkKCQkJCQkgd2hpbXBlciAKCQkJCQkKCQkJCQkKCQkJCQoJCQkJCgkJCQkKCQkJCSBpIGdyb3cgb2xkIAoJCQkJCgkJCQkKCQkJCSBwbHVvdD8gCgkJCQkKCQkJ
ChildNodes : 9 : 22, 23, 25, 26, 44, 45, 47, 48, 50
ChildElementCount : 4
FirstElementChild : 23
LastElementChild : 48
Children : 4 : 23, 26, 45, 48
PreviousElementSibling : null 
NextElementSibling : 52
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : ul
TagName : UL
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 22
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 23
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 23
NodeType : 1
NodeName : LI
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : 24
LastChild : 24
PreviousSibling : 22
NextSibling : 25
NodeValue : === null
TextContent : == CgkJCQkgdGhlIHdheSAKCQkJCQ==
ChildNodes : 1 : 24
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : 26
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : li
TagName : LI
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 24
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 23
ParentElement : 23
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCQkgdGhlIHdheSAKCQkJCQ==
TextContent : == CgkJCQkgdGhlIHdheSAKCQkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkgdGhlIHdheSAKCQkJCQ==
Length : 19
WholeText : == CgkJCQkgdGhlIHdheSAKCQkJCQ==

NodeId : 25
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : null 
LastChild : null 
PreviousSibling : 23
NextSibling : 26
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 26
NodeType : 1
NodeName : LI
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : 27
LastChild : 43
PreviousSibling : 25
NextSibling : 44
NodeValue : === null
TextContent : == CgkJCQkKCQkJCSB0aGUgd29ybGQgZW5kcyAKCQkJCQoJCQkJCgkJCQkJCgkJCQkJIGJhbmcgCgkJCQkJCgkJCQkJCgkJCQkJCgkJCQkJIHdoaW1wZXIgCgkJCQkJCgkJCQkJCgkJCQkKCQkJCQ==
ChildNodes : 5 : 27, 28, 30, 31, 43
ChildElementCount : 2
FirstElementChild : 28
LastElementChild : 31
Children : 2 : 28, 31
PreviousElementSibling : 23
NextElementSibling : 45
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : li
TagName : LI
Id : bar
ClassName : === empty
ClassList : 0 : 
Attributes : 1

NamespaceURI : === null
Prefix : === null
LocalName : id
Name : id
Value : bar

NodeId : 27
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 26
ParentElement : 26
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 28
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 28
NodeType : 1
NodeName : P
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 26
ParentElement : 26
FirstChild : 29
LastChild : 29
PreviousSibling : 27
NextSibling : 30
NodeValue : === null
TextContent : == CgkJCQkgdGhlIHdvcmxkIGVuZHMgCgkJCQk=
ChildNodes : 1 : 29
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : 31
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : p
TagName : P
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 29
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 28
ParentElement : 28
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCQkgdGhlIHdvcmxkIGVuZHMgCgkJCQk=
TextContent : == CgkJCQkgdGhlIHdvcmxkIGVuZHMgCgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkgdGhlIHdvcmxkIGVuZHMgCgkJCQk=
Length : 26
WholeText : == CgkJCQkgdGhlIHdvcmxkIGVuZHMgCgkJCQk=

NodeId : 30
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 26
ParentElement : 26
FirstChild : null 
LastChild : null 
PreviousSibling : 28
NextSibling : 31
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 31
NodeType : 1
NodeName : == Rk9STQ==
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 26
ParentElement : 26
FirstChild : 32
LastChild : 42
PreviousSibling : 30
NextSibling : 43
NodeValue : === null
TextContent : == CgkJCQkJCgkJCQkJIGJhbmcgCgkJCQkJCgkJCQkJCgkJCQkJCgkJCQkJIHdoaW1wZXIgCgkJCQkJCgkJCQkJCgkJCQk=
ChildNodes : 5 : 32, 33, 37, 38, 42
ChildElementCount : 2
FirstElementChild : 33
LastElementChild : 38
Children : 2 : 33, 38
PreviousElementSibling : 28
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : form
TagName : == Rk9STQ==
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 2

NamespaceURI : === null
Prefix : === null
LocalName : action
Name : action
Value : ./

NamespaceURI : === null
Prefix : === null
LocalName : method
Name : method
Value : get

NodeId : 32
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 31
ParentElement : 31
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 33
NodeValue : == CgkJCQkJ
TextContent : == CgkJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJ
Length : 6
WholeText : == CgkJCQkJ

NodeId : 33
NodeType : 1
NodeName : P
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 31
ParentElement : 31
FirstChild : 34
LastChild : 36
PreviousSibling : 32
NextSibling : 37
NodeValue : === null
TextContent : == CgkJCQkJIGJhbmcgCgkJCQkJCgkJCQkJ
ChildNodes : 3 : 34, 35, 36
ChildElementCount : 1
FirstElementChild : 35
LastElementChild : 35
Children : 1 : 35
PreviousElementSibling : null 
NextElementSibling : 38
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : p
TagName : P
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 34
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 33
ParentElement : 33
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 35
NodeValue : == CgkJCQkJIGJhbmcgCgkJCQkJ
TextContent : == CgkJCQkJIGJhbmcgCgkJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJIGJhbmcgCgkJCQkJ
Length : 18
WholeText : == CgkJCQkJIGJhbmcgCgkJCQkJ

NodeId : 35
NodeType : 1
NodeName : INPUT
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 33
ParentElement : 33
FirstChild : null 
LastChild : null 
PreviousSibling : 34
NextSibling : 36
NodeValue : === null
TextContent : === empty
ChildNodes : 0 : 
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : input
TagName : INPUT
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 3

NamespaceURI : === null
Prefix : === null
LocalName : name
Name : name
Value : foo

NamespaceURI : === null
Prefix : === null
LocalName : type
Name : type
Value : radio

NamespaceURI : === null
Prefix : === null
LocalName : value
Name : value
Value : off

NodeId : 36
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 33
ParentElement : 33
FirstChild : null 
LastChild : null 
PreviousSibling : 35
NextSibling : null 
NodeValue : == CgkJCQkJ
TextContent : == CgkJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJ
Length : 6
WholeText : == CgkJCQkJ

NodeId : 37
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 31
ParentElement : 31
FirstChild : null 
LastChild : null 
PreviousSibling : 33
NextSibling : 38
NodeValue : == CgkJCQkJ
TextContent : == CgkJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJ
Length : 6
WholeText : == CgkJCQkJ

NodeId : 38
NodeType : 1
NodeName : P
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 31
ParentElement : 31
FirstChild : 39
LastChild : 41
PreviousSibling : 37
NextSibling : 42
NodeValue : === null
TextContent : == CgkJCQkJIHdoaW1wZXIgCgkJCQkJCgkJCQkJ
ChildNodes : 3 : 39, 40, 41
ChildElementCount : 1
FirstElementChild : 40
LastElementChild : 40
Children : 1 : 40
PreviousElementSibling : 33
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : p
TagName : P
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 39
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 38
ParentElement : 38
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 40
NodeValue : == CgkJCQkJIHdoaW1wZXIgCgkJCQkJ
TextContent : == CgkJCQkJIHdoaW1wZXIgCgkJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJIHdoaW1wZXIgCgkJCQkJ
Length : 21
WholeText : == CgkJCQkJIHdoaW1wZXIgCgkJCQkJ

NodeId : 40
NodeType : 1
NodeName : INPUT
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 38
ParentElement : 38
FirstChild : null 
LastChild : null 
PreviousSibling : 39
NextSibling : 41
NodeValue : === null
TextContent : === empty
ChildNodes : 0 : 
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : input
TagName : INPUT
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 3

NamespaceURI : === null
Prefix : === null
LocalName : name
Name : name
Value : foo2

NamespaceURI : === null
Prefix : === null
LocalName : type
Name : type
Value : radio

NamespaceURI : === null
Prefix : === null
LocalName : value
Name : value
Value : on

NodeId : 41
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 38
ParentElement : 38
FirstChild : null 
LastChild : null 
PreviousSibling : 40
NextSibling : null 
NodeValue : == CgkJCQkJ
TextContent : == CgkJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJ
Length : 6
WholeText : == CgkJCQkJ

NodeId : 42
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 31
ParentElement : 31
FirstChild : null 
LastChild : null 
PreviousSibling : 38
NextSibling : null 
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 43
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 26
ParentElement : 26
FirstChild : null 
LastChild : null 
PreviousSibling : 31
NextSibling : null 
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 44
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : null 
LastChild : null 
PreviousSibling : 26
NextSibling : 45
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 45
NodeType : 1
NodeName : LI
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : 46
LastChild : 46
PreviousSibling : 44
NextSibling : 47
NodeValue : === null
TextContent : == CgkJCQkgaSBncm93IG9sZCAKCQkJCQ==
ChildNodes : 1 : 46
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : 26
NextElementSibling : 48
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : li
TagName : LI
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 46
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 45
ParentElement : 45
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCQkgaSBncm93IG9sZCAKCQkJCQ==
TextContent : == CgkJCQkgaSBncm93IG9sZCAKCQkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkgaSBncm93IG9sZCAKCQkJCQ==
Length : 22
WholeText : == CgkJCQkgaSBncm93IG9sZCAKCQkJCQ==

NodeId : 47
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : null 
LastChild : null 
PreviousSibling : 45
NextSibling : 48
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 48
NodeType : 1
NodeName : LI
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : 49
LastChild : 49
PreviousSibling : 47
NextSibling : 50
NodeValue : === null
TextContent : == CgkJCQkgcGx1b3Q/IAoJCQkJ
ChildNodes : 1 : 49
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : 45
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : li
TagName : LI
Id : baz
ClassName : === empty
ClassList : 0 : 
Attributes : 1

NamespaceURI : === null
Prefix : === null
LocalName : id
Name : id
Value : baz

NodeId : 49
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 48
ParentElement : 48
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCQkgcGx1b3Q/IAoJCQkJ
TextContent : == CgkJCQkgcGx1b3Q/IAoJCQkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkgcGx1b3Q/IAoJCQkJ
Length : 18
WholeText : == CgkJCQkgcGx1b3Q/IAoJCQkJ

NodeId : 50
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 21
ParentElement : 21
FirstChild : null 
LastChild : null 
PreviousSibling : 48
NextSibling : null 
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 51
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : null 
LastChild : null 
PreviousSibling : 21
NextSibling : 52
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 52
NodeType : 1
NodeName : BLOCKQUOTE
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : 53
LastChild : 56
PreviousSibling : 51
NextSibling : 57
NodeValue : === null
TextContent : == CgkJCQkKCQkJCQkgYmFyIG1haWRzLCAKCQkJCQoJCQk=
ChildNodes : 3 : 53, 54, 56
ChildElementCount : 1
FirstElementChild : 54
LastElementChild : 54
Children : 1 : 54
PreviousElementSibling : 21
NextElementSibling : 58
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : blockquote
TagName : BLOCKQUOTE
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 53
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 52
ParentElement : 52
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 54
NodeValue : == CgkJCQk=
TextContent : == CgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQk=
Length : 5
WholeText : == CgkJCQk=

NodeId : 54
NodeType : 1
NodeName : ADDRESS
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 52
ParentElement : 52
FirstChild : 55
LastChild : 55
PreviousSibling : 53
NextSibling : 56
NodeValue : === null
TextContent : == CgkJCQkJIGJhciBtYWlkcywgCgkJCQk=
ChildNodes : 1 : 55
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : address
TagName : ADDRESS
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 55
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 54
ParentElement : 54
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCQkJIGJhciBtYWlkcywgCgkJCQk=
TextContent : == CgkJCQkJIGJhciBtYWlkcywgCgkJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkJIGJhciBtYWlkcywgCgkJCQk=
Length : 23
WholeText : == CgkJCQkJIGJhciBtYWlkcywgCgkJCQk=

NodeId : 56
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 52
ParentElement : 52
FirstChild : null 
LastChild : null 
PreviousSibling : 54
NextSibling : null 
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 57
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : null 
LastChild : null 
PreviousSibling : 52
NextSibling : 58
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 58
NodeType : 1
NodeName : H1
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : 59
LastChild : 59
PreviousSibling : 57
NextSibling : 60
NodeValue : === null
TextContent : == CgkJCQkgc2luZyB0byBtZSwgZXJiYXJtZSBkaWNoIAoJCQk=
ChildNodes : 1 : 59
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : 52
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : h1
TagName : H1
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 0

NodeId : 59
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 58
ParentElement : 58
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : == CgkJCQkgc2luZyB0byBtZSwgZXJiYXJtZSBkaWNoIAoJCQk=
TextContent : == CgkJCQkgc2luZyB0byBtZSwgZXJiYXJtZSBkaWNoIAoJCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQkgc2luZyB0byBtZSwgZXJiYXJtZSBkaWNoIAoJCQk=
Length : 35
WholeText : == CgkJCQkgc2luZyB0byBtZSwgZXJiYXJtZSBkaWNoIAoJCQk=

NodeId : 60
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 19
ParentElement : 19
FirstChild : null 
LastChild : null 
PreviousSibling : 58
NextSibling : null 
NodeValue : == CgkJCQ==
TextContent : == CgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJCQ==
Length : 4
WholeText : == CgkJCQ==

NodeId : 61
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 14
ParentElement : 14
FirstChild : null 
LastChild : null 
PreviousSibling : 19
NextSibling : null 
NodeValue : == CgkJ
TextContent : == CgkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJ
Length : 3
WholeText : == CgkJ

NodeId : 62
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 12
ParentElement : 12
FirstChild : null 
LastChild : null 
PreviousSibling : 14
NextSibling : 63
NodeValue : == CgkJ
TextContent : == CgkJ
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJ
Length : 3
WholeText : == CgkJ

NodeId : 63
NodeType : 1
NodeName : P
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 12
ParentElement : 12
FirstChild : 64
LastChild : 70
PreviousSibling : 62
NextSibling : 71
NodeValue : === null
TextContent : == CgkJIFRoaXMgaXMgYSBub25zZW5zaWNhbCBkb2N1bWVudCwgYnV0IHN5bnRhY3RpY2FsbHkgdmFsaWQgSFRNTCA0LjAuIEFsbCAxMDAlLWNvbmZvcm1hbnQgQ1NTMSBhZ2VudHMgc2hvdWxkIGJlIGFibGUgdG8gcmVuZGVyIHRoZSBkb2N1bWVudCBlbGVtZW50cyBhYm92ZSB0aGlzIHBhcmFncmFwaCBpbmRpc3Rpbmd1aXNoYWJseSAodG8gdGhlIHBpeGVsKSBmcm9tIHRoaXMgCgkJCXJlZmVyZW5jZSByZW5kZXJpbmcsCgkJIChleGNlcHQgZm9udCByYXN0ZXJpemF0aW9uIGFuZCBmb3JtIHdpZGdldHMpLiBBbGwgZGlzY3JlcGFuY2llcyBzaG91bGQgYmUgdHJhY2VhYmxlIHRvIENTUzEgaW1wbGVtZW50YXRpb24gc2hvcnRjb21pbmdzLiBPbmNlIHlvdSBoYXZlIGZpbmlzaGVkIGV2YWx1YXRpbmcgdGhpcyB0ZXN0LCB5b3UgY2FuIHJldHVybiB0byB0aGUgcGFyZW50IHBhZ2UuIAoJCQ==
ChildNodes : 5 : 64, 65, 67, 68, 70
ChildElementCount : 2
FirstElementChild : 65
LastElementChild : 68
Children : 2 : 65, 68
PreviousElementSibling : 14
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : p
TagName : P
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 1

NamespaceURI : === null
Prefix : === null
LocalName : style
Name : style
Value : color: black; font-size: 1em; line-height: 1.3em; clear: both

NodeId : 64
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 63
ParentElement : 63
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : 65
NodeValue : == CgkJIFRoaXMgaXMgYSBub25zZW5zaWNhbCBkb2N1bWVudCwgYnV0IHN5bnRhY3RpY2FsbHkgdmFsaWQgSFRNTCA0LjAuIEFsbCAxMDAlLWNvbmZvcm1hbnQgQ1NTMSBhZ2VudHMgc2hvdWxkIGJlIGFibGUgdG8gcmVuZGVyIHRoZSBkb2N1bWVudCBlbGVtZW50cyBhYm92ZSB0aGlzIHBhcmFncmFwaCBpbmRpc3Rpbmd1aXNoYWJseSAodG8gdGhlIHBpeGVsKSBmcm9tIHRoaXMgCgkJCQ==
TextContent : == CgkJIFRoaXMgaXMgYSBub25zZW5zaWNhbCBkb2N1bWVudCwgYnV0IHN5bnRhY3RpY2FsbHkgdmFsaWQgSFRNTCA0LjAuIEFsbCAxMDAlLWNvbmZvcm1hbnQgQ1NTMSBhZ2VudHMgc2hvdWxkIGJlIGFibGUgdG8gcmVuZGVyIHRoZSBkb2N1bWVudCBlbGVtZW50cyBhYm92ZSB0aGlzIHBhcmFncmFwaCBpbmRpc3Rpbmd1aXNoYWJseSAodG8gdGhlIHBpeGVsKSBmcm9tIHRoaXMgCgkJCQ==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJIFRoaXMgaXMgYSBub25zZW5zaWNhbCBkb2N1bWVudCwgYnV0IHN5bnRhY3RpY2FsbHkgdmFsaWQgSFRNTCA0LjAuIEFsbCAxMDAlLWNvbmZvcm1hbnQgQ1NTMSBhZ2VudHMgc2hvdWxkIGJlIGFibGUgdG8gcmVuZGVyIHRoZSBkb2N1bWVudCBlbGVtZW50cyBhYm92ZSB0aGlzIHBhcmFncmFwaCBpbmRpc3Rpbmd1aXNoYWJseSAodG8gdGhlIHBpeGVsKSBmcm9tIHRoaXMgCgkJCQ==
Length : 217
WholeText : == CgkJIFRoaXMgaXMgYSBub25zZW5zaWNhbCBkb2N1bWVudCwgYnV0IHN5bnRhY3RpY2FsbHkgdmFsaWQgSFRNTCA0LjAuIEFsbCAxMDAlLWNvbmZvcm1hbnQgQ1NTMSBhZ2VudHMgc2hvdWxkIGJlIGFibGUgdG8gcmVuZGVyIHRoZSBkb2N1bWVudCBlbGVtZW50cyBhYm92ZSB0aGlzIHBhcmFncmFwaCBpbmRpc3Rpbmd1aXNoYWJseSAodG8gdGhlIHBpeGVsKSBmcm9tIHRoaXMgCgkJCQ==

NodeId : 65
NodeType : 1
NodeName : A
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 63
ParentElement : 63
FirstChild : 66
LastChild : 66
PreviousSibling : 64
NextSibling : 67
NodeValue : === null
TextContent : reference rendering,
ChildNodes : 1 : 66
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : null 
NextElementSibling : 68
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : a
TagName : A
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 1

NamespaceURI : === null
Prefix : === null
LocalName : href
Name : href
Value : sec5526c.gif

NodeId : 66
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 65
ParentElement : 65
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : reference rendering,
TextContent : reference rendering,
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : reference rendering,
Length : 20
WholeText : reference rendering,

NodeId : 67
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 63
ParentElement : 63
FirstChild : null 
LastChild : null 
PreviousSibling : 65
NextSibling : 68
NodeValue : == CgkJIChleGNlcHQgZm9udCByYXN0ZXJpemF0aW9uIGFuZCBmb3JtIHdpZGdldHMpLiBBbGwgZGlzY3JlcGFuY2llcyBzaG91bGQgYmUgdHJhY2VhYmxlIHRvIENTUzEgaW1wbGVtZW50YXRpb24gc2hvcnRjb21pbmdzLiBPbmNlIHlvdSBoYXZlIGZpbmlzaGVkIGV2YWx1YXRpbmcgdGhpcyB0ZXN0LCB5b3UgY2FuIHJldHVybiB0byB0aGUg
TextContent : == CgkJIChleGNlcHQgZm9udCByYXN0ZXJpemF0aW9uIGFuZCBmb3JtIHdpZGdldHMpLiBBbGwgZGlzY3JlcGFuY2llcyBzaG91bGQgYmUgdHJhY2VhYmxlIHRvIENTUzEgaW1wbGVtZW50YXRpb24gc2hvcnRjb21pbmdzLiBPbmNlIHlvdSBoYXZlIGZpbmlzaGVkIGV2YWx1YXRpbmcgdGhpcyB0ZXN0LCB5b3UgY2FuIHJldHVybiB0byB0aGUg
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkJIChleGNlcHQgZm9udCByYXN0ZXJpemF0aW9uIGFuZCBmb3JtIHdpZGdldHMpLiBBbGwgZGlzY3JlcGFuY2llcyBzaG91bGQgYmUgdHJhY2VhYmxlIHRvIENTUzEgaW1wbGVtZW50YXRpb24gc2hvcnRjb21pbmdzLiBPbmNlIHlvdSBoYXZlIGZpbmlzaGVkIGV2YWx1YXRpbmcgdGhpcyB0ZXN0LCB5b3UgY2FuIHJldHVybiB0byB0aGUg
Length : 192
WholeText : == CgkJIChleGNlcHQgZm9udCByYXN0ZXJpemF0aW9uIGFuZCBmb3JtIHdpZGdldHMpLiBBbGwgZGlzY3JlcGFuY2llcyBzaG91bGQgYmUgdHJhY2VhYmxlIHRvIENTUzEgaW1wbGVtZW50YXRpb24gc2hvcnRjb21pbmdzLiBPbmNlIHlvdSBoYXZlIGZpbmlzaGVkIGV2YWx1YXRpbmcgdGhpcyB0ZXN0LCB5b3UgY2FuIHJldHVybiB0byB0aGUg

NodeId : 68
NodeType : 1
NodeName : A
BaseUri : http://localhost:14873/DomAnalyzer.html
OwnerDocument : 0
ParentNode : 63
ParentElement : 63
FirstChild : 69
LastChild : 69
PreviousSibling : 67
NextSibling : 70
NodeValue : === null
TextContent : parent page
ChildNodes : 1 : 69
ChildElementCount : 0
FirstElementChild : null 
LastElementChild : null 
Children : 0 : 
PreviousElementSibling : 65
NextElementSibling : null 
NamespaceURI : http://www.w3.org/1999/xhtml
Prefix : === null
LocalName : a
TagName : A
Id : === empty
ClassName : === empty
ClassList : 0 : 
Attributes : 1

NamespaceURI : === null
Prefix : === null
LocalName : href
Name : href
Value : sec5526c.htm

NodeId : 69
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 68
ParentElement : 68
FirstChild : null 
LastChild : null 
PreviousSibling : null 
NextSibling : null 
NodeValue : parent page
TextContent : parent page
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : parent page
Length : 11
WholeText : parent page

NodeId : 70
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 63
ParentElement : 63
FirstChild : null 
LastChild : null 
PreviousSibling : 68
NextSibling : null 
NodeValue : == LiAKCQk=
TextContent : == LiAKCQk=
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == LiAKCQk=
Length : 5
WholeText : == LiAKCQk=

NodeId : 71
NodeType : 3
NodeName : #text
BaseUri : === null
OwnerDocument : 0
ParentNode : 12
ParentElement : 12
FirstChild : null 
LastChild : null 
PreviousSibling : 63
NextSibling : null 
NodeValue : == CgkKCg==
TextContent : == CgkKCg==
ChildNodes : 0 : 
PreviousElementSibling : undefined 
NextElementSibling : undefined 
Data : == CgkKCg==
Length : 4
WholeText : == CgkKCg==
";
    }
}

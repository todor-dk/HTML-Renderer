using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public class XXTest
    {
        public static void Test1()
        {
            string html = "<!DOCTYPE HTML>\n<HTML><HEAD>\n<META http-equiv=\"Content-type\" content=\"text/html; charset=utf-8\">\n<TITLE>ATLAS.ti Html \nDocument</TITLE>\n<STYLE type=\"text/css\">body {font-family:\"Microsoft Sans Serif\";font-size:12pt;font-weight:400;white-space:pre-wrap;}p {margin-bottom:0;margin-top:0;text-align:left;}span, .pre-ws {white-space:pre-wrap;}.charFormat1 {font-family:\"System\";font-size:9.75pt;font-weight:700;}table {border-collapse:collapse;table-layout:fixed;}table, td, th {border:0px solid black;vertical-align:top;}</STYLE>\n</HEAD>\n<BODY id=\"0\"><P id=\"3\">Hallo World&apos;&gtcc;</P></BODY></HTML>";

            StringHtmlStream stream = new StringHtmlStream(html);

            // Tokenizer tokenizer = new Tokenizer(stream, new TokenizerClient());
            // tokenizer.Tokenize();

            DomParser parser = new Parsing.DomParser();
            parser.Parse(stream);
        }

        private class TokenizerClient : ITokenizerClient
        {
            public void ParseError(ParseErrors error)
            {
                Console.Write("**** PARSE ERROR {0} ****", error);
            }

            public void ReceiveCharacter(char character)
            {
                Console.Write(character);
            }

            public void ReceiveComment(Func<string> data)
            {
                Console.WriteLine("<!--{0}-->", data());
            }

            public void ReceiveDocType(string name, string publicIdentifier, string systemIdentifier, bool forceQuirks)
            {
                Console.WriteLine("<!DOCTYPE {0} {1} {2} {3}>", name, publicIdentifier, systemIdentifier, forceQuirks ? "QUIRKS" : "");
            }

            public void ReceiveEndOfFile()
            {
            }

            public void ReceiveEndTag(string tagName, bool isSelfClosing, Attribute[] attributes)
            {
                Console.WriteLine("</{0}>", tagName);
            }

            public void ReceiveStartTag(string tagName, bool isSelfClosing, Attribute[] attributes)
            {
                Console.Write("<{0}", tagName);
                foreach (var attr in attributes)
                    Console.Write(" {0}=\"{1}\"", attr.Name, attr.Value);
                if (isSelfClosing)
                    Console.Write(" />");
                else
                    Console.Write(">");
            }
        }
    }
}

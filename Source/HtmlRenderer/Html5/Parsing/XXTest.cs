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
            string html = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">\n <HTML><HEAD>\n<META http-equiv=\"Content-type\" content=\"text/html; charset=utf-8\">\n<TITLE>ATLAS.ti Html \nDocument</TITLE>\n<STYLE type=\"text/css\">body {font-family:\"Microsoft Sans Serif\";font-size:12pt;font-weight:400;white-space:pre-wrap;}p {margin-bottom:0;margin-top:0;text-align:left;}span, .pre-ws {white-space:pre-wrap;}.charFormat1 {font-family:\"System\";font-size:9.75pt;font-weight:700;}table {border-collapse:collapse;table-layout:fixed;}table, td, th {border:0px solid black;vertical-align:top;}</STYLE>\n</HEAD>\n<BODY id=\"0\"><P id=\"3\">Hallo World&apos;&gtcc;</P></BODY></HTML>";

            StringHtmlStream stream = new StringHtmlStream(html);

            Tokenizer tokenizer = new Tokenizer(stream);
            tokenizer.ParseError += (s, e) => Console.Write("**** PARSE ERROR {0} ****", e.ParseError);
            Token token;
            do
            {
                token = tokenizer.GetNextToken();
                EmitToken(token);
            }
            while (token.Type != TokenType.EndOfFile);

            //ParsingContext ctx = new ParsingContext();

            //DomParser parser = new Parsing.DomParser(ctx);
            //parser.Parse(stream);
        }

        private static void EmitToken(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Unknown:
                    break;
                case TokenType.Character:
                    Console.Write(token.Character);
                    break;
                case TokenType.EndOfFile:
                    break;
                case TokenType.DocType:
                    Console.WriteLine("<!DOCTYPE {0} {1} {2} {3}>", token.DocTypeName, token.DocTypePublicIdentifier, token.DocTypeSystemIdentifier, token.DocTypeForceQuirks ? "QUIRKS" : "");
                    break;
                case TokenType.Comment:
                    Console.WriteLine("<!--{0}-->", token.CommentData);
                    break;
                case TokenType.StartTag:
                    Console.Write("<{0}", token.TagName);
                    foreach (var attr in token.TagAttributes)
                        Console.Write(" {0}=\"{1}\"", attr.Name, attr.Value);
                    if (token.TagIsSelfClosing)
                        Console.Write(" />");
                    else
                        Console.Write(">");
                    break;
                case TokenType.EndTag:
                    Console.WriteLine("</{0}>", token.TagName);
                    break;
                default:
                    break;
            }
        }
    }
}

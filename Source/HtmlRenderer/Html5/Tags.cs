using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5
{
    internal static class Tags
    {
        public const string Html = "html";

        public const string Head = "head";

        public const string Body = "body";

        public const string Br = "br";

        public const string Base = "base";

        public const string BaseFont = "basefont";

        public const string BgSound = "bgsound";

        public const string Link = "link";

        public const string Meta = "meta";

        public const string Title = "title";

        public const string Script = "script";

        public const string Template = "template";

        public const string NoScript = "noscript";

        public const string NoFrames = "noframes";

        public const string Style = "style";

        public const string Frameset = "frameset";

        public const string Address = "address ";

        public const string Article = "article ";

        public const string Aside = "aside ";

        public const string BlockQuote = "blockquote ";

        public const string Center = "center ";

        public const string Details = "details ";

        public const string Dialog = "dialog";

        public const string Dir = "dir ";

        public const string Div = "div ";

        public const string Dl = "dl ";

        public const string FieldSet = "fieldset ";

        public const string FigCaption = "figcaption ";

        public const string Figure = "figure ";

        public const string Footer = "footer ";

        public const string Header = "header ";

        public const string HGroup = "hgroup ";

        public const string Main = "main ";

        public const string Nav = "nav ";

        public const string Ol = "ol ";

        public const string P = "p";

        public const string Section = "section ";

        public const string Summary = "summary ";

        public const string Ul = "ul";

        public const string H1 = "h1";

        public const string H2 = "h2";

        public const string H3 = "h3";

        public const string H4 = "h4";

        public const string H5 = "h5";

        public const string H6 = "h6";

        public const string Pre = "pre";

        public const string Listing = "listing";

        public const string Form = "form";

        public const string Li = "li";

        public const string Dd = "dd";

        public const string Dt = "dt";

        public const string PlainText = "plaintext";

        public const string Button = "button";

        public const string Sarcasm = "sarcasm";

        public const string A = "a";

        public const string B = "b";

        public const string Big = "big";

        public const string Code = "code";

        public const string Em = "em";

        public const string Font = "font";

        public const string I = "i";

        public const string S = "s";

        public const string Small = "small";

        public const string Strike = "strike";

        public const string Strong = "strong";

        public const string Tt = "tt";

        public const string U = "u";

        public const string Nobr = "nobr";

        public const string Applet = "applet";

        public const string Marquee = "marquee";

        public const string Object = "object";

        public const string Table = "table";

        public const string Tr = "tr";

        public const string Td = "td";

        public const string Th = "th";

        public const string TBody = "tbody";

        public const string THead = "thead";

        public const string TFoot = "tfoot";

        public const string Option = "option";

        public const string Caption = "caption";

        public const string Col = "col";

        public const string ColGroup = "colgroup";

        public const string OptGroup = "optgroup";

        public const string Select = "select";

        public const string Frame = "frame";

        public const string Input = "input";

        public const string KeyGen = "keygen";

        public const string TextArea = "textarea";

        public const string Math = "math";

        public const string Rb = "rb";

        public const string Rp = "rp";

        public const string Rt = "rt";

        public const string Rtc = "rtc";

        public const string Svg = "svg";

        public const string IFrame = "iframe";

        public const string Xmp = "xmp";

        public const string IsIndex = "isindex";

        public const string Image = "image";

        public const string Hr = "hr";

        public const string Param = "param";

        public const string Source = "source";

        public const string Track = "track";

        public const string Area = "area";

        public const string Img = "img";

        public const string Embed = "embed";

        public const string Wbr = "wbr";

        public const string NoEmbed = "noembed";

        public const string Menu = "menu";

        public const string MenuItem = "menuitem";

        public const string Xml = "xmp";

        public static readonly string[] AllTags = new string[]
        {
            A, Address, Applet, Area, Article, Aside, B, Base, BaseFont, BgSound, Big, BlockQuote, Body, Br, Button,
            Caption, Center, Code, Col, ColGroup, Dd, Details, Dialog, Dir, Div, Dl, Dt, Em, Embed, FieldSet, FigCaption,
            Figure, Font, Footer, Form, Frame, Frameset, H1, H2, H3, H4, H5, H6, Head, Header, HGroup, Hr, Html, I,
            IFrame, Image, Img, Input, IsIndex, KeyGen, Li, Link, Listing, Main, Marquee, Math, Menu, MenuItem, Meta,
            Nav, Nobr, NoEmbed, NoFrames, NoScript, Object, Ol, OptGroup, Option, P, Param, PlainText, Pre, Rb, Rp, Rt, Rtc,
            S, Sarcasm, Script, Section, Select, Small, Source, Strike, Strong, Style, Summary, Svg, Table, TBody, Td,
            Template, TextArea, TFoot, Th, THead, Title, Tr, Track, Tt, U, Ul, Wbr, Xml, Xmp
        };

        /// <summary>
        /// The following elements have varying levels of special parsing rules.
        /// See: http://www.w3.org/TR/html51/syntax.html#the-stack-of-open-elements
        /// </summary>
        internal static readonly string[] SpecialTags = new string[]
        {
            Address, Applet, Area, Article, Aside, Base, BaseFont, BgSound, BlockQuote, Body, Br, Button, Caption, Center, Col, ColGroup,
            Dd, Details, Dir, Div, Dl, Dt, Embed, FieldSet, FigCaption, Figure, Footer, Form, Frame, Frameset, H1, H2, H3, H4, H5, H6,
            Head, Header, Hr, Html, IFrame, Img, Input, Li, Link, Listing, Main, Marquee, Menu, MenuItem, Meta, Nav, NoEmbed, NoFrames,
            NoScript, Object, Ol, P, Param, PlainText, Pre, Script, Section, Select, Source, Style, Summary, Table, TBody, Td, Template,
            TextArea, TFoot, Th, THead, Title, Tr, Track, Ul, Wbr, Xmp
        };

        /// <summary>
        /// Determines if the given elements have varying levels of special parsing rules.
        /// See: http://www.w3.org/TR/html51/syntax.html#the-stack-of-open-elements
        /// </summary>
        internal static bool IsSpecial(string tagName)
        {
            for (int i = 0; i < Tags.SpecialTags.Length; i++)
            {
                if (Tags.SpecialTags[i] == tagName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the given elements have varying levels of special parsing rules.
        /// See: http://www.w3.org/TR/html51/syntax.html#the-stack-of-open-elements
        /// </summary>
        internal static bool IsSpecial(this Element self)
        {
            Contract.RequiresNotNull(self, nameof(self));
            return Tags.IsSpecial(self.TagName);
        }

        /// <summary>
        /// See: http://www.w3.org/TR/html51/syntax.html#the-stack-of-open-elements
        /// </summary>
        internal static readonly string[] FormattingTags = new string[]
        {
            A, B, Big, Code, Em, Font, I, Nobr, S, Small, Strike, Strong, Tt, U
        };

        internal static bool IsFormatting(string tagName)
        {
            for (int i = 0; i < Tags.FormattingTags.Length; i++)
            {
                if (Tags.SpecialTags[i] == tagName)
                    return true;
            }

            return false;
        }

        internal static bool IsFormatting(this Element self)
        {
            Contract.RequiresNotNull(self, nameof(self));
            return Tags.IsFormatting(self.TagName);
        }
    }
}

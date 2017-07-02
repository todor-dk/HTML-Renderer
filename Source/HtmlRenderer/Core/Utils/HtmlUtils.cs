// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;

namespace Scientia.HtmlRenderer.Core.Utils
{
    internal static class HtmlUtils
    {
        #region Fields and Consts

        /// <summary>
        /// List of html tags that don't have content
        /// </summary>
        private static readonly List<string> List = new List<string>(
            new[]
            {
                "area", "base", "basefont", "br", "col",
                "frame", "hr", "img", "input", "isindex",
                "link", "meta", "param"
            });

        /// <summary>
        /// the html encode\decode pairs
        /// </summary>
        private static readonly KeyValuePair<string, string>[] EncodeDecode = new[]
        {
            new KeyValuePair<string, string>("&lt;", "<"),
            new KeyValuePair<string, string>("&gt;", ">"),
            new KeyValuePair<string, string>("&quot;", "\""),
            new KeyValuePair<string, string>("&amp;", "&"),
        };

        /// <summary>
        /// the html decode only pairs
        /// </summary>
        private static readonly Dictionary<string, char> DecodeOnly = new Dictionary<string, char>(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        static HtmlUtils()
        {
            DecodeOnly["nbsp"] = ' ';
            DecodeOnly["rdquo"] = '"';
            DecodeOnly["lsquo"] = '\'';
            DecodeOnly["apos"] = '\'';

            // ISO 8859-1 Symbols
            DecodeOnly["iexcl"] = Convert.ToChar(161);
            DecodeOnly["cent"] = Convert.ToChar(162);
            DecodeOnly["pound"] = Convert.ToChar(163);
            DecodeOnly["curren"] = Convert.ToChar(164);
            DecodeOnly["yen"] = Convert.ToChar(165);
            DecodeOnly["brvbar"] = Convert.ToChar(166);
            DecodeOnly["sect"] = Convert.ToChar(167);
            DecodeOnly["uml"] = Convert.ToChar(168);
            DecodeOnly["copy"] = Convert.ToChar(169);
            DecodeOnly["ordf"] = Convert.ToChar(170);
            DecodeOnly["laquo"] = Convert.ToChar(171);
            DecodeOnly["not"] = Convert.ToChar(172);
            DecodeOnly["shy"] = Convert.ToChar(173);
            DecodeOnly["reg"] = Convert.ToChar(174);
            DecodeOnly["macr"] = Convert.ToChar(175);
            DecodeOnly["deg"] = Convert.ToChar(176);
            DecodeOnly["plusmn"] = Convert.ToChar(177);
            DecodeOnly["sup2"] = Convert.ToChar(178);
            DecodeOnly["sup3"] = Convert.ToChar(179);
            DecodeOnly["acute"] = Convert.ToChar(180);
            DecodeOnly["micro"] = Convert.ToChar(181);
            DecodeOnly["para"] = Convert.ToChar(182);
            DecodeOnly["middot"] = Convert.ToChar(183);
            DecodeOnly["cedil"] = Convert.ToChar(184);
            DecodeOnly["sup1"] = Convert.ToChar(185);
            DecodeOnly["ordm"] = Convert.ToChar(186);
            DecodeOnly["raquo"] = Convert.ToChar(187);
            DecodeOnly["frac14"] = Convert.ToChar(188);
            DecodeOnly["frac12"] = Convert.ToChar(189);
            DecodeOnly["frac34"] = Convert.ToChar(190);
            DecodeOnly["iquest"] = Convert.ToChar(191);
            DecodeOnly["times"] = Convert.ToChar(215);
            DecodeOnly["divide"] = Convert.ToChar(247);

            // ISO 8859-1 Characters
            DecodeOnly["Agrave"] = Convert.ToChar(192);
            DecodeOnly["Aacute"] = Convert.ToChar(193);
            DecodeOnly["Acirc"] = Convert.ToChar(194);
            DecodeOnly["Atilde"] = Convert.ToChar(195);
            DecodeOnly["Auml"] = Convert.ToChar(196);
            DecodeOnly["Aring"] = Convert.ToChar(197);
            DecodeOnly["AElig"] = Convert.ToChar(198);
            DecodeOnly["Ccedil"] = Convert.ToChar(199);
            DecodeOnly["Egrave"] = Convert.ToChar(200);
            DecodeOnly["Eacute"] = Convert.ToChar(201);
            DecodeOnly["Ecirc"] = Convert.ToChar(202);
            DecodeOnly["Euml"] = Convert.ToChar(203);
            DecodeOnly["Igrave"] = Convert.ToChar(204);
            DecodeOnly["Iacute"] = Convert.ToChar(205);
            DecodeOnly["Icirc"] = Convert.ToChar(206);
            DecodeOnly["Iuml"] = Convert.ToChar(207);
            DecodeOnly["ETH"] = Convert.ToChar(208);
            DecodeOnly["Ntilde"] = Convert.ToChar(209);
            DecodeOnly["Ograve"] = Convert.ToChar(210);
            DecodeOnly["Oacute"] = Convert.ToChar(211);
            DecodeOnly["Ocirc"] = Convert.ToChar(212);
            DecodeOnly["Otilde"] = Convert.ToChar(213);
            DecodeOnly["Ouml"] = Convert.ToChar(214);
            DecodeOnly["Oslash"] = Convert.ToChar(216);
            DecodeOnly["Ugrave"] = Convert.ToChar(217);
            DecodeOnly["Uacute"] = Convert.ToChar(218);
            DecodeOnly["Ucirc"] = Convert.ToChar(219);
            DecodeOnly["Uuml"] = Convert.ToChar(220);
            DecodeOnly["Yacute"] = Convert.ToChar(221);
            DecodeOnly["THORN"] = Convert.ToChar(222);
            DecodeOnly["szlig"] = Convert.ToChar(223);
            DecodeOnly["agrave"] = Convert.ToChar(224);
            DecodeOnly["aacute"] = Convert.ToChar(225);
            DecodeOnly["acirc"] = Convert.ToChar(226);
            DecodeOnly["atilde"] = Convert.ToChar(227);
            DecodeOnly["auml"] = Convert.ToChar(228);
            DecodeOnly["aring"] = Convert.ToChar(229);
            DecodeOnly["aelig"] = Convert.ToChar(230);
            DecodeOnly["ccedil"] = Convert.ToChar(231);
            DecodeOnly["egrave"] = Convert.ToChar(232);
            DecodeOnly["eacute"] = Convert.ToChar(233);
            DecodeOnly["ecirc"] = Convert.ToChar(234);
            DecodeOnly["euml"] = Convert.ToChar(235);
            DecodeOnly["igrave"] = Convert.ToChar(236);
            DecodeOnly["iacute"] = Convert.ToChar(237);
            DecodeOnly["icirc"] = Convert.ToChar(238);
            DecodeOnly["iuml"] = Convert.ToChar(239);
            DecodeOnly["eth"] = Convert.ToChar(240);
            DecodeOnly["ntilde"] = Convert.ToChar(241);
            DecodeOnly["ograve"] = Convert.ToChar(242);
            DecodeOnly["oacute"] = Convert.ToChar(243);
            DecodeOnly["ocirc"] = Convert.ToChar(244);
            DecodeOnly["otilde"] = Convert.ToChar(245);
            DecodeOnly["ouml"] = Convert.ToChar(246);
            DecodeOnly["oslash"] = Convert.ToChar(248);
            DecodeOnly["ugrave"] = Convert.ToChar(249);
            DecodeOnly["uacute"] = Convert.ToChar(250);
            DecodeOnly["ucirc"] = Convert.ToChar(251);
            DecodeOnly["uuml"] = Convert.ToChar(252);
            DecodeOnly["yacute"] = Convert.ToChar(253);
            DecodeOnly["thorn"] = Convert.ToChar(254);
            DecodeOnly["yuml"] = Convert.ToChar(255);

            // Math Symbols Supported by HTML
            DecodeOnly["forall"] = Convert.ToChar(8704);
            DecodeOnly["part"] = Convert.ToChar(8706);
            DecodeOnly["exist"] = Convert.ToChar(8707);
            DecodeOnly["empty"] = Convert.ToChar(8709);
            DecodeOnly["nabla"] = Convert.ToChar(8711);
            DecodeOnly["isin"] = Convert.ToChar(8712);
            DecodeOnly["notin"] = Convert.ToChar(8713);
            DecodeOnly["ni"] = Convert.ToChar(8715);
            DecodeOnly["prod"] = Convert.ToChar(8719);
            DecodeOnly["sum"] = Convert.ToChar(8721);
            DecodeOnly["minus"] = Convert.ToChar(8722);
            DecodeOnly["lowast"] = Convert.ToChar(8727);
            DecodeOnly["radic"] = Convert.ToChar(8730);
            DecodeOnly["prop"] = Convert.ToChar(8733);
            DecodeOnly["infin"] = Convert.ToChar(8734);
            DecodeOnly["ang"] = Convert.ToChar(8736);
            DecodeOnly["and"] = Convert.ToChar(8743);
            DecodeOnly["or"] = Convert.ToChar(8744);
            DecodeOnly["cap"] = Convert.ToChar(8745);
            DecodeOnly["cup"] = Convert.ToChar(8746);
            DecodeOnly["int"] = Convert.ToChar(8747);
            DecodeOnly["there4"] = Convert.ToChar(8756);
            DecodeOnly["sim"] = Convert.ToChar(8764);
            DecodeOnly["cong"] = Convert.ToChar(8773);
            DecodeOnly["asymp"] = Convert.ToChar(8776);
            DecodeOnly["ne"] = Convert.ToChar(8800);
            DecodeOnly["equiv"] = Convert.ToChar(8801);
            DecodeOnly["le"] = Convert.ToChar(8804);
            DecodeOnly["ge"] = Convert.ToChar(8805);
            DecodeOnly["sub"] = Convert.ToChar(8834);
            DecodeOnly["sup"] = Convert.ToChar(8835);
            DecodeOnly["nsub"] = Convert.ToChar(8836);
            DecodeOnly["sube"] = Convert.ToChar(8838);
            DecodeOnly["supe"] = Convert.ToChar(8839);
            DecodeOnly["oplus"] = Convert.ToChar(8853);
            DecodeOnly["otimes"] = Convert.ToChar(8855);
            DecodeOnly["perp"] = Convert.ToChar(8869);
            DecodeOnly["sdot"] = Convert.ToChar(8901);

            // Greek Letters Supported by HTML
            DecodeOnly["Alpha"] = Convert.ToChar(913);
            DecodeOnly["Beta"] = Convert.ToChar(914);
            DecodeOnly["Gamma"] = Convert.ToChar(915);
            DecodeOnly["Delta"] = Convert.ToChar(916);
            DecodeOnly["Epsilon"] = Convert.ToChar(917);
            DecodeOnly["Zeta"] = Convert.ToChar(918);
            DecodeOnly["Eta"] = Convert.ToChar(919);
            DecodeOnly["Theta"] = Convert.ToChar(920);
            DecodeOnly["Iota"] = Convert.ToChar(921);
            DecodeOnly["Kappa"] = Convert.ToChar(922);
            DecodeOnly["Lambda"] = Convert.ToChar(923);
            DecodeOnly["Mu"] = Convert.ToChar(924);
            DecodeOnly["Nu"] = Convert.ToChar(925);
            DecodeOnly["Xi"] = Convert.ToChar(926);
            DecodeOnly["Omicron"] = Convert.ToChar(927);
            DecodeOnly["Pi"] = Convert.ToChar(928);
            DecodeOnly["Rho"] = Convert.ToChar(929);
            DecodeOnly["Sigma"] = Convert.ToChar(931);
            DecodeOnly["Tau"] = Convert.ToChar(932);
            DecodeOnly["Upsilon"] = Convert.ToChar(933);
            DecodeOnly["Phi"] = Convert.ToChar(934);
            DecodeOnly["Chi"] = Convert.ToChar(935);
            DecodeOnly["Psi"] = Convert.ToChar(936);
            DecodeOnly["Omega"] = Convert.ToChar(937);
            DecodeOnly["alpha"] = Convert.ToChar(945);
            DecodeOnly["beta"] = Convert.ToChar(946);
            DecodeOnly["gamma"] = Convert.ToChar(947);
            DecodeOnly["delta"] = Convert.ToChar(948);
            DecodeOnly["epsilon"] = Convert.ToChar(949);
            DecodeOnly["zeta"] = Convert.ToChar(950);
            DecodeOnly["eta"] = Convert.ToChar(951);
            DecodeOnly["theta"] = Convert.ToChar(952);
            DecodeOnly["iota"] = Convert.ToChar(953);
            DecodeOnly["kappa"] = Convert.ToChar(954);
            DecodeOnly["lambda"] = Convert.ToChar(955);
            DecodeOnly["mu"] = Convert.ToChar(956);
            DecodeOnly["nu"] = Convert.ToChar(957);
            DecodeOnly["xi"] = Convert.ToChar(958);
            DecodeOnly["omicron"] = Convert.ToChar(959);
            DecodeOnly["pi"] = Convert.ToChar(960);
            DecodeOnly["rho"] = Convert.ToChar(961);
            DecodeOnly["sigmaf"] = Convert.ToChar(962);
            DecodeOnly["sigma"] = Convert.ToChar(963);
            DecodeOnly["tau"] = Convert.ToChar(964);
            DecodeOnly["upsilon"] = Convert.ToChar(965);
            DecodeOnly["phi"] = Convert.ToChar(966);
            DecodeOnly["chi"] = Convert.ToChar(967);
            DecodeOnly["psi"] = Convert.ToChar(968);
            DecodeOnly["omega"] = Convert.ToChar(969);
            DecodeOnly["thetasym"] = Convert.ToChar(977);
            DecodeOnly["upsih"] = Convert.ToChar(978);
            DecodeOnly["piv"] = Convert.ToChar(982);

            // Other Entities Supported by HTML
            DecodeOnly["OElig"] = Convert.ToChar(338);
            DecodeOnly["oelig"] = Convert.ToChar(339);
            DecodeOnly["Scaron"] = Convert.ToChar(352);
            DecodeOnly["scaron"] = Convert.ToChar(353);
            DecodeOnly["Yuml"] = Convert.ToChar(376);
            DecodeOnly["fnof"] = Convert.ToChar(402);
            DecodeOnly["circ"] = Convert.ToChar(710);
            DecodeOnly["tilde"] = Convert.ToChar(732);
            DecodeOnly["ndash"] = Convert.ToChar(8211);
            DecodeOnly["mdash"] = Convert.ToChar(8212);
            DecodeOnly["lsquo"] = Convert.ToChar(8216);
            DecodeOnly["rsquo"] = Convert.ToChar(8217);
            DecodeOnly["sbquo"] = Convert.ToChar(8218);
            DecodeOnly["ldquo"] = Convert.ToChar(8220);
            DecodeOnly["rdquo"] = Convert.ToChar(8221);
            DecodeOnly["bdquo"] = Convert.ToChar(8222);
            DecodeOnly["dagger"] = Convert.ToChar(8224);
            DecodeOnly["Dagger"] = Convert.ToChar(8225);
            DecodeOnly["bull"] = Convert.ToChar(8226);
            DecodeOnly["hellip"] = Convert.ToChar(8230);
            DecodeOnly["permil"] = Convert.ToChar(8240);
            DecodeOnly["prime"] = Convert.ToChar(8242);
            DecodeOnly["Prime"] = Convert.ToChar(8243);
            DecodeOnly["lsaquo"] = Convert.ToChar(8249);
            DecodeOnly["rsaquo"] = Convert.ToChar(8250);
            DecodeOnly["oline"] = Convert.ToChar(8254);
            DecodeOnly["euro"] = Convert.ToChar(8364);
            DecodeOnly["trade"] = Convert.ToChar(153);
            DecodeOnly["larr"] = Convert.ToChar(8592);
            DecodeOnly["uarr"] = Convert.ToChar(8593);
            DecodeOnly["rarr"] = Convert.ToChar(8594);
            DecodeOnly["darr"] = Convert.ToChar(8595);
            DecodeOnly["harr"] = Convert.ToChar(8596);
            DecodeOnly["crarr"] = Convert.ToChar(8629);
            DecodeOnly["lceil"] = Convert.ToChar(8968);
            DecodeOnly["rceil"] = Convert.ToChar(8969);
            DecodeOnly["lfloor"] = Convert.ToChar(8970);
            DecodeOnly["rfloor"] = Convert.ToChar(8971);
            DecodeOnly["loz"] = Convert.ToChar(9674);
            DecodeOnly["spades"] = Convert.ToChar(9824);
            DecodeOnly["clubs"] = Convert.ToChar(9827);
            DecodeOnly["hearts"] = Convert.ToChar(9829);
            DecodeOnly["diams"] = Convert.ToChar(9830);
        }

        /// <summary>
        /// Is the given html tag is single tag or can have content.
        /// </summary>
        /// <param name="tagName">the tag to check (must be lower case)</param>
        /// <returns>true - is single tag, false - otherwise</returns>
        public static bool IsSingleTag(string tagName)
        {
            return List.Contains(tagName);
        }

        /// <summary>
        /// Decode html encoded string to regular string.<br/>
        /// Handles &lt;, &gt;, "&amp;.
        /// </summary>
        /// <param name="str">the string to decode</param>
        /// <returns>decoded string</returns>
        public static string DecodeHtml(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = DecodeHtmlCharByCode(str);

                str = DecodeHtmlCharByName(str);

                foreach (var encPair in EncodeDecode)
                {
                    str = str.Replace(encPair.Key, encPair.Value);
                }
            }

            return str;
        }

        /// <summary>
        /// Encode regular string into html encoded string.<br/>
        /// Handles &lt;, &gt;, "&amp;.
        /// </summary>
        /// <param name="str">the string to encode</param>
        /// <returns>encoded string</returns>
        public static string EncodeHtml(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = EncodeDecode.Length - 1; i >= 0; i--)
                {
                    str = str.Replace(EncodeDecode[i].Value, EncodeDecode[i].Key);
                }
            }

            return str;
        }

        #region Private methods

        /// <summary>
        /// Decode html special charecters encoded using char entity code (&#8364;)
        /// </summary>
        /// <param name="str">the string to decode</param>
        /// <returns>decoded string</returns>
        private static string DecodeHtmlCharByCode(string str)
        {
            var idx = str.IndexOf("&#", StringComparison.OrdinalIgnoreCase);
            while (idx > -1)
            {
                bool hex = str.Length > idx + 3 && char.ToLower(str[idx + 2]) == 'x';
                var endIdx = idx + 2 + (hex ? 1 : 0);

                long num = 0;
                while (endIdx < str.Length && CommonUtils.IsDigit(str[endIdx], hex))
                    num = (num * (hex ? 16 : 10)) + CommonUtils.ToDigit(str[endIdx++], hex);
                endIdx += (endIdx < str.Length && str[endIdx] == ';') ? 1 : 0;

                str = str.Remove(idx, endIdx - idx);
                str = str.Insert(idx, Convert.ToChar(num).ToString());

                idx = str.IndexOf("&#", idx + 1);
            }

            return str;
        }

        /// <summary>
        /// Decode html special charecters encoded using char entity name (&#euro;)
        /// </summary>
        /// <param name="str">the string to decode</param>
        /// <returns>decoded string</returns>
        private static string DecodeHtmlCharByName(string str)
        {
            var idx = str.IndexOf('&');
            while (idx > -1)
            {
                var endIdx = str.IndexOf(';', idx);
                if (endIdx > -1 && endIdx - idx < 8)
                {
                    var key = str.Substring(idx + 1, endIdx - idx - 1);
                    char c;
                    if (DecodeOnly.TryGetValue(key, out c))
                    {
                        str = str.Remove(idx, endIdx - idx + 1);
                        str = str.Insert(idx, c.ToString());
                    }
                }

                idx = str.IndexOf('&', idx + 1);
            }

            return str;
        }

        #endregion
    }
}
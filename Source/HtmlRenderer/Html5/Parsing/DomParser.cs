using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    internal class DomParser : ITokenizerClient
    {
        /// <summary>
        /// The insertion mode is a state variable that controls the primary operation of the tree construction stage.
        /// </summary>
        private enum InsertionModeEnum : byte
        {
            Initial,
            BeforeHtml,
            BeforeHead,
            InHead,
            InHeadNoscript,
            AfterHead,
            InBody,
            Text,
            InTable,
            InTableText,
            InCaption,
            InColumnGroup,
            InTableBody,
            InRow,
            InCell,
            InSelect,
            InSelectInTable,
            InTemplate,
            AfterBody,
            InFrameset,
            AfterFrameset,
            AfterAfterBody,
            AfterAfterFrameset
        }

        private InsertionModeEnum InsertionMode;

        private InsertionModeEnum OriginalInsertionMode;

        private InsertionModeEnum ActiveInsertionMode;

        private void Switch(InsertionModeEnum mode)
        {
            if (this.InsertionMode == mode)
                return;

            // When the insertion mode is switched to "text" or "in table text",the original insertion mode
            // is also set. This is the insertion mode to which the tree construction stage will return.
            if ((mode == InsertionModeEnum.Text) || (mode == InsertionModeEnum.InTableText))
                this.OriginalInsertionMode = this.InsertionMode;

            // When the algorithm below says that the user agent is to do something "using the rules
            // for the m insertion mode", where m is one of these modes, the user agent must use the
            // rules described under the m insertion mode's section, but must leave the insertion mode
            // unchanged unless the rules in m themselves switch the insertion mode to a new value.
            this.InsertionMode = mode;
            this.ActiveInsertionMode = mode;
        }

        private void Using(InsertionModeEnum mode, Action action)
        {
            // When the algorithm below says that the user agent is to do something "using the rules
            // for the m insertion mode", where m is one of these modes, the user agent must use the
            // rules described under the m insertion mode's section, but must leave the insertion mode
            // unchanged unless the rules in m themselves switch the insertion mode to a new value.
            this.ActiveInsertionMode = mode;
            try
            {
                action();
            }
            finally
            {
                // Restore the activa insertion mode. This also takes care if the mode was switched.
                this.ActiveInsertionMode = this.InsertionMode;
            }
        }

        #region Tokanization

        private enum TokenType : byte
        {
            Unknown,

            Character,

            EndOfFile,

            DocType,

            Comment,

            StartTag,

            EndTag
        }

        private struct TokenData
        {
            #region Fields - keep this compact, so it can fit in a wide (16 byte) register.

            private TokenType _Type;

            private bool _TagIsSelfClosing;

            private char _Character;

            private string Name;

            public Attribute[] _TagAttributes;

            private object Data;

            #endregion

            public TokenType Type
            {
                get { return this._Type; }
            }

            public char Character
            {
                get
                {
#if DEBUG
                    if (this._Type != TokenType.Character)
                        throw new InvalidOperationException();
#endif
                    return this._Character;
                }
            }

            public string TagName
            {
                get
                {
#if DEBUG
                    if ((this._Type != TokenType.StartTag) && (this._Type != TokenType.EndTag))
                        throw new InvalidOperationException();
#endif
                    return this.Name;
                }
            }

            public bool TagIsSelfClosing
            {
                get
                {
#if DEBUG
                    if ((this._Type != TokenType.StartTag) && (this._Type != TokenType.EndTag))
                        throw new InvalidOperationException();
#endif
                    return this._TagIsSelfClosing;
                }
            }

            public Attribute[] TagAttributes
            {
                get
                {
#if DEBUG
                    if ((this._Type != TokenType.StartTag) && (this._Type != TokenType.EndTag))
                        throw new InvalidOperationException();
#endif
                    return this._TagAttributes;
                }
            }

            public string DocTypeName
            {
                get
                {
#if DEBUG
                    if (this._Type != TokenType.DocType)
                        throw new InvalidOperationException();
#endif
                    return this.Name;
                }
            }

            public string DocTypePublicIdentifier
            {
                get
                {
#if DEBUG
                    if (this._Type != TokenType.DocType)
                        throw new InvalidOperationException();
#endif
                    return ((Tuple<string, string, bool>)this.Data).Item1;
                }
            }

            public string DocTypeSystemIdentifier
            {
                get
                {
#if DEBUG
                    if (this._Type != TokenType.DocType)
                        throw new InvalidOperationException();
#endif
                    return ((Tuple<string, string, bool>)this.Data).Item2;
                }
            }

            public bool DocTypeForceQuirks
            {
                get
                {
#if DEBUG
                    if (this._Type != TokenType.DocType)
                        throw new InvalidOperationException();
#endif
                    return ((Tuple<string, string, bool>)this.Data).Item3;
                }
            }

            public string CommentData
            {
                get
                {
#if DEBUG
                    if (this._Type != TokenType.Comment)
                        throw new InvalidOperationException();
#endif
                    return ((Func<string>)this.Data)();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetToken()
            {
                this._Type = TokenType.Unknown;
                this.Data = null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCharacter(char ch)
            {
                this._Type = TokenType.Character;
                this._Character = ch;
            }

            public void SetEndOfFile()
            {
                this._Type = TokenType.EndOfFile;
            }

            public void SetDocType(string name, string publicIdentifier, string systemIdentifier, bool forceQuirks)
            {
                this._Type = TokenType.DocType;
                this.Name = name;
                this.Data = new Tuple<string, string, bool>(publicIdentifier, systemIdentifier, forceQuirks);
            }

            public void SetComment(Func<string> data)
            {
                this._Type = TokenType.Comment;
                this.Data = data;
            }

            public void SetStartTag(string tagName, bool isSelfClosing, Attribute[] attributes)
            {
                this._Type = TokenType.StartTag;
                this.Name = tagName;
                this._TagIsSelfClosing = isSelfClosing;
                this._TagAttributes = attributes;
            }

            public void SetEndTag(string tagName, bool isSelfClosing, Attribute[] attributes)
            {
                this._Type = TokenType.EndTag;
                this.Name = tagName;
                this._TagIsSelfClosing = isSelfClosing;
                this._TagAttributes = attributes;
            }

            /// <summary>
            /// A character token that is one of U + 0009 CHARACTER TABULATION, "LF"(U + 000A),
            /// "FF"(U + 000C), "CR"(U + 000D), or U + 0020 SPACE
            /// </summary>
            /// <returns></returns>
            public bool IsCharacterWhitespace()
            {
                // U + 0009 CHARACTER TABULATION, "LF"(U + 000A), "FF"(U + 000C), "CR"(U + 000D), or U + 0020 SPACE
                if (this._Type != TokenType.Character)
                    return false;
                return (this._Character == '\u0020') || (this._Character == '\u0009') ||
                    (this._Character == '\u000A') || (this._Character == '\u000C') || (this._Character == '\u000D');
            }

            public bool IsStartTagNamed(string tagName)
            {
                return (this._Type == TokenType.StartTag) && (this.Name == tagName);
            }

            public bool IsStartTagNamed(string tagName1, string tagName2)
            {
                return (this._Type == TokenType.StartTag) && ((this.Name == tagName1) || (this.Name == tagName2));
            }

            public bool IsStartTagNamed(string tagName1, string tagName2, string tagName3)
            {
                return (this._Type == TokenType.StartTag) && ((this.Name == tagName1) || (this.Name == tagName2) || (this.Name == tagName3));
            }

            public bool IsStartTagNamed(params string[] tagNames)
            {
                if ((tagNames == null) || (tagNames.Length < 1))
                    return false;

                if (this._Type != TokenType.StartTag)
                    return false;

                for (int i = 0; i < tagNames.Length; i++)
                {
                    if (this.Name == tagNames[i])
                        return true;
                }

                return false;
            }

            public bool IsEndTagNamed(string tagName)
            {
                return (this._Type == TokenType.EndTag) && (this.Name == tagName);
            }

            public bool IsEndTagNamed(string tagName1, string tagName2)
            {
                return (this._Type == TokenType.EndTag) && ((this.Name == tagName1) || (this.Name == tagName2));
            }

            public bool IsEndTagNamed(string tagName1, string tagName2, string tagName3)
            {
                return (this._Type == TokenType.EndTag) && ((this.Name == tagName1) || (this.Name == tagName2) || (this.Name == tagName3));
            }

            public bool IsEndTagNamed(params string[] tagNames)
            {
                if ((tagNames == null) || (tagNames.Length < 1))
                    return false;

                if (this._Type != TokenType.EndTag)
                    return false;

                for (int i = 0; i < tagNames.Length; i++)
                {
                    if (this.Name == tagNames[i])
                        return true;
                }

                return false;
            }

            public override string ToString()
            {
                switch (this._Type)
                {
                    case TokenType.Unknown:
                        return "** UNKNOWN **";
                    case TokenType.Character:
                        return this.Character.ToString();
                    case TokenType.EndOfFile:
                        return "** EOF **";
                    case TokenType.DocType:
                        return String.Format(
                            "<!DOCTYPE {0} {1} {2} {3}>",
                            this.DocTypeName,
                            this.DocTypePublicIdentifier,
                            this.DocTypeSystemIdentifier,
                            this.DocTypeForceQuirks ? "QUIRKS" : "");
                    case TokenType.Comment:
                        return String.Format("<--{0}-->", this.CommentData);
                    case TokenType.StartTag:
                        return String.Format(
                            "<{0} {1} {2}>",
                            this.TagName,
                            String.Join(" ", this.TagAttributes.Select(attr => String.Format("{0}=\"{1}\"", attr.Name, attr.Value))),
                            this.TagIsSelfClosing ? "/" : "");
                    case TokenType.EndTag:
                        return String.Format("</{0}>", this.TagName);
                    default:
                        return "** UNKNOWN **";
                }
            }
        }

        private TokenData Token;

        #region ITokenizerClient interface

        void ITokenizerClient.ParseError(ParseErrors error)
        {
            throw new NotImplementedException();
        }

        void ITokenizerClient.ReceiveCharacter(char character)
        {
            try
            {
                this.Token.SetCharacter(character);
                this.ProcessToken();
            }
            finally
            {
                this.Token.ResetToken();
            }
        }

        void ITokenizerClient.ReceiveComment(Func<string> data)
        {
            try
            {
                this.Token.SetComment(data);
                this.ProcessToken();
            }
            finally
            {
                this.Token.ResetToken();
            }
        }

        void ITokenizerClient.ReceiveDocType(string name, string publicIdentifier, string systemIdentifier, bool forceQuirks)
        {
            try
            {
                this.Token.SetDocType(name, publicIdentifier, systemIdentifier, forceQuirks);
                this.ProcessToken();
            }
            finally
            {
                this.Token.ResetToken();
            }
        }

        void ITokenizerClient.ReceiveEndOfFile()
        {
            try
            {
                this.Token.SetEndOfFile();
                this.ProcessToken();
            }
            finally
            {
                this.Token.ResetToken();
            }
        }

        void ITokenizerClient.ReceiveEndTag(string tagName, bool isSelfClosing, Attribute[] attributes)
        {
            try
            {
                this.Token.SetEndTag(tagName, isSelfClosing, attributes);
                this.ProcessToken();
            }
            finally
            {
                this.Token.ResetToken();
            }
        }

        void ITokenizerClient.ReceiveStartTag(string tagName, bool isSelfClosing, Attribute[] attributes)
        {
            try
            {
                this.Token.SetStartTag(tagName, isSelfClosing, attributes);
                this.ProcessToken();
            }
            finally
            {
                this.Token.ResetToken();
            }
        }

        #endregion

        public void Parse(HtmlStream html)
        {
            Tokenizer tokenizer = new Tokenizer(html, this);
            tokenizer.Tokenize();
        }

        #endregion

        #region Tree Construction Helpers

        private void ParseError(ParseErrors error)
        {
            throw new NotImplementedException();
        }

        private void InsertComment()
        {

        }

        #endregion

        #region 8.2.5 Tree construction

        private void ProcessToken()
        {
            switch (this.ActiveInsertionMode)
            {
                case InsertionModeEnum.Initial:
                    this.HandleTokenInInitialMode();
                    break;
                case InsertionModeEnum.BeforeHtml:
                    this.HandleTokenInBeforeHtmlMode();
                    break;
                case InsertionModeEnum.BeforeHead:
                    this.HandleTokenInBeforeHeadMode();
                    break;
                case InsertionModeEnum.InHead:
                    this.HandleTokenInInHeadMode();
                    break;
                case InsertionModeEnum.InHeadNoscript:
                    this.HandleTokenInInHeadNoscriptMode();
                    break;
                case InsertionModeEnum.AfterHead:
                    this.HandleTokenInAfterHeadMode();
                    break;
                case InsertionModeEnum.InBody:
                    this.HandleTokenInInBodyMode();
                    break;
                case InsertionModeEnum.Text:
                    this.HandleTokenInTextMode();
                    break;
                case InsertionModeEnum.InTable:
                    this.HandleTokenInInTableMode();
                    break;
                case InsertionModeEnum.InTableText:
                    this.HandleTokenInInTableTextMode();
                    break;
                case InsertionModeEnum.InCaption:
                    this.HandleTokenInInCaptionMode();
                    break;
                case InsertionModeEnum.InColumnGroup:
                    this.HandleTokenInInColumnGroupMode();
                    break;
                case InsertionModeEnum.InTableBody:
                    this.HandleTokenInInTableBodyMode();
                    break;
                case InsertionModeEnum.InRow:
                    this.HandleTokenInInRowMode();
                    break;
                case InsertionModeEnum.InCell:
                    this.HandleTokenInInCellMode();
                    break;
                case InsertionModeEnum.InSelect:
                    this.HandleTokenInInSelectMode();
                    break;
                case InsertionModeEnum.InSelectInTable:
                    this.HandleTokenInInSelectInTableMode();
                    break;
                case InsertionModeEnum.InTemplate:
                    this.HandleTokenInInTemplateMode();
                    break;
                case InsertionModeEnum.AfterBody:
                    this.HandleTokenInAfterBodyMode();
                    break;
                case InsertionModeEnum.InFrameset:
                    this.HandleTokenInInFramesetMode();
                    break;
                case InsertionModeEnum.AfterFrameset:
                    this.HandleTokenInAfterFramesetMode();
                    break;
                case InsertionModeEnum.AfterAfterBody:
                    this.HandleTokenInAfterAfterBodyMode();
                    break;
                case InsertionModeEnum.AfterAfterFrameset:
                    this.HandleTokenInAfterAfterFramesetMode();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInitialMode()
        {
            /*
            When the user agent is to apply the rules for the "initial" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token that is one of U + 0009 CHARACTER TABULATION, "LF"(U + 000A),
            // "FF"(U + 000C), "CR"(U + 000D), or U + 0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                // Ignore the token.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment as the last child of the Document object.
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //If the DOCTYPE token's name is not a case-sensitive match for the string "html",
                //or the token's public identifier is not missing, or the token's system identifier
                //is neither missing nor a case-sensitive match for the string "about:legacy-compat",
                //and none of the sets of conditions in the following list are matched, then there is a parse error.
                //    The DOCTYPE token's name is a case-sensitive match for the string "html", the token's public identifier is the case-sensitive string "-//W3C//DTD HTML 4.0//EN", and the token's system identifier is either missing or the case-sensitive string "http://www.w3.org/TR/REC-html40/strict.dtd".
                //    The DOCTYPE token's name is a case-sensitive match for the string "html", the token's public identifier is the case-sensitive string "-//W3C//DTD HTML 4.01//EN", and the token's system identifier is either missing or the case-sensitive string "http://www.w3.org/TR/html4/strict.dtd".
                //    The DOCTYPE token's name is a case-sensitive match for the string "html", the token's public identifier is the case-sensitive string "-//W3C//DTD XHTML 1.0 Strict//EN", and the token's system identifier is the case-sensitive string "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd".
                //    The DOCTYPE token's name is a case-sensitive match for the string "html", the token's public identifier is the case-sensitive string "-//W3C//DTD XHTML 1.1//EN", and the token's system identifier is the case-sensitive string "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd".

                //Conformance checkers may, based on the values(including presence or lack thereof) of the
                //DOCTYPE token's name, public identifier, or system identifier, switch to a conformance
                //checking mode for another language (e.g. based on the DOCTYPE token a conformance checker
                //could recognize that the document is an HTML4-era document, and defer to an HTML4 conformance checker.)

                //Append a DocumentType node to the Document node, with the name attribute set to the name
                //given in the DOCTYPE token, or the empty string if the name was missing; the publicId attribute
                //set to the public identifier given in the DOCTYPE token, or the empty string if the
                //public identifier was missing; the systemId attribute set to the system identifier given
                //in the DOCTYPE token, or the empty string if the system identifier was missing; and the other
                //attributes specific to DocumentType objects set to null and empty lists as appropriate.Associate
                //the DocumentType node with the Document object so that it is returned as the value of the doctype
                //attribute of the Document object.

                //Then, if the document is not an iframe srcdoc document, and the DOCTYPE token matches one of the
                //conditions in the following list, then set the Document to quirks mode:
                //    The force-quirks flag is set to on.
                //    The name is set to anything other than "html" (compared case-sensitively).
                //    The public identifier starts with: "+//Silmaril//dtd html Pro v0r11 19970101//"
                //    The public identifier starts with: "-//AdvaSoft Ltd//DTD HTML 3.0 asWedit + extensions//"
                //    The public identifier starts with: "-//AS//DTD HTML 3.0 asWedit + extensions//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.0 Level 1//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.0 Level 2//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.0 Strict Level 1//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.0 Strict Level 2//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.0 Strict//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.0//"
                //    The public identifier starts with: "-//IETF//DTD HTML 2.1E//"
                //    The public identifier starts with: "-//IETF//DTD HTML 3.0//"
                //    The public identifier starts with: "-//IETF//DTD HTML 3.2 Final//"
                //    The public identifier starts with: "-//IETF//DTD HTML 3.2//"
                //    The public identifier starts with: "-//IETF//DTD HTML 3//"
                //    The public identifier starts with: "-//IETF//DTD HTML Level 0//"
                //    The public identifier starts with: "-//IETF//DTD HTML Level 1//"
                //    The public identifier starts with: "-//IETF//DTD HTML Level 2//"
                //    The public identifier starts with: "-//IETF//DTD HTML Level 3//"
                //    The public identifier starts with: "-//IETF//DTD HTML Strict Level 0//"
                //    The public identifier starts with: "-//IETF//DTD HTML Strict Level 1//"
                //    The public identifier starts with: "-//IETF//DTD HTML Strict Level 2//"
                //    The public identifier starts with: "-//IETF//DTD HTML Strict Level 3//"
                //    The public identifier starts with: "-//IETF//DTD HTML Strict//"
                //    The public identifier starts with: "-//IETF//DTD HTML//"
                //    The public identifier starts with: "-//Metrius//DTD Metrius Presentational//"
                //    The public identifier starts with: "-//Microsoft//DTD Internet Explorer 2.0 HTML Strict//"
                //    The public identifier starts with: "-//Microsoft//DTD Internet Explorer 2.0 HTML//"
                //    The public identifier starts with: "-//Microsoft//DTD Internet Explorer 2.0 Tables//"
                //    The public identifier starts with: "-//Microsoft//DTD Internet Explorer 3.0 HTML Strict//"
                //    The public identifier starts with: "-//Microsoft//DTD Internet Explorer 3.0 HTML//"
                //    The public identifier starts with: "-//Microsoft//DTD Internet Explorer 3.0 Tables//"
                //    The public identifier starts with: "-//Netscape Comm. Corp.//DTD HTML//"
                //    The public identifier starts with: "-//Netscape Comm. Corp.//DTD Strict HTML//"
                //    The public identifier starts with: "-//O'Reilly and Associates//DTD HTML 2.0//"
                //    The public identifier starts with: "-//O'Reilly and Associates//DTD HTML Extended 1.0//"
                //    The public identifier starts with: "-//O'Reilly and Associates//DTD HTML Extended Relaxed 1.0//"
                //    The public identifier starts with: "-//SoftQuad Software//DTD HoTMetaL PRO 6.0::19990601::extensions to HTML 4.0//"
                //    The public identifier starts with: "-//SoftQuad//DTD HoTMetaL PRO 4.0::19971010::extensions to HTML 4.0//"
                //    The public identifier starts with: "-//Spyglass//DTD HTML 2.0 Extended//"
                //    The public identifier starts with: "-//SQ//DTD HTML 2.0 HoTMetaL + extensions//"
                //    The public identifier starts with: "-//Sun Microsystems Corp.//DTD HotJava HTML//"
                //    The public identifier starts with: "-//Sun Microsystems Corp.//DTD HotJava Strict HTML//"
                //    The public identifier starts with: "-//W3C//DTD HTML 3 1995-03-24//"
                //    The public identifier starts with: "-//W3C//DTD HTML 3.2 Draft//"
                //    The public identifier starts with: "-//W3C//DTD HTML 3.2 Final//"
                //    The public identifier starts with: "-//W3C//DTD HTML 3.2//"
                //    The public identifier starts with: "-//W3C//DTD HTML 3.2S Draft//"
                //    The public identifier starts with: "-//W3C//DTD HTML 4.0 Frameset//"
                //    The public identifier starts with: "-//W3C//DTD HTML 4.0 Transitional//"
                //    The public identifier starts with: "-//W3C//DTD HTML Experimental 19960712//"
                //    The public identifier starts with: "-//W3C//DTD HTML Experimental 970421//"
                //    The public identifier starts with: "-//W3C//DTD W3 HTML//"
                //    The public identifier starts with: "-//W3O//DTD W3 HTML 3.0//"
                //    The public identifier is set to: "-//W3O//DTD W3 HTML Strict 3.0//EN//"
                //    The public identifier starts with: "-//WebTechs//DTD Mozilla HTML 2.0//"
                //    The public identifier starts with: "-//WebTechs//DTD Mozilla HTML//"
                //    The public identifier is set to: "-/W3C/DTD HTML 4.0 Transitional/EN"
                //    The public identifier is set to: "HTML"
                //    The system identifier is set to: "http://www.ibm.com/data/dtd/v11/ibmxhtml1-transitional.dtd"
                //    The system identifier is missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Frameset//"
                //    The system identifier is missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Transitional//"

                //Otherwise, if the document is not an iframe srcdoc document, and the DOCTYPE token matches one of the conditions
                //in the following list, then set the Document to limited-quirks mode:
                //    The public identifier starts with: "-//W3C//DTD XHTML 1.0 Frameset//"
                //    The public identifier starts with: "-//W3C//DTD XHTML 1.0 Transitional//"
                //    The system identifier is not missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Frameset//"
                //    The system identifier is not missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Transitional//"

                //The system identifier and public identifier strings must be compared to the values given
                //in the lists above in an ASCII case-insensitive manner.A system identifier whose value
                //is the empty string is not considered missing for the purposes of the conditions above.

                //Then, switch the insertion mode to "before html".
            }

            // Anything else
            else
            {
                //If the document is not an iframe srcdoc document, then this is a parse error; set the Document to quirks mode.
                //In any case, switch the insertion mode to "before html", then reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInBeforeHtmlMode()
        {
            /*
            8.2.5.4.2 The "before html" insertion mode

            When the user agent is to apply the rules for the "before html" insertion mode,
            the user agent must handle the token as follows:
            */

            bool anythingElse = false;

            // A DOCTYPE token
            if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.ParseError(ParseErrors.UnexpectedTag);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment as the last child of the Document object.
                throw new NotImplementedException();
            }

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            else if (this.Token.IsCharacterWhitespace())
            {
                // Ignore the token.
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Create an element for the token in the HTML namespace, with the Document as the intended parent.
                // Append it to the Document object. Put this element in the stack of open elements.

                // If the Document is being loaded as part of navigation of a browsing context, then:
                // if the newly created element has a manifest attribute whose value is not the empty string,
                // then resolve the value of that attribute to an absolute URL, relative to the newly created element,
                // and if that is successful, run the application cache selection algorithm with the result of applying
                // the URL serializer algorithm to the resulting parsed URL with the exclude fragment flag set; otherwise,
                // if there is no such attribute, or its value is the empty string, or resolving its value fails,
                // run the application cache selection algorithm with no manifest.The algorithm must be passed the Document object.

                // Switch the insertion mode to "before head".
            }

            // An end tag whose tag name is one of: "head", "body", "html", "br"
            else if (this.Token.IsEndTagNamed(Tags.Html, Tags.Body, Tags.Html, Tags.Br))
            {
                // Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                // Parse error. Ignore the token.
                this.ParseError(ParseErrors.UnespectedEndTag);
            }

            // Anything else 
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Create an html element whose ownerDocument is the Document object. Append it to the Document object.
                // Put this element in the stack of open elements.

                // If the Document is being loaded as part of navigation of a browsing context, then:
                // run the application cache selection algorithm with no manifest, passing it the Document object.

                // Switch the insertion mode to "before head", then reprocess the token.
            }

            /*
            The root element can end up being removed from the Document object, e.g. by scripts;
            nothing in particular happens in such cases, content continues being appended to the nodes
            as described in the next section.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInBeforeHeadMode()
        {
            /*
            8.2.5.4.3 The "before head" insertion mode

            When the user agent is to apply the rules for the "before head" insertion mode,
            the user agent must handle the token as follows:
            */

            bool anythingElse = false;

            //A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            //"FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                //    Ignore the token.
            }

            //A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment.
            }

            //A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            //A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            //A start tag whose tag name is "head"
            else if (this.Token.IsStartTagNamed(Tags.Head))
            {
                //    Insert an HTML element for the token.

                //    Set the head element pointer to the newly created head element.

                //    Switch the insertion mode to "in head".
            }

            //An end tag whose tag name is one of: "head", "body", "html", "br"
            else if (this.Token.IsEndTagNamed(Tags.Head, Tags.Body, Tags.Html, Tags.Br))
            {
                //    Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            //Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                //    Parse error. Ignore the token.
            }

            //Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                //    Insert an HTML element for a "head" start tag token with no attributes.

                //    Set the head element pointer to the newly created head element.

                //    Switch the insertion mode to "in head".

                //    Reprocess the current token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInHeadMode()
        {
            /*
            8.2.5.4.4 The "in head" insertion mode

            When the user agent is to apply the rules for the "in head" insertion mode,
            the user agent must handle the token as follows:
            */

            bool anythingElse = false;

            //A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            //"FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                //    Insert the character.
            }

            //A comment token
            else if (this.Token)
            {
                //    Insert a comment.
            }

            //A DOCTYPE token
            else if (this.Token)
            {
                //    Parse error. Ignore the token.
            }

            //A start tag whose tag name is "html"
            else if (this.Token)
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            //A start tag whose tag name is one of: "base", "basefont", "bgsound", "link"
            else if (this.Token)
            {
                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.
            }

            //A start tag whose tag name is "meta"
            else if (this.Token)
            {
                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.

                //    If the element has a charset attribute, and getting an encoding from its value results in a
                //    supported ASCII-compatible character encoding or a UTF-16 encoding, and the confidence is
                //    currently tentative, then change the encoding to the resulting encoding.

                //    Otherwise, if the element has an http-equiv attribute whose value is an ASCII case-insensitive
                //    match for the string "Content-Type", and the element has a content attribute, and applying the
                //    algorithm for extracting a character encoding from a meta element to that attribute's value returns
                //    a supported ASCII-compatible character encoding or a UTF-16 encoding, and the confidence is currently
                //    tentative, then change the encoding to the extracted encoding.
            }

            //A start tag whose tag name is "title"
            else if (this.Token)
            {
                //    Follow the generic RCDATA element parsing algorithm.
            }

            //A start tag whose tag name is "noscript", if the scripting flag is enabled
            //A start tag whose tag name is one of: "noframes", "style"
            else if (this.Token)
            {
                //    Follow the generic raw text element parsing algorithm.
            }

            //A start tag whose tag name is "noscript", if the scripting flag is disabled
            else if (this.Token)
            {
                //    Insert an HTML element for the token.

                //    Switch the insertion mode to "in head noscript".
            }

            //A start tag whose tag name is "script"
            else if (this.Token)
            {
                //    Run these steps:
                //        1. Let the adjusted insertion location be the appropriate place for inserting a node.
                //        2. Create an element for the token in the HTML namespace, with the intended parent being
                //           the element in which the adjusted insertion location finds itself.
                //        3. Mark the element as being "parser-inserted" and unset the element's "force-async" flag.
                //            NOTE: This ensures that, if the script is external, any document.write() calls in the
                //            script will execute in-line, instead of blowing the document away, as would happen in
                //            most other cases. It also prevents the script from executing until the end tag is seen.
                //        4. If the parser was originally created for the HTML fragment parsing algorithm, then mark the
                //           script element as "already started". (fragment case)
                //        5. Insert the newly created element at the adjusted insertion location.
                //        6. Push the element onto the stack of open elements so that it is the new current node.
                //        7. Switch the tokenizer to the script data state.
                //        8. Let the original insertion mode be the current insertion mode.
                //        9. Switch the insertion mode to "text".
            }

            //An end tag whose tag name is "head"
            else if (this.Token)
            {
                //    Pop the current node (which will be the head element) off the stack of open elements.

                //    Switch the insertion mode to "after head".
            }

            //An end tag whose tag name is one of: "body", "html", "br"
            else if (this.Token)
            {
                //    Act as described in the "anything else" entry below.
            }

            //A start tag whose tag name is "template"
            else if (this.Token)
            {
                //    Insert an HTML element for the token.

                //    Insert a marker at the end of the list of active formatting elements.

                //    Set the frameset-ok flag to "not ok".

                //    Switch the insertion mode to "in template".

                //    Push "in template" onto the stack of template insertion modes so that it is the new
                //    current template insertion mode.
            }

            //An end tag whose tag name is "template"
            else if (this.Token)
            {
                //    If there is no template element on the stack of open elements, then this is a parse error; ignore the token.

                //    Otherwise, run these steps:
                //        1. Generate implied end tags.
                //        2. If the current node is not a template element, then this is a parse error.
                //        3. Pop elements from the stack of open elements until a template element has been popped from the stack.
                //        4. Clear the list of active formatting elements up to the last marker.
                //        5. Pop the current template insertion mode off the stack of template insertion modes.
                //        6. Reset the insertion mode appropriately.
            }

            //A start tag whose tag name is "head"
            //Any other end tag
            else if (this.Token)
            {
                //    Parse error. Ignore the token.
            }

            //Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                //    Pop the current node (which will be the head element) off the stack of open elements.

                //    Switch the insertion mode to "after head".

                //    Reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInHeadNoscriptMode()
        {
            /*
            8.2.5.4.5 The "in head noscript" insertion mode

            When the user agent is to apply the rules for the "in head noscript" insertion mode,
            the user agent must handle the token as follows:
            */

            bool anythingElse = false;

            //A DOCTYPE token
            if (this.Token)
            {
                //    Parse error. Ignore the token.
            }

            //A start tag whose tag name is "html"
            else if (this.Token)
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            //An end tag whose tag name is "noscript"
            else if (this.Token)
            {
                //    Pop the current node (which will be a noscript element) from the stack of open elements;
                //    the new current node will be a head element.

                //    Switch the insertion mode to "in head".
            }

            //A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            //"FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            //A comment token
            //A start tag whose tag name is one of: "basefont", "bgsound", "link", "meta", "noframes", "style"
            else if (this.Token)
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            //An end tag whose tag name is "br"
            else if (this.Token)
            {
                //    Act as described in the "anything else" entry below.
            }

            //A start tag whose tag name is one of: "head", "noscript"
            //Any other end tag
            else if (this.Token)
            {
                //    Parse error. Ignore the token.
            }

            //Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                //    Parse error.

                //    Pop the current node (which will be a noscript element) from the stack of open elements;
                //    the new current node will be a head element.

                //    Switch the insertion mode to "in head".

                //    Reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterHeadMode()
        {
            /*
            8.2.5.4.6 The "after head" insertion mode

            When the user agent is to apply the rules for the "after head" insertion mode,
            the user agent must handle the token as follows:

            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
                Insert the character.

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            A start tag whose tag name is "body"
                Insert an HTML element for the token.

                Set the frameset-ok flag to "not ok".

                Switch the insertion mode to "in body".

            A start tag whose tag name is "frameset"
                Insert an HTML element for the token.

                Switch the insertion mode to "in frameset".

            A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta",
            "noframes", "script", "style", "template", "title"
                Parse error.

                Push the node pointed to by the head element pointer onto the stack of open elements.

                Process the token using the rules for the "in head" insertion mode.

                Remove the node pointed to by the head element pointer from the stack of open elements.
                (It might not be the current node at this point.)

                NOTE: The head element pointer cannot be null at this point.

            An end tag whose tag name is "template"
                Process the token using the rules for the "in head" insertion mode.

            An end tag whose tag name is one of: "body", "html", "br"
                Act as described in the "anything else" entry below.

            A start tag whose tag name is "head"
            Any other end tag
                Parse error. Ignore the token.

            Anything else
                Insert an HTML element for a "body" start tag token with no attributes.

                Switch the insertion mode to "in body".

                Reprocess the current token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInBodyMode()
        {
            /*
            8.2.5.4.7 The "in body" insertion mode

            When the user agent is to apply the rules for the "in body" insertion mode,
            the user agent must handle the token as follows:

            A character token that is U+0000 NULL
                Parse error. Ignore the token.

            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
                Reconstruct the active formatting elements, if any.

                Insert the token's character.

            Any other character token
                Reconstruct the active formatting elements, if any.

                Insert the token's character.

                Set the frameset-ok flag to "not ok".

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Parse error.

                If there is a template element on the stack of open elements, then ignore the token.

                Otherwise, for each attribute on the token, check to see if the attribute is already present
                on the top element of the stack of open elements. If it is not, add the attribute and its
                corresponding value to that element.

            A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta", "noframes",
            "script", "style", "template", "title"
            An end tag whose tag name is "template"
                Process the token using the rules for the "in head" insertion mode.

            A start tag whose tag name is "body"
                Parse error.

                If the second element on the stack of open elements is not a body element, if the stack of open elements
                has only one node on it, or if there is a template element on the stack of open elements,
                then ignore the token. (fragment case)

                Otherwise, set the frameset-ok flag to "not ok"; then, for each attribute on the token, check to see if
                the attribute is already present on the body element (the second element) on the stack of open elements,
                and if it is not, add the attribute and its corresponding value to that element.

            A start tag whose tag name is "frameset"
                Parse error.

                If the stack of open elements has only one node on it, or if the second element on the stack of open
                elements is not a body element, then ignore the token. (fragment case)

                If the frameset-ok flag is set to "not ok", ignore the token.

                Otherwise, run the following steps:
                    1. Remove the second element on the stack of open elements from its parent node, if it has one.
                    2. Pop all the nodes from the bottom of the stack of open elements, from the current node up to,
                       but not including, the root html element.
                    3. Insert an HTML element for the token.
                    4. Switch the insertion mode to "in frameset".

            An end-of-file token
                If there is a node in the stack of open elements that is not either a dd element, a dt element,
                an li element, a p element, a tbody element, a td element, a tfoot element, a th element,
                a thead element, a tr element, the body element, or the html element, then this is a parse error.

                If the stack of template insertion modes is not empty, then process the token using the rules for
                the "in template" insertion mode.

                Otherwise, stop parsing.

            An end tag whose tag name is "body"
                If the stack of open elements does not have a body element in scope, this is a parse error; ignore the token.

                Otherwise, if there is a node in the stack of open elements that is not either a dd element,
                a dt element, an li element, an optgroup element, an option element, a p element, an rb element,
                an rp element, an rt element, an rtc element, a tbody element, a td element, a tfoot element,
                a th element, a thead element, a tr element, the body element, or the html element, then this is a parse error.

                Switch the insertion mode to "after body".

            An end tag whose tag name is "html"
                If the stack of open elements does not have a body element in scope, this is a parse error; ignore the token.

                Otherwise, if there is a node in the stack of open elements that is not either a dd element, a dt element,
                an li element, an optgroup element, an option element, a p element, an rb element, an rp element,
                an rt element, an rtc element, a tbody element, a td element, a tfoot element, a th element, a thead element,
                a tr element, the body element, or the html element, then this is a parse error.

                Switch the insertion mode to "after body".

                Reprocess the token.

            A start tag whose tag name is one of: "address", "article", "aside", "blockquote", "center", "details", "dialog",
            "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup", "main", "nav", "ol", "p",
            "section", "summary", "ul"
                If the stack of open elements has a p element in button scope, then close a p element.

                Insert an HTML element for the token.

            A start tag whose tag name is one of: "h1", "h2", "h3", "h4", "h5", "h6"
                If the stack of open elements has a p element in button scope, then close a p element.

                If the current node is an HTML element whose tag name is one of "h1", "h2", "h3", "h4", "h5", or "h6",
                then this is a parse error; pop the current node off the stack of open elements.

                Insert an HTML element for the token.

            A start tag whose tag name is one of: "pre", "listing"
                If the stack of open elements has a p element in button scope, then close a p element.

                Insert an HTML element for the token.

                If the next token is a "LF" (U+000A) character token, then ignore that token and move on to the
                next one. (Newlines at the start of pre blocks are ignored as an authoring convenience.)

                Set the frameset-ok flag to "not ok".

            A start tag whose tag name is "form"
                If the form element pointer is not null, and there is no template element on the stack of open elements,
                then this is a parse error; ignore the token.

                Otherwise:
                    If the stack of open elements has a p element in button scope, then close a p element.

                Insert an HTML element for the token, and, if there is no template element on the stack of
                open elements, set the form element pointer to point to the element created.

            A start tag whose tag name is "li"
                Run these steps:
                    1. Set the frameset-ok flag to "not ok".
                    2. Initialize node to be the current node (the bottommost node of the stack).
                    3. Loop: If node is an li element, then run these substeps:
                        1. Generate implied end tags, except for li elements.
                        2. If the current node is not an li element, then this is a parse error.
                        3. Pop elements from the stack of open elements until an li element has been popped from the stack.
                        4. Jump to the step labeled done below.
                    4. If node is in the special category, but is not an address, div, or p element,
                       then jump to the step labeled done below.
                    5. Otherwise, set node to the previous entry in the stack of open elements and return to the step labeled loop.
                    6. Done: If the stack of open elements has a p element in button scope, then close a p element.
                    7. Finally, insert an HTML element for the token.

            A start tag whose tag name is one of: "dd", "dt"
                Run these steps:
                    1. Set the frameset-ok flag to "not ok".
                    2. Initialize node to be the current node (the bottommost node of the stack).
                    3. Loop: If node is a dd element, then run these substeps:
                        1. Generate implied end tags, except for dd elements.
                        2. If the current node is not a dd element, then this is a parse error.
                        3. Pop elements from the stack of open elements until a dd element has been popped from the stack.
                        4. Jump to the step labeled done below.
                    4. If node is a dt element, then run these substeps:
                        1. Generate implied end tags, except for dt elements.
                        2. If the current node is not a dt element, then this is a parse error.
                        3. Pop elements from the stack of open elements until a dt element has been popped from the stack.
                        4. Jump to the step labeled done below.
                    5. If node is in the special category, but is not an address, div, or p element,
                       then jump to the step labeled done below.
                    6. Otherwise, set node to the previous entry in the stack of open elements and return to the step labeled loop.
                    7. Done: If the stack of open elements has a p element in button scope, then close a p element.
                    8. Finally, insert an HTML element for the token.

            A start tag whose tag name is "plaintext"
                If the stack of open elements has a p element in button scope, then close a p element.

                Insert an HTML element for the token.

                Switch the tokenizer to the PLAINTEXT state.

                NOTE: Once a start tag with the tag name "plaintext" has been seen, that will be the last
                token ever seen other than character tokens (and the end-of-file token), because there is
                no way to switch out of the PLAINTEXT state.

            A start tag whose tag name is "button"
                1. If the stack of open elements has a button element in scope, then run these substeps:
                    1. Parse error.
                    2. Generate implied end tags.
                    3. Pop elements from the stack of open elements until a button element has been popped from the stack.
                2. Reconstruct the active formatting elements, if any.
                3. Insert an HTML element for the token.
                4. Set the frameset-ok flag to "not ok".

            An end tag whose tag name is one of: "address", "article", "aside", "blockquote", "button", "center", "details",
            "dialog", "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup", "listing", "main",
            "nav", "ol", "pre", "section", "summary", "ul"
                If the stack of open elements does not have an element in scope that is an HTML element and with the same
                tag name as that of the token, then this is a parse error; ignore the token.

                Otherwise, run these steps:
                    1. Generate implied end tags.
                    2. If the current node is not an HTML element with the same tag name as that of the token,
                       then this is a parse error.
                    3. Pop elements from the stack of open elements until an HTML element with the same tag name
                       as the token has been popped from the stack.

            An end tag whose tag name is "form"
                If there is no template element on the stack of open elements, then run these substeps:
                    1. Let node be the element that the form element pointer is set to, or null if it is not set to an element.
                    2. Set the form element pointer to null. Otherwise, let node be null.
                    3. If node is null or if the stack of open elements does not have node in scope, then this is a parse error;
                       abort these steps and ignore the token.
                    4. Generate implied end tags.
                    5. If the current node is not node, then this is a parse error.
                    6. Remove node from the stack of open elements.

                If there is a template element on the stack of open elements, then run these substeps instead:
                    1. If the stack of open elements does not have a form element in scope, then this is a parse error;
                       abort these steps and ignore the token.
                    2. Generate implied end tags.
                    3. If the current node is not a form element, then this is a parse error.
                    4. Pop elements from the stack of open elements until a form element has been popped from the stack.

            An end tag whose tag name is "p"
                If the stack of open elements does not have a p element in button scope, then this is a parse error;
                insert an HTML element for a "p" start tag token with no attributes.

                Close a p element.

            An end tag whose tag name is "li"
                If the stack of open elements does not have an li element in list item scope,
                then this is a parse error; ignore the token.

                Otherwise, run these steps:
                    1. Generate implied end tags, except for li elements.
                    2. If the current node is not an li element, then this is a parse error.
                    3. Pop elements from the stack of open elements until an li element has been popped from the stack.

            An end tag whose tag name is one of: "dd", "dt"
                If the stack of open elements does not have an element in scope that is an HTML element and with the
                same tag name as that of the token, then this is a parse error; ignore the token.

                Otherwise, run these steps:
                    1. Generate implied end tags, except for HTML elements with the same tag name as the token.
                    2. If the current node is not an HTML element with the same tag name as that of the token,
                       then this is a parse error.
                    3. Pop elements from the stack of open elements until an HTML element with the same tag name
                       as the token has been popped from the stack.

            An end tag whose tag name is one of: "h1", "h2", "h3", "h4", "h5", "h6"
                If the stack of open elements does not have an element in scope that is an HTML element and whose tag
                name is one of "h1", "h2", "h3", "h4", "h5", or "h6", then this is a parse error; ignore the token.

                Otherwise, run these steps:
                    1. Generate implied end tags.
                    2. If the current node is not an HTML element with the same tag name as that of the token, then this
                       is a parse error.
                    3. Pop elements from the stack of open elements until an HTML element whose tag name is one of
                       "h1", "h2", "h3", "h4", "h5", or "h6" has been popped from the stack.

            An end tag whose tag name is "sarcasm"
                Take a deep breath, then act as described in the "any other end tag" entry below.

            A start tag whose tag name is "a"
                If the list of active formatting elements contains an a element between the end of the
                list and the last marker on the list (or the start of the list if there is no marker on the list),
                then this is a parse error; run the adoption agency algorithm for the tag name "a", then remove that
                element from the list of active formatting elements and the stack of open elements if the adoption agency
                algorithm didn't already remove it (it might not have if the element is not in table scope).

                    In the non-conforming stream <a href="a">a<table><a href="b">b</table>x, the first a element would be
                    closed upon seeing the second one, and the "x" character would be inside a link to "b", not to "a".
                    This is despite the fact that the outer a element is not in table scope (meaning that a regular </a>
                    end tag at the start of the table wouldn't close the outer a element). The result is that the two a
                    elements are indirectly nested inside each other — non-conforming markup will often result in
                    non-conforming DOMs when parsed.

                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token. Push onto the list of active formatting elements that element.

            A start tag whose tag name is one of: "b", "big", "code", "em", "font", "i", "s", "small", "strike",
            "strong", "tt", "u"
                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token. Push onto the list of active formatting elements that element.

            A start tag whose tag name is "nobr"
                Reconstruct the active formatting elements, if any.

                If the stack of open elements has a nobr element in scope, then this is a parse error; run the adoption
                agency algorithm for the tag name "nobr", then once again reconstruct the active formatting elements, if any.

                Insert an HTML element for the token. Push onto the list of active formatting elements that element.

            An end tag whose tag name is one of: "a", "b", "big", "code", "em", "font", "i", "nobr", "s", "small",
            "strike", "strong", "tt", "u"
                Run the adoption agency algorithm for the token's tag name.

            A start tag whose tag name is one of: "applet", "marquee", "object"
                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token.

                Insert a marker at the end of the list of active formatting elements.

                Set the frameset-ok flag to "not ok".

            An end tag token whose tag name is one of: "applet", "marquee", "object"
                If the stack of open elements does not have an element in scope that is an HTML element and with the
                same tag name as that of the token, then this is a parse error; ignore the token.

                Otherwise, run these steps:
                    1. Generate implied end tags.
                    2. If the current node is not an HTML element with the same tag name as that of the token, then this
                       is a parse error.
                    3. Pop elements from the stack of open elements until an HTML element with the same tag name as the
                       token has been popped from the stack.
                    4. Clear the list of active formatting elements up to the last marker.

            A start tag whose tag name is "table"
                If the Document is not set to quirks mode, and the stack of open elements has a p element
                in button scope, then close a p element.

                Insert an HTML element for the token.

                Set the frameset-ok flag to "not ok".

                Switch the insertion mode to "in table".

            An end tag whose tag name is "br"
                Parse error. Act as described in the next entry, as if this
                was a "br" start tag token, rather than an end tag token.

            A start tag whose tag name is one of: "area", "br", "embed", "img", "keygen", "wbr"
                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                Acknowledge the token's self-closing flag, if it is set.

                Set the frameset-ok flag to "not ok".

            A start tag whose tag name is "input"
                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                Acknowledge the token's self-closing flag, if it is set.

                If the token does not have an attribute with the name "type", or if it does, but that attribute's value
                is not an ASCII case-insensitive match for the string "hidden", then: set the frameset-ok flag to "not ok".

            A start tag whose tag name is one of: "param", "source", "track"
                Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                Acknowledge the token's self-closing flag, if it is set.

            A start tag whose tag name is "hr"
                If the stack of open elements has a p element in button scope, then close a p element.

                Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                Acknowledge the token's self-closing flag, if it is set.

                Set the frameset-ok flag to "not ok".

            A start tag whose tag name is "image"
                Parse error. Change the token's tag name to "img" and reprocess it. (Don't ask.)

            A start tag whose tag name is "isindex"
                Parse error.

                If there is no template element on the stack of open elements and the form element pointer
                is not null, then ignore the token.

                Otherwise:
                    Acknowledge the token's self-closing flag, if it is set.

                    Set the frameset-ok flag to "not ok".

                    If the stack of open elements has a p element in button scope, then close a p element.

                    Insert an HTML element for a "form" start tag token with no attributes, and, if there
                    is no template element on the stack of open elements, set the form element pointer to point
                    to the element created.

                    If the token has an attribute called "action", set the action attribute on the resulting form
                    element to the value of the "action" attribute of the token.

                    Insert an HTML element for an "hr" start tag token with no attributes. Immediately pop the
                    current node off the stack of open elements.

                    Reconstruct the active formatting elements, if any.

                    Insert an HTML element for a "label" start tag token with no attributes.

                    Insert characters (see below for what they should say).

                    Insert an HTML element for an "input" start tag token with all the attributes from the "isindex"
                    token except "name", "action", and "prompt", and with an attribute named "name" with the value
                    "isindex". (This creates an input element with the name attribute set to the magic balue "isindex".)
                    Immediately pop the current node off the stack of open elements.

                    Insert more characters (see below for what they should say).

                    Pop the current node (which will be the label element created earlier) off the stack of open elements.

                    Insert an HTML element for an "hr" start tag token with no attributes. Immediately pop the current node
                    off the stack of open elements.

                    Pop the current node (which will be the form element created earlier) off the stack of open elements, and,
                    if there is no template element on the stack of open elements, set the form element pointer back to null.

                    Prompt: If the token has an attribute with the name "prompt", then the first stream of characters must be
                    the same string as given in that attribute, and the second stream of characters must be empty. Otherwise,
                    the two streams of character tokens together should, together with the input element, express the
                    equivalent of "This is a searchable index. Enter search keywords: (input field)" in the user's preferred
                    language.

            A start tag whose tag name is "textarea"
                Run these steps:
                    1. Insert an HTML element for the token.
                    2. If the next token is a "LF" (U+000A) character token, then ignore that token and move on to the
                       next one. (Newlines at the start of textarea elements are ignored as an authoring convenience.)
                    3. Switch the tokenizer to the RCDATA state.
                    4. Let the original insertion mode be the current insertion mode.
                    5. Set the frameset-ok flag to "not ok".
                    6. Switch the insertion mode to "text".

            A start tag whose tag name is "xmp"
                If the stack of open elements has a p element in button scope, then close a p element.

                Reconstruct the active formatting elements, if any.

                Set the frameset-ok flag to "not ok".

                Follow the generic raw text element parsing algorithm.

            A start tag whose tag name is "iframe"
                Set the frameset-ok flag to "not ok".

                Follow the generic raw text element parsing algorithm.

            A start tag whose tag name is "noembed"
            A start tag whose tag name is "noscript", if the scripting flag is enabled
                Follow the generic raw text element parsing algorithm.

            A start tag whose tag name is "select"
                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token.

                Set the frameset-ok flag to "not ok".

                If the insertion mode is one of "in table", "in caption", "in table body", "in row",
                or "in cell", then switch the insertion mode to "in select in table". Otherwise,
                switch the insertion mode to "in select".

            A start tag whose tag name is one of: "optgroup", "option"
                If the current node is an option element, then pop the current node off the stack of open elements.

                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token.

            A start tag whose tag name is one of: "rb", "rp", "rtc"
                If the stack of open elements has a ruby element in scope, then generate implied end tags.
                If the current node is not then a ruby element, this is a parse error.

                Insert an HTML element for the token.

            A start tag whose tag name is "rt"
                If the stack of open elements has a ruby element in scope, then generate implied end tags, except for
                rtc elements. If the current node is not then a ruby element or an rtc element, this is a parse error.

                Insert an HTML element for the token.

            A start tag whose tag name is "math"
                Reconstruct the active formatting elements, if any.

                Adjust MathML attributes for the token. (This fixes the case of MathML attributes that are not all lowercase.)

                Adjust foreign attributes for the token. (This fixes the use of namespaced attributes, in particular XLink.)

                Insert a foreign element for the token, in the MathML namespace.

                If the token has its self-closing flag set, pop the current node off the stack of open elements and acknowledge
                the token's self-closing flag.

            A start tag whose tag name is "svg"
                Reconstruct the active formatting elements, if any.

                Adjust SVG attributes for the token. (This fixes the case of SVG attributes that are not all lowercase.)

                Adjust foreign attributes for the token. (This fixes the use of namespaced attributes, in particular XLink in SVG.)

                Insert a foreign element for the token, in the SVG namespace.

                If the token has its self-closing flag set, pop the current node off the stack of open elements
                and acknowledge the token's self-closing flag.

            A start tag whose tag name is one of: "caption", "col", "colgroup", "frame", "head", "tbody", "td", "tfoot",
            "th", "thead", "tr"
                Parse error. Ignore the token.

            Any other start tag
                Reconstruct the active formatting elements, if any.

                Insert an HTML element for the token.

                NOTE: This element will be an ordinary element.

            Any other end tag
                Run these steps:
                    1. Initialize node to be the current node (the bottommost node of the stack).
                    2. Loop: If node is an HTML element with the same tag name as the token, then:
                        1. Generate implied end tags, except for HTML elements with the same tag name as the token.
                        2. If node is not the current node, then this is a parse error.
                        3. Pop all the nodes from the current node up to node, including node, then stop these steps.
                    3. Otherwise, if node is in the special category, then this is a parse error; ignore the token, and abort these steps.
                    4. Set node to the previous entry in the stack of open elements.
                    5. Return to the step labeled loop.



            When the steps above say the user agent is to close a p element, it means that the
            user agent must run the following steps:
                1. Generate implied end tags, except for p elements.
                2. If the current node is not a p element, then this is a parse error.
                3. Pop elements from the stack of open elements until a p element has been popped from the stack.

            The adoption agency algorithm, which takes as its only argument a tag name subject for which the
            algorithm is being run, consists of the following steps:
                1. If the current node is an HTML element whose tag name is subject, then run these substeps:
                    1. Let element be the current node.
                    2. Pop element off the stack of open elements.
                    3. If element is also in the list of active formatting elements, remove the element from the list.
                    4. Abort the adoption agency algorithm.
                2. Let outer loop counter be zero.
                3. Outer loop: If outer loop counter is greater than or equal to eight, then abort these steps.
                4. Increment outer loop counter by one.
                5. Let formatting element be the last element in the list of active formatting elements that:
                    * is between the end of the list and the last scope marker in the list, if any,
                      or the start of the list otherwise, and
                    * has the tag name subject.

                    If there is no such element, then abort these steps and instead act as described in the
                    "any other end tag" entry above.
                6. If formatting element is not in the stack of open elements, then this is a parse error;
                   remove the element from the list, and abort these steps.
                7. If formatting element is in the stack of open elements, but the element is not in scope,
                   then this is a parse error; abort these steps.
                8. If formatting element is not the current node, this is a parse error. (But do not abort these steps.)
                9. Let furthest block be the topmost node in the stack of open elements that is lower in the stack
                   than formatting element, and is an element in the special category. There might not be one.
                10. If there is no furthest block, then the UA must first pop all the nodes from the bottom of the
                    stack of open elements, from the current node up to and including formatting element, then remove
                    formatting element from the list of active formatting elements, and finally abort these steps.
                11. Let common ancestor be the element immediately above formatting element in the stack of open elements.
                12. Let a bookmark note the position of formatting element in the list of active formatting elements
                    relative to the elements on either side of it in the list.
                13. Let node and last node be furthest block. Follow these steps:
                    1. Let inner loop counter be zero.
                    2. Inner loop: Increment inner loop counter by one.
                    3. Let node be the element immediately above node in the stack of open elements, or if node is no
                       longer in the stack of open elements (e.g. because it got removed by this algorithm), the element
                       that was immediately above node in the stack of open elements before node was removed.
                    4. If node is formatting element, then go to the next step in the overall algorithm.
                    5. If inner loop counter is greater than three and node is in the list of active formatting elements,
                       then remove node from the list of active formatting elements.
                    6. If node is not in the list of active formatting elements, then remove node from the stack of open
                        elements and then go back to the step labeled inner loop.
                    7. Create an element for the token for which the element node was created, in the HTML namespace, with
                       common ancestor as the intended parent; replace the entry for node in the list of active formatting
                       elements with an entry for the new element, replace the entry for node in the stack of open elements
                       with an entry for the new element, and let node be the new element.
                    8. If last node is furthest block, then move the aforementioned bookmark to be immediately after the new
                       node in the list of active formatting elements.
                    9. Insert last node into node, first removing it from its previous parent node if any.
                    10. Let last node be node.
                    11. Return to the step labeled inner loop.
                14. Insert whatever last node ended up being in the previous step at the appropriate place for inserting a node,
                    but using common ancestor as the override target.
                15. Create an element for the token for which formatting element was created, in the HTML namespace,
                    with furthest block as the intended parent.
                16. Take all of the child nodes of furthest block and append them to the element created in the last step.
                17. Append that new element to furthest block.
                18. Remove formatting element from the list of active formatting elements, and insert the new element into
                    the list of active formatting elements at the position of the aforementioned bookmark.
                19. Remove formatting element from the stack of open elements, and insert the new element into the stack of
                    open elements immediately below the position of furthest block in that stack.
                20. Jump back to the step labeled outer loop.

                NOTE: This algorithm's name, the "adoption agency algorithm", comes from the way it causes elements to change
                parents, and is in contrast with other possible algorithms for dealing with misnested content, which included
                the "incest algorithm", the "secret affair algorithm", and the "Heisenberg algorithm".
                */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInTextMode()
        {
            /*
            8.2.5.4.8 The "text" insertion mode

            When the user agent is to apply the rules for the "text" insertion mode,
            the user agent must handle the token as follows:

            A character token
                Insert the token's character.

                NOTE: This can never be a U+0000 NULL character; the tokenizer converts those
                to U+FFFD REPLACEMENT CHARACTER characters.

            An end-of-file token
                Parse error.

                If the current node is a script element, mark the script element as "already started".

                Pop the current node off the stack of open elements.

                Switch the insertion mode to the original insertion mode and reprocess the token.

            An end tag whose tag name is "script"
                Perform a microtask checkpoint.

                Provide a stable state.

                Let script be the current node (which will be a script element).

                Pop the current node off the stack of open elements.

                Switch the insertion mode to the original insertion mode.

                Let the old insertion point have the same value as the current insertion point. Let the insertion
                point be just before the next input character.

                Increment the parser's script nesting level by one.

                Prepare the script. This might cause some script to execute, which might cause new characters to be
                inserted into the tokenizer, and might cause the tokenizer to output more tokens, resulting in a
                reentrant invocation of the parser.

                Decrement the parser's script nesting level by one. If the parser's script nesting level is zero,
                then set the parser pause flag to false.

                Let the insertion point have the value of the old insertion point. (In other words, restore the insertion
                point to its previous value. This value might be the "undefined" value.)

                At this stage, if there is a pending parsing-blocking script, then:
                    - If the script nesting level is not zero:
                        Set the parser pause flag to true, and abort the processing of any nested invocations
                        of the tokenizer, yielding control back to the caller. (Tokenization will resume when
                        the caller returns to the "outer" tree construction stage.)

                        NOTE: The tree construction stage of this particular parser is being called reentrantly,
                        say from a call to document.write().

                    - Otherwise:
                        Run these steps:
                            1. Let the script be the pending parsing-blocking script. There is no
                               longer a pending parsing-blocking script.
                            2. Block the tokenizer for this instance of the HTML parser, such that the event
                               loop will not run tasks that invoke the tokenizer.
                            3. If the parser's Document has a style sheet that is blocking scripts or the script's
                               "ready to be parser-executed" flag is not set: spin the event loop until the parser's
                                Document has no style sheet that is blocking scripts and the script's "ready to be
                                parser-executed" flag is set.
                            4. If this parser has been aborted in the meantime, abort these steps.
                                NOTE: This could happen if, e.g., while the spin the event loop algorithm is running,
                                the browsing context gets closed, or the document.open() method gets invoked on the Document.
                            5. Unblock the tokenizer for this instance of the HTML parser, such that tasks that invoke the
                               tokenizer can again be run.
                            6. Let the insertion point be just before the next input character.
                            7. Increment the parser's script nesting level by one (it should be zero before this step,
                               so this sets it to one).
                            8. Execute the script.
                            9. Decrement the parser's script nesting level by one. If the parser's script nesting level is
                               zero (which it always should be at this point), then set the parser pause flag to false.
                            10. Let the insertion point be undefined again.
                            11. If there is once again a pending parsing-blocking script, then repeat these steps from step 1.

            Any other end tag
                Pop the current node off the stack of open elements.

                Switch the insertion mode to the original insertion mode.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTableMode()
        {
            /*
            8.2.5.4.9 The "in table" insertion mode

            When the user agent is to apply the rules for the "in table" insertion mode,
            the user agent must handle the token as follows:

            A character token, if the current node is table, tbody, tfoot, thead, or tr element
                Let the pending table character tokens be an empty list of tokens.

                Let the original insertion mode be the current insertion mode.

                Switch the insertion mode to "in table text" and reprocess the token.

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "caption"
                Clear the stack back to a table context. (See below.)

                Insert a marker at the end of the list of active formatting elements.

                Insert an HTML element for the token, then switch the insertion mode to "in caption".

            A start tag whose tag name is "colgroup"
                Clear the stack back to a table context. (See below.)

                Insert an HTML element for the token, then switch the insertion mode to "in column group".

            A start tag whose tag name is "col"
                Clear the stack back to a table context. (See below.)

                Insert an HTML element for a "colgroup" start tag token with no attributes, then switch the
                insertion mode to "in column group".

                Reprocess the current token.

            A start tag whose tag name is one of: "tbody", "tfoot", "thead"
                Clear the stack back to a table context. (See below.)

                Insert an HTML element for the token, then switch the insertion mode to "in table body".

            A start tag whose tag name is one of: "td", "th", "tr"
                Clear the stack back to a table context. (See below.)

                Insert an HTML element for a "tbody" start tag token with no attributes,
                then switch the insertion mode to "in table body".

                Reprocess the current token.

            A start tag whose tag name is "table"
                Parse error.

                If the stack of open elements does not have a table element in table scope, ignore the token.

                Otherwise:
                    Pop elements from this stack until a table element has been popped from the stack.

                    Reset the insertion mode appropriately.

                    Reprocess the token.

            An end tag whose tag name is "table"
                If the stack of open elements does not have a table element in table scope, this is a parse error;
                ignore the token.

                Otherwise:
                    Pop elements from this stack until a table element has been popped from the stack.

                    Reset the insertion mode appropriately.

            An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "tbody", "td",
            "tfoot", "th", "thead", "tr"
                Parse error. Ignore the token.

            A start tag whose tag name is one of: "style", "script", "template"
            An end tag whose tag name is "template"
                Process the token using the rules for the "in head" insertion mode.

            A start tag whose tag name is "input"
                If the token does not have an attribute with the name "type", or if it does, but that attribute's
                value is not an ASCII case-insensitive match for the string "hidden", then: act as described in the
                "anything else" entry below.

                Otherwise:
                    Parse error.

                    Insert an HTML element for the token.

                    Pop that input element off the stack of open elements.

                    Acknowledge the token's self-closing flag, if it is set.

            A start tag whose tag name is "form"
                Parse error.

                If there is a template element on the stack of open elements, or if the form element pointer
                is not null, ignore the token.

                Otherwise:
                    Insert an HTML element for the token, and set the form element pointer to point to the element created.

                    Pop that form element off the stack of open elements.

            An end-of-file token
                Process the token using the rules for the "in body" insertion mode.

            Anything else
                Parse error. Enable foster parenting, process the token using the rules for the "in body"
                insertion mode, and then disable foster parenting.


            When the steps above require the UA to clear the stack back to a table context, it means that the
            UA must, while the current node is not a table, template, or html element, pop elements from the
            stack of open elements.

            NOTE: The current node being an html element after this process is a fragment case.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTableTextMode()
        {
            /*
            8.2.5.4.10 The "in table text" insertion mode

            When the user agent is to apply the rules for the "in table text" insertion mode,
            the user agent must handle the token as follows:

            A character token that is U+0000 NULL
                Parse error. Ignore the token.

            Any other character token
                Append the character token to the pending table character tokens list.

            Anything else
                If any of the tokens in the pending table character tokens list are character
                tokens that are not space characters, then reprocess the character tokens in the
                pending table character tokens list using the rules given in the "anything else"
                entry in the "in table" insertion mode.

                Otherwise, insert the characters given by the pending table character tokens list.

                Switch the insertion mode to the original insertion mode and reprocess the token.
             */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInCaptionMode()
        {
            /*
            8.2.5.4.11 The "in caption" insertion mode

            When the user agent is to apply the rules for the "in caption" insertion mode,
            the user agent must handle the token as follows:

            An end tag whose tag name is "caption"
                If the stack of open elements does not have a caption element in table scope,
                this is a parse error; ignore the token. (fragment case)

                Otherwise:
                    Generate implied end tags.

                    Now, if the current node is not a caption element, then this is a parse error.

                    Pop elements from this stack until a caption element has been popped from the stack.

                    Clear the list of active formatting elements up to the last marker.

                    Switch the insertion mode to "in table".

            A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"
            An end tag whose tag name is "table"
                Parse error.

                If the stack of open elements does not have a caption element in table scope, ignore the token. (fragment case)

                Otherwise:
                    Pop elements from this stack until a caption element has been popped from the stack.

                    Clear the list of active formatting elements up to the last marker.

                    Switch the insertion mode to "in table".

                    Reprocess the token.

            An end tag whose tag name is one of: "body", "col", "colgroup", "html", "tbody", "td", "tfoot", "th", "thead", "tr"
                Parse error. Ignore the token.

            Anything else
                Process the token using the rules for the "in body" insertion mode.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInColumnGroupMode()
        {
            /*
            8.2.5.4.12 The "in column group" insertion mode

            When the user agent is to apply the rules for the "in column group" insertion mode,
            the user agent must handle the token as follows:

            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
                Insert the character.

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            A start tag whose tag name is "col"
                Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                Acknowledge the token's self-closing flag, if it is set.

            An end tag whose tag name is "colgroup"
                If the current node is not a colgroup element, then this is a parse error; ignore the token.

                Otherwise, pop the current node from the stack of open elements. Switch the insertion mode to "in table".

            An end tag whose tag name is "col"
                Parse error. Ignore the token.

            A start tag whose tag name is "template"
            An end tag whose tag name is "template"
                Process the token using the rules for the "in head" insertion mode.

            An end-of-file token
                Process the token using the rules for the "in body" insertion mode.

            Anything else
                If the current node is not a colgroup element, then this is a parse error; ignore the token.

                Otherwise, pop the current node from the stack of open elements.

                Switch the insertion mode to "in table".

                Reprocess the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTableBodyMode()
        {
            /*
            8.2.5.4.13 The "in table body" insertion mode

            When the user agent is to apply the rules for the "in table body" insertion mode,
            the user agent must handle the token as follows:

            A start tag whose tag name is "tr"
                Clear the stack back to a table body context. (See below.)

                Insert an HTML element for the token, then switch the insertion mode to "in row".

            A start tag whose tag name is one of: "th", "td"
                Parse error.

                Clear the stack back to a table body context. (See below.)

                Insert an HTML element for a "tr" start tag token with no attributes, then switch the insertion mode to "in row".

                Reprocess the current token.

            An end tag whose tag name is one of: "tbody", "tfoot", "thead"
                If the stack of open elements does not have an element in table scope that is an HTML element
                and with the same tag name as the token, this is a parse error; ignore the token.

                Otherwise:
                    Clear the stack back to a table body context. (See below.)

                    Pop the current node from the stack of open elements. Switch the insertion mode to "in table".

            A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "tfoot", "thead"
            An end tag whose tag name is "table"
                If the stack of open elements does not have a tbody, thead, or tfoot element in table scope,
                this is a parse error; ignore the token.

                Otherwise:
                    Clear the stack back to a table body context. (See below.)

                    Pop the current node from the stack of open elements. Switch the insertion mode to "in table".

                    Reprocess the token.

            An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "td", "th", "tr"
                Parse error. Ignore the token.

            Anything else
                Process the token using the rules for the "in table" insertion mode.

            When the steps above require the UA to clear the stack back to a table body context,
            it means that the UA must, while the current node is not a tbody, tfoot, thead, template,
            or html element, pop elements from the stack of open elements.

            NOTE: The current node being an html element after this process is a fragment case.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInRowMode()
        {
            /*
            8.2.5.4.14 The "in row" insertion mode

            When the user agent is to apply the rules for the "in row" insertion mode,
            the user agent must handle the token as follows:

            A start tag whose tag name is one of: "th", "td"
                Clear the stack back to a table row context. (See below.)

                Insert an HTML element for the token, then switch the insertion mode to "in cell".

                Insert a marker at the end of the list of active formatting elements.

            An end tag whose tag name is "tr"
                If the stack of open elements does not have a tr element in table scope, this is a parse error; ignore the token.

                Otherwise:
                    Clear the stack back to a table row context. (See below.)

                    Pop the current node (which will be a tr element) from the stack of open elements. Switch the insertion mode to "in table body".

            A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "tfoot", "thead", "tr"
            An end tag whose tag name is "table"
                If the stack of open elements does not have a tr element in table scope, this is a parse error; ignore the token.

                Otherwise:
                    Clear the stack back to a table row context. (See below.)

                    Pop the current node (which will be a tr element) from the stack of open elements. Switch the insertion mode to "in table body".

                    Reprocess the token.

            An end tag whose tag name is one of: "tbody", "tfoot", "thead"
                If the stack of open elements does not have an element in table scope that is an HTML element
                and with the same tag name as the token, this is a parse error; ignore the token.

                If the stack of open elements does not have a tr element in table scope, ignore the token.

                Otherwise:
                    Clear the stack back to a table row context. (See below.)

                    Pop the current node (which will be a tr element) from the stack of open elements. Switch the insertion mode to "in table body".

                    Reprocess the token.

            An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "td", "th"
                Parse error. Ignore the token.

            Anything else
                Process the token using the rules for the "in table" insertion mode.

            When the steps above require the UA to clear the stack back to a table row context, it means that the UA must,
            while the current node is not a tr, template, or html element, pop elements from the stack of open elements.

            NOTE: The current node being an html element after this process is a fragment case.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInCellMode()
        {
            /*
            8.2.5.4.15 The "in cell" insertion mode

            When the user agent is to apply the rules for the "in cell" insertion mode,
            the user agent must handle the token as follows:

            An end tag whose tag name is one of: "td", "th"
                If the stack of open elements does not have an element in table scope that is an HTML element and
                with the same tag name as that of the token, then this is a parse error; ignore the token.

                Otherwise:
                    Generate implied end tags.

                    Now, if the current node is not an HTML element with the same tag name as the token,
                    then this is a parse error.

                    Pop elements from the stack of open elements stack until an HTML element with the same
                    tag name as the token has been popped from the stack.

                    Clear the list of active formatting elements up to the last marker.

                    Switch the insertion mode to "in row".

            A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"
                If the stack of open elements does not have a td or th element in table scope, then this is a parse error;
                ignore the token. (fragment case)

                Otherwise, close the cell (see below) and reprocess the token.

            An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html"
                Parse error. Ignore the token.

            An end tag whose tag name is one of: "table", "tbody", "tfoot", "thead", "tr"
                If the stack of open elements does not have an element in table scope that is an HTML element
                and with the same tag name as that of the token, then this is a parse error; ignore the token.

                Otherwise, close the cell (see below) and reprocess the token.

            Anything else
                Process the token using the rules for the "in body" insertion mode.


            Where the steps above say to close the cell, they mean to run the following algorithm:
            1. Generate implied end tags.
            2. If the current node is not now a td element or a th element, then this is a parse error.
            3. Pop elements from the stack of open elements stack until a td element or a th element has been popped from the stack.
            4. Clear the list of active formatting elements up to the last marker.
            5. Switch the insertion mode to "in row".

            NOTE: The stack of open elements cannot have both a td and a th element in table scope at the same time,
            nor can it have neither when the close the cell algorithm is invoked.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInSelectMode()
        {
            /*
            8.2.5.4.16 The "in select" insertion mode

            When the user agent is to apply the rules for the "in select" insertion mode,
            the user agent must handle the token as follows:

            A character token that is U+0000 NULL
                Parse error. Ignore the token.

            Any other character token
                Insert the token's character.

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            A start tag whose tag name is "option"
                If the current node is an option element, pop that node from the stack of open elements.

                Insert an HTML element for the token.

            A start tag whose tag name is "optgroup"
                If the current node is an option element, pop that node from the stack of open elements.

                If the current node is an optgroup element, pop that node from the stack of open elements.

                Insert an HTML element for the token.

            An end tag whose tag name is "optgroup"
                First, if the current node is an option element, and the node immediately before it in the stack
                of open elements is an optgroup element, then pop the current node from the stack of open elements.

                If the current node is an optgroup element, then pop that node from the stack of open elements.
                Otherwise, this is a parse error; ignore the token.

            An end tag whose tag name is "option"
                If the current node is an option element, then pop that node from the stack of open elements.
                Otherwise, this is a parse error; ignore the token.

            An end tag whose tag name is "select"
                If the stack of open elements does not have a select element in select scope, this is a parse error;
                ignore the token. (fragment case)

                Otherwise:
                    Pop elements from the stack of open elements until a select element has been popped from the stack.

                    Reset the insertion mode appropriately.

            A start tag whose tag name is "select"
                Parse error.

                Pop elements from the stack of open elements until a select element has been popped from the stack.

                Reset the insertion mode appropriately.

                It just gets treated like an end tag.

            A start tag whose tag name is one of: "input", "keygen", "textarea"
                Parse error.

                If the stack of open elements does not have a select element in select scope, ignore the token. (fragment case)

                Pop elements from the stack of open elements until a select element has been popped from the stack.

                Reset the insertion mode appropriately.

                Reprocess the token.

            A start tag whose tag name is one of: "script", "template"
            An end tag whose tag name is "template"
                Process the token using the rules for the "in head" insertion mode.

            An end-of-file token
                Process the token using the rules for the "in body" insertion mode.

            Anything else
                Parse error. Ignore the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInSelectInTableMode()
        {
            /*
            8.2.5.4.17 The "in select in table" insertion mode

            When the user agent is to apply the rules for the "in select in table" insertion mode,
            the user agent must handle the token as follows:

            A start tag whose tag name is one of: "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"
                Parse error.

                Pop elements from the stack of open elements until a select element has been popped from the stack.

                Reset the insertion mode appropriately.

                Reprocess the token.

            An end tag whose tag name is one of: "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"
                Parse error.

                If the stack of open elements does not have an element in table scope that is an HTML element
                and with the same tag name as that of the token, then ignore the token.

                Otherwise:
                    Pop elements from the stack of open elements until a select element has been popped from the stack.

                    Reset the insertion mode appropriately.

                    Reprocess the token.

            Anything else
                Process the token using the rules for the "in select" insertion mode.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTemplateMode()
        {
            /*
            8.2.5.4.18 The "in template" insertion mode

            When the user agent is to apply the rules for the "in template" insertion mode,
            the user agent must handle the token as follows:

            A character token
            A comment token
            A DOCTYPE token
                Process the token using the rules for the "in body" insertion mode.

            A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta", "noframes", "script",
            "style", "template", "title"
            An end tag whose tag name is "template"
                Process the token using the rules for the "in head" insertion mode.

            A start tag whose tag name is one of: "caption", "colgroup", "tbody", "tfoot", "thead"
                Pop the current template insertion mode off the stack of template insertion modes.

                Push "in table" onto the stack of template insertion modes so that it is the new current template insertion mode.

                Switch the insertion mode to "in table", and reprocess the token.

            A start tag whose tag name is "col"
                Pop the current template insertion mode off the stack of template insertion modes.

                Push "in column group" onto the stack of template insertion modes so that it is the new
                current template insertion mode.

                Switch the insertion mode to "in column group", and reprocess the token.

            A start tag whose tag name is "tr"
                Pop the current template insertion mode off the stack of template insertion modes.

                Push "in table body" onto the stack of template insertion modes so that it is the new
                current template insertion mode.

                Switch the insertion mode to "in table body", and reprocess the token.

            A start tag whose tag name is one of: "td", "th"
                Pop the current template insertion mode off the stack of template insertion modes.

                Push "in row" onto the stack of template insertion modes so that it is the new current template insertion mode.

                Switch the insertion mode to "in row", and reprocess the token.

            Any other start tag
                Pop the current template insertion mode off the stack of template insertion modes.

                Push "in body" onto the stack of template insertion modes so that it is the new current template insertion mode.

                Switch the insertion mode to "in body", and reprocess the token.

            Any other end tag
                Parse error. Ignore the token.

            An end-of-file token
                If there is no template element on the stack of open elements, then stop parsing. (fragment case)

                Otherwise, this is a parse error.

                Pop elements from the stack of open elements until a template element has been popped from the stack.

                Clear the list of active formatting elements up to the last marker.

                Pop the current template insertion mode off the stack of template insertion modes.

                Reset the insertion mode appropriately.

                Reprocess the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterBodyMode()
        {
            /*
            8.2.5.4.19 The "after body" insertion mode

            When the user agent is to apply the rules for the "after body" insertion mode,
            the user agent must handle the token as follows:

            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
                Process the token using the rules for the "in body" insertion mode.

            A comment token
                Insert a comment as the last child of the first element in the stack of open elements (the html element).

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            An end tag whose tag name is "html"
                If the parser was originally created as part of the HTML fragment parsing algorithm,
                this is a parse error; ignore the token. (fragment case)

                Otherwise, switch the insertion mode to "after after body".

            An end-of-file token
                Stop parsing.

            Anything else
                Parse error. Switch the insertion mode to "in body" and reprocess the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInFramesetMode()
        {
            /*
            8.2.5.4.20 The "in frameset" insertion mode

            When the user agent is to apply the rules for the "in frameset" insertion mode,
            the user agent must handle the token as follows:

            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
                Insert the character.

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            A start tag whose tag name is "frameset"
                Insert an HTML element for the token.

            An end tag whose tag name is "frameset"
                If the current node is the root html element, then this is a parse error; ignore the token. (fragment case)

                Otherwise, pop the current node from the stack of open elements.

                If the parser was not originally created as part of the HTML fragment parsing algorithm (fragment case),
                and the current node is no longer a frameset element, then switch the insertion mode to "after frameset".

            A start tag whose tag name is "frame"
                Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                Acknowledge the token's self-closing flag, if it is set.

            A start tag whose tag name is "noframes"
                Process the token using the rules for the "in head" insertion mode.

            An end-of-file token
                If the current node is not the root html element, then this is a parse error.

                The current node can only be the root html element in the fragment case.

                Stop parsing.

            Anything else
                Parse error. Ignore the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterFramesetMode()
        {
            /*
            8.2.5.4.21 The "after frameset" insertion mode

            When the user agent is to apply the rules for the "after frameset" insertion mode,
            the user agent must handle the token as follows:

            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
                Insert the character.

            A comment token
                Insert a comment.

            A DOCTYPE token
                Parse error. Ignore the token.

            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            An end tag whose tag name is "html"
                Switch the insertion mode to "after after frameset".

            A start tag whose tag name is "noframes"
                Process the token using the rules for the "in head" insertion mode.

            An end-of-file token
                Stop parsing.

            Anything else
                Parse error. Ignore the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterAfterBodyMode()
        {
            /*
            8.2.5.4.22 The "after after body" insertion mode

            When the user agent is to apply the rules for the "after after body" insertion mode,
            the user agent must handle the token as follows:

            A comment token
                Insert a comment as the last child of the Document object.

            A DOCTYPE token
            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            An end-of-file token
                Stop parsing.

            Anything else
                Parse error. Switch the insertion mode to "in body" and reprocess the token.
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterAfterFramesetMode()
        {
            /*
            8.2.5.4.23 The "after after frameset" insertion mode

            When the user agent is to apply the rules for the "after after frameset" insertion mode,
            the user agent must handle the token as follows:

            A comment token
                Insert a comment as the last child of the Document object.

            A DOCTYPE token
            A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            A start tag whose tag name is "html"
                Process the token using the rules for the "in body" insertion mode.

            An end-of-file token
                Stop parsing.

            A start tag whose tag name is "noframes"
                Process the token using the rules for the "in head" insertion mode.

            Anything else
                Parse error. Ignore the token.
            */
        }

        #endregion
    }
}

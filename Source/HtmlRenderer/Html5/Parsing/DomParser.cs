using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    internal class DomParser
    {
        public readonly ParsingContext ParsingContext;

        public readonly DomFactory DomFactory;

        public readonly ParsingState ParsingState;

        private readonly Tokenizer Tokenizer;

        public DomParser(ParsingContext parsingContext, HtmlStream html)
        {
            Contract.RequiresNotNull(parsingContext, nameof(parsingContext));

            this.ParsingContext = parsingContext;
            this.DomFactory = parsingContext.CreateDomFactory(this);
            this.ParsingState = new ParsingState();
            this.Tokenizer = new Tokenizer(html);
            this.Tokenizer.ParseError += (s, e) => this.ParseError(e.ParseError);
        }

        #region Tokanization

        /// <summary>
        /// Contains information about the last token received from the <see cref="Tokenizer"/>.
        /// </summary>
        private Token Token;

        public void Parse()
        {
            do
            {
                this.Token = this.Tokenizer.GetNextToken();
                this.ProcessToken();
            }
            while (this.Token.Type != TokenType.EndOfFile);
        }

        #endregion

        #region 8.2.3 Parse State

        #region 8.2.3.1. The insertion mode. See: http://www.w3.org/TR/html5/syntax.html#the-insertion-mode

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

        private InsertionModeEnum InsertionMode = InsertionModeEnum.Initial;

        private InsertionModeEnum OriginalInsertionMode = InsertionModeEnum.Initial;

        private InsertionModeEnum ActiveInsertionMode = InsertionModeEnum.Initial;

        /// <summary>
        /// To parse nested template elements, a stack of template insertion modes is used. It is initially empty
        /// </summary>
        /// <remarks>
        /// The current template insertion mode is the insertion mode that was most recently added to the stack of template insertion modes.
        /// </remarks>
        private readonly Stack<InsertionModeEnum> TemplateInsertionMode = new Stack<InsertionModeEnum>();

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
                // Restore the active insertion mode. This also takes care if the mode was switched.
                this.ActiveInsertionMode = this.InsertionMode;
            }
        }

        #endregion

        #region 8.2.3.2. The stack of open elements. See: http://www.w3.org/TR/html5/syntax.html#stack-of-open-elements

        /// <summary>
        /// 8.2.3.2. The stack of open elements. See: http://www.w3.org/TR/html5/syntax.html#stack-of-open-elements
        /// </summary>
        /// <remarks>
        /// Initially, the stack of open elements is empty. The stack grows downwards;
        /// the topmost node on the stack is the first one added to the stack,
        /// and the bottommost node of the stack is the most recently added node in the stack
        /// (notwithstanding when the stack is manipulated in a random access fashion as part
        /// of the handling for misnested tags).
        /// <para/>
        /// IMPORTANT: Most other stacks, including .Net uses different terminology, where the
        /// stack grows upwords. This means, the latest added element in most stacks is on the
        /// top of the stack while in the HTML specification, it is at the BOTTOM!
        /// </remarks>
        private OpenElementsStack OpenElements = new OpenElementsStack();

        private class OpenElementsStack
        {
            private readonly List<Element> Elements = new List<Element>();

            public Element Pop()
            {
                if (this.Elements.Count == 0)
                    throw new InvalidOperationException("Stack is empty");

                Element element = this.Elements[this.Elements.Count - 1];
                this.Elements.RemoveAt(this.Elements.Count - 1);
                return element;
            }

            public void Push(Element element)
            {
                Contract.RequiresNotNull(element, nameof(element));

                this.Elements.Add(element);
            }

            public void Remove(Element element)
            {
                Contract.RequiresNotNull(element, nameof(element));

                this.Elements.Remove(element);
            }

            public bool Any(Func<Element, bool> predicate)
            {
                return this.Elements.Any(predicate);
            }

            public bool Contains(string tagName)
            {
                return this.Elements.Any(elem => elem.TagName == tagName);
            }

            public bool Contains(params string[] tagNames)
            {
                if (tagNames == null)
                    return false;
                return this.Elements.Any(elem => tagNames.Contains(elem.TagName));
            }

            public Element Top
            {
                get
                {
                    if (this.Elements.Count == 0)
                        return null;

                    return this.Elements[0];
                }
            }

            public Element Bottom
            {
                get
                {
                    if (this.Elements.Count == 0)
                        return null;

                    return this.Elements[this.Elements.Count - 1];
                }
            }

            public int Count
            {
                get
                {
                    return this.Elements.Count;
                }
            }

            public Element this[int index]
            {
                get
                {
                    return this.Elements[index];
                }
            }

            public bool HasElementInSpecificScope(string targetNode, params string[] list)
            {
                if (list == null)
                    list = Array.Empty<string>();

                // The stack of open elements is said to have an element target node in a specific scope consisting 
                // of a list of element types list when the following algorithm terminates in a match state:

                // 1. Initialize node to be the current node(the bottommost node of the stack).
                int index = this.Count - 1;
                while (index >= 0)
                {
                    Element node = this.Elements[index];

                    // 2. If node is the target node, terminate in a match state.
                    if (node.TagName == targetNode)
                        return true;

                    // 3. Otherwise, if node is one of the element types in list, terminate in a failure state.
                    if (list.Contains(node.TagName))
                        return false;

                    // 4. Otherwise, set node to the previous entry in the stack of open elements and return to step 2.
                    // (This will never fail, since the loop will always terminate in the previous step if the top
                    // of the stack — an html element — is reached.)
                    index--;
                }

                return false;
            }

            /// <summary>
            /// IMPORTANT: The name the HTML5 specification uses is "Scope". This should be distinguished
            /// from other scopes, such as "Item Scope", "Button Scope" etc. In other words, this should
            /// be read somethink like "Default Scope".
            /// </para>
            /// ALSO: Do not confuse the above with "Specific Scope"; This means that we need to use one
            /// of the scopes above, incl. the scope named "Scope". There is no scope named "Specific Scope".
            /// </summary>
            internal static readonly string[] ScopeTags = new string[]
            {
                Tags.Applet, Tags.Caption, Tags.Html, Tags.Table, Tags.Td, Tags.Th, Tags.Marquee, Tags.Object, Tags.Template

                // MathML_TODO ... MathML Tags
                // SVG_TODO ... SVG Tags
            };

            internal static readonly string[] ItemScopeTags = ScopeTags.FailIfNull().With(Tags.Ol, Tags.Ul);

            internal static readonly string[] ButtonScopeTags = ScopeTags.FailIfNull().With(Tags.Button);

            internal static readonly string[] TableScopeTags = new string[]
            {
                Tags.Html, Tags.Table, Tags.Template
            };

            internal static readonly string[] SelectScopeTags = new string[]
            {
                Tags.OptGroup, Tags.Option
            };
        }

        /// <summary>
        /// The current node is the bottommost node in this stack of open elements.
        /// </summary>
        private Element CurrentNode
        {
            get
            {
                return this.OpenElements.Bottom;
            }
        }

        public bool IsCurrentNode(string tagName)
        {
            string currentNodeName = this.CurrentNode.TagName;
            return currentNodeName == tagName;
        }

        public bool IsCurrentNode(string tagName1, string tagName2)
        {
            string currentNodeName = this.CurrentNode.TagName;
            return (currentNodeName == tagName1) || (currentNodeName == tagName2);
        }

        public bool IsCurrentNode(params string[] tagNames)
        {
            if ((tagNames == null) || (tagNames.Length == 0))
                return false;
            string currentNodeName = this.CurrentNode.TagName;
            for (int i = 0; i < tagNames.Length; i++)
            {
                if (currentNodeName == tagNames[i])
                    return true;
            }

            return false;
        }

        #endregion

        #region 8.2.3.3. The list of active formatting elements. See http://www.w3.org/TR/html5/syntax.html#the-list-of-active-formatting-elements

        /// <summary>
        /// Push onto the list of active formatting elements.
        /// </summary>
        private void PushFormattingElement(Element element)
        {
            Contract.RequiresNotNull(element, nameof(element));

            // When the steps below require the user agent to *push onto the list of active formatting elements* an element
            // element, the user agent must perform the following steps:

            // 1. If there are already three elements in the list of active formatting elements after the last marker,
            // if any, or anywhere in the list if there are no markers, that have the same tag name, namespace,
            // and attributes as element, then remove the earliest such element from the list of active formatting elements.
            // For these purposes, the attributes must be compared as they were when the elements were created by the parser;
            // two elements have the same attributes if all their parsed attributes can be paired such that the two attributes
            // in each pair have identical names, namespaces, and values(the order of the attributes does not matter).

            // NOTE: This is the Noah’s Ark clause.But with three per family instead of two.

            // 2. Add element to the list of active formatting elements.
        }

        /// <summary>
        /// Reconstruct the active formatting elements.
        /// </summary>
        private void ReconstructFormattingElements()
        {
            // When the steps below require the user agent to *reconstruct the active formatting elements*, the user agent must
            // perform the following steps:
            // 1. If there are no entries in the list of active formatting elements, then there is nothing to reconstruct;
            // stop this algorithm.

            // 2. If the last (most recently added) entry in the list of active formatting elements is a marker, or if it is
            // an element that is in the stack of open elements, then there is nothing to reconstruct; stop this algorithm.

            // 3. Let entry be the last (most recently added) element in the list of active formatting elements.

            // 4. Rewind: If there are no entries before entry in the list of active formatting elements, then jump to 
            // the step labeled create.

            // 5. Let entry be the entry one earlier than entry in the list of active formatting elements.

            // 6. If entry is neither a marker nor an element that is also in the stack of open elements,
            // go to the step labeled rewind.
            // 7. Advance: Let entry be the element one later than entry in the list of active formatting elements.

            // 8. Create: Insert an HTML element for the token for which the element entry was created,
            // to obtain new element.

            // 9. Replace the entry for entry in the list with an entry for new element.

            // 10. If the entry for new element in the list of active formatting elements is not the last entry in the list,
            // return to the step labeled advance.

            // This has the effect of reopening all the formatting elements that were opened in the current body, cell,
            // or caption(whichever is youngest) that haven’t been explicitly closed.

            // NOTE: The way this specification is written, the list of active formatting elements always consists of elements
            // in chronological order with the least recently added element first and the most recently added element last
            // (except for while steps 7 to 10 of the above algorithm are being executed, of course).
        }

        /// <summary>
        /// Clear the list of active formatting elements up to the last marker.
        /// </summary>
        private void ClearFormattingElementsUpToMarker()
        {
            // When the steps below require the user agent to clear the list of active formatting elements
            // up to the last marker, the user agent must perform the following steps:

            // 1. Let entry be the last(most recently added) entry in the list of active formatting elements.

            // 2. Remove entry from the list of active formatting elements.

            // 3. If entry was a marker, then stop the algorithm at this point.
            // The list has been cleared up to the last marker.

            // 4. Go to step 1.
        }

        #endregion

        #endregion

        #region Tree Construction Helpers

        private void ParseError(ParseError error)
            {
                throw new NotImplementedException();
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

        #region 8.2.5.3. Closing elements that have implied end tags

        private void GenerateImpliedEndTag(string excludedTag = null)
        {
            while (true)
            {
                // When the steps below require the user agent to generate implied end tags, then, while the current node is a dd
                // element, a dt element, an li element, an option element, an optgroup element, a p element, an rb element, an rp
                // element, an rt element, or an rtc element, the user agent must pop the current node off the stack of open elements.
                string tag = this.CurrentNode.TagName;

                // If a step requires the user agent to generate implied end tags but lists an element to exclude from the process,
                // then the user agent must perform the above steps as if that element was not in the above list.
                if (tag == excludedTag)
                    break; // Done!

                if ((tag == Tags.Dd) || (tag == Tags.Dt) || (tag == Tags.Li) || (tag == Tags.Option) || (tag == Tags.OptGroup)
                    || (tag == Tags.P) || (tag == Tags.Rb) || (tag == Tags.Rp) || (tag == Tags.Rt) || (tag == Tags.Rtc))
                {
                    this.OpenElements.Pop();
                }
                else
                {
                    break; // Done!
                }
            }
        }

        private void GenerateAllImpliedEndTagThoroughly()
        {
            while (true)
            {
                // When the steps below require the user agent to generate all implied end tags thoroughly, then,
                // while the current node is a caption element, a colgroup element, a dd element, a dt element,
                // an li element, an optgroup element, an option element, a p element, an rb element, an rp element,
                // an rt element, an rtc element, a tbody element, a td element, a tfoot element, a th element, a thead
                // element, or a tr element, the user agent must pop the current node off the stack of open elements.
                string tag = this.CurrentNode.TagName;

                if ((tag == Tags.Caption) || (tag == Tags.ColGroup) || (tag == Tags.Dd) || (tag == Tags.Dt)
                    || (tag == Tags.Li) || (tag == Tags.OptGroup) || (tag == Tags.Option) || (tag == Tags.P)
                    || (tag == Tags.Rb) || (tag == Tags.Rp) || (tag == Tags.Rt) || (tag == Tags.Rtc)
                    || (tag == Tags.TBody) || (tag == Tags.Td) || (tag == Tags.TFoot) || (tag == Tags.Th)
                    || (tag == Tags.THead) || (tag == Tags.Tr))
                {
                    this.OpenElements.Pop();
                }
                else
                {
                    break; // Done!
                }
            }
        }

        #endregion

        #region 8.2.5.4. The rules for parsing tokens in HTML content

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInitialMode()
        {
            /*
            8.2.5.4.1 The "initial" insertion mode

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
                throw new NotImplementedException();
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // If the DOCTYPE token's name is not a case-sensitive match for the string "html",
                // or the token's public identifier is not missing, or the token's system identifier
                // is neither missing nor a case-sensitive match for the string "about:legacy-compat",
                // and none of the sets of conditions in the following list are matched, then there is a parse error.
                //    * The DOCTYPE token's name is a case-sensitive match for the string "html",
                //      the token's public identifier is the case-sensitive string "-//W3C//DTD HTML 4.0//EN",
                //      and the token's system identifier is either missing or the case-sensitive string "http://www.w3.org/TR/REC-html40/strict.dtd".
                //    * The DOCTYPE token's name is a case-sensitive match for the string "html",
                //      the token's public identifier is the case-sensitive string "-//W3C//DTD HTML 4.01//EN",
                //      and the token's system identifier is either missing or the case-sensitive string "http://www.w3.org/TR/html4/strict.dtd".
                //    * The DOCTYPE token's name is a case-sensitive match for the string "html",
                //      the token's public identifier is the case-sensitive string "-//W3C//DTD XHTML 1.0 Strict//EN",
                //      and the token's system identifier is the case-sensitive string "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd".
                //    * The DOCTYPE token's name is a case-sensitive match for the string "html",
                //      the token's public identifier is the case-sensitive string "-//W3C//DTD XHTML 1.1//EN",
                //      and the token's system identifier is the case-sensitive string "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd".
                if (
                    (this.Token.DocTypeName != "html") ||
                    (this.Token.DocTypePublicIdentifier != null) ||
                    ((this.Token.DocTypeSystemIdentifier != null) && (this.Token.DocTypeSystemIdentifier != "about:legacy-compat")))
                {
                    // ISSUE: Does "... and none of the..." applies to all the conditions or only to the last condition? I read it as all.
                    if (
                        (this.Token.DocTypeName == "html") &&
                        (this.Token.DocTypePublicIdentifier == "-//W3C//DTD HTML 4.0//EN") &&
                        ((this.Token.DocTypeSystemIdentifier == null) || (this.Token.DocTypeSystemIdentifier == "http://www.w3.org/TR/REC-html40/strict.dtd")))
                    {
                        // Match condition 1.
                    }
                    else if (
                        (this.Token.DocTypeName == "html") &&
                        (this.Token.DocTypePublicIdentifier == "-//W3C//DTD HTML 4.01//EN") &&
                        ((this.Token.DocTypeSystemIdentifier == null) || (this.Token.DocTypeSystemIdentifier == "http://www.w3.org/TR/html4/strict.dtd")))
                    {
                        // Match condition 2.
                    }
                    else if (
                      (this.Token.DocTypeName == "html") &&
                      (this.Token.DocTypePublicIdentifier == "-//W3C//DTD XHTML 1.0 Strict//EN") &&
                      (this.Token.DocTypeSystemIdentifier == "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"))
                    {
                        // Match condition 3.
                    }
                    else if (
                      (this.Token.DocTypeName == "html") &&
                      (this.Token.DocTypePublicIdentifier == "-//W3C//DTD XHTML 1.1//EN") &&
                      (this.Token.DocTypeSystemIdentifier == "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"))
                    {
                        // Match condition 4.
                    }
                    else
                    {
                        // No match ... there is a parse error.
                        this.ParseError(Parsing.ParseError.InvalidDocType);
                    }

                    // Conformance checkers may, based on the values(including presence or lack thereof) of the
                    // DOCTYPE token's name, public identifier, or system identifier, switch to a conformance
                    // checking mode for another language (e.g. based on the DOCTYPE token a conformance checker
                    // could recognize that the document is an HTML4-era document, and defer to an HTML4 conformance checker.)
                }

                // Append a DocumentType node to the Document node, with the name attribute set to the name
                // given in the DOCTYPE token, or the empty string if the name was missing; the publicId attribute
                // set to the public identifier given in the DOCTYPE token, or the empty string if the
                // public identifier was missing; the systemId attribute set to the system identifier given
                // in the DOCTYPE token, or the empty string if the system identifier was missing; and the other
                // attributes specific to DocumentType objects set to null and empty lists as appropriate.Associate
                // the DocumentType node with the Document object so that it is returned as the value of the doctype
                // attribute of the Document object.
                this.DomFactory.AppendDocType(
                    this.Token.DocTypeName ?? String.Empty,
                    this.Token.DocTypePublicIdentifier ?? String.Empty,
                    this.Token.DocTypeSystemIdentifier ?? String.Empty);

                bool quirks = false;

                // Then, if the document is not an iframe srcdoc document, and the DOCTYPE token matches one of the
                // conditions in the following list, then set the Document to quirks mode:
                if (!this.ParsingContext.IsIFrameSource)
                {
                    string publicId = this.Token.DocTypePublicIdentifier ?? String.Empty;

                    // The force-quirks flag is set to on.
                    if (this.Token.DocTypeForceQuirks)
                        quirks = true;

                    // The name is set to anything other than "html" (compared case-sensitively).
                    else if (this.Token.DocTypeName != "html")
                        quirks = true;

                    // The public identifier starts with: "+//Silmaril//dtd html Pro v0r11 19970101//"
                    else if (publicId.StartsWith("+//Silmaril//dtd html Pro v0r11 19970101//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//AdvaSoft Ltd//DTD HTML 3.0 asWedit + extensions//"
                    else if (publicId.StartsWith("-//AdvaSoft Ltd//DTD HTML 3.0 asWedit + extensions//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//AS//DTD HTML 3.0 asWedit + extensions//"
                    else if (publicId.StartsWith("-//AS//DTD HTML 3.0 asWedit + extensions//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.0 Level 1//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.0 Level 1//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.0 Level 2//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.0 Level 2//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.0 Strict Level 1//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.0 Strict Level 1//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.0 Strict Level 2//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.0 Strict Level 2//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.0 Strict//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.0 Strict//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.0//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 2.1E//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 2.1E//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 3.0//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 3.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 3.2 Final//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 3.2 Final//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 3.2//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 3.2//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML 3//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML 3//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Level 0//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Level 0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Level 1//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Level 1//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Level 2//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Level 2//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Level 3//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Level 3//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Strict Level 0//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Strict Level 0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Strict Level 1//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Strict Level 1//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Strict Level 2//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Strict Level 2//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Strict Level 3//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Strict Level 3//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML Strict//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML Strict//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//IETF//DTD HTML//"
                    else if (publicId.StartsWith("-//IETF//DTD HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Metrius//DTD Metrius Presentational//"
                    else if (publicId.StartsWith("-//Metrius//DTD Metrius Presentational//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Microsoft//DTD Internet Explorer 2.0 HTML Strict//"
                    else if (publicId.StartsWith("-//Microsoft//DTD Internet Explorer 2.0 HTML Strict//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Microsoft//DTD Internet Explorer 2.0 HTML//"
                    else if (publicId.StartsWith("-//Microsoft//DTD Internet Explorer 2.0 HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Microsoft//DTD Internet Explorer 2.0 Tables//"
                    else if (publicId.StartsWith("-//Microsoft//DTD Internet Explorer 2.0 Tables//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Microsoft//DTD Internet Explorer 3.0 HTML Strict//"
                    else if (publicId.StartsWith("-//Microsoft//DTD Internet Explorer 3.0 HTML Strict//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Microsoft//DTD Internet Explorer 3.0 HTML//"
                    else if (publicId.StartsWith("-//Microsoft//DTD Internet Explorer 3.0 HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Microsoft//DTD Internet Explorer 3.0 Tables//"
                    else if (publicId.StartsWith("-//Microsoft//DTD Internet Explorer 3.0 Tables//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Netscape Comm. Corp.//DTD HTML//"
                    else if (publicId.StartsWith("-//Netscape Comm. Corp.//DTD HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Netscape Comm. Corp.//DTD Strict HTML//"
                    else if (publicId.StartsWith("-//Netscape Comm. Corp.//DTD Strict HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//O'Reilly and Associates//DTD HTML 2.0//"
                    else if (publicId.StartsWith("-//O'Reilly and Associates//DTD HTML 2.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//O'Reilly and Associates//DTD HTML Extended 1.0//"
                    else if (publicId.StartsWith("-//O'Reilly and Associates//DTD HTML Extended 1.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//O'Reilly and Associates//DTD HTML Extended Relaxed 1.0//"
                    else if (publicId.StartsWith("-//O'Reilly and Associates//DTD HTML Extended Relaxed 1.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//SoftQuad Software//DTD HoTMetaL PRO 6.0::19990601::extensions to HTML 4.0//"
                    else if (publicId.StartsWith("-//SoftQuad Software//DTD HoTMetaL PRO 6.0::19990601::extensions to HTML 4.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//SoftQuad//DTD HoTMetaL PRO 4.0::19971010::extensions to HTML 4.0//"
                    else if (publicId.StartsWith("-//SoftQuad//DTD HoTMetaL PRO 4.0::19971010::extensions to HTML 4.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Spyglass//DTD HTML 2.0 Extended//"
                    else if (publicId.StartsWith("-//Spyglass//DTD HTML 2.0 Extended//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//SQ//DTD HTML 2.0 HoTMetaL + extensions//"
                    else if (publicId.StartsWith("-//SQ//DTD HTML 2.0 HoTMetaL + extensions//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Sun Microsystems Corp.//DTD HotJava HTML//"
                    else if (publicId.StartsWith("-//Sun Microsystems Corp.//DTD HotJava HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//Sun Microsystems Corp.//DTD HotJava Strict HTML//"
                    else if (publicId.StartsWith("-//Sun Microsystems Corp.//DTD HotJava Strict HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 3 1995-03-24//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 3 1995-03-24//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 3.2 Draft//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 3.2 Draft//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 3.2 Final//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 3.2 Final//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 3.2//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 3.2//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 3.2S Draft//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 3.2S Draft//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 4.0 Frameset//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 4.0 Frameset//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML 4.0 Transitional//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML 4.0 Transitional//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML Experimental 19960712//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML Experimental 19960712//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD HTML Experimental 970421//"
                    else if (publicId.StartsWith("-//W3C//DTD HTML Experimental 970421//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD W3 HTML//"
                    else if (publicId.StartsWith("-//W3C//DTD W3 HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3O//DTD W3 HTML 3.0//"
                    else if (publicId.StartsWith("-//W3O//DTD W3 HTML 3.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier is set to: "-//W3O//DTD W3 HTML Strict 3.0//EN//"
                    else if (publicId.Equals("-//W3O//DTD W3 HTML Strict 3.0//EN//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//WebTechs//DTD Mozilla HTML 2.0//"
                    else if (publicId.StartsWith("-//WebTechs//DTD Mozilla HTML 2.0//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//WebTechs//DTD Mozilla HTML//"
                    else if (publicId.StartsWith("-//WebTechs//DTD Mozilla HTML//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier is set to: "-/W3C/DTD HTML 4.0 Transitional/EN"
                    else if (publicId.Equals("-/W3C/DTD HTML 4.0 Transitional/EN", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier is set to: "HTML"
                    else if (publicId.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The system identifier is set to: "http://www.ibm.com/data/dtd/v11/ibmxhtml1-transitional.dtd"
                    else if ((this.Token.DocTypeSystemIdentifier != null) && this.Token.DocTypeSystemIdentifier.Equals("-/W3C/DTD HTML 4.0 Transitional/EN", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The system identifier is missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Frameset//"
                    else if ((this.Token.DocTypeSystemIdentifier == null) && publicId.StartsWith("-//W3C//DTD HTML 4.01 Frameset//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The system identifier is missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Transitional//"
                    else if ((this.Token.DocTypeSystemIdentifier == null) && publicId.StartsWith("-//W3C//DTD HTML 4.01 Transitional//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    if (quirks)
                        this.DomFactory.SetQuirksMode(DomFactory.QuirksMode.On);
                }

                // Otherwise, if the document is not an iframe srcdoc document, and the DOCTYPE token matches one of the conditions
                // in the following list, then set the Document to limited-quirks mode:
                if (!quirks && !this.ParsingContext.IsIFrameSource)
                {
                    string publicId = this.Token.DocTypePublicIdentifier ?? String.Empty;

                    // The public identifier starts with: "-//W3C//DTD XHTML 1.0 Frameset//"
                    if (publicId.StartsWith("-//W3C//DTD XHTML 1.0 Frameset//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The public identifier starts with: "-//W3C//DTD XHTML 1.0 Transitional//"
                    else if (publicId.StartsWith("-//W3C//DTD XHTML 1.0 Transitional//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The system identifier is not missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Frameset//"
                    else if ((this.Token.DocTypeSystemIdentifier != null) && publicId.StartsWith("-//W3C//DTD HTML 4.01 Frameset//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    // The system identifier is not missing and the public identifier starts with: "-//W3C//DTD HTML 4.01 Transitional//"
                    else if ((this.Token.DocTypeSystemIdentifier != null) && publicId.StartsWith("-//W3C//DTD HTML 4.01 Transitional//", StringComparison.OrdinalIgnoreCase))
                        quirks = true;

                    if (quirks)
                        this.DomFactory.SetQuirksMode(DomFactory.QuirksMode.Limited);
                }

                // The system identifier and public identifier strings must be compared to the values given
                // in the lists above in an ASCII case-insensitive manner. A system identifier whose value
                // is the empty string is not considered missing for the purposes of the conditions above.

                // Then, switch the insertion mode to "before html".
                this.Switch(InsertionModeEnum.BeforeHtml);
            }

            // Anything else
            else
            {
                // If the document is not an iframe srcdoc document, then this is a parse error; set the Document to quirks mode.
                if (!this.ParsingContext.IsIFrameSource)
                {
                    this.ParseError(Parsing.ParseError.UnexpectedTag);
                    this.DomFactory.SetQuirksMode(DomFactory.QuirksMode.On);
                }

                // In any case, switch the insertion mode to "before html", then reprocess the token.
                this.Switch(InsertionModeEnum.BeforeHtml);
                this.ProcessToken();
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
                this.ParseError(Parsing.ParseError.UnexpectedTag);
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
                Element html = this.DomFactory.CreateElement(this.Token.TagName, this.Token.TagAttributes, true);
                this.ParsingState.Html = html;

                // If the Document is being loaded as part of navigation of a browsing context, then:
                // if the newly created element has a manifest attribute whose value is not the empty string,
                // then resolve the value of that attribute to an absolute URL, relative to the newly created element,
                // and if that is successful, run the application cache selection algorithm with the result of applying
                // the URL serializer algorithm to the resulting parsed URL with the exclude fragment flag set; otherwise,
                // if there is no such attribute, or its value is the empty string, or resolving its value fails,
                // run the application cache selection algorithm with no manifest.The algorithm must be passed the Document object.
                // UNSUPPORTED: We do not support application cache.

                // Switch the insertion mode to "before head".
                this.Switch(InsertionModeEnum.BeforeHead);
            }

            // An end tag whose tag name is one of: "head", "body", "html", "br"
            else if (this.Token.IsEndTagNamed(Tags.Head, Tags.Body, Tags.Html, Tags.Br))
            {
                // Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedEndTag);
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
                Element html = this.DomFactory.CreateElement(Tags.Html, Attribute.None, true);
                this.ParsingState.Html = html;

                // If the Document is being loaded as part of navigation of a browsing context, then:
                // run the application cache selection algorithm with no manifest, passing it the Document object.
                // UNSUPPORTED: We do not support application cache.

                // Switch the insertion mode to "before head", then reprocess the token.
                this.Switch(InsertionModeEnum.BeforeHead);
                this.ProcessToken();
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

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                // Ignore the token.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.DomFactory.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.Using(InsertionModeEnum.InBody, this.ProcessToken);
            }

            // A start tag whose tag name is "head"
            else if (this.Token.IsStartTagNamed(Tags.Head))
            {
                // Insert an HTML element for the token.
                Element head = this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);

                // Set the head element pointer to the newly created head element.
                this.ParsingState.Head = head;

                // Switch the insertion mode to "in head".
                this.Switch(InsertionModeEnum.InHead);
            }

            // An end tag whose tag name is one of: "head", "body", "html", "br"
            else if (this.Token.IsEndTagNamed(Tags.Head, Tags.Body, Tags.Html, Tags.Br))
            {
                // Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Insert an HTML element for a "head" start tag token with no attributes.
                Element head = this.DomFactory.InsertHtmlElement(Tags.Head, Attribute.None);

                // Set the head element pointer to the newly created head element.
                this.ParsingState.Head = head;

                // Switch the insertion mode to "in head".
                this.Switch(InsertionModeEnum.InHead);

                // Reprocess the current token.
                this.ProcessToken();
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

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                // Insert the character.
                this.DomFactory.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.DomFactory.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.Using(InsertionModeEnum.InBody, this.ProcessToken);
            }

            // A start tag whose tag name is one of: "base", "basefont", "bgsound", "link"
            else if (this.Token.IsStartTagNamed(Tags.Base, Tags.BaseFont, Tags.BgSound, Tags.Link))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();
            }

            // A start tag whose tag name is "meta"
            else if (this.Token.IsStartTagNamed(Tags.Meta))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                Element meta = this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();

                // If the element has a charset attribute, and getting an encoding from its value results in an encoding,
                // and the confidence is currently tentative, then change the encoding to the resulting encoding.

                // Otherwise, if the element has an http-equiv attribute whose value is an ASCII case-insensitive
                // match for the string "Content-Type", and the element has a content attribute, and applying the
                // algorithm for extracting a character encoding from a meta element to that attribute's value returns
                // a supported ASCII-compatible character encoding or a UTF-16 encoding, and the confidence is currently
                // tentative, then change the encoding to the extracted encoding.

                // ISSUE. TODO. Handle encoding.
            }

            // A start tag whose tag name is "title"
            else if (this.Token.IsStartTagNamed(Tags.Title))
            {
                // Follow the generic RCDATA element parsing algorithm.
                this.ParseElement(ParsingAlgorithm.GenericRcData);
            }

            // A start tag whose tag name is "noscript", if the scripting flag is enabled
            // A start tag whose tag name is one of: "noframes", "style"
            else if ((this.ParsingContext.Scripting && this.Token.IsStartTagNamed(Tags.NoScript)) || this.Token.IsStartTagNamed(Tags.NoFrames, Tags.Style))
            {
                // Follow the generic raw text element parsing algorithm.
                this.ParseElement(ParsingAlgorithm.GenericRawText);
            }

            // A start tag whose tag name is "noscript", if the scripting flag is disabled
            else if (!this.ParsingContext.Scripting && this.Token.IsStartTagNamed(Tags.NoScript))
            {
                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);

                // Switch the insertion mode to "in head noscript".
                this.Switch(InsertionModeEnum.InHeadNoscript);
            }

            // A start tag whose tag name is "script"
            else if (this.Token.IsStartTagNamed(Tags.Script))
            {
                // Run these steps:
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
                throw new NotImplementedException();
            }

            // An end tag whose tag name is "head"
            else if (this.Token.IsEndTagNamed(Tags.Head))
            {
                // Pop the current node (which will be the head element) off the stack of open elements.
                Element head = this.OpenElements.Pop();

                // Switch the insertion mode to "after head".
                this.Switch(InsertionModeEnum.AfterHead);
            }

            // An end tag whose tag name is one of: "body", "html", "br"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Html, Tags.Br))
            {
                // Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            // A start tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Template))
            {
                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);

                // Insert a marker at the end of the list of active formatting elements.

                // Set the frameset-ok flag to "not ok".

                // Switch the insertion mode to "in template".

                // Push "in template" onto the stack of template insertion modes so that it is the new
                // current template insertion mode.
                throw new NotImplementedException();
            }

            // An end tag whose tag name is "template"
            else if (this.Token.IsEndTagNamed(Tags.Template))
            {
                // If there is no template element on the stack of open elements, then this is a parse error; ignore the token.

                // Otherwise, run these steps:
                //        1. Generate implied end tags.
                //        2. If the current node is not a template element, then this is a parse error.
                //        3. Pop elements from the stack of open elements until a template element has been popped from the stack.
                //        4. Clear the list of active formatting elements up to the last marker.
                //        5. Pop the current template insertion mode off the stack of template insertion modes.
                //        6. Reset the insertion mode appropriately.
                throw new NotImplementedException();
            }

            // A start tag whose tag name is "head"
            // Any other end tag
            else if (this.Token.IsStartTagNamed(Tags.Head) || (this.Token.Type == TokenType.EndTag))
            {
                // Parse error. Ignore the token.
                if (this.Token.Type == TokenType.StartTag)
                    this.ParseError(Parsing.ParseError.UnexpectedStartTag);
                else
                    this.ParseError(Parsing.ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Pop the current node (which will be the head element) off the stack of open elements.
                Element head = this.OpenElements.Pop();

                // Switch the insertion mode to "after head".
                this.Switch(InsertionModeEnum.AfterHead);

                // Reprocess the token.
                this.ProcessToken();
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

            // A DOCTYPE token
            if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.Using(InsertionModeEnum.InBody, this.ProcessToken);
            }

            // An end tag whose tag name is "noscript"
            else if (this.Token.IsEndTagNamed(Tags.NoScript))
            {
                // Pop the current node (which will be a noscript element) from the stack of open elements;
                // the new current node will be a head element.
                Element noscript = this.OpenElements.Pop();

                // Switch the insertion mode to "in head".
                this.Switch(InsertionModeEnum.InHead);
            }

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            // A comment token
            // A start tag whose tag name is one of: "basefont", "bgsound", "link", "meta", "noframes", "style"
            else if (this.Token.IsCharacterWhitespace() || (this.Token.Type == TokenType.Comment) || this.Token.IsStartTagNamed(Tags.BaseFont, Tags.BgSound, Tags.Link, Tags.Meta, Tags.NoFrames, Tags.Style))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.Using(InsertionModeEnum.InHead, this.ProcessToken);
            }

            // An end tag whose tag name is "br"
            else if (this.Token.IsEndTagNamed(Tags.Br))
            {
                // Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            // A start tag whose tag name is one of: "head", "noscript"
            // Any other end tag
            else if (this.Token.IsStartTagNamed(Tags.Head, Tags.NoScript) || (this.Token.Type == TokenType.EndTag))
            {
                // Parse error. Ignore the token.
                if (this.Token.Type == TokenType.StartTag)
                    this.ParseError(Parsing.ParseError.UnexpectedStartTag);
                else
                    this.ParseError(Parsing.ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Parse error.
                this.ParseError(Parsing.ParseError.UnexpectedTag);

                // Pop the current node (which will be a noscript element) from the stack of open elements;
                // the new current node will be a head element.
                Element noscript = this.OpenElements.Pop();

                // Switch the insertion mode to "in head".
                this.Switch(InsertionModeEnum.InHead);

                // Reprocess the token.
                this.ProcessToken();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterHeadMode()
        {
            /*
            8.2.5.4.6 The "after head" insertion mode

            When the user agent is to apply the rules for the "after head" insertion mode,
            the user agent must handle the token as follows:
            */

            bool anythingElse = false;

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                // Insert the character.
                this.DomFactory.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.DomFactory.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.Using(InsertionModeEnum.InBody, this.ProcessToken);
            }

            // A start tag whose tag name is "body"
            else if (this.Token.IsStartTagNamed(Tags.Body))
            {
                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(Tags.Body, this.Token.TagAttributes);

                // Set the frameset-ok flag to "not ok".
                this.ParsingState.FramesetOk = false;

                // Switch the insertion mode to "in body".
                this.Switch(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "frameset"
            else if (this.Token.IsStartTagNamed(Tags.Frameset))
            {
                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(Tags.Frameset, this.Token.TagAttributes);

                // Switch the insertion mode to "in frameset".
                this.Switch(InsertionModeEnum.InFrameset);
            }

            // A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta",
            // "noframes", "script", "style", "template", "title"
            else if (this.Token.IsStartTagNamed(Tags.Base, Tags.BaseFont, Tags.BgSound, Tags.Link, Tags.Meta, Tags.NoFrames, Tags.Script, Tags.Style, Tags.Template, Tags.Title))
            {
                // Parse error.
                this.ParseError(Parsing.ParseError.UnexpectedStartTag);

                // Push the node pointed to by the head element pointer onto the stack of open elements.
                this.OpenElements.Push(this.ParsingState.Head);

                // Process the token using the rules for the "in head" insertion mode.
                this.Using(InsertionModeEnum.InHead, this.ProcessToken);

                // Remove the node pointed to by the head element pointer from the stack of open elements.
                // (It might not be the current node at this point.)
                this.OpenElements.Remove(this.ParsingState.Head);

                // NOTE: The head element pointer cannot be null at this point.
            }

            // An end tag whose tag name is "template"
            else if (this.Token.IsEndTagNamed(Tags.Template))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.Using(InsertionModeEnum.InHead, this.ProcessToken);
            }

            // An end tag whose tag name is one of: "body", "html", "br"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Html, Tags.Br))
            {
                // Act as described in the "anything else" entry below.
                anythingElse = true;
            }

            // A start tag whose tag name is "head"
            // Any other end tag
            else if (this.Token.IsStartTagNamed(Tags.Head) || (this.Token.Type == TokenType.EndTag))
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            // Anything else
            if (anythingElse)
            {
                // Insert an HTML element for a "body" start tag token with no attributes.
                this.DomFactory.InsertHtmlElement(Tags.Body, Attribute.None);

                // Switch the insertion mode to "in body".
                this.Switch(InsertionModeEnum.InBody);

                // Reprocess the current token.
                this.ProcessToken();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInBodyMode()
        {
            /*
            8.2.5.4.7 The "in body" insertion mode

            When the user agent is to apply the rules for the "in body" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token that is U+0000 NULL
            if (this.Token.IsCharacterNull())
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.NullCharacter);
            }

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            else if (this.Token.IsCharacterWhitespace())
            {
                // Reconstruct the active formatting elements, if any.
                this.ReconstructFormattingElements();

                // Insert the token's character.
                this.DomFactory.InsertCharacter(this.Token.Character);
            }

            // Any other character token
            else if (this.Token.Type == TokenType.Character)
            {
                // Reconstruct the active formatting elements, if any.
                this.ReconstructFormattingElements();

                // Insert the token's character.
                this.DomFactory.InsertCharacter(this.Token.Character);

                // Set the frameset-ok flag to "not ok".
                this.ParsingState.FramesetOk = false;
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.DomFactory.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.ParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Parse error.
                this.ParseError(Parsing.ParseError.UnexpectedTag);

                // If there is a template element on the stack of open elements, then ignore the token.
                if (this.OpenElements.Contains(Tags.Template))
                    return;

                // Otherwise, for each attribute on the token, check to see if the attribute is already present
                // on the top element of the stack of open elements. If it is not, add the attribute and its
                // corresponding value to that element.
                if (this.Token.TagAttributes.Length != 0)
                {
                    Element element = this.OpenElements.Top;
                    foreach (Attribute attrib in this.Token.TagAttributes)
                    {
                        if (!element.Attributes.Any(existing => existing.Name == attrib.Name))
                            element.SetAttribute(attrib.Name, attrib.Value);
                    }
                }
            }

            // A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta", "noframes",
            // "script", "style", "template", "title"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Base, Tags.BaseFont, Tags.BgSound, Tags.Link, Tags.Meta, Tags.NoFrames, Tags.Script, Tags.Style, Tags.Template, Tags.Title) || this.Token.IsEndTagNamed(Tags.Template))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.Using(InsertionModeEnum.InHead, this.ProcessToken);
            }

            // A start tag whose tag name is "body"
            else if (this.Token.IsStartTagNamed(Tags.Body))
            {
                // Parse error.
                this.ParseError(Parsing.ParseError.UnexpectedTag);

                // If the second element on the stack of open elements is not a body element, if the stack of open elements
                // has only one node on it, or if there is a template element on the stack of open elements,
                // then ignore the token. (fragment case)
                if (((this.OpenElements.Count >= 2) && (this.OpenElements[1].TagName == Tags.Body)) ||
                    (this.OpenElements.Count == 1) || this.OpenElements.Contains(Tags.Template))
                    return;

                // Otherwise, set the frameset-ok flag to "not ok"; then, for each attribute on the token, check to see if
                // the attribute is already present on the body element (the second element) on the stack of open elements,
                // and if it is not, add the attribute and its corresponding value to that element.
                this.ParsingState.FramesetOk = false;
                if (this.Token.TagAttributes.Length != 0)
                {
                    Element body = this.OpenElements[1];
                    foreach (Attribute attrib in this.Token.TagAttributes)
                    {
                        if (!body.Attributes.Any(existing => existing.Name == attrib.Name))
                            body.SetAttribute(attrib.Name, attrib.Value);
                    }
                }
            }

            // A start tag whose tag name is "frameset"
            else if (this.Token.IsStartTagNamed(Tags.Frameset))
            {
                // Parse error.
                this.ParseError(Parsing.ParseError.UnexpectedStartTag);

                throw new NotImplementedException();

                // If the stack of open elements has only one node on it, or if the second element on the stack of open
                // elements is not a body element, then ignore the token. (fragment case)

                //    If the frameset-ok flag is set to "not ok", ignore the token.

                //    Otherwise, run the following steps:
                //        1. Remove the second element on the stack of open elements from its parent node, if it has one.
                //        2. Pop all the nodes from the bottom of the stack of open elements, from the current node up to,
                //           but not including, the root html element.
                //        3. Insert an HTML element for the token.
                //        4. Switch the insertion mode to "in frameset".
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // If the stack of template insertion modes is not empty, then process the token using the rules for
                // the "in template" insertion mode.
                if (this.TemplateInsertionMode.Count != 0)
                {
                    this.Using(InsertionModeEnum.InTemplate, this.ProcessToken);
                }
                else
                {
                    // If there is a node in the stack of open elements that is not either a dd element, a dt element,
                    // an li element, a p element, a tbody element, a td element, a tfoot element, a th element,
                    // a thead element, a tr element, the body element, or the html element, then this is a parse error.
                    if (this.OpenElements.Contains(Tags.Dd, Tags.Dt, Tags.Li, Tags.P, Tags.TBody, Tags.Td, Tags.TFoot,
                        Tags.Th, Tags.THead, Tags.Tr, Tags.Body, Tags.Html))
                    {
                        this.ParseError(Parsing.ParseError.PrematureEndOfFile);
                    }

                    // Stop parsing.
                    this.StopParsing();
                }
            }

            // An end tag whose tag name is "body"
            else if (this.Token.IsEndTagNamed(Tags.Body))
            {
                // If the stack of open elements does not have a body element in scope, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInSpecificScope(Tags.Body, OpenElementsStack.ScopeTags))
                {
                    this.ParseError(Parsing.ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, if there is a node in the stack of open elements that is not either a dd element,
                // a dt element, an li element, an optgroup element, an option element, a p element, an rb element,
                // an rp element, an rt element, an rtc element, a tbody element, a td element, a tfoot element,
                // a th element, a thead element, a tr element, the body element, or the html element, then this is a parse error.
                if (this.OpenElements.Contains(Tags.Dd, Tags.Dt, Tags.Li, Tags.OptGroup, Tags.Option, Tags.P, Tags.Rb, Tags.Rp, 
                    Tags.Rt, Tags.Rtc, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr, Tags.Body, Tags.Html))
                {
                    this.ParseError(Parsing.ParseError.UnexpectedEndTag);
                }

                // Switch the insertion mode to "after body".
                this.Switch(InsertionModeEnum.AfterBody);
            }

            // An end tag whose tag name is "html"
            else if (this.Token.IsEndTagNamed(Tags.Html))
            {
                // If the stack of open elements does not have a body element in scope, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInSpecificScope(Tags.Body, OpenElementsStack.ScopeTags))
                {
                    this.ParseError(Parsing.ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, if there is a node in the stack of open elements that is not either a dd element, a dt element,
                // an li element, an optgroup element, an option element, a p element, an rb element, an rp element,
                // an rt element, an rtc element, a tbody element, a td element, a tfoot element, a th element, a thead element,
                // a tr element, the body element, or the html element, then this is a parse error.
                if (this.OpenElements.Contains(Tags.Dd, Tags.Dt, Tags.Li, Tags.OptGroup, Tags.Option, Tags.P, Tags.Rb, Tags.Rp,
                    Tags.Rt, Tags.Rtc, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr, Tags.Body, Tags.Html))
                {
                    this.ParseError(Parsing.ParseError.UnexpectedEndTag);
                }

                // Switch the insertion mode to "after body".
                this.Switch(InsertionModeEnum.AfterBody);

                // Reprocess the token.
                this.ProcessToken();
            }

            // A start tag whose tag name is one of: "address", "article", "aside", "blockquote", "center", "details", "dialog",
            // "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup", "main", "nav", "ol", "p",
            // "section", "summary", "ul"
            else if (this.Token.IsStartTagNamed(
                Tags.Address, Tags.Article, Tags.Aside, Tags.BlockQuote, Tags.Center, Tags.Details,  Tags.Dialog, 
                Tags.Dir, Tags.Div, Tags.Dl, Tags.FieldSet, Tags.FigCaption, Tags.Figure, Tags.Footer, Tags.Header,  Tags.HGroup,
                Tags.Main, Tags.Nav, Tags.Ol, Tags.P, Tags.Section, Tags.Summary, Tags.Ul))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInSpecificScope(Tags.P, OpenElementsStack.ButtonScopeTags))
                    this.ClosePElement();

                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);
            }

            // A start tag whose tag name is one of: "h1", "h2", "h3", "h4", "h5", "h6"
            else if (this.Token.IsStartTagNamed(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInSpecificScope(Tags.P, OpenElementsStack.ButtonScopeTags))
                    this.ClosePElement();

                // If the current node is an HTML element whose tag name is one of "h1", "h2", "h3", "h4", "h5", or "h6",
                // then this is a parse error; pop the current node off the stack of open elements.
                if (this.IsCurrentNode(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
                {
                    this.ParseError(Parsing.ParseError.UnexpectedStartTag);
                    this.OpenElements.Pop();
                }

                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);
            }

            // A start tag whose tag name is one of: "pre", "listing"
            else if (this.Token.IsStartTagNamed(Tags.Pre, Tags.Listing))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInSpecificScope(Tags.P, OpenElementsStack.ButtonScopeTags))
                    this.ClosePElement();

                // Insert an HTML element for the token.
                this.DomFactory.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);

                //    If the next token is a "LF" (U+000A) character token, then ignore that token and move on to the
                //    next one. (Newlines at the start of pre blocks are ignored as an authoring convenience.)

                //    Set the frameset-ok flag to "not ok".
            }

            // A start tag whose tag name is "form"
            else if (this.Token.IsStartTagNamed(Tags.Form))
            {
                //    If the form element pointer is not null, and there is no template element on the stack of open elements,
                //    then this is a parse error; ignore the token.

                //    Otherwise:
                //        If the stack of open elements has a p element in button scope, then close a p element.

                //    Insert an HTML element for the token, and, if there is no template element on the stack of
                //    open elements, set the form element pointer to point to the element created.
            }

            // A start tag whose tag name is "li"
            else if (this.Token.IsStartTagNamed(Tags.Li))
            {
                //    Run these steps:
                //        1. Set the frameset-ok flag to "not ok".
                //        2. Initialize node to be the current node (the bottommost node of the stack).
                //        3. Loop: If node is an li element, then run these substeps:
                //            1. Generate implied end tags, except for li elements.
                //            2. If the current node is not an li element, then this is a parse error.
                //            3. Pop elements from the stack of open elements until an li element has been popped from the stack.
                //            4. Jump to the step labeled done below.
                //        4. If node is in the special category, but is not an address, div, or p element,
                //           then jump to the step labeled done below.
                //        5. Otherwise, set node to the previous entry in the stack of open elements and return to the step labeled loop.
                //        6. Done: If the stack of open elements has a p element in button scope, then close a p element.
                //        7. Finally, insert an HTML element for the token.
            }

            // A start tag whose tag name is one of: "dd", "dt"
            else if (this.Token.IsStartTagNamed(Tags.Dd, Tags.Dt))
            {
                //    Run these steps:
                //        1. Set the frameset-ok flag to "not ok".
                //        2. Initialize node to be the current node (the bottommost node of the stack).
                //        3. Loop: If node is a dd element, then run these substeps:
                //            1. Generate implied end tags, except for dd elements.
                //            2. If the current node is not a dd element, then this is a parse error.
                //            3. Pop elements from the stack of open elements until a dd element has been popped from the stack.
                //            4. Jump to the step labeled done below.
                //        4. If node is a dt element, then run these substeps:
                //            1. Generate implied end tags, except for dt elements.
                //            2. If the current node is not a dt element, then this is a parse error.
                //            3. Pop elements from the stack of open elements until a dt element has been popped from the stack.
                //            4. Jump to the step labeled done below.
                //        5. If node is in the special category, but is not an address, div, or p element,
                //           then jump to the step labeled done below.
                //        6. Otherwise, set node to the previous entry in the stack of open elements and return to the step labeled loop.
                //        7. Done: If the stack of open elements has a p element in button scope, then close a p element.
                //        8. Finally, insert an HTML element for the token.
            }

            // A start tag whose tag name is "plaintext"
            else if (this.Token.IsStartTagNamed(Tags.PlainText))
            {
                //    If the stack of open elements has a p element in button scope, then close a p element.

                //    Insert an HTML element for the token.

                //    Switch the tokenizer to the PLAINTEXT state.

                //    NOTE: Once a start tag with the tag name "plaintext" has been seen, that will be the last
                //    token ever seen other than character tokens (and the end-of-file token), because there is
                //    no way to switch out of the PLAINTEXT state.
            }

            // A start tag whose tag name is "button"
            else if (this.Token.IsStartTagNamed(Tags.Button))
            {
                //    1. If the stack of open elements has a button element in scope, then run these substeps:
                //        1. Parse error.
                //        2. Generate implied end tags.
                //        3. Pop elements from the stack of open elements until a button element has been popped from the stack.
                //    2. Reconstruct the active formatting elements, if any.
                //    3. Insert an HTML element for the token.
                //    4. Set the frameset-ok flag to "not ok".
            }

            // An end tag whose tag name is one of: "address", "article", "aside", "blockquote", "button", "center", "details",
            // "dialog", "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup", "listing", "main",
            // "nav", "ol", "pre", "section", "summary", "ul"
            else if (this.Token.IsEndTagNamed(
                Tags.Address, Tags.Article, Tags.Aside, Tags.BlockQuote, Tags.Button, Tags.Center, Tags.Details,
                Tags.Dialog, Tags.Dir, Tags.Div, Tags.Dl, Tags.FieldSet, Tags.FigCaption, Tags.Figure, Tags.Footer, Tags.Header,
                Tags.HGroup, Tags.Listing, Tags.Main, Tags.Nav, Tags.Ol, Tags.P, Tags.Section, Tags.Summary, Tags.Ul))
            {
                //    If the stack of open elements does not have an element in scope that is an HTML element and with the same
                //    tag name as that of the token, then this is a parse error; ignore the token.

                //    Otherwise, run these steps:
                //        1. Generate implied end tags.
                //        2. If the current node is not an HTML element with the same tag name as that of the token,
                //           then this is a parse error.
                //        3. Pop elements from the stack of open elements until an HTML element with the same tag name
                //           as the token has been popped from the stack.
            }

            // An end tag whose tag name is "form"
            else if (this.Token.IsEndTagNamed(Tags.Form))
            {
                //    If there is no template element on the stack of open elements, then run these substeps:
                //        1. Let node be the element that the form element pointer is set to, or null if it is not set to an element.
                //        2. Set the form element pointer to null. Otherwise, let node be null.
                //        3. If node is null or if the stack of open elements does not have node in scope, then this is a parse error;
                //           abort these steps and ignore the token.
                //        4. Generate implied end tags.
                //        5. If the current node is not node, then this is a parse error.
                //        6. Remove node from the stack of open elements.

                //    If there is a template element on the stack of open elements, then run these substeps instead:
                //        1. If the stack of open elements does not have a form element in scope, then this is a parse error;
                //           abort these steps and ignore the token.
                //        2. Generate implied end tags.
                //        3. If the current node is not a form element, then this is a parse error.
                //        4. Pop elements from the stack of open elements until a form element has been popped from the stack.
            }

            // An end tag whose tag name is "p"
            else if (this.Token.IsEndTagNamed(Tags.P))
            {
                //    If the stack of open elements does not have a p element in button scope, then this is a parse error;
                //    insert an HTML element for a "p" start tag token with no attributes.

                //    Close a p element.
            }

            // An end tag whose tag name is "li"
            else if (this.Token.IsEndTagNamed(Tags.Li))
            {
                //    If the stack of open elements does not have an li element in list item scope,
                //    then this is a parse error; ignore the token.

                //    Otherwise, run these steps:
                //        1. Generate implied end tags, except for li elements.
                //        2. If the current node is not an li element, then this is a parse error.
                //        3. Pop elements from the stack of open elements until an li element has been popped from the stack.
            }

            // An end tag whose tag name is one of: "dd", "dt"
            else if (this.Token.IsEndTagNamed(Tags.Dd, Tags.Dt))
            {
                //    If the stack of open elements does not have an element in scope that is an HTML element and with the
                //    same tag name as that of the token, then this is a parse error; ignore the token.

                //    Otherwise, run these steps:
                //        1. Generate implied end tags, except for HTML elements with the same tag name as the token.
                //        2. If the current node is not an HTML element with the same tag name as that of the token,
                //           then this is a parse error.
                //        3. Pop elements from the stack of open elements until an HTML element with the same tag name
                //           as the token has been popped from the stack.
            }

            // An end tag whose tag name is one of: "h1", "h2", "h3", "h4", "h5", "h6"
            else if (this.Token.IsEndTagNamed(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
            {
                //    If the stack of open elements does not have an element in scope that is an HTML element and whose tag
                //    name is one of "h1", "h2", "h3", "h4", "h5", or "h6", then this is a parse error; ignore the token.

                //    Otherwise, run these steps:
                //        1. Generate implied end tags.
                //        2. If the current node is not an HTML element with the same tag name as that of the token, then this
                //           is a parse error.
                //        3. Pop elements from the stack of open elements until an HTML element whose tag name is one of
                //           "h1", "h2", "h3", "h4", "h5", or "h6" has been popped from the stack.
            }

            // An end tag whose tag name is "sarcasm"
            else if (this.Token.IsEndTagNamed(Tags.Sarcasm))
            {
                //    Take a deep breath, then act as described in the "any other end tag" entry below.
            }

            // A start tag whose tag name is "a"
            else if (this.Token.IsStartTagNamed(Tags.A))
            {
                //    If the list of active formatting elements contains an a element between the end of the
                //    list and the last marker on the list (or the start of the list if there is no marker on the list),
                //    then this is a parse error; run the adoption agency algorithm for the tag name "a", then remove that
                //    element from the list of active formatting elements and the stack of open elements if the adoption agency
                //    algorithm didn't already remove it (it might not have if the element is not in table scope).

                //        In the non-conforming stream <a href="a">a<table><a href="b">b</table>x, the first a element would be
                //        closed upon seeing the second one, and the "x" character would be inside a link to "b", not to "a".
                //        This is despite the fact that the outer a element is not in table scope (meaning that a regular </a>
                //        end tag at the start of the table wouldn't close the outer a element). The result is that the two a
                //        elements are indirectly nested inside each other — non-conforming markup will often result in
                //        non-conforming DOMs when parsed.

                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token. Push onto the list of active formatting elements that element.
            }

            // A start tag whose tag name is one of: "b", "big", "code", "em", "font", "i", "s", "small", "strike",
            // "strong", "tt", "u"
            else if (this.Token.IsStartTagNamed(
                Tags.B, Tags.Big, Tags.Code, Tags.Em, Tags.Font, Tags.I, Tags.S, Tags.Small, Tags.Strike,
                Tags.Strong, Tags.Tt, Tags.U))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token. Push onto the list of active formatting elements that element.
            }

            // A start tag whose tag name is "nobr"
            else if (this.Token.IsStartTagNamed(Tags.Nobr))
            {
                //    Reconstruct the active formatting elements, if any.

                //    If the stack of open elements has a nobr element in scope, then this is a parse error; run the adoption
                //    agency algorithm for the tag name "nobr", then once again reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token. Push onto the list of active formatting elements that element.

            }

            // An end tag whose tag name is one of: "a", "b", "big", "code", "em", "font", "i", "nobr", "s", "small",
            // "strike", "strong", "tt", "u"
            else if (this.Token.IsEndTagNamed(
                Tags.A, Tags.B, Tags.Big, Tags.Code, Tags.Em, Tags.Font, Tags.I, Tags.Nobr, Tags.S, Tags.Small,
                Tags.Strike, Tags.Strong, Tags.Tt, Tags.U))
            {
                //    Run the adoption agency algorithm for the token's tag name.
            }

            // A start tag whose tag name is one of: "applet", "marquee", "object"
            else if (this.Token.IsStartTagNamed(Tags.Applet, Tags.Marquee, Tags.Object))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token.

                //    Insert a marker at the end of the list of active formatting elements.

                //    Set the frameset-ok flag to "not ok".
            }

            // An end tag token whose tag name is one of: "applet", "marquee", "object"
            else if (this.Token.IsEndTagNamed(Tags.Applet, Tags.Marquee, Tags.Object))
            {
                //    If the stack of open elements does not have an element in scope that is an HTML element and with the
                //    same tag name as that of the token, then this is a parse error; ignore the token.

                //    Otherwise, run these steps:
                //        1. Generate implied end tags.
                //        2. If the current node is not an HTML element with the same tag name as that of the token, then this
                //           is a parse error.
                //        3. Pop elements from the stack of open elements until an HTML element with the same tag name as the
                //           token has been popped from the stack.
                //        4. Clear the list of active formatting elements up to the last marker.
            }

            // A start tag whose tag name is "table"
            else if (this.Token.IsStartTagNamed(Tags.Table))
            {
                //    If the Document is not set to quirks mode, and the stack of open elements has a p element
                //    in button scope, then close a p element.

                //    Insert an HTML element for the token.

                //    Set the frameset-ok flag to "not ok".

                //    Switch the insertion mode to "in table".
            }

            // An end tag whose tag name is "br"
            else if (this.Token.IsEndTagNamed(Tags.Br))
            {
                //    Parse error. Act as described in the next entry, as if this
                //    was a "br" start tag token, rather than an end tag token.
            }

            // A start tag whose tag name is one of: "area", "br", "embed", "img", "keygen", "wbr"
            else if (this.Token.IsStartTagNamed(Tags.Area, Tags.Br, Tags.Embed, Tags.Img, Tags.KeyGen, Tags.Wbr))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.

                //    Set the frameset-ok flag to "not ok".
            }

            // A start tag whose tag name is "input"
            else if (this.Token.IsStartTagNamed(Tags.Input))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.

                //    If the token does not have an attribute with the name "type", or if it does, but that attribute's value
                //    is not an ASCII case-insensitive match for the string "hidden", then: set the frameset-ok flag to "not ok".
            }

            // A start tag whose tag name is one of: "param", "source", "track"
            else if (this.Token.IsStartTagNamed(Tags.Param, Tags.Source, Tags.Track))
            {
                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.
            }

            // A start tag whose tag name is "hr"
            else if (this.Token.IsStartTagNamed(Tags.Hr))
            {
                //    If the stack of open elements has a p element in button scope, then close a p element.

                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.

                //    Set the frameset-ok flag to "not ok".
            }

            // A start tag whose tag name is "image"
            else if (this.Token.IsStartTagNamed(Tags.Image))
            {
                //    Parse error. Change the token's tag name to "img" and reprocess it. (Don't ask.)
            }

            // A start tag whose tag name is "isindex"
            else if (this.Token.IsStartTagNamed(Tags.IsIndex))
            {
                //    Parse error.

                //    If there is no template element on the stack of open elements and the form element pointer
                //    is not null, then ignore the token.

                //    Otherwise:
                //        Acknowledge the token's self-closing flag, if it is set.

                //        Set the frameset-ok flag to "not ok".

                //        If the stack of open elements has a p element in button scope, then close a p element.

                //        Insert an HTML element for a "form" start tag token with no attributes, and, if there
                //        is no template element on the stack of open elements, set the form element pointer to point
                //        to the element created.

                //        If the token has an attribute called "action", set the action attribute on the resulting form
                //        element to the value of the "action" attribute of the token.

                //        Insert an HTML element for an "hr" start tag token with no attributes. Immediately pop the
                //        current node off the stack of open elements.

                //        Reconstruct the active formatting elements, if any.

                //        Insert an HTML element for a "label" start tag token with no attributes.

                //        Insert characters (see below for what they should say).

                //        Insert an HTML element for an "input" start tag token with all the attributes from the "isindex"
                //        token except "name", "action", and "prompt", and with an attribute named "name" with the value
                //        "isindex". (This creates an input element with the name attribute set to the magic balue "isindex".)
                //        Immediately pop the current node off the stack of open elements.

                //        Insert more characters (see below for what they should say).

                //        Pop the current node (which will be the label element created earlier) off the stack of open elements.

                //        Insert an HTML element for an "hr" start tag token with no attributes. Immediately pop the current node
                //        off the stack of open elements.

                //        Pop the current node (which will be the form element created earlier) off the stack of open elements, and,
                //        if there is no template element on the stack of open elements, set the form element pointer back to null.

                //        Prompt: If the token has an attribute with the name "prompt", then the first stream of characters must be
                //        the same string as given in that attribute, and the second stream of characters must be empty. Otherwise,
                //        the two streams of character tokens together should, together with the input element, express the
                //        equivalent of "This is a searchable index. Enter search keywords: (input field)" in the user's preferred
                //        language.
            }

            // A start tag whose tag name is "textarea"
            else if (this.Token.IsStartTagNamed(Tags.TextArea))
            {
                //    Run these steps:
                //        1. Insert an HTML element for the token.
                //        2. If the next token is a "LF" (U+000A) character token, then ignore that token and move on to the
                //           next one. (Newlines at the start of textarea elements are ignored as an authoring convenience.)
                //        3. Switch the tokenizer to the RCDATA state.
                //        4. Let the original insertion mode be the current insertion mode.
                //        5. Set the frameset-ok flag to "not ok".
                //        6. Switch the insertion mode to "text".
            }

            // A start tag whose tag name is "xmp"
            else if (this.Token.IsStartTagNamed(Tags.Xmp))
            {
                //    If the stack of open elements has a p element in button scope, then close a p element.

                //    Reconstruct the active formatting elements, if any.

                //    Set the frameset-ok flag to "not ok".

                //    Follow the generic raw text element parsing algorithm.
            }

            // A start tag whose tag name is "iframe"
            else if (this.Token.IsStartTagNamed(Tags.IFrame))
            {
                //    Set the frameset-ok flag to "not ok".

                //    Follow the generic raw text element parsing algorithm.
            }

            // A start tag whose tag name is "noembed"
            // A start tag whose tag name is "noscript", if the scripting flag is enabled
            else if (this.Token.IsStartTagNamed(Tags.NoEmbed) || (this.ParsingContext.Scripting && this.Token.IsStartTagNamed(Tags.NoScript)))
            {
                //    Follow the generic raw text element parsing algorithm.
            }

            // A start tag whose tag name is "select"
            else if (this.Token.IsStartTagNamed(Tags.Select))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token.

                //    Set the frameset-ok flag to "not ok".

                //    If the insertion mode is one of "in table", "in caption", "in table body", "in row",
                //    or "in cell", then switch the insertion mode to "in select in table". Otherwise,
                //    switch the insertion mode to "in select".
            }

            // A start tag whose tag name is one of: "optgroup", "option"
            else if (this.Token.IsStartTagNamed(Tags.OptGroup, Tags.Option))
            {
                //    If the current node is an option element, then pop the current node off the stack of open elements.

                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token.
            }

            // A start tag whose tag name is one of: "rb", "rp", "rtc"
            else if (this.Token.IsStartTagNamed(Tags.Rb, Tags.Rp, Tags.Rtc))
            {
                //    If the stack of open elements has a ruby element in scope, then generate implied end tags.
                //    If the current node is not then a ruby element, this is a parse error.

                //    Insert an HTML element for the token.
            }

            // A start tag whose tag name is "rt"
            else if (this.Token.IsStartTagNamed(Tags.Rt))
            {
                //    If the stack of open elements has a ruby element in scope, then generate implied end tags, except for
                //    rtc elements. If the current node is not then a ruby element or an rtc element, this is a parse error.

                //    Insert an HTML element for the token.
            }

            // A start tag whose tag name is "math"
            else if (this.Token.IsStartTagNamed(Tags.Math))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Adjust MathML attributes for the token. (This fixes the case of MathML attributes that are not all lowercase.)

                //    Adjust foreign attributes for the token. (This fixes the use of namespaced attributes, in particular XLink.)

                //    Insert a foreign element for the token, in the MathML namespace.

                //    If the token has its self-closing flag set, pop the current node off the stack of open elements and acknowledge
                //    the token's self-closing flag.
            }

            // A start tag whose tag name is "svg"
            else if (this.Token.IsStartTagNamed(Tags.Svg))
            {
                //    Reconstruct the active formatting elements, if any.

                //    Adjust SVG attributes for the token. (This fixes the case of SVG attributes that are not all lowercase.)

                //    Adjust foreign attributes for the token. (This fixes the use of namespaced attributes, in particular XLink in SVG.)

                //    Insert a foreign element for the token, in the SVG namespace.

                //    If the token has its self-closing flag set, pop the current node off the stack of open elements
                //    and acknowledge the token's self-closing flag.
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "frame", "head", "tbody", "td", "tfoot",
            // "th", "thead", "tr"
            else if (this.Token.IsStartTagNamed(
                Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Frame, Tags.Head, Tags.TBody, Tags.Td, Tags.TFoot,
                Tags.Th, Tags.THead, Tags.Tr))
            {
                //    Parse error. Ignore the token.
            }

            // Any other start tag
            else if (this.Token.Type == TokenType.StartTag)
            {
                //    Reconstruct the active formatting elements, if any.

                //    Insert an HTML element for the token.

                //    NOTE: This element will be an ordinary element.
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                //    Run these steps:
                //        1. Initialize node to be the current node (the bottommost node of the stack).
                //        2. Loop: If node is an HTML element with the same tag name as the token, then:
                //            1. Generate implied end tags, except for HTML elements with the same tag name as the token.
                //            2. If node is not the current node, then this is a parse error.
                //            3. Pop all the nodes from the current node up to node, including node, then stop these steps.
                //        3. Otherwise, if node is in the special category, then this is a parse error; ignore the token, and abort these steps.
                //        4. Set node to the previous entry in the stack of open elements.
                //        5. Return to the step labeled loop.
            }

            /*
            

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

        private void ClosePElement()
        {
            // When the steps above say the user agent is to close a p element, it means that the
            // user agent must run the following steps:

            // 1. Generate implied end tags, except for p elements.
            this.GenerateImpliedEndTag(Tags.P);

            // 2. If the current node is not a p element, then this is a parse error.
            if (!this.IsCurrentNode(Tags.P))
                this.ParseError(Parsing.ParseError.UnexpectedTag);

            // 3. Pop elements from the stack of open elements until a p element has been popped from the stack.
            while (this.OpenElements.Pop().TagName != Tags.P)
            {
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInTextMode()
        {
            /*
            8.2.5.4.8 The "text" insertion mode

            When the user agent is to apply the rules for the "text" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token
            if (this.Token.Type == TokenType.Character)
            {
                //    Insert the token's character.

                //    NOTE: This can never be a U+0000 NULL character; the tokenizer converts those
                //    to U+FFFD REPLACEMENT CHARACTER characters.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Parse error.

                //    If the current node is a script element, mark the script element as "already started".

                //    Pop the current node off the stack of open elements.

                //    Switch the insertion mode to the original insertion mode and reprocess the token.
            }

            // An end tag whose tag name is "script"
            else if (this.Token.IsEndTagNamed(Tags.Script))
            {
                //    Perform a microtask checkpoint.

                //    Provide a stable state.

                //    Let script be the current node (which will be a script element).

                //    Pop the current node off the stack of open elements.

                //    Switch the insertion mode to the original insertion mode.

                //    Let the old insertion point have the same value as the current insertion point. Let the insertion
                //    point be just before the next input character.

                //    Increment the parser's script nesting level by one.

                //    Prepare the script. This might cause some script to execute, which might cause new characters to be
                //    inserted into the tokenizer, and might cause the tokenizer to output more tokens, resulting in a
                //    reentrant invocation of the parser.

                //    Decrement the parser's script nesting level by one. If the parser's script nesting level is zero,
                //    then set the parser pause flag to false.

                //    Let the insertion point have the value of the old insertion point. (In other words, restore the insertion
                //    point to its previous value. This value might be the "undefined" value.)

                //    At this stage, if there is a pending parsing-blocking script, then:
                //        - If the script nesting level is not zero:
                //            Set the parser pause flag to true, and abort the processing of any nested invocations
                //            of the tokenizer, yielding control back to the caller. (Tokenization will resume when
                //            the caller returns to the "outer" tree construction stage.)

                //            NOTE: The tree construction stage of this particular parser is being called reentrantly,
                //            say from a call to document.write().

                //        - Otherwise:
                //            Run these steps:
                //                1. Let the script be the pending parsing-blocking script. There is no
                //                   longer a pending parsing-blocking script.
                //                2. Block the tokenizer for this instance of the HTML parser, such that the event
                //                   loop will not run tasks that invoke the tokenizer.
                //                3. If the parser's Document has a style sheet that is blocking scripts or the script's
                //                   "ready to be parser-executed" flag is not set: spin the event loop until the parser's
                //                    Document has no style sheet that is blocking scripts and the script's "ready to be
                //                    parser-executed" flag is set.
                //                4. If this parser has been aborted in the meantime, abort these steps.
                //                    NOTE: This could happen if, e.g., while the spin the event loop algorithm is running,
                //                    the browsing context gets closed, or the document.open() method gets invoked on the Document.
                //                5. Unblock the tokenizer for this instance of the HTML parser, such that tasks that invoke the
                //                   tokenizer can again be run.
                //                6. Let the insertion point be just before the next input character.
                //                7. Increment the parser's script nesting level by one (it should be zero before this step,
                //                   so this sets it to one).
                //                8. Execute the script.
                //                9. Decrement the parser's script nesting level by one. If the parser's script nesting level is
                //                   zero (which it always should be at this point), then set the parser pause flag to false.
                //                10. Let the insertion point be undefined again.
                //                11. If there is once again a pending parsing-blocking script, then repeat these steps from step 1.
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                //    Pop the current node off the stack of open elements.

                //    Switch the insertion mode to the original insertion mode.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTableMode()
        {
            /*
            8.2.5.4.9 The "in table" insertion mode

            When the user agent is to apply the rules for the "in table" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token, if the current node is table, tbody, tfoot, thead, or tr element
            if ((this.Token.Type == TokenType.Character) && this.IsCurrentNode(Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr))
            {
                //    Let the pending table character tokens be an empty list of tokens.

                //    Let the original insertion mode be the current insertion mode.

                //    Switch the insertion mode to "in table text" and reprocess the token.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment.
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "caption"
            else if (this.Token.IsStartTagNamed(Tags.Caption))
            {
                //    Clear the stack back to a table context. (See below.)

                //    Insert a marker at the end of the list of active formatting elements.

                //    Insert an HTML element for the token, then switch the insertion mode to "in caption".
            }

            // A start tag whose tag name is "colgroup"
            else if (this.Token.IsStartTagNamed(Tags.ColGroup))
            {
                //    Clear the stack back to a table context. (See below.)

                //    Insert an HTML element for the token, then switch the insertion mode to "in column group".
            }

            // A start tag whose tag name is "col"
            else if (this.Token.IsStartTagNamed(Tags.Col))
            {
                //    Clear the stack back to a table context. (See below.)

                //    Insert an HTML element for a "colgroup" start tag token with no attributes, then switch the
                //    insertion mode to "in column group".

                //    Reprocess the current token.
            }

            // A start tag whose tag name is one of: "tbody", "tfoot", "thead"
            else if (this.Token.IsStartTagNamed(Tags.TBody, Tags.TFoot, Tags.THead))
            {
                //    Clear the stack back to a table context. (See below.)

                //    Insert an HTML element for the token, then switch the insertion mode to "in table body".
            }

            // A start tag whose tag name is one of: "td", "th", "tr"
            else if (this.Token.IsStartTagNamed(Tags.Td, Tags.Th, Tags.Tr))
            {
                //    Clear the stack back to a table context. (See below.)

                //    Insert an HTML element for a "tbody" start tag token with no attributes,
                //    then switch the insertion mode to "in table body".

                //    Reprocess the current token.
            }

            // A start tag whose tag name is "table"
            else if (this.Token.IsStartTagNamed(Tags.Table))
            {
                //    Parse error.

                //    If the stack of open elements does not have a table element in table scope, ignore the token.

                //    Otherwise:
                //        Pop elements from this stack until a table element has been popped from the stack.

                //        Reset the insertion mode appropriately.

                //        Reprocess the token.
            }

            // An end tag whose tag name is "table"
            else if (this.Token.IsEndTagNamed(Tags.Table))
            {
                //    If the stack of open elements does not have a table element in table scope, this is a parse error;
                //    ignore the token.

                //    Otherwise:
                //        Pop elements from this stack until a table element has been popped from the stack.

                //        Reset the insertion mode appropriately.
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "tbody", "td",
            // "tfoot", "th", "thead", "tr"
            else if (this.Token.IsEndTagNamed(
                Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html, Tags.TBody, Tags.Td,
                Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is one of: "style", "script", "template"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Style, Tags.Script, Tags.Template) || this.Token.IsEndTagNamed(Tags.Template))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // A start tag whose tag name is "input"
            else if (this.Token.IsStartTagNamed(Tags.Input))
            {
                //    If the token does not have an attribute with the name "type", or if it does, but that attribute's
                //    value is not an ASCII case-insensitive match for the string "hidden", then: act as described in the
                //    "anything else" entry below.

                //    Otherwise:
                //        Parse error.

                //        Insert an HTML element for the token.

                //        Pop that input element off the stack of open elements.

                //        Acknowledge the token's self-closing flag, if it is set.
            }

            // A start tag whose tag name is "form"
            else if (this.Token.IsStartTagNamed(Tags.Form))
            {
                //    Parse error.

                //    If there is a template element on the stack of open elements, or if the form element pointer
                //    is not null, ignore the token.

                //    Otherwise:
                //        Insert an HTML element for the token, and set the form element pointer to point to the element created.

                //        Pop that form element off the stack of open elements.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // Anything else
            else
            {
                //    Parse error. Enable foster parenting, process the token using the rules for the "in body"
                //    insertion mode, and then disable foster parenting.
            }

            /*
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
            */

            // A character token that is U+0000 NULL
            if (this.Token.IsCharacterNull())
            {
                //    Parse error. Ignore the token.
            }

            // Any other character token
            else if (this.Token.Type == TokenType.Character)
            {
                //    Append the character token to the pending table character tokens list.
            }

            // Anything else
            else
            {
                //    If any of the tokens in the pending table character tokens list are character
                //    tokens that are not space characters, then reprocess the character tokens in the
                //    pending table character tokens list using the rules given in the "anything else"
                //    entry in the "in table" insertion mode.

                //    Otherwise, insert the characters given by the pending table character tokens list.

                //    Switch the insertion mode to the original insertion mode and reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInCaptionMode()
        {
            /*
            8.2.5.4.11 The "in caption" insertion mode

            When the user agent is to apply the rules for the "in caption" insertion mode,
            the user agent must handle the token as follows:
            */

            // An end tag whose tag name is "caption"
            if (this.Token.IsEndTagNamed(Tags.Caption))
            {
                //    If the stack of open elements does not have a caption element in table scope,
                //    this is a parse error; ignore the token. (fragment case)

                //    Otherwise:
                //        Generate implied end tags.

                //        Now, if the current node is not a caption element, then this is a parse error.

                //        Pop elements from this stack until a caption element has been popped from the stack.

                //        Clear the list of active formatting elements up to the last marker.

                //        Switch the insertion mode to "in table".
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"
            // An end tag whose tag name is "table"
            else if (
                this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr) ||
                this.Token.IsEndTagNamed(Tags.Table))
            {
                //    Parse error.

                //    If the stack of open elements does not have a caption element in table scope, ignore the token. (fragment case)

                //    Otherwise:
                //        Pop elements from this stack until a caption element has been popped from the stack.

                //        Clear the list of active formatting elements up to the last marker.

                //        Switch the insertion mode to "in table".

                //        Reprocess the token.
            }

            // An end tag whose tag name is one of: "body", "col", "colgroup", "html", "tbody", "td", "tfoot", "th", "thead", "tr"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Col, Tags.ColGroup, Tags.Html, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
            {
                //    Parse error. Ignore the token.
            }

            // Anything else
            else
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInColumnGroupMode()
        {
            /*
            8.2.5.4.12 The "in column group" insertion mode

            When the user agent is to apply the rules for the "in column group" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                //    Insert the character.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment.
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // A start tag whose tag name is "col"
            else if (this.Token.IsStartTagNamed(Tags.Col))
            {
                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.
            }

            // An end tag whose tag name is "colgroup"
            else if (this.Token.IsEndTagNamed(Tags.ColGroup))
            {
                //    If the current node is not a colgroup element, then this is a parse error; ignore the token.

                //    Otherwise, pop the current node from the stack of open elements. Switch the insertion mode to "in table".
            }

            // An end tag whose tag name is "col"
            else if (this.Token.IsEndTagNamed(Tags.Col))
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "template"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Template) || this.Token.IsEndTagNamed(Tags.Template))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // Anything else
            else
            {
                //    If the current node is not a colgroup element, then this is a parse error; ignore the token.

                //    Otherwise, pop the current node from the stack of open elements.

                //    Switch the insertion mode to "in table".

                //    Reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTableBodyMode()
        {
            /*
            8.2.5.4.13 The "in table body" insertion mode

            When the user agent is to apply the rules for the "in table body" insertion mode,
            the user agent must handle the token as follows:
            */

            // A start tag whose tag name is "tr"
            if (this.Token.IsStartTagNamed(Tags.Tr))
            {
                //    Clear the stack back to a table body context. (See below.)

                //    Insert an HTML element for the token, then switch the insertion mode to "in row".
            }

            // A start tag whose tag name is one of: "th", "td"
            else if (this.Token.IsStartTagNamed(Tags.Th, Tags.Td))
            {
                //    Parse error.

                //    Clear the stack back to a table body context. (See below.)

                //    Insert an HTML element for a "tr" start tag token with no attributes, then switch the insertion mode to "in row".

                //    Reprocess the current token.
            }

            // An end tag whose tag name is one of: "tbody", "tfoot", "thead"
            else if (this.Token.IsEndTagNamed(Tags.TBody, Tags.TFoot, Tags.THead))
            {
                //    If the stack of open elements does not have an element in table scope that is an HTML element
                //    and with the same tag name as the token, this is a parse error; ignore the token.

                //    Otherwise:
                //        Clear the stack back to a table body context. (See below.)

                //        Pop the current node from the stack of open elements. Switch the insertion mode to "in table".
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "tfoot", "thead"
            // An end tag whose tag name is "table"
            else if (
                this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.TFoot, Tags.THead) ||
                this.Token.IsEndTagNamed(Tags.Table))
            {
                //    If the stack of open elements does not have a tbody, thead, or tfoot element in table scope,
                //    this is a parse error; ignore the token.

                //    Otherwise:
                //        Clear the stack back to a table body context. (See below.)

                //        Pop the current node from the stack of open elements. Switch the insertion mode to "in table".

                //        Reprocess the token.
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "td", "th", "tr"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html, Tags.Td, Tags.Th, Tags.Tr))
            {
                //    Parse error. Ignore the token.
            }

            // Anything else
            else
            {
                //    Process the token using the rules for the "in table" insertion mode.

            }

            /*
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
            */

            // A start tag whose tag name is one of: "th", "td"
            if (this.Token.IsStartTagNamed(Tags.Th, Tags.Td))
            {
                //    Clear the stack back to a table row context. (See below.)

                //    Insert an HTML element for the token, then switch the insertion mode to "in cell".

                //    Insert a marker at the end of the list of active formatting elements.
            }

            // An end tag whose tag name is "tr"
            else if (this.Token.IsEndTagNamed(Tags.Tr))
            {
                //    If the stack of open elements does not have a tr element in table scope, this is a parse error; ignore the token.

                //    Otherwise:
                //        Clear the stack back to a table row context. (See below.)

                //        Pop the current node (which will be a tr element) from the stack of open elements. Switch the insertion mode to "in table body".
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "tfoot", "thead", "tr"
            // An end tag whose tag name is "table"
            else if (
                this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr) ||
                this.Token.IsEndTagNamed(Tags.Table))
            {
                //    If the stack of open elements does not have a tr element in table scope, this is a parse error; ignore the token.

                //    Otherwise:
                //        Clear the stack back to a table row context. (See below.)

                //        Pop the current node (which will be a tr element) from the stack of open elements. Switch the insertion mode to "in table body".

                //        Reprocess the token.
            }

            // An end tag whose tag name is one of: "tbody", "tfoot", "thead"
            else if (this.Token.IsEndTagNamed(Tags.TBody, Tags.TFoot, Tags.THead))
            {
                //    If the stack of open elements does not have an element in table scope that is an HTML element
                //    and with the same tag name as the token, this is a parse error; ignore the token.

                //    If the stack of open elements does not have a tr element in table scope, ignore the token.

                //    Otherwise:
                //        Clear the stack back to a table row context. (See below.)

                //        Pop the current node (which will be a tr element) from the stack of open elements. Switch the insertion mode to "in table body".

                //        Reprocess the token.
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "td", "th"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html, Tags.Td, Tags.Th))
            {
                //    Parse error. Ignore the token.
            }

            // Anything else
            else
            {
                //    Process the token using the rules for the "in table" insertion mode.

            }

            /*
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
            */

            // An end tag whose tag name is one of: "td", "th"
            if (this.Token.IsEndTagNamed(Tags.Td, Tags.Th))
            {
                //    If the stack of open elements does not have an element in table scope that is an HTML element and
                //    with the same tag name as that of the token, then this is a parse error; ignore the token.

                //    Otherwise:
                //        Generate implied end tags.

                //        Now, if the current node is not an HTML element with the same tag name as the token,
                //        then this is a parse error.

                //        Pop elements from the stack of open elements stack until an HTML element with the same
                //        tag name as the token has been popped from the stack.

                //        Clear the list of active formatting elements up to the last marker.

                //        Switch the insertion mode to "in row".
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"
            else if (this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
            {
                //    If the stack of open elements does not have a td or th element in table scope, then this is a parse error;
                //    ignore the token. (fragment case)

                //    Otherwise, close the cell (see below) and reprocess the token.
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html))
            {
                //    Parse error. Ignore the token.
            }

            // An end tag whose tag name is one of: "table", "tbody", "tfoot", "thead", "tr"
            else if (this.Token.IsEndTagNamed(Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr))
            {
                //    If the stack of open elements does not have an element in table scope that is an HTML element
                //    and with the same tag name as that of the token, then this is a parse error; ignore the token.

                //    Otherwise, close the cell (see below) and reprocess the token.
            }

            // Anything else
            else
            {
                //    Process the token using the rules for the "in body" insertion mode.


            }

            /*
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
            */

            // A character token that is U+0000 NULL
            if (this.Token.IsCharacterNull())
            {
                //    Parse error. Ignore the token.
            }

            // Any other character token
            else if (this.Token.Type == TokenType.Character)
            {
                //    Insert the token's character.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment.
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // A start tag whose tag name is "option"
            else if (this.Token.IsStartTagNamed(Tags.Option))
            {
                //    If the current node is an option element, pop that node from the stack of open elements.

                //    Insert an HTML element for the token.
            }

            // A start tag whose tag name is "optgroup"
            else if (this.Token.IsStartTagNamed(Tags.OptGroup))
            {
                //    If the current node is an option element, pop that node from the stack of open elements.

                //    If the current node is an optgroup element, pop that node from the stack of open elements.

                //    Insert an HTML element for the token.
            }

            // An end tag whose tag name is "optgroup"
            else if (this.Token.IsEndTagNamed(Tags.OptGroup))
            {
                //    First, if the current node is an option element, and the node immediately before it in the stack
                //    of open elements is an optgroup element, then pop the current node from the stack of open elements.

                //    If the current node is an optgroup element, then pop that node from the stack of open elements.
                //    Otherwise, this is a parse error; ignore the token.
            }

            // An end tag whose tag name is "option"
            else if (this.Token.IsEndTagNamed(Tags.Option))
            {
                //    If the current node is an option element, then pop that node from the stack of open elements.
                //    Otherwise, this is a parse error; ignore the token.
            }

            // An end tag whose tag name is "select"
            else if (this.Token.IsEndTagNamed(Tags.Select))
            {
                //    If the stack of open elements does not have a select element in select scope, this is a parse error;
                //    ignore the token. (fragment case)

                //    Otherwise:
                //        Pop elements from the stack of open elements until a select element has been popped from the stack.

                //        Reset the insertion mode appropriately.
            }

            // A start tag whose tag name is "select"
            else if (this.Token.IsStartTagNamed(Tags.Select))
            {
                //    Parse error.

                //    Pop elements from the stack of open elements until a select element has been popped from the stack.

                //    Reset the insertion mode appropriately.

                //    It just gets treated like an end tag.
            }

            // A start tag whose tag name is one of: "input", "keygen", "textarea"
            else if (this.Token.IsStartTagNamed(Tags.Input, Tags.KeyGen, Tags.TextArea))
            {
                //    Parse error.

                //    If the stack of open elements does not have a select element in select scope, ignore the token. (fragment case)

                //    Pop elements from the stack of open elements until a select element has been popped from the stack.

                //    Reset the insertion mode appropriately.

                //    Reprocess the token.
            }

            // A start tag whose tag name is one of: "script", "template"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Script, Tags.Template) || this.Token.IsEndTagNamed(Tags.Template))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // Anything else
            else
            {
                //    Parse error. Ignore the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInSelectInTableMode()
        {
            /*
            8.2.5.4.17 The "in select in table" insertion mode

            When the user agent is to apply the rules for the "in select in table" insertion mode,
            the user agent must handle the token as follows:
            */

            // A start tag whose tag name is one of: "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"
            if (this.Token.IsStartTagNamed(Tags.Caption, Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr, Tags.Td, Tags.Th))
            {
                //    Parse error.

                //    Pop elements from the stack of open elements until a select element has been popped from the stack.

                //    Reset the insertion mode appropriately.

                //    Reprocess the token.
            }

            // An end tag whose tag name is one of: "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"
            else if (this.Token.IsEndTagNamed(Tags.Caption, Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr, Tags.Td, Tags.Th))
            {
                //    Parse error.

                //    If the stack of open elements does not have an element in table scope that is an HTML element
                //    and with the same tag name as that of the token, then ignore the token.

                //    Otherwise:
                //        Pop elements from the stack of open elements until a select element has been popped from the stack.

                //        Reset the insertion mode appropriately.

                //        Reprocess the token.
            }

            // Anything else
            else
            {
                //    Process the token using the rules for the "in select" insertion mode.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTemplateMode()
        {
            /*
            8.2.5.4.18 The "in template" insertion mode

            When the user agent is to apply the rules for the "in template" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token
            // A comment token
            // A DOCTYPE token
            if ((this.Token.Type == TokenType.Character) || (this.Token.Type == TokenType.Comment) || (this.Token.Type == TokenType.DocType))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta", "noframes", "script",
            // "style", "template", "title"
            // An end tag whose tag name is "template"
            else if (
                this.Token.IsStartTagNamed(
                    Tags.Base, Tags.BaseFont, Tags.BgSound, Tags.Link, Tags.Meta, Tags.NoFrames, Tags.Script, 
                    Tags.Style, Tags.Template, Tags.Title) ||
                this.Token.IsEndTagNamed(Tags.Template))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // A start tag whose tag name is one of: "caption", "colgroup", "tbody", "tfoot", "thead"
            else if (this.Token.IsStartTagNamed(Tags.Caption, Tags.ColGroup, Tags.TBody, Tags.TFoot, Tags.THead))
            {
                //    Pop the current template insertion mode off the stack of template insertion modes.

                //    Push "in table" onto the stack of template insertion modes so that it is the new current template insertion mode.

                //    Switch the insertion mode to "in table", and reprocess the token.
            }

            // A start tag whose tag name is "col"
            else if (this.Token.IsStartTagNamed(Tags.Col))
            {
                //    Pop the current template insertion mode off the stack of template insertion modes.

                //    Push "in column group" onto the stack of template insertion modes so that it is the new
                //    current template insertion mode.

                //    Switch the insertion mode to "in column group", and reprocess the token.
            }

            // A start tag whose tag name is "tr"
            else if (this.Token.IsStartTagNamed(Tags.Tr))
            {
                //    Pop the current template insertion mode off the stack of template insertion modes.

                //    Push "in table body" onto the stack of template insertion modes so that it is the new
                //    current template insertion mode.

                //    Switch the insertion mode to "in table body", and reprocess the token.
            }

            // A start tag whose tag name is one of: "td", "th"
            else if (this.Token.IsStartTagNamed(Tags.Td, Tags.Th))
            {
                //    Pop the current template insertion mode off the stack of template insertion modes.

                //    Push "in row" onto the stack of template insertion modes so that it is the new current template insertion mode.

                //    Switch the insertion mode to "in row", and reprocess the token.
            }

            // Any other start tag
            else if (this.Token.Type == TokenType.StartTag)
            {
                //    Pop the current template insertion mode off the stack of template insertion modes.

                //    Push "in body" onto the stack of template insertion modes so that it is the new current template insertion mode.

                //    Switch the insertion mode to "in body", and reprocess the token.
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                //    Parse error. Ignore the token.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    If there is no template element on the stack of open elements, then stop parsing. (fragment case)

                //    Otherwise, this is a parse error.

                //    Pop elements from the stack of open elements until a template element has been popped from the stack.

                //    Clear the list of active formatting elements up to the last marker.

                //    Pop the current template insertion mode off the stack of template insertion modes.

                //    Reset the insertion mode appropriately.

                //    Reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterBodyMode()
        {
            /*
            8.2.5.4.19 The "after body" insertion mode

            When the user agent is to apply the rules for the "after body" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment as the last child of the first element in the stack of open elements (the html element).
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // An end tag whose tag name is "html"
            else if (this.Token.IsEndTagNamed(Tags.Html))
            {
                //    If the parser was originally created as part of the HTML fragment parsing algorithm,
                //    this is a parse error; ignore the token. (fragment case)

                //    Otherwise, switch the insertion mode to "after after body".
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Stop parsing.
            }

            // Anything else
            else
            {
                //    Parse error. Switch the insertion mode to "in body" and reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInFramesetMode()
        {
            /*
            8.2.5.4.20 The "in frameset" insertion mode

            When the user agent is to apply the rules for the "in frameset" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                //    Insert the character.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment.
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // A start tag whose tag name is "frameset"
            else if (this.Token.IsStartTagNamed(Tags.Frameset))
            {
                //    Insert an HTML element for the token.
            }

            // An end tag whose tag name is "frameset"
            else if (this.Token.IsEndTagNamed(Tags.Frameset))
            {
                //    If the current node is the root html element, then this is a parse error; ignore the token. (fragment case)

                //    Otherwise, pop the current node from the stack of open elements.

                //    If the parser was not originally created as part of the HTML fragment parsing algorithm (fragment case),
                //    and the current node is no longer a frameset element, then switch the insertion mode to "after frameset".
            }

            // A start tag whose tag name is "frame"
            else if (this.Token.IsStartTagNamed(Tags.Frame))
            {
                //    Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.

                //    Acknowledge the token's self-closing flag, if it is set.
            }

            // A start tag whose tag name is "noframes"
            else if (this.Token.IsStartTagNamed(Tags.NoFrames))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    If the current node is not the root html element, then this is a parse error.

                //    The current node can only be the root html element in the fragment case.

                //    Stop parsing.
            }

            // Anything else
            else
            {
                //    Parse error. Ignore the token.
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterFramesetMode()
        {
            /*
            8.2.5.4.21 The "after frameset" insertion mode

            When the user agent is to apply the rules for the "after frameset" insertion mode,
            the user agent must handle the token as follows:
            */

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            if (this.Token.IsCharacterWhitespace())
            {
                //    Insert the character.
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment.
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                //    Parse error. Ignore the token.
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // An end tag whose tag name is "html"
            else if (this.Token.IsEndTagNamed(Tags.Html))
            {
                //    Switch the insertion mode to "after after frameset".
            }

            // A start tag whose tag name is "noframes"
            else if (this.Token.IsStartTagNamed(Tags.NoFrames))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Stop parsing.
            }

            // Anything else
            else
            {
                //    Parse error. Ignore the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterAfterBodyMode()
        {
            /*
            8.2.5.4.22 The "after after body" insertion mode

            When the user agent is to apply the rules for the "after after body" insertion mode,
            the user agent must handle the token as follows:
            */

            // A comment token
            if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment as the last child of the Document object.
            }

            // A DOCTYPE token
            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            // A start tag whose tag name is "html"
            else if ((this.Token.Type == TokenType.DocType) || this.Token.IsCharacterWhitespace() || this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Stop parsing.
            }

            // Anything else
            else
            {
                //    Parse error. Switch the insertion mode to "in body" and reprocess the token.
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInAfterAfterFramesetMode()
        {
            /*
            8.2.5.4.23 The "after after frameset" insertion mode

            When the user agent is to apply the rules for the "after after frameset" insertion mode,
            the user agent must handle the token as follows:
            */

            // A comment token
            if (this.Token.Type == TokenType.Comment)
            {
                //    Insert a comment as the last child of the Document object.
            }

            // A DOCTYPE token
            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            // A start tag whose tag name is "html"
            else if ((this.Token.Type == TokenType.DocType) || this.Token.IsCharacterWhitespace() || this.Token.IsStartTagNamed(Tags.Html))
            {
                //    Process the token using the rules for the "in body" insertion mode.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                //    Stop parsing.
            }

            // A start tag whose tag name is "noframes"
            else if (this.Token.IsStartTagNamed(Tags.NoFrames))
            {
                //    Process the token using the rules for the "in head" insertion mode.
            }

            // Anything else
            else
            {
                //    Parse error. Ignore the token.
            }
        }

        #endregion

        private enum ParsingAlgorithm
        {
            GenericRcData,
            GenericRawText
        }

        private void ParseElement(ParsingAlgorithm algorithm)
        {
        }

        #endregion

        #region 8.2.6 The End

        /// <summary>
        /// The user agent stops parsing the document.
        /// See: http://www.w3.org/TR/html51/syntax.html#stopped
        /// </summary>
        private void StopParsing()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

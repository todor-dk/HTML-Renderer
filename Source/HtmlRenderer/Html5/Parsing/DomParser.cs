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

        private readonly Tokenizer Tokenizer;

        public void ParseDocument(ParsingContext parsingContext, HtmlStream html)
        {
            Contract.RequiresNotNull(parsingContext, nameof(parsingContext));
            Contract.RequiresNotNull(html, nameof(html));

            DomParser parser = new Parsing.DomParser(parsingContext, html);
            parser.Parse();
        }

        private DomParser(ParsingContext parsingContext, HtmlStream html)
        {
            Contract.RequiresNotNull(parsingContext, nameof(parsingContext));

            this.ActiveFormattingElements = new ActiveFormattingElementsList(this);
            this.ParsingContext = parsingContext;
            this.DomFactory = parsingContext.GetDomFactory();
            this.Tokenizer = new Tokenizer(html);
            this.Tokenizer.ParseError += (s, e) => this.InformParseError(e.ParseError);
        }

        #region Tokanization

        public void Parse()
        {
            do
            {
                // Do we have a stored "next-token"?
                if (this.NextToken.Type != TokenType.Unknown)
                {
                    // Use the stored "next-token"
                    this.Token = this.NextToken;

                    // ... and mark it as "used"
                    this.NextToken.ResetToken();
                }
                else
                {
                    // Get the token from the tokenizer.
                    this.Token = this.Tokenizer.GetNextToken();
                }

                this.DispatchToken();
            }
            while (this.Token.Type != TokenType.EndOfFile);
        }

        #endregion

        #region 8.2.3 Parse State

        #region 8.2.3.1. The insertion mode. See: http://www.w3.org/TR/html51/syntax.html#the-insertion-mode

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

        /// <summary>
        /// The insertion mode is a state variable that controls the primary operation of the tree construction stage.
        /// Initially, the insertion mode is "initial". The insertion mode affects how tokens are processed
        /// and whether CDATA sections are supported.
        /// </summary>
        private InsertionModeEnum InsertionMode = InsertionModeEnum.Initial;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Switch(InsertionModeEnum mode)
        {
            this.InsertionMode = mode;
        }

        private void ProcessTokenUsing(InsertionModeEnum mode)
        {
            // When the algorithm below says that the user agent is to do something "using the rules
            // for the m insertion mode", where m is one of these modes, the user agent must use the
            // rules described under the m insertion mode's section, but must leave the insertion mode
            // unchanged unless the rules in m themselves switch the insertion mode to a new value.

            // NB: We just call the ProcessToken with the given "mode" instead of the current insertion
            // mode and let it dispatch to the relevant handler for that insertion mode.
            this.ProcessToken(mode);
        }

        /// <summary>
        /// When the insertion mode is switched to "text" or "in table text",the original insertion mode
        /// is also set. This is the insertion mode to which the tree construction stage will return.
        /// NB: Setting the orig. mode is done explicitely by the handler logic in each step.
        /// </summary>
        private InsertionModeEnum OriginalInsertionMode = InsertionModeEnum.Initial;

        /// <summary>
        /// To parse nested template elements, a stack of template insertion modes is used. It is initially empty
        /// </summary>
        /// <remarks>
        /// The current template insertion mode is the insertion mode that was most recently added to the stack
        /// of template insertion modes. The algorithms in the sections below will push insertion modes onto this
        /// stack, meaning that the specified insertion mode is to be added to the stack, and pop insertion modes
        /// from the stack, which means that the most recently added insertion mode must be removed from the stack.
        /// </remarks>
        private readonly Stack<InsertionModeEnum> TemplateInsertionMode = new Stack<InsertionModeEnum>();

        private void ResetInsertionModeAppropriately()
        {
            // When the steps below require the user agent to reset the insertion mode appropriately,
            // it means the user agent must follow these steps:

            // 1. Let last be false.
            bool last = false;

            // 2. Let node be the last node in the stack of open elements.
            int i = this.OpenElements.Count - 1;
            Element node = this.OpenElements[i];

            // Loop:
            do
            {
                // 3. Loop: If node is the first node in the stack of open elements, then set last to true, and, if the parser
                // was originally created as part of the HTML fragment parsing algorithm (fragment case), set node to the context
                // element passed to that algorithm.
                if (i == 0)
                {
                    last = true;
                    if (this.ParsingContext.IsFragmentParsing)
                        node = this.ParsingContext.FragmentContextElement;
                }

                // 4. If node is a select element, run these substeps:
                if (node.Is(Tags.Select))
                {
                    // 1. If last is true, jump to the step below labeled done.
                    if (!last)
                    {
                        // 2. Let ancestor be node.
                        Element ancestor = node;

                        do
                        {
                            // 3. Loop: If ancestor is the first node in the stack of open elements, jump to the step below labeled done.
                            if (ancestor == this.OpenElements[0])
                                break;

                            // 4. Let ancestor be the node before ancestor in the stack of open elements.
                            i--;
                            ancestor = this.OpenElements[i];

                            // 5. If ancestor is a template node, jump to the step below labeled done.
                            if (ancestor.Is(Tags.Template))
                                break;

                            // 6. If ancestor is a table node, switch the insertion mode to "in select in table" and abort these steps.
                            if (ancestor.Is(Tags.Table))
                            {
                                this.Switch(InsertionModeEnum.InSelectInTable);
                                return;
                            }

                            // 7. Jump back to the step labeled loop.
                        }
                        while (true);
                    }

                    // 8. Done: Switch the insertion mode to "in select" and abort these steps.
                    this.Switch(InsertionModeEnum.InSelect);
                    return;
                }

                // 5. If node is a td or th element and last is false, then switch the insertion mode to "in cell" and abort these steps.
                if (!last && node.Is(Tags.Td, Tags.Th))
                {
                    this.Switch(InsertionModeEnum.InCell);
                    return;
                }

                // 6. If node is a tr element, then switch the insertion mode to "in row" and abort these steps.
                if (node.Is(Tags.Tr))
                {
                    this.Switch(InsertionModeEnum.InRow);
                    return;
                }

                // 7. If node is a tbody, thead, or tfoot element, then switch the insertion mode to "in table body" and abort these steps.
                if (node.Is(Tags.TBody, Tags.THead, Tags.TFoot))
                {
                    this.Switch(InsertionModeEnum.InTableBody);
                    return;
                }

                // 8. If node is a caption element, then switch the insertion mode to "in caption" and abort these steps.
                if (node.Is(Tags.Caption))
                {
                    this.Switch(InsertionModeEnum.InCaption);
                    return;
                }

                // 9. If node is a colgroup element, then switch the insertion mode to "in column group" and abort these steps.
                if (node.Is(Tags.ColGroup))
                {
                    this.Switch(InsertionModeEnum.InColumnGroup);
                    return;
                }

                // 10. If node is a table element, then switch the insertion mode to "in table" and abort these steps.
                if (node.Is(Tags.Table))
                {
                    this.Switch(InsertionModeEnum.InTable);
                    return;
                }

                // 11. If node is a template element, then switch the insertion mode to the current template insertion mode
                // and abort these steps.
                if (node.Is(Tags.Template))
                {
                    this.Switch(this.TemplateInsertionMode.Peek());
                    return;
                }

                // 12. If node is a head element and last is false, then switch the insertion mode to "in head" and abort these steps.
                if (!last && node.Is(Tags.Head))
                {
                    this.Switch(InsertionModeEnum.InHead);
                    return;
                }

                // 13. If node is a body element, then switch the insertion mode to "in body" and abort these steps.
                if (node.Is(Tags.Body))
                {
                    this.Switch(InsertionModeEnum.InBody);
                    return;
                }

                // 14. If node is a frameset element, then switch the insertion mode to "in frameset" and abort these steps. (fragment case)
                if (node.Is(Tags.Frameset))
                {
                    this.Switch(InsertionModeEnum.InFrameset);
                    return;
                }

                // 15. If node is an html element, run these substeps:
                if (node.Is(Tags.Html))
                {
                    // 1. If the head element pointer is null, switch the insertion mode to "before head"
                    // and abort these steps. (fragment case)
                    if (this.Head == null)
                    {
                        this.Switch(InsertionModeEnum.BeforeHead);
                        return;
                    }

                    // 2. Otherwise, the head element pointer is not null, switch the insertion mode to "after head" and abort these steps.
                    this.Switch(InsertionModeEnum.AfterHead);
                    return;
                }

                // 16. If last is true, then switch the insertion mode to "in body" and abort these steps. (fragment case)
                if (last)
                {
                    this.Switch(InsertionModeEnum.InBody);
                    return;
                }

                // 17. Let node now be the node before node in the stack of open elements.
                i--;
                node = this.OpenElements[i];

                // 18. Return to the step labeled loop.
            }
            while (true);
        }

        #endregion

        #region 8.2.3.2. The stack of open elements. See: http://www.w3.org/TR/html51/syntax.html#the-stack-of-open-elements

        /// <summary>
        /// 8.2.3.2. The stack of open elements. See: http://www.w3.org/TR/html51/syntax.html#the-stack-of-open-elements
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
        private readonly OpenElementsStack OpenElements = new OpenElementsStack();

        /// <summary>
        /// Helper class that implements the "stack of open elements".
        /// It exposes often-used helper methods.
        /// NB: Be aware that the stack here grows "downwards" (not upwards).
        /// </summary>
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

            public void RemoveLast(string tagName)
            {
                for (int i = this.Elements.Count - 1; i >= 0; i--)
                {
                    if (this.Elements[i].Is(tagName))
                    {
                        this.Elements.RemoveAt(i);
                        return;
                    }
                }
            }

            public Element Last(string tagName)
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    if (this.Elements[i].Is(tagName))
                        return this.Elements[i];
                }

                return null;
            }

            public void Replace(Element existing, Element replacement)
            {
                Contract.RequiresNotNull(existing, nameof(existing));
                Contract.RequiresNotNull(replacement, nameof(replacement));

                // Do reverse, because chances are the <existing> is near the end.
                for (int i = this.Elements.Count - 1; i >= 0; i--)
                {
                    if (this.Elements[i] == existing)
                    {
                        this.Elements[i] = replacement;
                        return;
                    }
                }
            }

            public bool Any(Func<Element, bool> predicate)
            {
                return this.Elements.Any(predicate);
            }

            public bool Contains(string tagName)
            {
                return this.Elements.Any(elem => elem.Is(tagName));
            }

            public bool Contains(params string[] tagNames)
            {
                if (tagNames == null)
                    return false;
                return this.Elements.Any(elem => elem.Is(tagNames));
            }

            public bool Contains(Element element)
            {
                return this.Elements.Contains(element);
            }

            /// <summary>
            /// Pop elements from the stack of open elements until an
            /// <paramref name="tagName"/> element has been popped from the stack.
            /// </summary>
            /// <returns>
            /// The element matches the given <paramref name="tagName"/>
            /// </returns>
            public Element PopUntil(string tagName)
            {
                while (true)
                {
                    Element elem = this.Pop();
                    if (elem.Is(tagName))
                        return elem;
                }
            }

            /// <summary>
            /// Pop elements from the stack of open elements until an
            /// <paramref name="tagNames"/> element has been popped from the stack.
            /// </summary>
            /// <returns>
            /// The element matches the given <paramref name="tagNames"/>
            /// </returns>
            public Element PopUntil(params string[] tagNames)
            {
                while (true)
                {
                    Element elem = this.Pop();
                    if (elem.Is(tagNames))
                        return elem;
                }
            }

            /// <summary>
            /// Pop elements from the stack of open elements until the given
            /// <paramref name="element"/> has been popped from the stack.
            /// </summary>
            public void PopUntil(Element element)
            {
                Contract.RequiresNotNull(element, nameof(element));

                while (true)
                {
                    Element elem = this.Pop();
                    if (elem == element)
                        return;
                }
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

            public Element ElementAbove(Element element)
            {
                for (int i = this.Count - 1; i > 0; i--)
                {
                    if (this.Elements[i] == element)
                        return this.Elements[i - 1];
                }

                return null;
            }

            public int LastIndexOf(Element element)
            {
                return this.Elements.LastIndexOf(element);
            }

            public void InsertBelow(Element newElement, Element referenceElement)
            {
                Contract.RequiresNotNull(newElement, nameof(newElement));

                int idx = this.LastIndexOf(referenceElement) + 1;
                this.Elements.Insert(idx, newElement);
            }

            private bool HasElementInSpecificScope(Predicate<Element> predicate, params string[] list)
            {
                Contract.RequiresNotNull(predicate, nameof(predicate));

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
                    if (predicate(node))
                        return true;

                    // 3. Otherwise, if node is one of the element types in list, terminate in a failure state.
                    if (node.Is(list))
                        return false;

                    // 4. Otherwise, set node to the previous entry in the stack of open elements and return to step 2.
                    // (This will never fail, since the loop will always terminate in the previous step if the top
                    // of the stack — an html element — is reached.)
                    index--;
                }

                return false;
            }

            private bool HasElementInSpecificScope(string targetNode, params string[] list)
            {
                return this.HasElementInSpecificScope(node => node.Is(targetNode), list);
            }

            public bool HasElementInScope(Predicate<Element> predicate)
            {
                return this.HasElementInSpecificScope(predicate, OpenElementsStack.ScopeTags);
            }

            public bool HasElementInScope(string targetNode)
            {
                return this.HasElementInSpecificScope(targetNode, OpenElementsStack.ScopeTags);
            }

            public bool HasElementInButtonScope(Predicate<Element> predicate)
            {
                return this.HasElementInSpecificScope(predicate, OpenElementsStack.ButtonScopeTags);
            }

            public bool HasElementInButtonScope(string targetNode)
            {
                return this.HasElementInSpecificScope(targetNode, OpenElementsStack.ButtonScopeTags);
            }

            public bool HasElementInListItemScope(Predicate<Element> predicate)
            {
                return this.HasElementInSpecificScope(predicate, OpenElementsStack.ListItemScopeTags);
            }

            public bool HasElementInListItemScope(string targetNode)
            {
                return this.HasElementInSpecificScope(targetNode, OpenElementsStack.ListItemScopeTags);
            }

            public bool HasElementInTableScope(Predicate<Element> predicate)
            {
                return this.HasElementInSpecificScope(predicate, OpenElementsStack.TableScopeTags);
            }

            public bool HasElementInTableScope(string targetNode)
            {
                return this.HasElementInSpecificScope(targetNode, OpenElementsStack.TableScopeTags);
            }

            public bool HasElementInSelectScope(Predicate<Element> predicate)
            {
                return this.HasElementInSpecificScope(predicate, OpenElementsStack.SelectScopeTags);
            }

            public bool HasElementInSelectScope(string targetNode)
            {
                return this.HasElementInSpecificScope(targetNode, OpenElementsStack.SelectScopeTags);
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

            internal static readonly string[] ListItemScopeTags = ScopeTags.FailIfNull().With(Tags.Ol, Tags.Ul);

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

        /// <summary>
        /// The adjusted current node is the context element if the parser was created by the
        /// HTML fragment parsing algorithm and the stack of open elements has only one element
        /// in it (fragment case); otherwise, the adjusted current node is the current node.
        /// </summary>
        private Element AdjustedCurrentNode
        {
            get
            {
                if (this.ParsingContext.IsFragmentParsing && (this.OpenElements.Count == 1))
                    return this.ParsingContext.FragmentContextElement;
                else
                    return this.CurrentNode;
            }
        }

        #endregion

        #region 8.2.3.3. The list of active formatting elements. See http://www.w3.org/TR/html51/syntax.html#the-list-of-active-formatting-elements

        /// <summary>
        /// The list contains elements in the formatting category, and markers.
        /// Initially, the list of active formatting elements is empty.
        /// It is used to handle mis-nested formatting element tags.
        /// </summary>
        private readonly ActiveFormattingElementsList ActiveFormattingElements;

        private class ActiveFormattingElementsList
        {
            private readonly List<Entry> Entries = new List<Entry>();

            private readonly DomParser Parser;

            public ActiveFormattingElementsList(DomParser parser)
            {
                this.Parser = parser;
            }

            private struct Entry
            {
                public static readonly Entry Marker = new Entry();

                private Element _Element;

                public Element Element
                {
                    get
                    {
#if DEBUG
                        if (this._Element == null)
                            throw new InvalidOperationException("This is a marker and Element is not available");
#endif
                        return this._Element;
                    }
                }

                private Token _Token;

                /// <summary>
                /// In addition, each element in the list of active formatting elements is
                /// associated with the token for which it was created, so that further
                /// elements can be created for that token if necessary.
                /// NB: Sometimes alg. require from us to create new elements, but we don't
                /// have access to this token. Then we fake it.
                /// </summary>
                public Token Token
                {
                    get
                    {
#if DEBUG
                        if (this._Element == null)
                            throw new InvalidOperationException("This is a marker and Token is not available");
#endif
                        return this._Token;
                    }
                }

                public bool IsMarker
                {
                    get { return this._Element == null; }
                }

                public Entry(Element element, Token token)
                {
                    this._Element = element;
                    this._Token = token;
                }

                public bool MatchesName(Element element)
                {
                    return (this._Element != null) && this._Element.Is(element);
                }

                public bool MatchesNameAndAttributes(Element element)
                {
                    if ((this._Element == null) || !this._Element.Is(element))
                        return false;

                    if (this._Element.Attributes.Count != element.Attributes.Count)
                        return false;

                    foreach (Attr attr in this._Element.Attributes)
                    {
                        Attr candidate = element.Attributes.GetNamedItemNS(attr.NamespaceUri, attr.LocalName) as Attr;
                        if (candidate == null)
                            return false;

                        if (attr.Value != candidate.Value)
                            return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// Push onto the list of active formatting elements.
            /// </summary>
            public void PushFormattingElement(Element element)
            {
                Contract.RequiresNotNull(element, nameof(element));

                // When the steps below require the user agent to *push onto the list of active formatting elements* an element
                // element, the user agent must perform the following steps:

                // 1. See RunNoahsArk() method.
                this.RunNoahsArk(element);

                // 2. Add element to the list of active formatting elements.
                this.Entries.Add(new Entry(element, this.Parser.Token));
            }

            private void RunNoahsArk(Element element)
            {
                // See: http://www.w3.org/TR/html51/syntax.html#list-of-active-formatting-elements

                // If there are already three elements in the list of active formatting elements after the last marker (if any),
                // or anywhere in the list (if there are no markers) ... that have the same tag name, namespace,
                // and attributes as element ... then remove the earliest such element from the list of active formatting elements.
                //
                // For these purposes, the attributes must be compared as they were when the elements were created by the parser;
                // two elements have the same attributes if all their parsed attributes can be paired such that the two attributes
                // in each pair have identical names, namespaces, and values(the order of the attributes does not matter).

                // NOTE: This is the Noah’s Ark clause.But with three per family instead of two.

                /* EXPLANATION: The above description in the HTML5 standard is shit! So, there is what they actually mean:

                   Let's define the list of active elements. Each element is in this exmaple written a a tag. A marker as '|'.
                   The list grows from left to right.

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <B> <U> <B> <I> <S> | <I> <B> <S> .... --->

                   When matching, the element must match 100% (name, namespace, attributes). For clarity, this example only matches names.

                   Mathcing is performed from the end backwords, i.e. from right to left *up until* the last marker.

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <B> <U> <B> <I> <S> | <I> <B> <S>
                                                                                   |<---matching---

                   If no marker exists, we match until the start of the list (the very left).

                   Let's say that a <B> element is being pushed and the algorythm is looking for equal <B> elements.

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <B> <U> <B> <I> <S> | <I> <B> <S>
                                                                                     ... *** ...

                   There is ONE match. The rule says that there has to be THREE matches for us to take action.
                   This means, we can just append the new <B> to the list. Result is therefor:

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <B> <U> <B> <I> <S> | <I> <B> <S> <B>


                   Now, let's look if the list look differently (shorter version of the above).

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <B> <U> <B> <I> <S>
                                                           *** *** ... *** ... ...

                   Now, there are THREE matches. The algorythm says that we cannot just append to the end without
                   removing an element. It has to be the earliest element (the left most element).
                   Also note, there will never be more than THREE matching elements, or the algorythm is broken.

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <B> <U> <B> <I> <S>
                                                           xxx *** ... *** ... ...

                   The result after the append will look like:

                   <B> <I> <U> <B> | <B> <I> <S> <I> <B> | <B> <U> <B> <I> <S> <B>
                 */

                // First, let's see if there are three elements that match name.
                if (this.Entries.Count < 3)
                    return;

                int matches = 0;
                for (int i = this.Entries.Count - 1; i >= 0; i--)
                {
                    if (this.Entries[i].IsMarker)
                        break;
                    if (this.Entries[i].MatchesName(element))
                        matches++;
                    if (matches == 3)
                        break;
                }

                if (matches < 2)
                    return; // No need to proceed. This will be the case 90% of the time.

                // Second, look for full matches and remove the element if there are three matches.
                matches = 0;
                for (int i = this.Entries.Count - 1; i >= 0; i--)
                {
                    if (this.Entries[i].IsMarker)
                        break;
                    if (this.Entries[i].MatchesNameAndAttributes(element))
                        matches++;

                    if (matches == 3)
                    {
                        // That's the third match. It must be removed, and we are done!
                        this.Entries.RemoveAt(i);
                        return;
                    }
                }
            }

            /// <summary>
            /// Reconstruct the active formatting elements.
            /// </summary>
            public void ReconstructFormattingElements()
            {
                // When the steps below require the user agent to *reconstruct the active formatting elements*, the user agent must
                // perform the following steps:

                // 1. If there are no entries in the list of active formatting elements, then there is nothing to reconstruct;
                // stop this algorithm.
                int i = this.Entries.Count;
                if (i == 0)
                    return;

                // 2. If the last (most recently added) entry in the list of active formatting elements is a marker, or if it is
                // an element that is in the stack of open elements, then there is nothing to reconstruct; stop this algorithm.
                i--;
                Entry entry = this.Entries[i];
                if (entry.IsMarker || this.Parser.OpenElements.Contains(entry.Element))
                    return;

                // 3. Let entry be the last (most recently added) element in the list of active formatting elements.

                // 4. Rewind: If there are no entries before entry in the list of active formatting elements, then jump to
                // the step labeled create.
                Rewind:
                if (i == 0)
                    goto Create;

                // 5. Let entry be the entry one earlier than entry in the list of active formatting elements.
                i--;
                entry = this.Entries[i];

                // 6. If entry is neither a marker nor an element that is also in the stack of open elements,
                // go to the step labeled rewind.
                if (!entry.IsMarker && !this.Parser.OpenElements.Contains(entry.Element))
                    goto Rewind;

                // 7. Advance: Let entry be the element one later than entry in the list of active formatting elements.
                Advance:
                i++;
                entry = this.Entries[i];

                // 8. Create: Insert an HTML element for the token for which the element entry was created,
                // to obtain new element.
                Create:
                Element element = this.Parser.InsertHtmlElement(entry.Token.TagName, entry.Token.TagAttributes);

                // 9. Replace the entry for entry in the list with an entry for new element.
                this.Entries[i] = new Entry(element, entry.Token);

                // 10. If the entry for new element in the list of active formatting elements is not the last entry in the list,
                // return to the step labeled advance.
                if (i != (this.Entries.Count - 1))
                    goto Advance;

                // This has the effect of reopening all the formatting elements that were opened in the current body, cell,
                // or caption(whichever is youngest) that haven’t been explicitly closed.

                // NOTE: The way this specification is written, the list of active formatting elements always consists of elements
                // in chronological order with the least recently added element first and the most recently added element last
                // (except for while steps 7 to 10 of the above algorithm are being executed, of course).
            }

            /// <summary>
            /// Clear the list of active formatting elements up to the last marker.
            /// </summary>
            public void ClearFormattingElementsUpToMarker()
            {
                // When the steps below require the user agent to clear the list of active formatting elements
                // up to the last marker, the user agent must perform the following steps:
                while (true)
                {
                    // 1. Let entry be the last(most recently added) entry in the list of active formatting elements.
                    int i = this.Entries.Count - 1;
                    Entry entry = this.Entries[i];

                    // 2. Remove entry from the list of active formatting elements.
                    this.Entries.RemoveAt(i);

                    // 3. If entry was a marker, then stop the algorithm at this point.
                    // The list has been cleared up to the last marker.
                    if (entry.IsMarker)
                        return;

                    // 4. Go to step 1.
                }
            }

            public Element ElementUpToLastMarker(string tagName)
            {
                for (int i = this.Entries.Count - 1; i >= 0; i--)
                {
                    Entry entry = this.Entries[i];
                    if (entry.IsMarker)
                        return null;
                    if (entry.Element.Is(tagName))
                        return entry.Element;
                }

                return null;
            }

            public bool ContainsUpToLastMarker(string tagName)
            {
                return this.ElementUpToLastMarker(tagName) != null;
            }

            public bool RemoveLast(string tagName)
            {
                for (int i = this.Entries.Count - 1; i >= 0; i--)
                {
                    Entry entry = this.Entries[i];
                    if (!entry.IsMarker && entry.Element.Is(tagName))
                    {
                        this.Entries.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }

            public void InsertMarker()
            {
                this.Entries.Add(Entry.Marker);
            }

            public bool Contains(Element element)
            {
                return this.Entries.Any(entry => !entry.IsMarker && (entry.Element == element));
            }

            public void Remove(Element element)
            {
                this.Entries.RemoveAll(entry => !entry.IsMarker && (entry.Element == element));
            }

            public void Replace(Element existing, Element replacement)
            {
                Contract.RequiresNotNull(existing, nameof(existing));
                Contract.RequiresNotNull(replacement, nameof(replacement));

                // Do reverse, because chances are the <existing> is near the end.
                for (int i = this.Entries.Count - 1; i >= 0; i--)
                {
                    Entry entry = this.Entries[i];
                    if (!entry.IsMarker && (entry.Element == existing))
                    {
                        // ISSUE. Is it OK to take the token from the old entry? Should we also ASSERT the attributes as well?
                        System.Diagnostics.Debug.Assert(entry.Token.TagName == replacement.TagName, "Is it OK to take the token from the old entry if they don't match?");

                        this.Entries[i] = new Entry(replacement, entry.Token);
                        return;
                    }
                }
            }

            public int IndexOf(Element element)
            {
                for (int i = 0; i < this.Entries.Count; i++)
                {
                    Entry entry = this.Entries[i];
                    if (!entry.IsMarker && (entry.Element == element))
                        return i;
                }

                return -1;
            }

            public int LastIndexOf(Element element)
            {
                for (int i = this.Entries.Count - 1; i >= 0; i--)
                {
                    Entry entry = this.Entries[i];
                    if (!entry.IsMarker && (entry.Element == element))
                        return i;
                }

                return -1;
            }

            public void Insert(Element element, Token token, int atIndex)
            {
                Contract.RequiresNotNull(element, nameof(element));

                this.Entries.Insert(atIndex, new Entry(element, token));
            }
        }

        #endregion

        #region 8.2.3.4. The element pointers. See: http://www.w3.org/TR/html51/syntax.html#the-element-pointers

        /// <summary>
        /// Initially, the head element is null. Once a head element has been parsed (whether
        /// implicitly or explicitly) the head element gets set to point to this node.
        /// </summary>
        private Element Head;

        /// <summary>
        /// Initially, the form element is null. The form element points to the last form element
        /// that was opened and whose end tag has not yet been seen. It is used to make form controls
        /// associate with forms in the face of dramatically bad markup, for historical reasons.
        /// It is ignored inside template elements.
        /// </summary>
        private Element Form;

        #endregion

        #region 8.2.3.5. Other parsing state flags. See: http://www.w3.org/TR/html51/syntax.html#other-parsing-state-flags

        /// <summary>
        /// ALWAYS DISABLED. We do not support scripting!
        /// The scripting flag is set to "enabled" if scripting was enabled for the Document with
        /// which the parser is associated when the parser was created, and "disabled" otherwise.
        /// </summary>
        private bool Scripting
        {
            get
            {
                // NOTE: The scripting flag can be enabled even when the parser was originally created for the HTML fragment
                // parsing algorithm, even though script elements don't execute in that case.

                // NOTE: This .Net implementation will probably NEVER implement scripting and therefore this will always be false.
                return false;
            }
        }

        /// <summary>
        /// The frameset-ok flag is set to "ok" when the parser is created.
        /// It is set to "not ok" after certain tokens are seen.
        /// </summary>
        private bool FramesetOk { get; set; } = true;

        #endregion

        #endregion

        #region Tree Construction Helpers

        private void InformParseError(ParseError error)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 8.2.5 Tree construction

        /// <summary>
        /// Contains information about the last token received from the <see cref="Tokenizer"/>.
        /// </summary>
        private Token Token;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DispatchToken()
        {
            // As each token is emitted from the tokenizer, the user agent must follow the appropriate
            // steps from the following list, known as the tree construction dispatcher:

            // Process the token according to the rules given in the section corresponding to the
            // current insertion mode in HTML content, if:

            // If the stack of open elements is empty
            if (this.OpenElements.Count == 0)
                this.ProcessToken();

            // If the adjusted current node is an element in the HTML namespace
            else if (this.AdjustedCurrentNode.IsHtmlElement())
                this.ProcessToken();

            // If the adjusted current node is a MathML text integration point and the token
            // is a start tag whose tag name is neither "mglyph" nor "malignmark"
            else if (this.AdjustedCurrentNode.IsMathMlTextIntegrationPoint() && (this.Token.Type == TokenType.StartTag) && (this.Token.TagName != "mglyph") && (this.Token.TagName != "malignmark"))
                this.ProcessToken();

            // If the adjusted current node is a MathML text integration point and the token is a character token
            else if (this.AdjustedCurrentNode.IsMathMlTextIntegrationPoint() && (this.Token.Type == TokenType.Character))
                this.ProcessToken();

            // If the adjusted current node is an <annotation-xml> element in the MathML namespace and
            // the token is a start tag whose tag name is "svg"
            else if (this.AdjustedCurrentNode.Is("annotation-xml") && (this.Token.Type == TokenType.StartTag) && (this.Token.TagName == "svg"))
                this.ProcessToken();

            // If the adjusted current node is an HTML integration point and the token is a start tag
            else if (this.AdjustedCurrentNode.IsHtmlIntegrationPoint() && (this.Token.Type == TokenType.StartTag))
                this.ProcessToken();

            // If the adjusted current node is an HTML integration point and the token is a character token
            else if (this.AdjustedCurrentNode.IsHtmlIntegrationPoint() && (this.Token.Type == TokenType.Character))
                this.ProcessToken();

            // If the token is an end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
                this.ProcessToken();

            // Otherwise
            // Process the token according to the rules given in the section for parsing tokens in foreign content.
            else
                this.ProcessTokenInForeignContent();
        }

        /// <summary>
        /// The next token is the token that is about to be processed by the tree construction dispatcher
        /// (even if the token is subsequently just ignored). See: http://www.w3.org/TR/html51/syntax.html#next-token
        /// </summary>
        private Token NextToken;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessToken()
        {
            this.ProcessToken(this.InsertionMode);
        }

        private void ProcessToken(InsertionModeEnum mode)
        {
            switch (mode)
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

        #region 8.2.5.1. Creating and inserting nodes

        /// <summary>
        /// While the parser is processing a token, it can enable or disable foster parenting.
        /// This affects the <see cref="AppropriatePlaceForInsertingNode"/> algorithm.
        /// See: http://www.w3.org/TR/html51/syntax.html#foster-parent
        /// </summary>
        private bool FosteringParent = false;

        private AdjustedInsertLocation AppropriatePlaceForInsertingNode(Element overrideTarget = null)
        {
            // See: http://www.w3.org/TR/html51/syntax.html#appropriate-place-for-inserting-a-node

            // The appropriate place for inserting a node, optionally using a particular override target,
            // is the position in an element returned by running the following steps:

            // 1. If there was an override target specified, then let target be the override target.
            // Otherwise, let target be the current node.
            Element target = overrideTarget ?? this.CurrentNode;

            // 2. Determine the adjusted insertion location using the first matching steps from the following list:
            Element adjustedInsertLocationElement = null;
            Element adjustedInsertLocationBeforeSibling = null;

            // * If foster parenting is enabled and target is a <table>, <tbody>, <tfoot>, <thead>, or <tr> element
            if (this.FosteringParent && target.Is(Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr))
            {
                // NOTE: Foster parenting happens when content is misnested in tables.
                // Run these substeps:
                do
                {
                    // 1. Let *last template* be the last <template> element in the stack of open elements, if any.
                    Element lastTemplate = this.OpenElements.Last(Tags.Template);

                    // 2. Let *last table* be the last <table> element in the stack of open elements, if any.
                    Element lastTable = this.OpenElements.Last(Tags.Table);

                    // 3. If there is a *last template* and either there is no *last table*, or there is one, but
                    // *last template* is lower (more recently added) than *last table* in the stack of open elements, then:
                    // NB: "Lower" means more recent, i.e. higher index.
                    if ((lastTemplate != null) && ((lastTable == null) || (this.OpenElements.LastIndexOf(lastTemplate) > this.OpenElements.LastIndexOf(lastTable))))
                    {
                        // let *adjusted insertion location* be inside *last template*’s template contents,
                        // after its last child(if any), and abort these substeps.
                        // ISSUE: Spec says "template contents" ... this is obviously not the <template> itself, or ???
                        adjustedInsertLocationElement = lastTemplate;
                        break;
                    }

                    // 4. If there is no *last table*, then let *adjusted insertion location* be inside the
                    // first element in the stack of open elements (the <html> element), after its last child(if any),
                    // and abort these substeps. (fragment case)
                    if (lastTable == null)
                    {
                        adjustedInsertLocationElement = this.OpenElements[0];
                        break;
                    }

                    // 5. If *last table* has a parent node, then let *adjusted insertion location* be inside
                    // *last table*’s parent node, immediately before *last table*, and abort these substeps.
                    if (lastTable.ParentNode != null)
                    {
                        adjustedInsertLocationElement = (Element)lastTable.ParentNode;
                        adjustedInsertLocationBeforeSibling = lastTable;
                        break;
                    }

                    // 6. Let *previous element* be the element immediately above *last table* in the stack of open elements.
                    Element previousElement = this.OpenElements.ElementAbove(lastTable);

                    // 7. Let *adjusted insertion location* be inside *previous element*, after its last child(if any).
                    adjustedInsertLocationElement = previousElement;
                }
                while (false);

                // NOTE: These steps are involved in part because it’s possible for elements, the table element in this
                // case in particular, to have been moved by a script around in the DOM, or indeed removed from the DOM
                // entirely, after the element was inserted by the parser.

                // NB: Should not happen to use, because we cannot run scripts ... but implement it in case we allow this later.
            }

            // * Otherwise
            else
            {
                // Let *adjusted insertion location* be inside *target*, after its last child (if any).
                adjustedInsertLocationElement = target;
            }

            // 3. If the *adjusted insertion location* is inside a *template* element, let it instead be inside
            // the <template> element’s template contents, after its last child (if any).
            if (adjustedInsertLocationElement.Is(Tags.Template))
                throw new NotImplementedException();

            // 4. Return the *adjusted insertion location*.
            return new AdjustedInsertLocation(adjustedInsertLocationElement, adjustedInsertLocationBeforeSibling);
        }

        public struct AdjustedInsertLocation
        {
            public Element ParentElement { get; private set; }

            /// <summary>
            /// Insert the new node before this sibling element,
            /// or append as the last child if this is not set.
            /// </summary>
            public Element BeforeSibling { get; private set; }

            public AdjustedInsertLocation(Element parentElement, Element beforeSibling)
                : this()
            {
                Contract.RequiresNotNull(parentElement, nameof(parentElement));

                this.ParentElement = parentElement;
                this.BeforeSibling = beforeSibling;
            }
        }

        private Element CreateElement(Element intendedParent, string namespaceUri, string tagName, Html5.Parsing.Attribute[] attributes)
        {
            Contract.RequiresNotNull(intendedParent, nameof(intendedParent));

            // See: http://www.w3.org/TR/html51/syntax.html#create-an-element-for-the-token
            // When the steps below require the user agent to create an element for a token in a particular
            // *given namespace* and with a particular *intended parent*, the user agent must run the following steps:

            // 1. Create a node implementing the interface appropriate for the element type corresponding to the
            // tag name of the token in *given namespace* (as given in the specification that defines that element,
            // e.g., for an a element in the HTML namespace, this specification defines it to be the HTMLAnchorElement
            // interface), with the tag name being the name of that element, with the node being in the given namespace,
            // and with the attributes on the node being those given in the given token.

            // The interface appropriate for an element in the HTML namespace that is not defined in this specification
            // (or other applicable specifications) is HTMLUnknownElement. Elements in other namespaces whose interface
            // is not defined by that namespace’s specification must use the interface Element.

            // The node document of the newly created element must be the node document of the intended parent.
            Element elem = this.DomFactory.CreateElement(intendedParent.OwnerDocument, namespaceUri, tagName, attributes);

            // 2. If the newly created element has an *xmlns* attribute in the *XMLNS namespace* whose value is not
            // exactly the same as the element’s namespace, that is a parse error. Similarly, if the newly created
            // element has an *xmlns:xlink* attribute in the *XMLNS namespace* whose value is not the *XLink namespace*,
            // that is a parse error.
            string attribValue = elem.GetAttribute(Attributes.Xmlns);
            if ((attribValue != null) && (attribValue != elem.NamespaceUri))
                this.InformParseError(ParseError.WrongNamespace);

            attribValue = elem.GetAttribute(Attributes.XmlnsXlink);
            if ((attribValue != null) && (attribValue != Namespaces.Xlink))
                this.InformParseError(ParseError.WrongNamespace);

            // 3. If the newly created element is a resettable element, invoke its reset algorithm. (This initializes the
            // element’s value and checkedness based on the element’s attributes.)
            if (Tags.IsResettable(elem))
                this.DomFactory.InvokeResetAlgorithm(elem);

            // 4. If the element is a form-associated element, and the form element pointer is not null, and there is no
            // template element on the stack of open elements, and the newly created element is either not reassociateable
            // or doesn’t have a form attribute, and the intended parent is in the same home subtree as the element pointed
            // to by the form element pointer, associate the newly created element with the form element pointed to by the
            // form element pointer, and suppress the running of the reset the form owner algorithm when the parser
            // subsequently attempts to insert the element.
            if (Tags.IsFormAssociated(elem) && (this.Form != null) && !this.OpenElements.Contains(Tags.Template)
                && (!Tags.IsReassociateable(elem) || (elem.GetAttribute(Attributes.Form) == null))
                && intendedParent.IsInSameSubtree(this.Form))
            {
                this.DomFactory.AssociateWithForm(elem, this.Form);

                // TODO: suppress the running of the reset the form owner algorithm
            }

            // 5. Return the newly created element.
            return elem;
        }

        private Element InsertForeignElement(string namespaceUri, string tagName, Attribute[] attributes)
        {
            // See: http://www.w3.org/TR/html51/syntax.html#insert-a-foreign-element
            // When the steps below require the user agent to insert a foreign element for a token
            // in a given namespace, the user agent must run these steps:

            // 1. Let the "adjusted insertion location" be the appropriate place for inserting a node.
            AdjustedInsertLocation adjustedInsertLocation = this.AppropriatePlaceForInsertingNode();

            // 2. Create an element for the token in the given namespace, with the intended parent being
            // the element in which the adjusted insertion location finds itself.
            Element elem = this.CreateElement(adjustedInsertLocation.ParentElement, namespaceUri, tagName, attributes);

            // 3. If it is possible to insert an element at the adjusted insertion location,
            // then insert the newly created element at the adjusted insertion location.

            // NOTE: If the adjusted insertion location cannot accept more elements, e.g., because it’s a Document
            // that already has an element child, then the newly created element is dropped on the floor.
            if (adjustedInsertLocation.BeforeSibling != null)
                this.DomFactory.InsertElementBefore(adjustedInsertLocation.ParentElement, elem, adjustedInsertLocation.BeforeSibling);
            else
                this.DomFactory.AppendElement(adjustedInsertLocation.ParentElement, elem);

            // 4. Push the element onto the stack of open elements so that it is the new current node.
            this.OpenElements.Push(elem);

            // 5. Return the newly created element.
            return elem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Element InsertHtmlElement(string name, Attribute[] attributes)
        {
            // See: http://www.w3.org/TR/html51/syntax.html#insert-an-html-element

            // When the steps below require the user agent to insert an HTML element for a token,
            // the user agent must insert a foreign element for the token, in the HTML namespace.
            return this.InsertForeignElement(Namespaces.Html, name, attributes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Element InsertHtmlElementForToken()
        {
            return this.InsertHtmlElement(this.Token.TagName, this.Token.TagAttributes);
        }

        public void InsertCharacter(char ch)
        {
            // See: http://www.w3.org/TR/html51/syntax.html#insert-characters

            // When the steps below require the user agent to insert a character while processing a token,
            // the user agent must run the following steps:

            // 1. Let data be the characters passed to the algorithm, or, if no characters were explicitly specified,
            //    the character of the character token being processed.

            // 2. Let the adjusted insertion location be the appropriate place for inserting a node.
            //    If the adjusted insertion location is in a <Document> node, then abort these steps.

            // NOTE: The DOM will not let Document nodes have Text node children, so they are dropped on the floor.

            // 3. If there is a Text node immediately before the adjusted insertion location,
            //    then append data to that Text node’s data.

            // 4. Otherwise, create a new Text node whose data is data and whose node document
            //    is the same as that of the element in which the adjusted insertion location finds itself,
            //    and insert the newly created node at the adjusted insertion location.
        }

        public void InsertComment(string data)
        {
            // See: http://www.w3.org/TR/html51/syntax.html#insert-a-comment

            // When the steps below require the user agent to insert a comment while processing a comment token,
            // optionally with an explicitly insertion position position, the user agent must run the following steps:

            // 1. Let data be the data given in the comment token being processed.

            // 2. If position was specified, then let the adjusted insertion location be position.
            //    Otherwise, let adjusted insertion location be the appropriate place for inserting a node.

            // 3. Create a Comment node whose data attribute is set to data and whose node document is the
            //    same as that of the node in which the adjusted insertion location finds itself.

            // 4. Insert the newly created node at the adjusted insertion location.
        }

        #endregion

        #region 8.2.5.2. Parsing elements that contain only text

        private void GenericRawTextElementParsingAlgorithm()
        {
            this.GenericTextElementParsingAlgorithm(Tokenizer.StateEnum.RawText);
        }

        private void GenericRcDataElementParsingAlgorithm()
        {
            this.GenericTextElementParsingAlgorithm(Tokenizer.StateEnum.RcData);
        }

        private void GenericTextElementParsingAlgorithm(Tokenizer.StateEnum state)
        {
            // The "generic raw text element parsing algorithm" and the "generic RCDATA element parsing algorithm"
            // consist of the following steps.These algorithms are always invoked in response to a start tag token.

            // 1. Insert an HTML element for the token.
            this.InsertHtmlElementForToken();

            // 2. If the algorithm that was invoked is the "generic raw text element parsing algorithm",
            //    switch the tokenizer to the §8.2.4.5 RAWTEXT state; otherwise the algorithm invoked was
            //    the "generic RCDATA element parsing algorithm", switch the tokenizer to the §8.2.4.3 RCDATA state.
            this.Tokenizer.SwitchTo(state);

            // 3. Let the original insertion mode be the current insertion mode.
            this.OriginalInsertionMode = this.InsertionMode;

            // 4. Then, switch the insertion mode to "text".
            this.Switch(InsertionModeEnum.Text);
        }

        #endregion

        #region 8.2.5.3. Closing elements that have implied end tags

        private void GenerateImpliedEndTag(Predicate<Element> excludePredicate)
        {
            Contract.RequiresNotNull(excludePredicate, nameof(excludePredicate));

            while (true)
            {
                // When the steps below require the user agent to generate implied end tags, then, while the current node is a dd
                // element, a dt element, an li element, an option element, an optgroup element, a p element, an rb element, an rp
                // element, an rt element, or an rtc element, the user agent must pop the current node off the stack of open elements.

                // If a step requires the user agent to generate implied end tags but lists an element to exclude from the process,
                // then the user agent must perform the above steps as if that element was not in the above list.
                if (excludePredicate(this.CurrentNode))
                    break; // Done!

                if (this.CurrentNode.Is(Tags.Dd, Tags.Dt, Tags.Li, Tags.Option, Tags.OptGroup, Tags.P, Tags.Rb, Tags.Rp, Tags.Rt, Tags.Rtc))
                    this.OpenElements.Pop();
                else
                    break; // Done!
            }
        }

        private void GenerateImpliedEndTag(string excludedTag = null)
        {
            if (excludedTag == null)
                this.GenerateImpliedEndTag(node => false);
            else
                this.GenerateImpliedEndTag(node => node.Is(excludedTag));
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
                if (this.CurrentNode.Is(Tags.Caption, Tags.ColGroup, Tags.Dd, Tags.Dt, Tags.Li, Tags.OptGroup, Tags.Option,
                    Tags.P, Tags.Rb, Tags.Rp, Tags.Rt, Tags.Rtc, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
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
                        this.InformParseError(Parsing.ParseError.InvalidDocType);
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
                        this.DomFactory.SetQuirksMode(QuirksMode.On);
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
                        this.DomFactory.SetQuirksMode(QuirksMode.Limited);
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
                    this.InformParseError(Parsing.ParseError.UnexpectedTag);
                    this.DomFactory.SetQuirksMode(QuirksMode.On);
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
                this.InformParseError(Parsing.ParseError.UnexpectedTag);
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
                Element html = this.CreateElement(this.ParsingContext.Document.DocumentElement, Namespaces.Html, this.Token.TagName, this.Token.TagAttributes);
                this.DomFactory.AppendElement(this.ParsingContext.Document.DocumentElement, html);
                this.OpenElements.Push(html);

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
                this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Create an html element whose node document is the Document object. Append it to the Document object.
                // Put this element in the stack of open elements.
                Element html = this.DomFactory.CreateElement(this.ParsingContext.Document, Namespaces.Html, Tags.Html, Attribute.None);
                this.DomFactory.AppendElement(this.ParsingContext.Document.DocumentElement, html);
                this.OpenElements.Push(html);

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
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "head"
            else if (this.Token.IsStartTagNamed(Tags.Head))
            {
                // Insert an HTML element for the token.
                Element head = this.InsertHtmlElementForToken();

                // Set the head element pointer to the newly created head element.
                this.Head = head;

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
                this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Insert an HTML element for a "head" start tag token with no attributes.
                Element head = this.InsertHtmlElement(Tags.Head, Attribute.None);

                // Set the head element pointer to the newly created head element.
                this.Head = head;

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
                this.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is one of: "base", "basefont", "bgsound", "link"
            else if (this.Token.IsStartTagNamed(Tags.Base, Tags.BaseFont, Tags.BgSound, Tags.Link))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();
            }

            // A start tag whose tag name is "meta"
            else if (this.Token.IsStartTagNamed(Tags.Meta))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                Element meta = this.InsertHtmlElementForToken();
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
            else if ((this.Scripting && this.Token.IsStartTagNamed(Tags.NoScript)) || this.Token.IsStartTagNamed(Tags.NoFrames, Tags.Style))
            {
                // Follow the generic raw text element parsing algorithm.
                this.ParseElement(ParsingAlgorithm.GenericRawText);
            }

            // A start tag whose tag name is "noscript", if the scripting flag is disabled
            else if (!this.Scripting && this.Token.IsStartTagNamed(Tags.NoScript))
            {
                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

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
                this.InsertHtmlElementForToken();

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
                    this.InformParseError(Parsing.ParseError.UnexpectedStartTag);
                else
                    this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
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
                this.InformParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
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
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
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
                    this.InformParseError(Parsing.ParseError.UnexpectedStartTag);
                else
                    this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Parse error.
                this.InformParseError(Parsing.ParseError.UnexpectedTag);

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
                this.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "body"
            else if (this.Token.IsStartTagNamed(Tags.Body))
            {
                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // Switch the insertion mode to "in body".
                this.Switch(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "frameset"
            else if (this.Token.IsStartTagNamed(Tags.Frameset))
            {
                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // Switch the insertion mode to "in frameset".
                this.Switch(InsertionModeEnum.InFrameset);
            }

            // A start tag whose tag name is one of: "base", "basefont", "bgsound", "link", "meta",
            // "noframes", "script", "style", "template", "title"
            else if (this.Token.IsStartTagNamed(Tags.Base, Tags.BaseFont, Tags.BgSound, Tags.Link, Tags.Meta, Tags.NoFrames, Tags.Script, Tags.Style, Tags.Template, Tags.Title))
            {
                // Parse error.
                this.InformParseError(Parsing.ParseError.UnexpectedStartTag);

                // Push the node pointed to by the head element pointer onto the stack of open elements.
                this.OpenElements.Push(this.Head);

                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);

                // Remove the node pointed to by the head element pointer from the stack of open elements.
                // (It might not be the current node at this point.)
                this.OpenElements.Remove(this.Head);

                // NOTE: The head element pointer cannot be null at this point.
            }

            // An end tag whose tag name is "template"
            else if (this.Token.IsEndTagNamed(Tags.Template))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
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
                this.InformParseError(Parsing.ParseError.UnexpectedTag);
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
                this.InsertHtmlElement(Tags.Body, Attribute.None);

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

            bool anyOtherEndTag = false;

            // A character token that is U+0000 NULL
            if (this.Token.IsCharacterNull())
            {
                // Parse error. Ignore the token.
                this.InformParseError(Parsing.ParseError.NullCharacter);
            }

            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            else if (this.Token.IsCharacterWhitespace())
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert the token's character.
                this.InsertCharacter(this.Token.Character);
            }

            // Any other character token
            else if (this.Token.Type == TokenType.Character)
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert the token's character.
                this.InsertCharacter(this.Token.Character);

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(Parsing.ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Parse error.
                this.InformParseError(Parsing.ParseError.UnexpectedTag);

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
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // A start tag whose tag name is "body"
            else if (this.Token.IsStartTagNamed(Tags.Body))
            {
                // Parse error.
                this.InformParseError(Parsing.ParseError.UnexpectedTag);

                // If the second element on the stack of open elements is not a body element, if the stack of open elements
                // has only one node on it, or if there is a template element on the stack of open elements,
                // then ignore the token. (fragment case)
                if (((this.OpenElements.Count >= 2) && !this.OpenElements[1].Is(Tags.Body)) ||
                    (this.OpenElements.Count == 1) || this.OpenElements.Contains(Tags.Template))
                    return;

                // Otherwise, set the frameset-ok flag to "not ok"; then, for each attribute on the token, check to see if
                // the attribute is already present on the body element (the second element) on the stack of open elements,
                // and if it is not, add the attribute and its corresponding value to that element.
                this.FramesetOk = false;
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
                this.InformParseError(Parsing.ParseError.UnexpectedStartTag);

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
                    this.ProcessTokenUsing(InsertionModeEnum.InTemplate);
                }
                else
                {
                    // If there is a node in the stack of open elements that is not either a dd element, a dt element,
                    // an li element, a p element, a tbody element, a td element, a tfoot element, a th element,
                    // a thead element, a tr element, the body element, or the html element, then this is a parse error.
                    if (this.OpenElements.Contains(Tags.Dd, Tags.Dt, Tags.Li, Tags.P, Tags.TBody, Tags.Td, Tags.TFoot,
                        Tags.Th, Tags.THead, Tags.Tr, Tags.Body, Tags.Html))
                    {
                        this.InformParseError(Parsing.ParseError.PrematureEndOfFile);
                    }

                    // Stop parsing.
                    this.StopParsing();
                }
            }

            // An end tag whose tag name is "body"
            else if (this.Token.IsEndTagNamed(Tags.Body))
            {
                // If the stack of open elements does not have a body element in scope, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInScope(Tags.Body))
                {
                    this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, if there is a node in the stack of open elements that is not either a dd element,
                // a dt element, an li element, an optgroup element, an option element, a p element, an rb element,
                // an rp element, an rt element, an rtc element, a tbody element, a td element, a tfoot element,
                // a th element, a thead element, a tr element, the body element, or the html element, then this is a parse error.
                if (this.OpenElements.Contains(Tags.Dd, Tags.Dt, Tags.Li, Tags.OptGroup, Tags.Option, Tags.P, Tags.Rb, Tags.Rp,
                    Tags.Rt, Tags.Rtc, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr, Tags.Body, Tags.Html))
                {
                    this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
                }

                // Switch the insertion mode to "after body".
                this.Switch(InsertionModeEnum.AfterBody);
            }

            // An end tag whose tag name is "html"
            else if (this.Token.IsEndTagNamed(Tags.Html))
            {
                // If the stack of open elements does not have a body element in scope, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInScope(Tags.Body))
                {
                    this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, if there is a node in the stack of open elements that is not either a dd element, a dt element,
                // an li element, an optgroup element, an option element, a p element, an rb element, an rp element,
                // an rt element, an rtc element, a tbody element, a td element, a tfoot element, a th element, a thead element,
                // a tr element, the body element, or the html element, then this is a parse error.
                if (this.OpenElements.Contains(Tags.Dd, Tags.Dt, Tags.Li, Tags.OptGroup, Tags.Option, Tags.P, Tags.Rb, Tags.Rp,
                    Tags.Rt, Tags.Rtc, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr, Tags.Body, Tags.Html))
                {
                    this.InformParseError(Parsing.ParseError.UnexpectedEndTag);
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
                Tags.Address, Tags.Article, Tags.Aside, Tags.BlockQuote, Tags.Center, Tags.Details, Tags.Dialog,
                Tags.Dir, Tags.Div, Tags.Dl, Tags.FieldSet, Tags.FigCaption, Tags.Figure, Tags.Footer, Tags.Header, Tags.HGroup,
                Tags.Main, Tags.Nav, Tags.Ol, Tags.P, Tags.Section, Tags.Summary, Tags.Ul))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is one of: "h1", "h2", "h3", "h4", "h5", "h6"
            else if (this.Token.IsStartTagNamed(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // If the current node is an HTML element whose tag name is one of "h1", "h2", "h3", "h4", "h5", or "h6",
                // then this is a parse error; pop the current node off the stack of open elements.
                if (this.CurrentNode.Is(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
                {
                    this.InformParseError(Parsing.ParseError.UnexpectedStartTag);
                    this.OpenElements.Pop();
                }

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is one of: "pre", "listing"
            else if (this.Token.IsStartTagNamed(Tags.Pre, Tags.Listing))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // If the next token is a "LF" (U+000A) character token, then ignore that token and move on to the
                // next one. (Newlines at the start of pre blocks are ignored as an authoring convenience.)
                this.NextToken = this.Tokenizer.GetNextToken();
                if ((this.NextToken.Type == TokenType.Character) && (this.NextToken.Character == Characters.Lf))
                {
                    // This will "waste" the "next-token". Without this, the next "get token"
                    // will return the "next-token" instead of asking the tokenizer.
                    this.NextToken.ResetToken();
                }

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // A start tag whose tag name is "form"
            else if (this.Token.IsStartTagNamed(Tags.Form))
            {
                // If the form element pointer is not null, and there is no template element on the stack of open elements,
                // then this is a parse error; ignore the token.
                if ((this.Form != null) && !this.OpenElements.Contains(Tags.Template))
                {
                    this.InformParseError(ParseError.UnexpectedStartTag);
                    return;
                }

                // Otherwise:
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // Insert an HTML element for the token, and, if there is no template element on the stack of
                // open elements, set the form element pointer to point to the element created.
                Element form = this.InsertHtmlElementForToken();
                if (!this.OpenElements.Contains(Tags.Template))
                    this.Form = form;
            }

            // A start tag whose tag name is "li"
            else if (this.Token.IsStartTagNamed(Tags.Li))
            {
                // Run these steps:
                // 1. Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // 2. Initialize node to be the current node (the bottommost node of the stack).
                int i = this.OpenElements.Count - 1;
                Element node = this.OpenElements[i];

                // 3. Loop:
                while (true)
                {
                    // If node is an li element, then run these substeps:
                    if (node.TagName == Tags.Li)
                    {
                        // 1. Generate implied end tags, except for li elements.
                        this.GenerateImpliedEndTag(Tags.Li);

                        // 2. If the current node is not an li element, then this is a parse error.
                        if (!this.CurrentNode.Is(Tags.Li))
                            this.InformParseError(ParseError.UnexpectedTag);

                        // 3. Pop elements from the stack of open elements until an li element has been popped from the stack.
                        this.OpenElements.PopUntil(Tags.Li);

                        // 4. Jump to the step labeled done below.
                        break;
                    }

                    // 4. If node is in the special category, but is not an address, div, or p element,
                    //    then jump to the step labeled done below.
                    if (node.IsSpecial() && !node.Is(Tags.Address, Tags.Div, Tags.P))
                        break;

                    // 5. Otherwise, set node to the previous entry in the stack of open elements and return to the step labeled loop.
                    i--;
                    node = this.OpenElements[i];
                }

                // 6. Done: If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // 7. Finally, insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is one of: "dd", "dt"
            else if (this.Token.IsStartTagNamed(Tags.Dd, Tags.Dt))
            {
                // Run these steps:
                // 1. Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // 2. Initialize node to be the current node (the bottommost node of the stack).
                int i = this.OpenElements.Count - 1;
                Element node = this.OpenElements[i];

                // 3. Loop:
                while (true)
                {
                    // If node is a dd element, then run these substeps:
                    if (node.Is(Tags.Dd))
                    {
                        // 1. Generate implied end tags, except for dd elements.
                        this.GenerateImpliedEndTag(Tags.Dd);

                        // 2. If the current node is not a dd element, then this is a parse error.
                        if (!this.CurrentNode.Is(Tags.Dd))
                            this.InformParseError(ParseError.UnexpectedTag);

                        // 3. Pop elements from the stack of open elements until a dd element has been popped from the stack.
                        this.OpenElements.PopUntil(Tags.Dd);

                        // 4. Jump to the step labeled done below.
                        break;
                    }

                    // 4. If node is a dt element, then run these substeps:
                    if (node.Is(Tags.Dt))
                    {
                        // 1. Generate implied end tags, except for dt elements.
                        this.GenerateImpliedEndTag(Tags.Dt);

                        // 2. If the current node is not a dt element, then this is a parse error.
                        if (!this.CurrentNode.Is(Tags.Dt))
                            this.InformParseError(ParseError.UnexpectedTag);

                        // 3. Pop elements from the stack of open elements until a dt element has been popped from the stack.
                        this.OpenElements.PopUntil(Tags.Dt);

                        // 4. Jump to the step labeled done below.
                        break;
                    }

                    // 5. If node is in the special category, but is not an address, div, or p element,
                    // then jump to the step labeled done below.
                    if (node.IsSpecial() && !node.Is(Tags.Address, Tags.Div, Tags.P))
                        break;

                    // 6. Otherwise, set node to the previous entry in the stack of open elements and return to the step labeled loop.
                    i--;
                    node = this.OpenElements[i];
                }

                // 7. Done: If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // 8. Finally, insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is "plaintext"
            else if (this.Token.IsStartTagNamed(Tags.PlainText))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // Switch the tokenizer to the PLAINTEXT state.
                this.Tokenizer.SwitchTo(Tokenizer.StateEnum.PlainText);

                // NOTE: Once a start tag with the tag name "plaintext" has been seen, that will be the last
                // token ever seen other than character tokens (and the end-of-file token), because there is
                // no way to switch out of the PLAINTEXT state.
            }

            // A start tag whose tag name is "button"
            else if (this.Token.IsStartTagNamed(Tags.Button))
            {
                // 1. If the stack of open elements has a button element in scope, then run these substeps:
                if (this.OpenElements.HasElementInScope(Tags.Button))
                {
                    // 1. Parse error.
                    this.InformParseError(ParseError.UnexpectedStartTag);

                    // 2. Generate implied end tags.
                    this.GenerateImpliedEndTag();

                    // 3. Pop elements from the stack of open elements until a button element has been popped from the stack.
                    this.OpenElements.PopUntil(Tags.Button);
                }

                // 2. Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // 3. Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // 4. Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // An end tag whose tag name is one of: "address", "article", "aside", "blockquote", "button", "center", "details",
            // "dialog", "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup", "listing", "main",
            // "nav", "ol", "pre", "section", "summary", "ul"
            else if (this.Token.IsEndTagNamed(
                Tags.Address, Tags.Article, Tags.Aside, Tags.BlockQuote, Tags.Button, Tags.Center, Tags.Details,
                Tags.Dialog, Tags.Dir, Tags.Div, Tags.Dl, Tags.FieldSet, Tags.FigCaption, Tags.Figure, Tags.Footer, Tags.Header,
                Tags.HGroup, Tags.Listing, Tags.Main, Tags.Nav, Tags.Ol, Tags.P, Tags.Section, Tags.Summary, Tags.Ul))
            {
                // If the stack of open elements does not have an element in scope that is an HTML element and with the same
                // tag name as that of the token, then this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInScope(node => node.IsHtmlElement() && node.Is(this.Token.TagName)))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, run these steps:
                // 1. Generate implied end tags.
                this.GenerateImpliedEndTag();

                // 2. If the current node is not an HTML element with the same tag name as that of the token,
                // then this is a parse error.
                if (!(this.CurrentNode.IsHtmlElement() && this.CurrentNode.Is(this.Token.TagName)))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // 3. Pop elements from the stack of open elements until an HTML element with the same tag name
                // as the token has been popped from the stack.
                while (true)
                {
                    Element node = this.OpenElements.Pop();
                    if (node.IsHtmlElement() && node.Is(this.Token.TagName))
                        break;
                }
            }

            // An end tag whose tag name is "form"
            else if (this.Token.IsEndTagNamed(Tags.Form))
            {
                // If there is no template element on the stack of open elements, then run these substeps:
                if (!this.OpenElements.Contains(Tags.Template))
                {
                    // 1. Let node be the element that the form element pointer is set to, or null if it is not set to an element.
                    Element node = this.Form;

                    // 2. Set the form element pointer to null. Otherwise, let node be null.
                    this.Form = null;

                    // 3. If node is null or if the stack of open elements does not have node in scope, then this is a parse error;
                    // abort these steps and ignore the token.
                    if ((node == null) || !this.OpenElements.HasElementInScope(n => n == node))
                    {
                        this.InformParseError(ParseError.UnexpectedEndTag);
                        return;
                    }

                    // 4. Generate implied end tags.
                    this.GenerateImpliedEndTag();

                    // 5. If the current node is not node, then this is a parse error.
                    if (this.CurrentNode != node)
                        this.InformParseError(ParseError.UnexpectedEndTag);

                    // 6. Remove node from the stack of open elements.
                    this.OpenElements.Remove(node);
                }
                else
                {
                    // If there is a template element on the stack of open elements, then run these substeps instead:

                    // 1. If the stack of open elements does not have a form element in scope, then this is a parse error;
                    // abort these steps and ignore the token.
                    if (!this.OpenElements.HasElementInScope(Tags.Form))
                    {
                        this.InformParseError(ParseError.UnexpectedEndTag);
                        return;
                    }

                    // 2. Generate implied end tags.
                    this.GenerateImpliedEndTag();

                    // 3. If the current node is not a form element, then this is a parse error.
                    if (!this.CurrentNode.Is(Tags.Form))
                        this.InformParseError(ParseError.UnexpectedEndTag);

                    // 4. Pop elements from the stack of open elements until a form element has been popped from the stack.
                    this.OpenElements.PopUntil(Tags.Form);
                }
            }

            // An end tag whose tag name is "p"
            else if (this.Token.IsEndTagNamed(Tags.P))
            {
                // If the stack of open elements does not have a p element in button scope, then this is a parse error;
                // insert an HTML element for a "p" start tag token with no attributes.
                if (!this.OpenElements.HasElementInButtonScope(Tags.P))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    this.InsertHtmlElement(Tags.P, Attribute.None);
                }

                // Close a p element.
                this.ClosePElement();
            }

            // An end tag whose tag name is "li"
            else if (this.Token.IsEndTagNamed(Tags.Li))
            {
                // If the stack of open elements does not have an li element in list item scope,
                // then this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInListItemScope(Tags.Li))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, run these steps:
                // 1. Generate implied end tags, except for li elements.
                this.GenerateImpliedEndTag(Tags.Li);

                // 2. If the current node is not an li element, then this is a parse error.
                if (!this.CurrentNode.Is(Tags.Li))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // 3. Pop elements from the stack of open elements until an li element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Li);
            }

            // An end tag whose tag name is one of: "dd", "dt"
            else if (this.Token.IsEndTagNamed(Tags.Dd, Tags.Dt))
            {
                // If the stack of open elements does not have an element in scope that is an HTML element and with the
                // same tag name as that of the token, then this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInScope(node => node.IsHtmlElement() && node.Is(this.Token.TagName)))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, run these steps:
                // 1. Generate implied end tags, except for HTML elements with the same tag name as the token.
                this.GenerateImpliedEndTag(node => node.IsHtmlElement() && node.Is(this.Token.TagName));

                // 2. If the current node is not an HTML element with the same tag name as that of the token,
                // then this is a parse error.
                if (!(this.CurrentNode.IsHtmlElement() && this.CurrentNode.Is(this.CurrentNode.TagName)))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // 3. Pop elements from the stack of open elements until an HTML element with the same tag name
                // as the token has been popped from the stack.
                while (true)
                {
                    Element node = this.OpenElements.Pop();
                    if (node.IsHtmlElement() && node.Is(this.Token.TagName))
                        break;
                }
            }

            // An end tag whose tag name is one of: "h1", "h2", "h3", "h4", "h5", "h6"
            else if (this.Token.IsEndTagNamed(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
            {
                // If the stack of open elements does not have an element in scope that is an HTML element and whose tag
                // name is one of "h1", "h2", "h3", "h4", "h5", or "h6", then this is a parse error; ignore the token.
                if (!this.OpenElements.Any(node => node.IsHtmlElement() && node.Is(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6)))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, run these steps:
                // 1. Generate implied end tags.
                this.GenerateImpliedEndTag();

                // 2. If the current node is not an HTML element with the same tag name as that of the token, then this
                // is a parse error.
                if (!(this.CurrentNode.IsHtmlElement() && this.CurrentNode.Is(this.Token.TagName)))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // 3. Pop elements from the stack of open elements until an HTML element whose tag name is one of
                // "h1", "h2", "h3", "h4", "h5", or "h6" has been popped from the stack.
                while (true)
                {
                    Element node = this.OpenElements.Pop();
                    if (node.IsHtmlElement() && node.Is(Tags.H1, Tags.H2, Tags.H3, Tags.H4, Tags.H5, Tags.H6))
                        break;
                }
            }

            // An end tag whose tag name is "sarcasm"
            else if (this.Token.IsEndTagNamed(Tags.Sarcasm))
            {
                // Take a deep breath, then act as described in the "any other end tag" entry below.
                anyOtherEndTag = true;
            }

            // A start tag whose tag name is "a"
            else if (this.Token.IsStartTagNamed(Tags.A))
            {
                // If the list of active formatting elements contains an <A> element between the end of the
                // list and the last marker on the list (or the start of the list if there is no marker on the list),
                // then this is a parse error; run the adoption agency algorithm for the tag name "a", then remove that
                // element from the list of active formatting elements and the stack of open elements if the adoption agency
                // algorithm didn't already remove it (it might not have if the element is not in table scope).
                if (this.ActiveFormattingElements.ContainsUpToLastMarker(Tags.A))
                {
                    this.InformParseError(ParseError.UnexpectedStartTag);
                    bool removed = this.RunAdoptionAgencyAlgorithm(Tags.A, ref anyOtherEndTag);

                    // ISSUE: If the RunAdoptionAgencyAlgorithm told us to "act as anyOtherEndTag", should we run the stuff below?
                    this.ActiveFormattingElements.RemoveLast(Tags.A);
                    if (!removed)
                    {
                        this.ActiveFormattingElements.RemoveLast(Tags.A);
                        this.OpenElements.RemoveLast(Tags.A);
                    }
                }

                // EXAMPLE:
                // In the non-conforming stream <a href="a">a<table><a href="b">b</table>x, the first a element would be
                // closed upon seeing the second one, and the "x" character would be inside a link to "b", not to "a".
                // This is despite the fact that the outer a element is not in table scope (meaning that a regular </a>
                // end tag at the start of the table wouldn't close the outer a element). The result is that the two a
                // elements are indirectly nested inside each other — non-conforming markup will often result in
                // non-conforming DOMs when parsed.

                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token. Push onto the list of active formatting elements that element.
                Element elem = this.InsertHtmlElementForToken();
                this.ActiveFormattingElements.PushFormattingElement(elem);
            }

            // A start tag whose tag name is one of: "b", "big", "code", "em", "font", "i", "s", "small", "strike",
            // "strong", "tt", "u"
            else if (this.Token.IsStartTagNamed(
                Tags.B, Tags.Big, Tags.Code, Tags.Em, Tags.Font, Tags.I, Tags.S, Tags.Small, Tags.Strike,
                Tags.Strong, Tags.Tt, Tags.U))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token. Push onto the list of active formatting elements that element.
                Element elem = this.InsertHtmlElementForToken();
                this.ActiveFormattingElements.PushFormattingElement(elem);
            }

            // A start tag whose tag name is "nobr"
            else if (this.Token.IsStartTagNamed(Tags.Nobr))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // If the stack of open elements has a nobr element in scope, then this is a parse error; run the adoption
                // agency algorithm for the tag name "nobr", then once again reconstruct the active formatting elements, if any.
                if (this.OpenElements.HasElementInScope(Tags.Nobr))
                {
                    this.InformParseError(ParseError.UnexpectedStartTag);
                    this.RunAdoptionAgencyAlgorithm(Tags.Nobr, ref anyOtherEndTag);

                    // ISSUE: If the RunAdoptionAgencyAlgorithm told us to "act as anyOtherEndTag", should we run the stuff below?
                    this.ActiveFormattingElements.ReconstructFormattingElements();
                }

                // Insert an HTML element for the token. Push onto the list of active formatting elements that element.
                Element elem = this.InsertHtmlElementForToken();
                this.ActiveFormattingElements.PushFormattingElement(elem);
            }

            // An end tag whose tag name is one of: "a", "b", "big", "code", "em", "font", "i", "nobr", "s", "small",
            // "strike", "strong", "tt", "u"
            else if (this.Token.IsEndTagNamed(
                Tags.A, Tags.B, Tags.Big, Tags.Code, Tags.Em, Tags.Font, Tags.I, Tags.Nobr, Tags.S, Tags.Small,
                Tags.Strike, Tags.Strong, Tags.Tt, Tags.U))
            {
                // Run the adoption agency algorithm for the token's tag name.
                this.RunAdoptionAgencyAlgorithm(this.Token.TagName, ref anyOtherEndTag);
            }

            // A start tag whose tag name is one of: "applet", "marquee", "object"
            else if (this.Token.IsStartTagNamed(Tags.Applet, Tags.Marquee, Tags.Object))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // Insert a marker at the end of the list of active formatting elements.
                this.ActiveFormattingElements.InsertMarker();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // An end tag token whose tag name is one of: "applet", "marquee", "object"
            else if (this.Token.IsEndTagNamed(Tags.Applet, Tags.Marquee, Tags.Object))
            {
                // If the stack of open elements does not have an element in scope that is an HTML element and with the
                // same tag name as that of the token, then this is a parse error; ignore the token.
                if (!this.OpenElements.Any(node => node.IsHtmlElement() && node.Is(this.Token.TagName)))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, run these steps:
                // 1. Generate implied end tags.
                this.GenerateImpliedEndTag();

                // 2. If the current node is not an HTML element with the same tag name as that of the token, then this
                // is a parse error.
                if (!(this.CurrentNode.IsHtmlElement() && this.CurrentNode.Is(this.Token.TagName)))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // 3. Pop elements from the stack of open elements until an HTML element with the same tag name as the
                // token has been popped from the stack.
                while (true)
                {
                    Element node = this.OpenElements.Pop();
                    if (node.IsHtmlElement() && node.Is(this.Token.TagName))
                        break;
                }

                // 4. Clear the list of active formatting elements up to the last marker.
                this.ActiveFormattingElements.ClearFormattingElementsUpToMarker();
            }

            // A start tag whose tag name is "table"
            else if (this.Token.IsStartTagNamed(Tags.Table))
            {
                // If the Document is not set to quirks mode, and the stack of open elements has a p element
                // in button scope, then close a p element.
                if ((this.ParsingContext.Document.QuirksMode != QuirksMode.On) && this.OpenElements.HasElementInButtonScope(Tags.P))
                    this.ClosePElement();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // Switch the insertion mode to "in table".
                this.Switch(InsertionModeEnum.InTable);
            }

            // An end tag whose tag name is "br"
            else if (this.Token.IsEndTagNamed(Tags.Br))
            {
                // Parse error. Act as described in the next entry, as if this
                // was a "br" start tag token, rather than an end tag token.
                this.InformParseError(ParseError.UnexpectedEndTag);

                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                this.Tokenizer.AcknowledgeSelfClosingTag();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // A start tag whose tag name is one of: "area", "br", "embed", "img", "keygen", "wbr"
            else if (this.Token.IsStartTagNamed(Tags.Area, Tags.Br, Tags.Embed, Tags.Img, Tags.KeyGen, Tags.Wbr))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // A start tag whose tag name is "input"
            else if (this.Token.IsStartTagNamed(Tags.Input))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();

                // If the token does not have an attribute with the name "type", or if it does, but that attribute's value
                // is not an ASCII case-insensitive match for the string "hidden", then: set the frameset-ok flag to "not ok".
                if (!this.Token.TagAttributes.Any(attr => (attr.Name == "type") && attr.Value.Equals("hidden", StringComparison.OrdinalIgnoreCase)))
                    this.FramesetOk = false;
            }

            // A start tag whose tag name is one of: "param", "source", "track"
            else if (this.Token.IsStartTagNamed(Tags.Param, Tags.Source, Tags.Track))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();
            }

            // A start tag whose tag name is "hr"
            else if (this.Token.IsStartTagNamed(Tags.Hr))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.Contains(Tags.P))
                    this.ClosePElement();

                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;
            }

            // A start tag whose tag name is "image"
            else if (this.Token.IsStartTagNamed(Tags.Image))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedTag);

                // Change the token's tag name to "img" and reprocess it. (Don't ask.)
                this.Token.SetStartTag(Tags.Img, this.Token.TagIsSelfClosing, this.Token.TagAttributes);
                this.ProcessToken();
            }

            // A start tag whose tag name is "textarea"
            else if (this.Token.IsStartTagNamed(Tags.TextArea))
            {
                // Run these steps:

                // 1. Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // 2. If the next token is a "LF" (U+000A) character token, then ignore that token and move on to the
                // next one. (Newlines at the start of textarea elements are ignored as an authoring convenience.)
                this.NextToken = this.Tokenizer.GetNextToken();
                if ((this.NextToken.Type == TokenType.Character) && (this.NextToken.Character == Characters.Lf))
                {
                    // This will "waste" the "next-token". Without this, the next "get token"
                    // will return the "next-token" instead of asking the tokenizer.
                    this.NextToken.ResetToken();
                }

                // 3. Switch the tokenizer to the RCDATA state.
                this.Tokenizer.SwitchTo(Tokenizer.StateEnum.RcData);

                // 4. Let the original insertion mode be the current insertion mode.
                this.OriginalInsertionMode = this.InsertionMode;

                // 5. Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // 6. Switch the insertion mode to "text".
                this.Switch(InsertionModeEnum.Text);
            }

            // A start tag whose tag name is "xmp"
            else if (this.Token.IsStartTagNamed(Tags.Xmp))
            {
                // If the stack of open elements has a p element in button scope, then close a p element.
                if (this.OpenElements.Contains(Tags.P))
                    this.ClosePElement();

                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // Follow the generic raw text element parsing algorithm.
                this.GenericRawTextElementParsingAlgorithm();
            }

            // A start tag whose tag name is "iframe"
            else if (this.Token.IsStartTagNamed(Tags.IFrame))
            {
                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // Follow the generic raw text element parsing algorithm.
                this.GenericRawTextElementParsingAlgorithm();
            }

            // A start tag whose tag name is "noembed"
            // A start tag whose tag name is "noscript", if the scripting flag is enabled
            else if (this.Token.IsStartTagNamed(Tags.NoEmbed) || (this.Scripting && this.Token.IsStartTagNamed(Tags.NoScript)))
            {
                // Follow the generic raw text element parsing algorithm.
                this.GenericRawTextElementParsingAlgorithm();
            }

            // A start tag whose tag name is "select"
            else if (this.Token.IsStartTagNamed(Tags.Select))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // Set the frameset-ok flag to "not ok".
                this.FramesetOk = false;

                // If the insertion mode is one of "in table", "in caption", "in table body", "in row",
                // or "in cell", then switch the insertion mode to "in select in table". Otherwise,
                // switch the insertion mode to "in select".
                InsertionModeEnum mode = this.InsertionMode;
                if ((mode == InsertionModeEnum.InTable) || (mode == InsertionModeEnum.InCaption) ||
                    (mode == InsertionModeEnum.InTableBody) || (mode == InsertionModeEnum.InRow) || (mode == InsertionModeEnum.InCell))
                {
                    this.Switch(InsertionModeEnum.InSelectInTable);
                }
                else
                {
                    this.Switch(InsertionModeEnum.InSelect);
                }
            }

            // A start tag whose tag name is one of: "optgroup", "option"
            else if (this.Token.IsStartTagNamed(Tags.OptGroup, Tags.Option))
            {
                // If the current node is an option element, then pop the current node off the stack of open elements.
                if (this.CurrentNode.Is(Tags.Option))
                    this.OpenElements.Pop();

                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is one of: "rb", "rp", "rtc"
            else if (this.Token.IsStartTagNamed(Tags.Rb, Tags.Rp, Tags.Rtc))
            {
                // If the stack of open elements has a ruby element in scope, then generate implied end tags.
                if (this.OpenElements.HasElementInScope(Tags.Ruby))
                    this.GenerateImpliedEndTag();

                // If the current node is not then a ruby element, this is a parse error.
                if (!this.CurrentNode.Is(Tags.Ruby))
                    this.InformParseError(ParseError.UnexpectedTag);

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is "rt"
            else if (this.Token.IsStartTagNamed(Tags.Rt))
            {
                // If the stack of open elements has a ruby element in scope, then generate implied end tags, except for
                // rtc elements.
                if (this.OpenElements.HasElementInScope(Tags.Ruby))
                    this.GenerateImpliedEndTag(Tags.Rtc);

                // If the current node is not then a ruby element or an rtc element, this is a parse error.
                if (!this.CurrentNode.Is(Tags.Ruby))
                    this.InformParseError(ParseError.UnexpectedTag);

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is "math"
            else if (this.Token.IsStartTagNamed(Tags.Math))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // MathML_TODO ... MathML Tags
                throw new NotImplementedException();

                //    Adjust MathML attributes for the token. (This fixes the case of MathML attributes that are not all lowercase.)

                //    Adjust foreign attributes for the token. (This fixes the use of namespaced attributes, in particular XLink.)

                //    Insert a foreign element for the token, in the MathML namespace.

                //    If the token has its self-closing flag set, pop the current node off the stack of open elements and acknowledge
                //    the token's self-closing flag.
            }

            // A start tag whose tag name is "svg"
            else if (this.Token.IsStartTagNamed(Tags.Svg))
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // SVG_TODO ... SVG Tags
                throw new NotImplementedException();

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
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedStartTag);
            }

            // Any other start tag
            else if (this.Token.Type == TokenType.StartTag)
            {
                // Reconstruct the active formatting elements, if any.
                this.ActiveFormattingElements.ReconstructFormattingElements();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();

                // NOTE: This element will be an ordinary element.
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                anyOtherEndTag = true;
            }

            if (anyOtherEndTag)
            {
                // Run these steps:
                // 1. Initialize node to be the current node (the bottommost node of the stack).
                int i = this.OpenElements.Count - 1;
                Element node = this.OpenElements[i];

                // Loop:
                do
                {
                    // 2. If node is an HTML element with the same tag name as the token, then:
                    if (node.Is(this.Token.TagName))
                    {
                        // 1. Generate implied end tags, except for HTML elements with the same tag name as the token.
                        this.GenerateImpliedEndTag(this.Token.TagName);

                        // 2. If node is not the current node, then this is a parse error.
                        if (this.CurrentNode != node)
                            this.InformParseError(ParseError.UnexpectedTag);

                        // 3. Pop all the nodes from the current node up to node, including node, then stop these steps.
                        System.Diagnostics.Debug.Assert(this.OpenElements.Contains(node), "Broken algorithm? Are we suppose to pop everything?");
                        this.OpenElements.PopUntil(node);
                        return; // Stop there steps
                    }

                    // 3. Otherwise, if node is in the special category, then this is a parse error; ignore the token, and abort these steps.
                    else if (node.IsSpecial())
                    {
                        this.InformParseError(ParseError.UnexpectedTag);
                        return; // Abort these steps.
                    }

                    // 4. Set node to the previous entry in the stack of open elements.
                    i--;
                    node = this.OpenElements[i];

                    // 5. Return to the step labeled loop.
                }
                while (true);
            }
        }

        private void ClosePElement()
        {
            // When the steps above say the user agent is to close a p element, it means that the
            // user agent must run the following steps:

            // 1. Generate implied end tags, except for p elements.
            this.GenerateImpliedEndTag(Tags.P);

            // 2. If the current node is not a p element, then this is a parse error.
            if (!this.CurrentNode.Is(Tags.P))
                this.InformParseError(Parsing.ParseError.UnexpectedTag);

            // 3. Pop elements from the stack of open elements until a p element has been popped from the stack.
            this.OpenElements.PopUntil(Tags.P);
        }

        /// <summary>
        /// Runs the "adoption agency algorithm".
        /// </summary>
        /// <param name="subject">The tag that is the "subject" of the operation.</param>
        /// <param name="anyOtherEndTag">Tells if we should act as described in the "any other end tag" entry above.</param>
        /// <returns>True if the element was removed from the stack of open elements and the list of active formatting elements.</returns>
        private bool RunAdoptionAgencyAlgorithm(string subject, ref bool anyOtherEndTag)
        {
            bool elementRemoved = false;

            // See: http://www.w3.org/TR/html51/syntax.html#closing-misnested-formatting-elements
            // The adoption agency algorithm, which takes as its only argument a tag name subject for which the
            // algorithm is being run, consists of the following steps:

            // 1. If the current node is an HTML element whose tag name is subject, then run these substeps:
            if (this.CurrentNode.Is(subject))
            {
                // 1. Let element be the current node.
                Element element = this.CurrentNode;

                // 2. Pop element off the stack of open elements.
                this.OpenElements.Pop();

                // 3. If element is also in the list of active formatting elements, remove the element from the list.
                if (this.ActiveFormattingElements.Contains(element))
                {
                    this.ActiveFormattingElements.Remove(element);
                    elementRemoved = true;
                }

                // 4. Abort the adoption agency algorithm.
                return elementRemoved;
            }

            // 2. Let outer loop counter be zero.
            int outerLoopCounter = 0;

            // Outer loop:
            do
            {
                // 3. If outer loop counter is greater than or equal to eight, then abort these steps.
                if (outerLoopCounter >= 8)
                    return elementRemoved;

                // 4. Increment outer loop counter by one.
                outerLoopCounter++;

                // 5. Let formatting element be the last element in the list of active formatting elements that:
                //    * is between the end of the list and the last scope marker in the list, if any,
                //        or the start of the list otherwise, and
                //    * has the tag name subject.
                Element formattingElement = this.ActiveFormattingElements.ElementUpToLastMarker(subject);

                // If there is no such element, then abort these steps and instead act as described in the
                // "any other end tag" entry above.
                if (formattingElement == null)
                {
                    anyOtherEndTag = true;
                    return elementRemoved;
                }

                // 6. If formatting element is not in the stack of open elements, then this is a parse error;
                //    remove the element from the list, and abort these steps.
                if (!this.OpenElements.Contains(formattingElement))
                {
                    this.ActiveFormattingElements.Remove(formattingElement);
                    return true;
                }

                // 7. If formatting element is in the stack of open elements, but the element is not in scope,
                //    then this is a parse error; abort these steps.
                if (!this.OpenElements.HasElementInScope(elem => elem == formattingElement))
                {
                    this.InformParseError(ParseError.UnexpectedTag);
                    return elementRemoved;
                }

                // 8. If formatting element is not the current node, this is a parse error. (But do not abort these steps.)
                if (formattingElement != this.CurrentNode)
                    this.InformParseError(ParseError.UnexpectedTag);

                // 9. Let furthest block be the topmost node in the stack of open elements that is lower in the stack
                //    than formatting element, and is an element in the special category. There might not be one.
                Element furthestBlock = null;
                bool formattingElementFound = false;

                // NB: The spec says, "... that is lower in the stack ...", meaing BELOW or AFTER.
                for (int j = 0; j < this.OpenElements.Count; j++)
                {
                    Element elem = this.OpenElements[j];
                    if (formattingElementFound)
                    {
                        if (elem.IsSpecial())
                        {
                            furthestBlock = elem;
                            break;
                        }
                    }
                    else
                    {
                        if (elem == formattingElement)
                            formattingElementFound = true;
                    }
                }

                // 10. If there is no furthest block, then the UA must first pop all the nodes from the bottom of the
                //    stack of open elements, from the current node up to and including formatting element, then remove
                //    formatting element from the list of active formatting elements, and finally abort these steps.
                if (furthestBlock == null)
                {
                    this.OpenElements.PopUntil(formattingElement);
                    this.ActiveFormattingElements.Remove(formattingElement);
                    return true;
                }

                // 11. Let common ancestor be the element immediately above formatting element in the stack of open elements.
                Element commonAncestor = this.OpenElements.ElementAbove(formattingElement);

                // 12. Let a bookmark note the position of formatting element in the list of active formatting elements
                //     relative to the elements on either side of it in the list.
                // ISSUE: Is index OK, or do we need an element instead (because stuff before the index shifts)???
                int bookmarkIndex = this.ActiveFormattingElements.IndexOf(formattingElement);

                // 13. Let node and last node be furthest block. Follow these steps:
                Element node = furthestBlock;
                Element lastNode = furthestBlock;

                // Inner loop:
                // 1. Let inner loop counter be zero.
                int innerLoopCounter = 0;

                Element nodeAboveNode = null;
                do
                {
                    // 2. Increment inner loop counter by one.
                    innerLoopCounter++;

                    // 3. Let node be the element immediately above node in the stack of open elements, or if node is no
                    //    longer in the stack of open elements (e.g. because it got removed by this algorithm), the element
                    //    that was immediately above node in the stack of open elements before node was removed.
                    node = this.OpenElements.Contains(node) ? this.OpenElements.ElementAbove(node) : nodeAboveNode;
                    nodeAboveNode = this.OpenElements.ElementAbove(node);

                    // 4. If node is formatting element, then go to the next step in the overall algorithm.
                    if (node.IsFormatting())
                        break;

                    // 5. If inner loop counter is greater than three and node is in the list of active formatting elements,
                    //    then remove node from the list of active formatting elements.
                    if ((innerLoopCounter > 3) && this.ActiveFormattingElements.Contains(node))
                        this.ActiveFormattingElements.Remove(node);

                    // 6. If node is not in the list of active formatting elements, then remove node from the stack of open
                    //    elements and then go back to the step labeled inner loop.
                    if (!this.ActiveFormattingElements.Contains(node))
                    {
                        this.OpenElements.Remove(node);
                        continue; // Inner loop
                    }

                    // 7. Create an element for the token for which the element node was created, in the HTML namespace, with
                    //    common ancestor as the intended parent; replace the entry for node in the list of active formatting
                    //    elements with an entry for the new element, replace the entry for node in the stack of open elements
                    //    with an entry for the new element, and let node be the new element.
                    Element newElement = this.CreateElement(commonAncestor, Namespaces.Html, node.TagName,
                        node.Attributes.Select(attr => new Attribute(attr.Name, attr.Value)).ToArray());
                    this.ActiveFormattingElements.Replace(node, newElement);
                    this.OpenElements.Replace(node, newElement);
                    node = newElement;

                    // 8. If last node is furthest block, then move the aforementioned bookmark to be immediately after the new
                    //    node in the list of active formatting elements.
                    if (lastNode == furthestBlock)
                        bookmarkIndex = this.ActiveFormattingElements.IndexOf(node) + 1;

                    // 9. Insert last node into node, first removing it from its previous parent node if any.
                    // ISSUE: Are we suppose to insert as the first or the last child?
                    this.DomFactory.InsertChildNode(node, lastNode, true);

                    // 10. Let last node be node.
                    lastNode = node;

                    // 11. Return to the step labeled inner loop.
                } while (true);

                // 14. Insert whatever last node ended up being in the previous step at the appropriate place for inserting a node,
                //     but using common ancestor as the override target.
                AdjustedInsertLocation location = this.AppropriatePlaceForInsertingNode(commonAncestor);
                if (location.BeforeSibling != null)
                    this.DomFactory.InsertElementBefore(location.ParentElement, lastNode, location.BeforeSibling);
                else
                    this.DomFactory.AppendElement(location.ParentElement, lastNode);


                // 15. Create an element for the token for which formatting element was created, in the HTML namespace,
                //    with furthest block as the intended parent.
                Token newElementToken = new Token();
                newElementToken.SetStartTag(formattingElement.TagName, false, node.Attributes.Select(attr => new Attribute(attr.Name, attr.Value)).ToArray());
                Element newElement2 = this.CreateElement(furthestBlock, Namespaces.Html, newElementToken.TagName, newElementToken.TagAttributes);

                // 16. Take all of the child nodes of furthest block and append them to the element created in the last step.
                foreach (Node child in furthestBlock.ChildNodes)
                    this.DomFactory.InsertChildNode(newElement2, child, true);

                // 17. Append that new element to furthest block.
                this.DomFactory.InsertChildNode(furthestBlock, newElement2, false);

                // 18. Remove formatting element from the list of active formatting elements, and insert the new element into
                //     the list of active formatting elements at the position of the aforementioned bookmark.
                this.ActiveFormattingElements.Remove(formattingElement);
                this.ActiveFormattingElements.Insert(newElement2, newElementToken, bookmarkIndex);

                // 19. Remove formatting element from the stack of open elements, and insert the new element into the stack of
                //     open elements immediately below the position of furthest block in that stack.
                this.OpenElements.Remove(formattingElement);
                this.OpenElements.InsertBelow(newElement2, furthestBlock);

                // 20. Jump back to the step labeled outer loop.
            } while (true);

            // NOTE: This algorithm's name, the "adoption agency algorithm", comes from the way it causes elements to change
            // parents, and is in contrast with other possible algorithms for dealing with misnested content, which included
            // the "incest algorithm", the "secret affair algorithm", and the "Heisenberg algorithm".
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
                // Insert the token's character.
                this.InsertCharacter(this.Token.Character);

                // NOTE: This can never be a U+0000 NULL character; the tokenizer converts those
                // to U+FFFD REPLACEMENT CHARACTER characters.
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Parse error.
                this.InformParseError(ParseError.PrematureEndOfFile);

                // If the current node is a script element, mark the script element as "already started".
                if (this.CurrentNode.Is(Tags.Script))
                    FutureVersions.CurrentlyIrrelevant();

                // Pop the current node off the stack of open elements.
                this.OpenElements.Pop();

                // Switch the insertion mode to the original insertion mode and reprocess the token.
                this.Switch(this.OriginalInsertionMode);
                this.ProcessToken();
            }

            // An end tag whose tag name is "script"
            else if (this.Token.IsEndTagNamed(Tags.Script))
            {
                throw new NotImplementedException();
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
                // Pop the current node off the stack of open elements.
                this.OpenElements.Pop();

                // Switch the insertion mode to the original insertion mode.
                this.Switch(this.OriginalInsertionMode);
            }
        }

        // Not really a token list, but char list
        private readonly List<char> PendingTableCharacterTokens = new List<char>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleTokenInInTableMode()
        {
            /*
            8.2.5.4.9 The "in table" insertion mode

            When the user agent is to apply the rules for the "in table" insertion mode,
            the user agent must handle the token as follows:
            */

            bool anythingElse = false;

            // A character token, if the current node is table, tbody, tfoot, thead, or tr element
            if ((this.Token.Type == TokenType.Character) && this.CurrentNode.Is(Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr))
            {
                // Let the pending table character tokens be an empty list of tokens.
                this.PendingTableCharacterTokens.Clear();

                // Let the original insertion mode be the current insertion mode.
                this.OriginalInsertionMode = this.InsertionMode;

                // Switch the insertion mode to "in table text" and reprocess the token.
                this.Switch(InsertionModeEnum.InTableText);
                this.ProcessToken();
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "caption"
            else if (this.Token.IsStartTagNamed(Tags.Caption))
            {
                // Clear the stack back to a table context. (See below.)
                this.ClearStackBackToTableContext();

                // Insert a marker at the end of the list of active formatting elements.
                this.ActiveFormattingElements.InsertMarker();

                // Insert an HTML element for the token, then switch the insertion mode to "in caption".
                this.InsertHtmlElementForToken();
                this.Switch(InsertionModeEnum.InCaption);
            }

            // A start tag whose tag name is "colgroup"
            else if (this.Token.IsStartTagNamed(Tags.ColGroup))
            {
                // Clear the stack back to a table context. (See below.)
                this.ClearStackBackToTableContext();

                // Insert an HTML element for the token, then switch the insertion mode to "in column group".
                this.InsertHtmlElementForToken();
                this.Switch(InsertionModeEnum.InColumnGroup);
            }

            // A start tag whose tag name is "col"
            else if (this.Token.IsStartTagNamed(Tags.Col))
            {
                // Clear the stack back to a table context. (See below.)
                this.ClearStackBackToTableContext();

                // Insert an HTML element for a "colgroup" start tag token with no attributes, then switch the
                // insertion mode to "in column group".
                this.InsertHtmlElement(Tags.ColGroup, Attribute.None);
                this.Switch(InsertionModeEnum.InColumnGroup);

                // Reprocess the current token.
                this.ProcessToken();
            }

            // A start tag whose tag name is one of: "tbody", "tfoot", "thead"
            else if (this.Token.IsStartTagNamed(Tags.TBody, Tags.TFoot, Tags.THead))
            {
                // Clear the stack back to a table context. (See below.)
                this.ClearStackBackToTableContext();

                // Insert an HTML element for the token, then switch the insertion mode to "in table body".
                this.InsertHtmlElementForToken();
                this.Switch(InsertionModeEnum.InTableBody);
            }

            // A start tag whose tag name is one of: "td", "th", "tr"
            else if (this.Token.IsStartTagNamed(Tags.Td, Tags.Th, Tags.Tr))
            {
                // Clear the stack back to a table context. (See below.)
                this.ClearStackBackToTableContext();

                // Insert an HTML element for a "tbody" start tag token with no attributes,
                // then switch the insertion mode to "in table body".
                this.InsertHtmlElement(Tags.TBody, Attribute.None);
                this.Switch(InsertionModeEnum.InTableBody);

                // Reprocess the current token.
                this.ProcessToken();
            }

            // A start tag whose tag name is "table"
            else if (this.Token.IsStartTagNamed(Tags.Table))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedStartTag);

                // If the stack of open elements does not have a table element in table scope, ignore the token.
                if (!this.OpenElements.HasElementInTableScope(Tags.Table))
                    return;

                // Otherwise:
                // Pop elements from this stack until a table element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Table);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();

                // Reprocess the token.
                this.ProcessToken();
            }

            // An end tag whose tag name is "table"
            else if (this.Token.IsEndTagNamed(Tags.Table))
            {
                // If the stack of open elements does not have a table element in table scope, this is a parse error;
                // ignore the token.
                if (!this.OpenElements.HasElementInTableScope(Tags.Table))
                {
                    this.InformParseError(ParseError.PrematureEndOfFile);
                    return;
                }

                // Otherwise:
                // Pop elements from this stack until a table element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Table);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "tbody", "td",
            // "tfoot", "th", "thead", "tr"
            else if (this.Token.IsEndTagNamed(
                Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html, Tags.TBody, Tags.Td,
                Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.PrematureEndOfFile);
            }

            // A start tag whose tag name is one of: "style", "script", "template"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Style, Tags.Script, Tags.Template) || this.Token.IsEndTagNamed(Tags.Template))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // A start tag whose tag name is "input"
            else if (this.Token.IsStartTagNamed(Tags.Input))
            {
                // If the token does not have an attribute with the name "type", or if it does, but that attribute's
                // value is not an ASCII case-insensitive match for the string "hidden", then: act as described in the
                // "anything else" entry below.
                if (!this.Token.TagAttributes.Any(attr => (attr.Name == "type") && attr.Value.Equals("hidden", StringComparison.OrdinalIgnoreCase)))
                {
                    anythingElse = true;
                }

                // Otherwise:
                else
                {
                    // Parse error.
                    this.InformParseError(ParseError.UnexpectedStartTag);

                    // Insert an HTML element for the token.
                    this.InsertHtmlElementForToken();

                    // Pop that input element off the stack of open elements.
                    this.OpenElements.Pop();

                    // Acknowledge the token's self-closing flag, if it is set.
                    if (this.Token.TagIsSelfClosing)
                        this.Tokenizer.AcknowledgeSelfClosingTag();
                }
            }

            // A start tag whose tag name is "form"
            else if (this.Token.IsStartTagNamed(Tags.Form))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedStartTag);

                // If there is a template element on the stack of open elements, or if the form element pointer
                // is not null, ignore the token.
                if (this.OpenElements.Contains(Tags.Template) || (this.Form != null))
                    return;

                // Otherwise:
                // Insert an HTML element for the token, and set the form element pointer to point to the element created.
                Element form = this.InsertHtmlElementForToken();
                this.Form = form;

                // Pop that form element off the stack of open elements.
                this.OpenElements.Pop();
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // Anything else
            else
            {
                anythingElse = true;
            }

            if (anythingElse)
            {
                // Parse error. Enable foster parenting, process the token using the rules for the "in body"
                // insertion mode, and then disable foster parenting.
                this.FosteringParent = true;
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
                this.FosteringParent = false;
            }
        }

        private void ClearStackBackToTableContext()
        {
            /*
                 When the steps above require the UA to clear the stack back to a table context, it means that the
                 UA must, while the current node is not a table, template, or html element, pop elements from the
                 stack of open elements.

                 NOTE: The current node being an html element after this process is a fragment case.
            */
            while (!this.CurrentNode.Is(Tags.Table, Tags.Template, Tags.Html))
            {
                this.OpenElements.Pop();
            }
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
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.NullCharacter);
                return;
            }

            // Any other character token
            else if (this.Token.Type == TokenType.Character)
            {
                // Append the character token to the pending table character tokens list.
                this.PendingTableCharacterTokens.Add(this.Token.Character);
            }

            // Anything else
            else
            {
                // If any of the tokens in the pending table character tokens list are character
                // tokens that are not space characters, then reprocess the character tokens in the
                // pending table character tokens list using the rules given in the "anything else"
                // entry in the "in table" insertion mode.
                if (this.PendingTableCharacterTokens.Any(ch => !Characters.IsSpaceCharacter(ch)))
                {
                    // Logic below taken from: "in table" insertion mode, "anything else".

                    // Parse error. Enable foster parenting, process the token using the rules for the "in body"
                    // insertion mode, and then disable foster parenting.
                    this.FosteringParent = true;
                    this.ProcessTokenUsing(InsertionModeEnum.InBody);
                    this.FosteringParent = false;
                }

                // Otherwise, insert the characters given by the pending table character tokens list.
                else
                {
                    foreach (char ch in this.PendingTableCharacterTokens)
                        this.InsertCharacter(ch);
                }

                // Switch the insertion mode to the original insertion mode and reprocess the token.
                this.Switch(this.OriginalInsertionMode);
                this.ProcessToken();
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
                // If the stack of open elements does not have a caption element in table scope,
                // this is a parse error; ignore the token. (fragment case)
                if (!this.OpenElements.HasElementInTableScope(Tags.Caption))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Generate implied end tags.
                this.GenerateImpliedEndTag();

                // Now, if the current node is not a caption element, then this is a parse error.
                if (!this.CurrentNode.Is(Tags.Caption))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // Pop elements from this stack until a caption element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Caption);

                // Clear the list of active formatting elements up to the last marker.
                this.ActiveFormattingElements.ClearFormattingElementsUpToMarker();

                // Switch the insertion mode to "in table".
                this.Switch(InsertionModeEnum.InTable);
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"
            // An end tag whose tag name is "table"
            else if (
                this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr) ||
                this.Token.IsEndTagNamed(Tags.Table))
            {
                // Parse error.
                if (this.Token.Type == TokenType.StartTag)
                    this.InformParseError(ParseError.UnexpectedStartTag);
                else
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // If the stack of open elements does not have a caption element in table scope, ignore the token. (fragment case)
                if (!this.OpenElements.HasElementInTableScope(Tags.Caption))
                    return;

                // Otherwise:
                // Pop elements from this stack until a caption element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Caption);

                // Clear the list of active formatting elements up to the last marker.
                this.ActiveFormattingElements.ClearFormattingElementsUpToMarker();

                // Switch the insertion mode to "in table".
                this.Switch(InsertionModeEnum.InTable);

                // Reprocess the token.
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "body", "col", "colgroup", "html", "tbody", "td", "tfoot", "th", "thead", "tr"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Col, Tags.ColGroup, Tags.Html, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
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
                // Insert the character.
                this.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "col"
            else if (this.Token.IsStartTagNamed(Tags.Col))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();
            }

            // An end tag whose tag name is "colgroup"
            else if (this.Token.IsEndTagNamed(Tags.ColGroup))
            {
                // If the current node is not a colgroup element, then this is a parse error; ignore the token.
                if (!this.CurrentNode.Is(Tags.ColGroup))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, pop the current node from the stack of open elements. Switch the insertion mode to "in table".
                this.OpenElements.Pop();
                this.Switch(InsertionModeEnum.InTable);
            }

            // An end tag whose tag name is "col"
            else if (this.Token.IsEndTagNamed(Tags.Col))
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // A start tag whose tag name is "template"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Template) || this.Token.IsEndTagNamed(Tags.Template))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // Anything else
            else
            {
                // If the current node is not a colgroup element, then this is a parse error; ignore the token.
                if (!this.CurrentNode.Is(Tags.ColGroup))
                {
                    this.InformParseError(ParseError.UnexpectedTag);
                    return;
                }

                // Otherwise, pop the current node from the stack of open elements.
                this.OpenElements.Pop();

                // Switch the insertion mode to "in table".
                this.Switch(InsertionModeEnum.InTable);

                // Reprocess the token.
                this.ProcessToken();
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
                // Clear the stack back to a table body context. (See below.)
                this.ClearStackBackToTableBodyContext();

                // Insert an HTML element for the token, then switch the insertion mode to "in row".
                this.InsertHtmlElementForToken();
                this.Switch(InsertionModeEnum.InRow);
            }

            // A start tag whose tag name is one of: "th", "td"
            else if (this.Token.IsStartTagNamed(Tags.Th, Tags.Td))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedStartTag);

                // Clear the stack back to a table body context. (See below.)
                this.ClearStackBackToTableBodyContext();

                // Insert an HTML element for a "tr" start tag token with no attributes, then switch the insertion mode to "in row".
                this.InsertHtmlElement(Tags.Tr, Attribute.None);
                this.Switch(InsertionModeEnum.InRow);

                // Reprocess the current token.
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "tbody", "tfoot", "thead"
            else if (this.Token.IsEndTagNamed(Tags.TBody, Tags.TFoot, Tags.THead))
            {
                // If the stack of open elements does not have an element in table scope that is an HTML element
                // and with the same tag name as the token, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(this.Token.TagName))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Clear the stack back to a table body context. (See below.)
                this.ClearStackBackToTableBodyContext();

                // Pop the current node from the stack of open elements. Switch the insertion mode to "in table".
                this.OpenElements.Pop();
                this.Switch(InsertionModeEnum.InTable);
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "tfoot", "thead"
            // An end tag whose tag name is "table"
            else if (
                this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.TFoot, Tags.THead) ||
                this.Token.IsEndTagNamed(Tags.Table))
            {
                // If the stack of open elements does not have a tbody, thead, or tfoot element in table scope,
                // this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(elem => elem.Is(Tags.TBody, Tags.THead, Tags.TFoot)))
                {
                    if (this.Token.Type == TokenType.StartTag)
                        this.InformParseError(ParseError.UnexpectedStartTag);
                    else
                        this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Clear the stack back to a table body context. (See below.)
                this.ClearStackBackToTableBodyContext();

                // Pop the current node from the stack of open elements. Switch the insertion mode to "in table".
                this.OpenElements.Pop();
                this.Switch(InsertionModeEnum.InTable);

                // Reprocess the token.
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "td", "th", "tr"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html, Tags.Td, Tags.Th, Tags.Tr))
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                // Process the token using the rules for the "in table" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InTable);
            }
        }

        private void ClearStackBackToTableBodyContext()
        {
            /*
                 When the steps above require the UA to clear the stack back to a table body context,
                 it means that the UA must, while the current node is not a tbody, tfoot, thead, template,
                 or html element, pop elements from the stack of open elements.

                 NOTE: The current node being an html element after this process is a fragment case.
            */
            while (!this.CurrentNode.Is(Tags.TBody, Tags.TFoot, Tags.THead, Tags.Template, Tags.Html))
            {
                this.OpenElements.Pop();
            }
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
                // Clear the stack back to a table row context. (See below.)
                this.ClearStackBackToTableRowContext();

                // Insert an HTML element for the token, then switch the insertion mode to "in cell".
                this.InsertHtmlElementForToken();
                this.Switch(InsertionModeEnum.InCell);

                // Insert a marker at the end of the list of active formatting elements.
                this.ActiveFormattingElements.InsertMarker();
            }

            // An end tag whose tag name is "tr"
            else if (this.Token.IsEndTagNamed(Tags.Tr))
            {
                // If the stack of open elements does not have a tr element in table scope, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(Tags.Tr))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Clear the stack back to a table row context. (See below.)
                this.ClearStackBackToTableRowContext();

                // Pop the current node (which will be a tr element) from the stack of open elements.
                this.OpenElements.Pop();

                // Switch the insertion mode to "in table body".
                this.Switch(InsertionModeEnum.InTableBody);
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "tfoot", "thead", "tr"
            // An end tag whose tag name is "table"
            else if (
                this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr) ||
                this.Token.IsEndTagNamed(Tags.Table))
            {
                // If the stack of open elements does not have a tr element in table scope, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(Tags.Tr))
                {
                    if (this.Token.Type == TokenType.StartTag)
                        this.InformParseError(ParseError.UnexpectedStartTag);
                    else
                        this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Clear the stack back to a table row context. (See below.)
                this.ClearStackBackToTableRowContext();

                // Pop the current node (which will be a tr element) from the stack of open elements.
                this.OpenElements.Pop();

                // Switch the insertion mode to "in table body".
                this.Switch(InsertionModeEnum.InTableBody);

                // Reprocess the token.
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "tbody", "tfoot", "thead"
            else if (this.Token.IsEndTagNamed(Tags.TBody, Tags.TFoot, Tags.THead))
            {
                // If the stack of open elements does not have an element in table scope that is an HTML element
                // and with the same tag name as the token, this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(this.Token.TagName))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // If the stack of open elements does not have a tr element in table scope, ignore the token.
                if (!this.OpenElements.HasElementInTableScope(Tags.Tr))
                    return;

                // Otherwise:
                // Clear the stack back to a table row context. (See below.)
                this.ClearStackBackToTableRowContext();

                // Pop the current node (which will be a tr element) from the stack of open elements.
                this.OpenElements.Pop();

                // Switch the insertion mode to "in table body".
                this.Switch(InsertionModeEnum.InTableBody);

                // Reprocess the token.
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html", "td", "th"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html, Tags.Td, Tags.Th))
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // Anything else
            else
            {
                // Process the token using the rules for the "in table" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InTable);
            }
        }

        private void ClearStackBackToTableRowContext()
        {
            /*
                 When the steps above require the UA to clear the stack back to a table row context, it means that the UA must,
                 while the current node is not a tr, template, or html element, pop elements from the stack of open elements.

                 NOTE: The current node being an html element after this process is a fragment case.
            */
            while (!this.CurrentNode.Is(Tags.Tr, Tags.Template, Tags.Html))
            {
                this.OpenElements.Pop();
            }
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
                // If the stack of open elements does not have an element in table scope that is an HTML element and
                // with the same tag name as that of the token, then this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(this.Token.TagName))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Generate implied end tags.
                this.GenerateImpliedEndTag();

                // Now, if the current node is not an HTML element with the same tag name as the token,
                // then this is a parse error.
                if (!this.CurrentNode.Is(this.Token.TagName))
                    this.InformParseError(ParseError.UnexpectedEndTag);

                // Pop elements from the stack of open elements stack until an HTML element with the same
                // tag name as the token has been popped from the stack.
                this.OpenElements.PopUntil(this.Token.TagName);

                // Clear the list of active formatting elements up to the last marker.
                this.ActiveFormattingElements.ClearFormattingElementsUpToMarker();

                // Switch the insertion mode to "in row".
                this.Switch(InsertionModeEnum.InRow);
            }

            // A start tag whose tag name is one of: "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"
            else if (this.Token.IsStartTagNamed(Tags.Caption, Tags.Col, Tags.ColGroup, Tags.TBody, Tags.Td, Tags.TFoot, Tags.Th, Tags.THead, Tags.Tr))
            {
                // If the stack of open elements does not have a td or th element in table scope, then this is a parse error;
                // ignore the token. (fragment case)
                if (!this.OpenElements.HasElementInTableScope(elem => elem.Is(Tags.Td, Tags.Th)))
                {
                    this.InformParseError(ParseError.UnexpectedStartTag);
                    return;
                }

                // Otherwise, close the cell (see below) and reprocess the token.
                this.CloseTableCells();
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "body", "caption", "col", "colgroup", "html"
            else if (this.Token.IsEndTagNamed(Tags.Body, Tags.Caption, Tags.Col, Tags.ColGroup, Tags.Html))
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // An end tag whose tag name is one of: "table", "tbody", "tfoot", "thead", "tr"
            else if (this.Token.IsEndTagNamed(Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr))
            {
                // If the stack of open elements does not have an element in table scope that is an HTML element
                // and with the same tag name as that of the token, then this is a parse error; ignore the token.
                if (!this.OpenElements.HasElementInTableScope(this.Token.TagName))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, close the cell (see below) and reprocess the token.
                this.CloseTableCells();
                this.ProcessToken();
            }

            // Anything else
            else
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }
        }

        private void CloseTableCells()
        {
            // Where the steps above say to close the cell, they mean to run the following algorithm:
            // 1. Generate implied end tags.
            this.GenerateImpliedEndTag();

            // 2. If the current node is not now a td element or a th element, then this is a parse error.
            if (!this.CurrentNode.Is(Tags.Td, Tags.Th))
                this.InformParseError(ParseError.UnexpectedTag);

            // 3. Pop elements from the stack of open elements stack until a td element or a th element has been popped from the stack.
            this.OpenElements.PopUntil(Tags.Td, Tags.Th);

            // 4. Clear the list of active formatting elements up to the last marker.
            this.ActiveFormattingElements.ClearFormattingElementsUpToMarker();

            // 5. Switch the insertion mode to "in row".
            this.Switch(InsertionModeEnum.InRow);

            // NOTE: The stack of open elements cannot have both a td and a th element in table scope at the same time,
            // nor can it have neither when the close the cell algorithm is invoked.
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
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.NullCharacter);
            }

            // Any other character token
            else if (this.Token.Type == TokenType.Character)
            {
                // Insert the token's character.
                this.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "option"
            else if (this.Token.IsStartTagNamed(Tags.Option))
            {
                // If the current node is an option element, pop that node from the stack of open elements.
                if (this.CurrentNode.Is(Tags.Option))
                    this.OpenElements.Pop();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // A start tag whose tag name is "optgroup"
            else if (this.Token.IsStartTagNamed(Tags.OptGroup))
            {
                // If the current node is an option element, pop that node from the stack of open elements.
                if (this.CurrentNode.Is(Tags.Option))
                    this.OpenElements.Pop();

                // If the current node is an optgroup element, pop that node from the stack of open elements.
                if (this.CurrentNode.Is(Tags.OptGroup))
                    this.OpenElements.Pop();

                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // An end tag whose tag name is "optgroup"
            else if (this.Token.IsEndTagNamed(Tags.OptGroup))
            {
                // First, if the current node is an option element, and the node immediately before it in the stack
                // of open elements is an optgroup element, then pop the current node from the stack of open elements.
                if (this.CurrentNode.Is(Tags.Option) && this.OpenElements[this.OpenElements.Count - 2].Is(Tags.OptGroup))
                    this.OpenElements.Pop();

                // If the current node is an optgroup element, then pop that node from the stack of open elements.
                // Otherwise, this is a parse error; ignore the token.
                if (this.CurrentNode.Is(Tags.OptGroup))
                    this.OpenElements.Pop();
                else
                    this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // An end tag whose tag name is "option"
            else if (this.Token.IsEndTagNamed(Tags.Option))
            {
                // If the current node is an option element, then pop that node from the stack of open elements.
                // Otherwise, this is a parse error; ignore the token.
                if (this.CurrentNode.Is(Tags.Option))
                    this.OpenElements.Pop();
                else
                    this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // An end tag whose tag name is "select"
            else if (this.Token.IsEndTagNamed(Tags.Select))
            {
                // If the stack of open elements does not have a select element in select scope, this is a parse error;
                // ignore the token. (fragment case)
                if (!this.OpenElements.HasElementInTableScope(Tags.Select))
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise:
                // Pop elements from the stack of open elements until a select element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Select);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();
            }

            // A start tag whose tag name is "select"
            else if (this.Token.IsStartTagNamed(Tags.Select))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedStartTag);

                // Pop elements from the stack of open elements until a select element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Select);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();

                // NOTE: It just gets treated like an end tag.
            }

            // A start tag whose tag name is one of: "input", "keygen", "textarea"
            else if (this.Token.IsStartTagNamed(Tags.Input, Tags.KeyGen, Tags.TextArea))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedStartTag);

                // If the stack of open elements does not have a select element in select scope, ignore the token. (fragment case)
                if (!this.OpenElements.HasElementInSelectScope(Tags.Select))
                    return;

                // Pop elements from the stack of open elements until a select element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Select);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();

                // Reprocess the token.
                this.ProcessToken();
            }

            // A start tag whose tag name is one of: "script", "template"
            // An end tag whose tag name is "template"
            else if (this.Token.IsStartTagNamed(Tags.Script, Tags.Template) || this.Token.IsEndTagNamed(Tags.Template))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // Anything else
            else
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedTag);
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
                // Parse error.
                this.InformParseError(ParseError.UnexpectedStartTag);

                // Pop elements from the stack of open elements until a select element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Select);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();

                // Reprocess the token.
                this.ProcessToken();
            }

            // An end tag whose tag name is one of: "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"
            else if (this.Token.IsEndTagNamed(Tags.Caption, Tags.Table, Tags.TBody, Tags.TFoot, Tags.THead, Tags.Tr, Tags.Td, Tags.Th))
            {
                // Parse error.
                this.InformParseError(ParseError.UnexpectedEndTag);

                // If the stack of open elements does not have an element in table scope that is an HTML element
                // and with the same tag name as that of the token, then ignore the token.
                if (!this.OpenElements.HasElementInTableScope(this.Token.TagName))
                    return;

                // Otherwise:
                // Pop elements from the stack of open elements until a select element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Section);

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();

                // Reprocess the token.
                this.ProcessToken();
            }

            // Anything else
            else
            {
                // Process the token using the rules for the "in select" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InSelect);
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
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
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
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // A start tag whose tag name is one of: "caption", "colgroup", "tbody", "tfoot", "thead"
            else if (this.Token.IsStartTagNamed(Tags.Caption, Tags.ColGroup, Tags.TBody, Tags.TFoot, Tags.THead))
            {
                // Pop the current template insertion mode off the stack of template insertion modes.
                this.TemplateInsertionMode.Pop();

                // Push "in table" onto the stack of template insertion modes so that it is the new current template insertion mode.
                this.TemplateInsertionMode.Push(InsertionModeEnum.InTable);

                // Switch the insertion mode to "in table", and reprocess the token.
                this.Switch(InsertionModeEnum.InTable);
                this.ProcessToken();
            }

            // A start tag whose tag name is "col"
            else if (this.Token.IsStartTagNamed(Tags.Col))
            {
                // Pop the current template insertion mode off the stack of template insertion modes.
                this.TemplateInsertionMode.Pop();

                // Push "in column group" onto the stack of template insertion modes so that it is the new
                // current template insertion mode.
                this.TemplateInsertionMode.Push(InsertionModeEnum.InColumnGroup);

                // Switch the insertion mode to "in column group", and reprocess the token.
                this.Switch(InsertionModeEnum.InColumnGroup);
                this.ProcessToken();
            }

            // A start tag whose tag name is "tr"
            else if (this.Token.IsStartTagNamed(Tags.Tr))
            {
                // Pop the current template insertion mode off the stack of template insertion modes.
                this.TemplateInsertionMode.Pop();

                // Push "in table body" onto the stack of template insertion modes so that it is the new
                // current template insertion mode.
                this.TemplateInsertionMode.Push(InsertionModeEnum.InTableBody);

                // Switch the insertion mode to "in table body", and reprocess the token.
                this.Switch(InsertionModeEnum.InTableBody);
                this.ProcessToken();
            }

            // A start tag whose tag name is one of: "td", "th"
            else if (this.Token.IsStartTagNamed(Tags.Td, Tags.Th))
            {
                // Pop the current template insertion mode off the stack of template insertion modes.
                this.TemplateInsertionMode.Pop();

                // Push "in row" onto the stack of template insertion modes so that it is the new current template insertion mode.
                this.TemplateInsertionMode.Push(InsertionModeEnum.InRow);

                // Switch the insertion mode to "in row", and reprocess the token.
                this.Switch(InsertionModeEnum.InRow);
                this.ProcessToken();
            }

            // Any other start tag
            else if (this.Token.Type == TokenType.StartTag)
            {
                // Pop the current template insertion mode off the stack of template insertion modes.
                this.TemplateInsertionMode.Pop();

                // Push "in body" onto the stack of template insertion modes so that it is the new current template insertion mode.
                this.TemplateInsertionMode.Push(InsertionModeEnum.InBody);

                // Switch the insertion mode to "in body", and reprocess the token.
                this.Switch(InsertionModeEnum.InBody);
                this.ProcessToken();
            }

            // Any other end tag
            else if (this.Token.Type == TokenType.EndTag)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedEndTag);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // If there is no template element on the stack of open elements, then stop parsing. (fragment case)
                if (!this.OpenElements.Contains(Tags.Template))
                {
                    this.StopParsing();
                    return;
                }

                // Otherwise, this is a parse error.
                this.InformParseError(ParseError.PrematureEndOfFile);

                // Pop elements from the stack of open elements until a template element has been popped from the stack.
                this.OpenElements.PopUntil(Tags.Template);

                // Clear the list of active formatting elements up to the last marker.
                this.ActiveFormattingElements.ClearFormattingElementsUpToMarker();

                // Pop the current template insertion mode off the stack of template insertion modes.
                this.TemplateInsertionMode.Pop();

                // Reset the insertion mode appropriately.
                this.ResetInsertionModeAppropriately();

                // Reprocess the token.
                this.ProcessToken();
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
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment as the last child of the first element in the stack of open elements (the html element).
                throw new NotImplementedException();
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // An end tag whose tag name is "html"
            else if (this.Token.IsEndTagNamed(Tags.Html))
            {
                // If the parser was originally created as part of the HTML fragment parsing algorithm,
                // this is a parse error; ignore the token. (fragment case)
                if (this.ParsingContext.IsFragmentParsing)
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, switch the insertion mode to "after after body".
                this.Switch(InsertionModeEnum.AfterAfterBody);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Stop parsing.
                this.StopParsing();
            }

            // Anything else
            else
            {
                // Parse error. Switch the insertion mode to "in body" and reprocess the token.
                this.InformParseError(ParseError.UnexpectedTag);
                this.Switch(InsertionModeEnum.InBody);
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
                // Insert the character.
                this.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // A start tag whose tag name is "frameset"
            else if (this.Token.IsStartTagNamed(Tags.Frameset))
            {
                // Insert an HTML element for the token.
                this.InsertHtmlElementForToken();
            }

            // An end tag whose tag name is "frameset"
            else if (this.Token.IsEndTagNamed(Tags.Frameset))
            {
                // If the current node is the root html element, then this is a parse error; ignore the token. (fragment case)
                if (this.CurrentNode == this.OpenElements[0])
                {
                    this.InformParseError(ParseError.UnexpectedEndTag);
                    return;
                }

                // Otherwise, pop the current node from the stack of open elements.
                this.OpenElements.Pop();

                // If the parser was not originally created as part of the HTML fragment parsing algorithm (fragment case),
                // and the current node is no longer a frameset element, then switch the insertion mode to "after frameset".
                if (!this.ParsingContext.IsFragmentParsing && !this.CurrentNode.Is(Tags.Frameset))
                    this.Switch(InsertionModeEnum.AfterFrameset);
            }

            // A start tag whose tag name is "frame"
            else if (this.Token.IsStartTagNamed(Tags.Frame))
            {
                // Insert an HTML element for the token. Immediately pop the current node off the stack of open elements.
                this.InsertHtmlElementForToken();
                this.OpenElements.Pop();

                // Acknowledge the token's self-closing flag, if it is set.
                if (this.Token.TagIsSelfClosing)
                    this.Tokenizer.AcknowledgeSelfClosingTag();
            }

            // A start tag whose tag name is "noframes"
            else if (this.Token.IsStartTagNamed(Tags.NoFrames))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // If the current node is not the root html element, then this is a parse error.
                if (this.CurrentNode != this.OpenElements[0])
                    this.InformParseError(ParseError.PrematureEndOfFile);

                // NOTE: The current node can only be the root html element in the fragment case.

                // Stop parsing.
                this.StopParsing();
            }

            // Anything else
            else
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedTag);
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
                // Insert the character.
                this.InsertCharacter(this.Token.Character);
            }

            // A comment token
            else if (this.Token.Type == TokenType.Comment)
            {
                // Insert a comment.
                this.InsertComment(this.Token.CommentData);
            }

            // A DOCTYPE token
            else if (this.Token.Type == TokenType.DocType)
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedDocType);
            }

            // A start tag whose tag name is "html"
            else if (this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // An end tag whose tag name is "html"
            else if (this.Token.IsEndTagNamed(Tags.Html))
            {
                // Switch the insertion mode to "after after frameset".
                this.Switch(InsertionModeEnum.AfterAfterFrameset);
            }

            // A start tag whose tag name is "noframes"
            else if (this.Token.IsStartTagNamed(Tags.NoFrames))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Stop parsing.
                this.StopParsing();
            }

            // Anything else
            else
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedTag);
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
                // Insert a comment as the last child of the Document object.
                throw new NotImplementedException();
            }

            // A DOCTYPE token
            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            // A start tag whose tag name is "html"
            else if ((this.Token.Type == TokenType.DocType) || this.Token.IsCharacterWhitespace() || this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Stop parsing.
                this.StopParsing();
            }

            // Anything else
            else
            {
                // Parse error. Switch the insertion mode to "in body" and reprocess the token.
                this.InformParseError(ParseError.UnexpectedTag);
                this.Switch(InsertionModeEnum.InBody);
                this.ProcessToken();
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
                // Insert a comment as the last child of the Document object.
                throw new NotImplementedException();
            }

            // A DOCTYPE token
            // A character token that is one of U+0009 CHARACTER TABULATION, "LF" (U+000A),
            // "FF" (U+000C), "CR" (U+000D), or U+0020 SPACE
            // A start tag whose tag name is "html"
            else if ((this.Token.Type == TokenType.DocType) || this.Token.IsCharacterWhitespace() || this.Token.IsStartTagNamed(Tags.Html))
            {
                // Process the token using the rules for the "in body" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InBody);
            }

            // An end-of-file token
            else if (this.Token.Type == TokenType.EndOfFile)
            {
                // Stop parsing.
                this.StopParsing();
            }

            // A start tag whose tag name is "noframes"
            else if (this.Token.IsStartTagNamed(Tags.NoFrames))
            {
                // Process the token using the rules for the "in head" insertion mode.
                this.ProcessTokenUsing(InsertionModeEnum.InHead);
            }

            // Anything else
            else
            {
                // Parse error. Ignore the token.
                this.InformParseError(ParseError.UnexpectedTag);
            }
        }

        #endregion

        #region 8.2.5.5. The rules for parsing tokens in foreign content

        private void ProcessTokenInForeignContent()
        {
            throw new NotImplementedException();
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
            while (this.OpenElements.Count > 0)
            {
                this.OpenElements.Pop();
            }

            // A lot of steps irrelevant to us.
            this.DomFactory.StopParsing();
        }

        #endregion
    }
}

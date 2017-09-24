/*
 * **************************************************************************
 *
 * Copyright (c) Todor Todorov / Scientia Software. 
 *
 * This source code is subject to terms and conditions of the 
 * license agreement found in the project directory. 
 * See: $(ProjectDir)\LICENSE.txt ... in the root of this project.
 * By using this source code in any fashion, you are agreeing 
 * to be bound by the terms of the license agreement.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Html5.Parsing
{
    internal class Tokenizer
    {
        internal enum StateEnum
        {
            /// <summary>
            /// Data state
            /// See: 8.2.4.1 Data state (http://www.w3.org/TR/html52/syntax.html#tokenizer-data-state)
            /// </summary>
            Data = 1,

            /// <summary>
            /// RCDATA state
            /// See: 8.2.4.2 RCDATA state (http://www.w3.org/TR/html52/syntax.html#tokenizer-rcdata-state)
            /// </summary>
            RcData = 2,

            /// <summary>
            /// RAWTEXT state
            /// See: 8.2.4.3 RAWTEXT state (http://www.w3.org/TR/html52/syntax.html#tokenizer-rawtext-state)
            /// </summary>
            RawText = 3,

            /// <summary>
            /// Script data state
            /// See: 8.2.4.4 Script data state (http://www.w3.org/TR/html52/syntax.html#tokenizer-script-data-state)
            /// </summary>
            ScriptData = 4,

            /// <summary>
            /// PLAINTEXT state
            /// See: 8.2.4.5 PLAINTEXT state (http://www.w3.org/TR/html52/syntax.html#plaintext-state)
            /// </summary>
            PlainText = 5,

            /// <summary>
            /// Tag open state
            /// See: 8.2.4.6 Tag open state (http://www.w3.org/TR/html52/syntax.html#tokenizer-tag-open-state)
            /// </summary>
            TagOpen = 6,

            /// <summary>
            /// End tag open state
            /// See: 8.2.4.7 End tag open state (http://www.w3.org/TR/html52/syntax.html#end-tag-open-state)
            /// </summary>
            EndTagOpen = 7,

            /// <summary>
            /// Tag name state
            /// See: 8.2.4.8 Tag name state (http://www.w3.org/TR/html52/syntax.html#tag-name-state)
            /// </summary>
            TagName = 8,

            /// <summary>
            /// RCDATA less-than sign state
            /// See: 8.2.4.9 RCDATA less-than sign state (http://www.w3.org/TR/html52/syntax.html#RCDATA-less-than-sign-state)
            /// </summary>
            RcDataLessThanSign = 9,

            /// <summary>
            /// RCDATA end tag open state
            /// See: 8.2.4.10 RCDATA end tag open state (http://www.w3.org/TR/html52/syntax.html#RCDATA-end-tag-open-state)
            /// </summary>
            RcDataEndTagOpen = 10,

            /// <summary>
            /// RCDATA end tag name state
            /// See: 8.2.4.11 RCDATA end tag name state (http://www.w3.org/TR/html52/syntax.html#RCDATA-end-tag-name-state)
            /// </summary>
            RcDataEndTagName = 11,

            /// <summary>
            /// RAWTEXT less-than sign state
            /// See: 8.2.4.12 RAWTEXT less-than sign state (http://www.w3.org/TR/html52/syntax.html#rawtext-less-than-sign-state)
            /// </summary>
            RawTextLessThanSign = 12,

            /// <summary>
            /// RAWTEXT end tag open state
            /// See: 8.2.4.13 RAWTEXT end tag open state (http://www.w3.org/TR/html52/syntax.html#rawtext-end-tag-open-state)
            /// </summary>
            RawTextEndTagOpen = 13,

            /// <summary>
            /// RAWTEXT end tag name state
            /// See: 8.2.4.14 RAWTEXT end tag name state (http://www.w3.org/TR/html52/syntax.html#rawtext-end-tag-name-state)
            /// </summary>
            RawTextEndTagName = 14,

            /// <summary>
            /// Script data less-than sign state
            /// See: 8.2.4.15 Script data less-than sign state (http://www.w3.org/TR/html52/syntax.html#script-data-less-than-sign-state)
            /// </summary>
            ScriptDataLessThanSign = 15,

            /// <summary>
            /// Script data end tag open state
            /// See: 8.2.4.16 Script data end tag open state (http://www.w3.org/TR/html52/syntax.html#script-data-end-tag-open-state)
            /// </summary>
            ScriptDataEndTagOpen = 16,

            /// <summary>
            /// Script data end tag name state
            /// See: 8.2.4.17 Script data end tag name state (http://www.w3.org/TR/html52/syntax.html#script-data-end-tag-name-state)
            /// </summary>
            ScriptDataEndTagName = 17,

            /// <summary>
            /// Script data escape start state
            /// See: 8.2.4.18 Script data escape start state (http://www.w3.org/TR/html52/syntax.html#script-data-escape-start-state)
            /// </summary>
            ScriptDataEscapeStart = 18,

            /// <summary>
            /// Script data escape start dash state
            /// See: 8.2.4.19 Script data escape start dash state (http://www.w3.org/TR/html52/syntax.html#script-data-escape-start-dash-state)
            /// </summary>
            ScriptDataEscapeStartDash = 19,

            /// <summary>
            /// Script data escaped state
            /// See: 8.2.4.20 Script data escaped state (http://www.w3.org/TR/html52/syntax.html#script-data-escaped-state)
            /// </summary>
            ScriptDataEscaped = 20,

            /// <summary>
            /// Script data escaped dash state
            /// See: 8.2.4.21 Script data escaped dash state (http://www.w3.org/TR/html52/syntax.html#script-data-escaped-dash-state)
            /// </summary>
            ScriptDataEscapedDash = 21,

            /// <summary>
            /// Script data escaped dash dash state
            /// See: 8.2.4.22 Script data escaped dash dash state (http://www.w3.org/TR/html52/syntax.html#script-data-escaped-dash-dash-state)
            /// </summary>
            ScriptDataEscapedDashDash = 22,

            /// <summary>
            /// Script data escaped less-than sign state
            /// See: 8.2.4.23 Script data escaped less-than sign state (http://www.w3.org/TR/html52/syntax.html#script-data-escaped-less-than-sign-state)
            /// </summary>
            ScriptDataEscapedLessThanSign = 23,

            /// <summary>
            /// Script data escaped end tag open state
            /// See: 8.2.4.24 Script data escaped end tag open state (http://www.w3.org/TR/html52/syntax.html#script-data-escaped-end-tag-open-state)
            /// </summary>
            ScriptDataEscapedEndTagOpen = 24,

            /// <summary>
            /// Script data escaped end tag name state
            /// See: 8.2.4.25 Script data escaped end tag name state (http://www.w3.org/TR/html52/syntax.html#script-data-escaped-end-tag-name-state)
            /// </summary>
            ScriptDataEscapedEndTagName = 25,

            /// <summary>
            /// Script data double escape start state
            /// See: 8.2.4.26 Script data double escape start state (http://www.w3.org/TR/html52/syntax.html#script-data-double-escape-start-state)
            /// </summary>
            ScriptDataDoubleEscapeStart = 26,

            /// <summary>
            /// Script data double escaped state
            /// See: 8.2.4.27 Script data double escaped state (http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-state)
            /// </summary>
            ScriptDataDoubleEscaped = 27,

            /// <summary>
            /// Script data double escaped dash state
            /// See: 8.2.4.28 Script data double escaped dash state (http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-dash-state)
            /// </summary>
            ScriptDataDoubleEscapedDash = 28,

            /// <summary>
            /// Script data double escaped dash dash state
            /// See: 8.2.4.29 Script data double escaped dash dash state (http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-dash-dash-state)
            /// </summary>
            ScriptDataDoubleEscapedDashDash = 29,

            /// <summary>
            /// Script data double escaped less-than sign state
            /// See: 8.2.4.30 Script data double escaped less-than sign state (http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-less-than-sign-state)
            /// </summary>
            ScriptDataDoubleEscapedLessThanSign = 30,

            /// <summary>
            /// Script data double escape end state
            /// See: 8.2.4.31 Script data double escape end state (http://www.w3.org/TR/html52/syntax.html#script-data-double-escape-end-state)
            /// </summary>
            ScriptDataDoubleEscapeEnd = 31,

            /// <summary>
            /// Before attribute name state
            /// See: 8.2.4.32 Before attribute name state (http://www.w3.org/TR/html52/syntax.html#before-attribute-name-state)
            /// </summary>
            BeforeAttributeName = 32,

            /// <summary>
            /// Attribute name state
            /// See: 8.2.4.33 Attribute name state (http://www.w3.org/TR/html52/syntax.html#attribute-name-state)
            /// </summary>
            AttributeName = 33,

            /// <summary>
            /// After attribute name state
            /// See: 8.2.4.34 After attribute name state (http://www.w3.org/TR/html52/syntax.html#after-attribute-name-state)
            /// </summary>
            AfterAttributeName = 34,

            /// <summary>
            /// Before attribute value state
            /// See: 8.2.4.35 Before attribute value state (http://www.w3.org/TR/html52/syntax.html#before-attribute-value-state)
            /// </summary>
            BeforeAttributeValue = 35,

            /// <summary>
            /// Attribute value (double-quoted) state
            /// See: 8.2.4.36 Attribute value (double-quoted) state (http://www.w3.org/TR/html52/syntax.html#attribute-value-double-quoted-state)
            /// </summary>
            AttributeValueDoubleQuoted = 36,

            /// <summary>
            /// Attribute value (single-quoted) state
            /// See: 8.2.4.37 Attribute value (single-quoted) state (http://www.w3.org/TR/html52/syntax.html#attribute-value-single-quoted-state)
            /// </summary>
            AttributeValueSingleQuoted = 37,

            /// <summary>
            /// Attribute value (unquoted) state
            /// See: 8.2.4.38 Attribute value (unquoted) state (http://www.w3.org/TR/html52/syntax.html#attribute-value-unquoted-state)
            /// </summary>
            AttributeValueUnquoted = 38,

            /// <summary>
            /// After attribute value (quoted) state
            /// See: 8.2.4.39 After attribute value (quoted) state (http://www.w3.org/TR/html52/syntax.html#after-attribute-value-quoted-state)
            /// </summary>
            AfterAttributeValueQuoted = 39,

            /// <summary>
            /// Self-closing start tag state
            /// See: 8.2.4.40 Self-closing start tag state (http://www.w3.org/TR/html52/syntax.html#self-closing-start-tag-state)
            /// </summary>
            SelfClosingStartTag = 40,

            /// <summary>
            /// Bogus comment state
            /// See: 8.2.4.41 Bogus comment state (http://www.w3.org/TR/html52/syntax.html#bogus-comment-state)
            /// </summary>
            BogusComment = 41,

            /// <summary>
            /// Markup declaration open state
            /// See: 8.2.4.42 Markup declaration open state (http://www.w3.org/TR/html52/syntax.html#markup-declaration-open-state)
            /// </summary>
            MarkupDeclarationOpen = 42,

            /// <summary>
            /// Comment start state
            /// See: 8.2.4.43 Comment start state (http://www.w3.org/TR/html52/syntax.html#comment-start-state)
            /// </summary>
            CommentStart = 43,

            /// <summary>
            /// Comment start dash state
            /// See: 8.2.4.44 Comment start dash state (http://www.w3.org/TR/html52/syntax.html#comment-start-dash-state)
            /// </summary>
            CommentStartDash = 44,

            /// <summary>
            /// Comment state
            /// See: 8.2.4.45 Comment state (http://www.w3.org/TR/html52/syntax.html#comment-state)
            /// </summary>
            Comment = 45,

            /// <summary>
            /// Comment less-than sign state
            /// See: 8.2.4.46 Comment less-than sign state (http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-state)
            /// </summary>
            CommentLessThanSign = 46,

            /// <summary>
            /// Comment less-than sign bang state
            /// See: 8.2.4.47 Comment less-than sign bang state (http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-bang-state)
            /// </summary>
            CommentLessThanSignBang = 47,

            /// <summary>
            /// Comment less-than sign bang dash state
            /// See: 8.2.4.48 Comment less-than sign bang dash state (http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-bang-dash-state)
            /// </summary>
            CommentLessThanSignBangDash = 48,

            /// <summary>
            /// Comment less-than sign bang dash dash state
            /// See: 8.2.4.49 Comment less-than sign bang dash dash state (http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-bang-dash-dash-state)
            /// </summary>
            CommentLessThanSignBangDashDash = 49,

            /// <summary>
            /// Comment end dash state
            /// See: 8.2.4.50 Comment end dash state (http://www.w3.org/TR/html52/syntax.html#comment-end-dash-state)
            /// </summary>
            CommentEndDash = 50,

            /// <summary>
            /// Comment end state
            /// See: 8.2.4.51 Comment end state (http://www.w3.org/TR/html52/syntax.html#comment-end-state)
            /// </summary>
            CommentEnd = 51,

            /// <summary>
            /// Comment end bang state
            /// See: 8.2.4.52 Comment end bang state (http://www.w3.org/TR/html52/syntax.html#comment-end-bang-state)
            /// </summary>
            CommentEndBang = 52,

            /// <summary>
            /// DOCTYPE state
            /// See: 8.2.4.53 DOCTYPE state (http://www.w3.org/TR/html52/syntax.html#doctype-state)
            /// </summary>
            DocType = 53,

            /// <summary>
            /// Before DOCTYPE name state
            /// See: 8.2.4.54 Before DOCTYPE name state (http://www.w3.org/TR/html52/syntax.html#before-doctype-name-state)
            /// </summary>
            BeforeDocTypeName = 54,

            /// <summary>
            /// DOCTYPE name state
            /// See: 8.2.4.55 DOCTYPE name state (http://www.w3.org/TR/html52/syntax.html#doctype-name-state)
            /// </summary>
            DocTypeName = 55,

            /// <summary>
            /// After DOCTYPE name state
            /// See: 8.2.4.56 After DOCTYPE name state (http://www.w3.org/TR/html52/syntax.html#after-doctype-name-state)
            /// </summary>
            AfterDocTypeName = 56,

            /// <summary>
            /// After DOCTYPE public keyword state
            /// See: 8.2.4.57 After DOCTYPE public keyword state (http://www.w3.org/TR/html52/syntax.html#after-doctype-public-keyword-state)
            /// </summary>
            AfterDocTypePublicKeyword = 57,

            /// <summary>
            /// Before DOCTYPE public identifier state
            /// See: 8.2.4.58 Before DOCTYPE public identifier state (http://www.w3.org/TR/html52/syntax.html#before-doctype-public-identifier-state)
            /// </summary>
            BeforeDocTypePublicIdentifier = 58,

            /// <summary>
            /// DOCTYPE public identifier (double-quoted) state
            /// See: 8.2.4.59 DOCTYPE public identifier (double-quoted) state (http://www.w3.org/TR/html52/syntax.html#doctype-public-identifier-double-quoted-state)
            /// </summary>
            DocTypePublicIdentifierDoubleQuoted = 59,

            /// <summary>
            /// DOCTYPE public identifier (single-quoted) state
            /// See: 8.2.4.60 DOCTYPE public identifier (single-quoted) state (http://www.w3.org/TR/html52/syntax.html#doctype-public-identifier-single-quoted-state)
            /// </summary>
            DocTypePublicIdentifierSingleQuoted = 60,

            /// <summary>
            /// After DOCTYPE public identifier state
            /// See: 8.2.4.61 After DOCTYPE public identifier state (http://www.w3.org/TR/html52/syntax.html#after-doctype-public-identifier-state)
            /// </summary>
            AfterDocTypePublicIdentifier = 61,

            /// <summary>
            /// Between DOCTYPE public and system identifiers state
            /// See: 8.2.4.62 Between DOCTYPE public and system identifiers state (http://www.w3.org/TR/html52/syntax.html#between-doctype-public-and-system-identifiers-state)
            /// </summary>
            BetweenDocTypePublicAndSystemIdentifiers = 62,

            /// <summary>
            /// After DOCTYPE system keyword state
            /// See: 8.2.4.63 After DOCTYPE system keyword state (http://www.w3.org/TR/html52/syntax.html#after-doctype-system-keyword-state)
            /// </summary>
            AfterDocTypeSystemKeyword = 63,

            /// <summary>
            /// Before DOCTYPE system identifier state
            /// See: 8.2.4.64 Before DOCTYPE system identifier state (http://www.w3.org/TR/html52/syntax.html#before-doctype-system-identifier-state)
            /// </summary>
            BeforeDocTypeSystemIdentifier = 64,

            /// <summary>
            /// DOCTYPE system identifier (double-quoted) state
            /// See: 8.2.4.65 DOCTYPE system identifier (double-quoted) state (http://www.w3.org/TR/html52/syntax.html#doctype-system-identifier-double-quoted-state)
            /// </summary>
            DocTypeSystemIdentifierDoubleQuoted = 65,

            /// <summary>
            /// DOCTYPE system identifier (single-quoted) state
            /// See: 8.2.4.66 DOCTYPE system identifier (single-quoted) state (http://www.w3.org/TR/html52/syntax.html#doctype-system-identifier-single-quoted-state)
            /// </summary>
            DocTypeSystemIdentifierSingleQuoted = 66,

            /// <summary>
            /// After DOCTYPE system identifier state
            /// See: 8.2.4.67 After DOCTYPE system identifier state (http://www.w3.org/TR/html52/syntax.html#after-doctype-system-identifier-state)
            /// </summary>
            AfterDocTypeSystemIdentifier = 67,

            /// <summary>
            /// Bogus DOCTYPE state
            /// See: 8.2.4.68 Bogus DOCTYPE state (http://www.w3.org/TR/html52/syntax.html#bogus-doctype-state)
            /// </summary>
            BogusDocType = 68,

            /// <summary>
            /// CDATA section state
            /// See: 8.2.4.69 CDATA section state (http://www.w3.org/TR/html52/syntax.html#CDATA-section-state)
            /// </summary>
            CDataSection = 69,

            /// <summary>
            /// CDATA section bracket state
            /// See: 8.2.4.70 CDATA section bracket state (http://www.w3.org/TR/html52/syntax.html#CDATA-section-bracket-state)
            /// </summary>
            CDataSectionBracket = 70,

            /// <summary>
            /// CDATA section end state
            /// See: 8.2.4.71 CDATA section end state (http://www.w3.org/TR/html52/syntax.html#CDATA-section-end-state)
            /// </summary>
            CDataSectionEnd = 71,

            /// <summary>
            /// Character reference state
            /// See: 8.2.4.72 Character reference state (http://www.w3.org/TR/html52/syntax.html#character-reference-state)
            /// </summary>
            CharacterReference = 72,

            /// <summary>
            /// Numeric character reference state
            /// See: 8.2.4.73 Numeric character reference state (http://www.w3.org/TR/html52/syntax.html#numeric-character-reference-state)
            /// </summary>
            NumericCharacterReference = 73,

            /// <summary>
            /// Hexadecimal character reference start state
            /// See: 8.2.4.74 Hexadecimal character reference start state (http://www.w3.org/TR/html52/syntax.html#hexadecimal-character-reference-start-state)
            /// </summary>
            HexadecimalCharacterReferenceStart = 74,

            /// <summary>
            /// Decimal character reference start state
            /// See: 8.2.4.75 Decimal character reference start state (http://www.w3.org/TR/html52/syntax.html#decimal-character-reference-start-state)
            /// </summary>
            DecimalCharacterReferenceStart = 75,

            /// <summary>
            /// Hexadecimal character reference state
            /// See: 8.2.4.76 Hexadecimal character reference state (http://www.w3.org/TR/html52/syntax.html#hexadecimal-character-reference-state)
            /// </summary>
            HexadecimalCharacterReference = 76,

            /// <summary>
            /// Decimal character reference state
            /// See: 8.2.4.77 Decimal character reference state (http://www.w3.org/TR/html52/syntax.html#decimal-character-reference-state)
            /// </summary>
            DecimalCharacterReference = 77,

            /// <summary>
            /// Numeric character reference end state
            /// See: 8.2.4.78 Numeric character reference end state (http://www.w3.org/TR/html52/syntax.html#numeric-character-reference-end-state)
            /// </summary>
            NumericCharacterReferenceEnd = 78,

            /// <summary>
            /// Character reference end state
            /// See: 8.2.4.79 Character reference end state (http://www.w3.org/TR/html52/syntax.html#character-reference-end-state)
            /// </summary>
            CharacterReferenceEnd = 79
        }

        #region Public Interface

        public Tokenizer(HtmlStream stream)
        {
            Contract.RequiresNotNull(stream, nameof(stream));

            this.HtmlStream = stream;
            this.CurrentLine = 1;
            this.CurrentColumn = 1;
        }

        public Token LastToken
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.CurrentToken;
            }
        }

        private Token CurrentToken;

        public Token GetNextToken()
        {
            // When a start tag token is emitted with its self-closing flag set,
            // if the flag is not acknowledged when it is processed by the tree
            // construction stage, that is a parse error.
            if ((this.LastToken.Type == TokenType.StartTag) && this.LastToken.TagIsSelfClosing && !this.SelfClosingTagAcknowledged)
                this.InformParseError(Parsing.ParseError.UnexpectedTag);

            this.SelfClosingTagAcknowledged = false;
            this.CurrentToken.ResetToken();

            // See if any token is enqueued.
            if (this.EmitTokenQueue.Count != 0)
            {
                Action emitAction = this.EmitTokenQueue.Dequeue();
                emitAction();
            }
            else
            {
                // Continue tokenizing until a token has been emitted.
                while (this.CurrentToken.Type == TokenType.Unknown)
                {
                    this.Tokenize();
                }
            }

            return this.CurrentToken;
        }

        private readonly Queue<Action> EmitTokenQueue = new Queue<Action>();

        /// <summary>
        /// Used by the tree parsing stage to inform the tokenizer that a
        /// self-closing tag has been acknowledged.
        /// </summary>
        public void AcknowledgeSelfClosingTag()
        {
            this.SelfClosingTagAcknowledged = true;
        }

        private bool SelfClosingTagAcknowledged = false;

        /// <summary>
        /// Raised when a parse error occurs during tokanization of the HTML stream.
        /// </summary>
        public event EventHandler<ParseErrorEventArgs> ParseError;

        #endregion

        #region Behavior and Logic

        #region State related logic

        // The state machine must start in the data state.
        private StateEnum State = StateEnum.Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Tokenize()
        {
            switch (this.State)
            {
                case StateEnum.Data:
                    this.HandleStateData();
                    break;
                case StateEnum.RcData:
                    this.HandleStateRcData();
                    break;
                case StateEnum.RawText:
                    this.HandleStateRawText();
                    break;
                case StateEnum.ScriptData:
                    this.HandleStateScriptData();
                    break;
                case StateEnum.PlainText:
                    this.HandleStatePlainText();
                    break;
                case StateEnum.TagOpen:
                    this.HandleStateTagOpen();
                    break;
                case StateEnum.EndTagOpen:
                    this.HandleStateEndTagOpen();
                    break;
                case StateEnum.TagName:
                    this.HandleStateTagName();
                    break;
                case StateEnum.RcDataLessThanSign:
                    this.HandleStateRcDataLessThanSign();
                    break;
                case StateEnum.RcDataEndTagOpen:
                    this.HandleStateRcDataEndTagOpen();
                    break;
                case StateEnum.RcDataEndTagName:
                    this.HandleStateRcDataEndTagName();
                    break;
                case StateEnum.RawTextLessThanSign:
                    this.HandleStateRawTextLessThanSign();
                    break;
                case StateEnum.RawTextEndTagOpen:
                    this.HandleStateRawTextEndTagOpen();
                    break;
                case StateEnum.RawTextEndTagName:
                    this.HandleStateRawTextEndTagName();
                    break;
                case StateEnum.ScriptDataLessThanSign:
                    this.HandleStateScriptDataLessThanSign();
                    break;
                case StateEnum.ScriptDataEndTagOpen:
                    this.HandleStateScriptDataEndTagOpen();
                    break;
                case StateEnum.ScriptDataEndTagName:
                    this.HandleStateScriptDataEndTagName();
                    break;
                case StateEnum.ScriptDataEscapeStart:
                    this.HandleStateScriptDataEscapeStart();
                    break;
                case StateEnum.ScriptDataEscapeStartDash:
                    this.HandleStateScriptDataEscapeStartDash();
                    break;
                case StateEnum.ScriptDataEscaped:
                    this.HandleStateScriptDataEscaped();
                    break;
                case StateEnum.ScriptDataEscapedDash:
                    this.HandleStateScriptDataEscapedDash();
                    break;
                case StateEnum.ScriptDataEscapedDashDash:
                    this.HandleStateScriptDataEscapedDashDash();
                    break;
                case StateEnum.ScriptDataEscapedLessThanSign:
                    this.HandleStateScriptDataEscapedLessThanSign();
                    break;
                case StateEnum.ScriptDataEscapedEndTagOpen:
                    this.HandleStateScriptDataEscapedEndTagOpen();
                    break;
                case StateEnum.ScriptDataEscapedEndTagName:
                    this.HandleStateScriptDataEscapedEndTagName();
                    break;
                case StateEnum.ScriptDataDoubleEscapeStart:
                    this.HandleStateScriptDataDoubleEscapeStart();
                    break;
                case StateEnum.ScriptDataDoubleEscaped:
                    this.HandleStateScriptDataDoubleEscaped();
                    break;
                case StateEnum.ScriptDataDoubleEscapedDash:
                    this.HandleStateScriptDataDoubleEscapedDash();
                    break;
                case StateEnum.ScriptDataDoubleEscapedDashDash:
                    this.HandleStateScriptDataDoubleEscapedDashDash();
                    break;
                case StateEnum.ScriptDataDoubleEscapedLessThanSign:
                    this.HandleStateScriptDataDoubleEscapedLessThanSign();
                    break;
                case StateEnum.ScriptDataDoubleEscapeEnd:
                    this.HandleStateScriptDataDoubleEscapeEnd();
                    break;
                case StateEnum.BeforeAttributeName:
                    this.HandleStateBeforeAttributeName();
                    break;
                case StateEnum.AttributeName:
                    this.HandleStateAttributeName();
                    break;
                case StateEnum.AfterAttributeName:
                    this.HandleStateAfterAttributeName();
                    break;
                case StateEnum.BeforeAttributeValue:
                    this.HandleStateBeforeAttributeValue();
                    break;
                case StateEnum.AttributeValueDoubleQuoted:
                    this.HandleStateAttributeValueDoubleQuoted();
                    break;
                case StateEnum.AttributeValueSingleQuoted:
                    this.HandleStateAttributeValueSingleQuoted();
                    break;
                case StateEnum.AttributeValueUnquoted:
                    this.HandleStateAttributeValueUnquoted();
                    break;
                case StateEnum.AfterAttributeValueQuoted:
                    this.HandleStateAfterAttributeValueQuoted();
                    break;
                case StateEnum.SelfClosingStartTag:
                    this.HandleStateSelfClosingStartTag();
                    break;
                case StateEnum.BogusComment:
                    this.HandleStateBogusComment();
                    break;
                case StateEnum.MarkupDeclarationOpen:
                    this.HandleStateMarkupDeclarationOpen();
                    break;
                case StateEnum.CommentStart:
                    this.HandleStateCommentStart();
                    break;
                case StateEnum.CommentStartDash:
                    this.HandleStateCommentStartDash();
                    break;
                case StateEnum.Comment:
                    this.HandleStateComment();
                    break;
                case StateEnum.CommentLessThanSign:
                    this.HandleStateCommentLessThanSign();
                    break;
                case StateEnum.CommentLessThanSignBang:
                    this.HandleStateCommentLessThanSignBang();
                    break;
                case StateEnum.CommentLessThanSignBangDash:
                    this.HandleStateCommentLessThanSignBangDash();
                    break;
                case StateEnum.CommentLessThanSignBangDashDash:
                    this.HandleStateCommentLessThanSignBangDashDash();
                    break;
                case StateEnum.CommentEndDash:
                    this.HandleStateCommentEndDash();
                    break;
                case StateEnum.CommentEnd:
                    this.HandleStateCommentEnd();
                    break;
                case StateEnum.CommentEndBang:
                    this.HandleStateCommentEndBang();
                    break;
                case StateEnum.DocType:
                    this.HandleStateDocType();
                    break;
                case StateEnum.BeforeDocTypeName:
                    this.HandleStateBeforeDocTypeName();
                    break;
                case StateEnum.DocTypeName:
                    this.HandleStateDocTypeName();
                    break;
                case StateEnum.AfterDocTypeName:
                    this.HandleStateAfterDocTypeName();
                    break;
                case StateEnum.AfterDocTypePublicKeyword:
                    this.HandleStateAfterDocTypePublicKeyword();
                    break;
                case StateEnum.BeforeDocTypePublicIdentifier:
                    this.HandleStateBeforeDocTypePublicIdentifier();
                    break;
                case StateEnum.DocTypePublicIdentifierDoubleQuoted:
                    this.HandleStateDocTypePublicIdentifierDoubleQuoted();
                    break;
                case StateEnum.DocTypePublicIdentifierSingleQuoted:
                    this.HandleStateDocTypePublicIdentifierSingleQuoted();
                    break;
                case StateEnum.AfterDocTypePublicIdentifier:
                    this.HandleStateAfterDocTypePublicIdentifier();
                    break;
                case StateEnum.BetweenDocTypePublicAndSystemIdentifiers:
                    this.HandleStateBetweenDocTypePublicAndSystemIdentifiers();
                    break;
                case StateEnum.AfterDocTypeSystemKeyword:
                    this.HandleStateAfterDocTypeSystemKeyword();
                    break;
                case StateEnum.BeforeDocTypeSystemIdentifier:
                    this.HandleStateBeforeDocTypeSystemIdentifier();
                    break;
                case StateEnum.DocTypeSystemIdentifierDoubleQuoted:
                    this.HandleStateDocTypeSystemIdentifierDoubleQuoted();
                    break;
                case StateEnum.DocTypeSystemIdentifierSingleQuoted:
                    this.HandleStateDocTypeSystemIdentifierSingleQuoted();
                    break;
                case StateEnum.AfterDocTypeSystemIdentifier:
                    this.HandleStateAfterDocTypeSystemIdentifier();
                    break;
                case StateEnum.BogusDocType:
                    this.HandleStateBogusDocType();
                    break;
                case StateEnum.CDataSection:
                    this.HandleStateCDataSection();
                    break;
                case StateEnum.CDataSectionBracket:
                    this.HandleStateCDataSectionBracket();
                    break;
                case StateEnum.CDataSectionEnd:
                    this.HandleStateCDataSectionEnd();
                    break;
                case StateEnum.CharacterReference:
                    this.HandleStateCharacterReference();
                    break;
                case StateEnum.NumericCharacterReference:
                    this.HandleStateNumericCharacterReference();
                    break;
                case StateEnum.HexadecimalCharacterReferenceStart:
                    this.HandleStateHexadecimalCharacterReferenceStart();
                    break;
                case StateEnum.DecimalCharacterReferenceStart:
                    this.HandleStateDecimalCharacterReferenceStart();
                    break;
                case StateEnum.HexadecimalCharacterReference:
                    this.HandleStateHexadecimalCharacterReference();
                    break;
                case StateEnum.DecimalCharacterReference:
                    this.HandleStateDecimalCharacterReference();
                    break;
                case StateEnum.NumericCharacterReferenceEnd:
                    this.HandleStateNumericCharacterReferenceEnd();
                    break;
                case StateEnum.CharacterReferenceEnd:
                    this.HandleStateCharacterReferenceEnd();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal void SwitchTo(StateEnum state)
        {
            switch (this.State)
            {
                case StateEnum.Data:
                    break;
                case StateEnum.RcData:
                    break;
                case StateEnum.RawText:
                    break;
                case StateEnum.ScriptData:
                    break;
                case StateEnum.PlainText:
                    break;
                case StateEnum.TagOpen:
                    break;
                case StateEnum.EndTagOpen:
                    break;
                case StateEnum.TagName:
                    break;
                case StateEnum.RcDataLessThanSign:
                    break;
                case StateEnum.RcDataEndTagOpen:
                    break;
                case StateEnum.RcDataEndTagName:
                    break;
                case StateEnum.RawTextLessThanSign:
                    break;
                case StateEnum.RawTextEndTagOpen:
                    break;
                case StateEnum.RawTextEndTagName:
                    break;
                case StateEnum.ScriptDataLessThanSign:
                    break;
                case StateEnum.ScriptDataEndTagOpen:
                    break;
                case StateEnum.ScriptDataEndTagName:
                    break;
                case StateEnum.ScriptDataEscapeStart:
                    break;
                case StateEnum.ScriptDataEscapeStartDash:
                    break;
                case StateEnum.ScriptDataEscaped:
                    break;
                case StateEnum.ScriptDataEscapedDash:
                    break;
                case StateEnum.ScriptDataEscapedDashDash:
                    break;
                case StateEnum.ScriptDataEscapedLessThanSign:
                    break;
                case StateEnum.ScriptDataEscapedEndTagOpen:
                    break;
                case StateEnum.ScriptDataEscapedEndTagName:
                    break;
                case StateEnum.ScriptDataDoubleEscapeStart:
                    break;
                case StateEnum.ScriptDataDoubleEscaped:
                    break;
                case StateEnum.ScriptDataDoubleEscapedDash:
                    break;
                case StateEnum.ScriptDataDoubleEscapedDashDash:
                    break;
                case StateEnum.ScriptDataDoubleEscapedLessThanSign:
                    break;
                case StateEnum.ScriptDataDoubleEscapeEnd:
                    break;
                case StateEnum.BeforeAttributeName:
                    break;
                case StateEnum.AttributeName:
                    /*
                    When the user agent leaves the attribute name state (and before emitting the tag token, if appropriate),
                    the complete attribute's name must be compared to the other attributes on the same token; if there is
                    already an attribute on the token with the exact same name, then this is a parse error and the new attribute
                    must be removed from the token.

                    NOTE: If an attribute is so removed from a token, it, along with the value that gets associated with it,
                    if any, are never subsequently used by the parser, and are therefore effectively discarded. Removing the
                    attribute in this way does not change its status as the "current attribute" for the purposes of the tokenizer,
                    however.
                    */
                    this.CheckForDuplicateName();
                    break;
                case StateEnum.AfterAttributeName:
                    break;
                case StateEnum.BeforeAttributeValue:
                    break;
                case StateEnum.AttributeValueDoubleQuoted:
                    break;
                case StateEnum.AttributeValueSingleQuoted:
                    break;
                case StateEnum.AttributeValueUnquoted:
                    break;
                case StateEnum.AfterAttributeValueQuoted:
                    break;
                case StateEnum.SelfClosingStartTag:
                    break;
                case StateEnum.BogusComment:
                    break;
                case StateEnum.MarkupDeclarationOpen:
                    break;
                case StateEnum.CommentStart:
                    break;
                case StateEnum.CommentStartDash:
                    break;
                case StateEnum.Comment:
                    break;
                case StateEnum.CommentLessThanSign:
                    break;
                case StateEnum.CommentLessThanSignBang:
                    break;
                case StateEnum.CommentLessThanSignBangDash:
                    break;
                case StateEnum.CommentLessThanSignBangDashDash:
                    break;
                case StateEnum.CommentEndDash:
                    break;
                case StateEnum.CommentEnd:
                    break;
                case StateEnum.CommentEndBang:
                    break;
                case StateEnum.DocType:
                    break;
                case StateEnum.BeforeDocTypeName:
                    break;
                case StateEnum.DocTypeName:
                    break;
                case StateEnum.AfterDocTypeName:
                    break;
                case StateEnum.AfterDocTypePublicKeyword:
                    break;
                case StateEnum.BeforeDocTypePublicIdentifier:
                    break;
                case StateEnum.DocTypePublicIdentifierDoubleQuoted:
                    break;
                case StateEnum.DocTypePublicIdentifierSingleQuoted:
                    break;
                case StateEnum.AfterDocTypePublicIdentifier:
                    break;
                case StateEnum.BetweenDocTypePublicAndSystemIdentifiers:
                    break;
                case StateEnum.AfterDocTypeSystemKeyword:
                    break;
                case StateEnum.BeforeDocTypeSystemIdentifier:
                    break;
                case StateEnum.DocTypeSystemIdentifierDoubleQuoted:
                    break;
                case StateEnum.DocTypeSystemIdentifierSingleQuoted:
                    break;
                case StateEnum.AfterDocTypeSystemIdentifier:
                    break;
                case StateEnum.BogusDocType:
                    break;
                case StateEnum.CDataSection:
                    break;
                case StateEnum.CDataSectionBracket:
                    break;
                case StateEnum.CDataSectionEnd:
                    break;
                case StateEnum.CharacterReference:
                    break;
                case StateEnum.NumericCharacterReference:
                    break;
                case StateEnum.HexadecimalCharacterReferenceStart:
                    break;
                case StateEnum.DecimalCharacterReferenceStart:
                    break;
                case StateEnum.HexadecimalCharacterReference:
                    break;
                case StateEnum.DecimalCharacterReference:
                    break;
                case StateEnum.NumericCharacterReferenceEnd:
                    break;
                case StateEnum.CharacterReferenceEnd:
                    break;
                default:
                    break;
            }

            this.State = state;
        }

        private void InformParseError(ParseError error)
        {
            this.ParseError?.Invoke(this, new ParseErrorEventArgs(error));
        }

        #endregion

        #region Character reading related logic

        /// <summary>
        /// Current line. This is used for diagnostic purposes.
        /// This is not 100% precise.
        /// </summary>
        internal int CurrentLine { get; private set; }

        /// <summary>
        /// Current line. This is used for diagnostic purposes.
        /// This is not 100% precise.
        /// </summary>
        internal int CurrentColumn { get; private set; }

        /// <summary>
        /// The source HTML stream, where we read characters from.
        /// </summary>
        internal readonly HtmlStream HtmlStream;

        /// <summary>
        /// The current input character is the last character to have been consumed.
        /// </summary>
        private char CurrentInputCharacter;

        /// <summary>
        /// Buffer of characters that are to be reconsumed. This may grow as needed.
        /// </summary>
        private char[] CharactersToReconsume = new char[10];

        /// <summary>
        /// Current index of the characters in <see cref="CharactersToReconsume"/>.
        /// </summary>
        private int CharactersToReconsumeIndex = -1;

        /// <summary>
        /// Certain states also use a temporary buffer to track progress.
        /// See: http://www.w3.org/TR/html52/syntax.html#temporary-buffer.
        /// </summary>
        /// <remarks>
        /// This is used by:
        /// 8.2.4.9 RCDATA less-than sign state
        /// 8.2.4.11 RCDATA end tag name state
        /// 8.2.4.12 RAWTEXT less-than sign state
        /// 8.2.4.14 RAWTEXT end tag name state
        /// 8.2.4.15 Script data less-than sign state
        /// 8.2.4.17 Script data end tag name state
        /// 8.2.4.23 Script data escaped less-than sign state
        /// 8.2.4.25 Script data escaped end tag name state
        /// 8.2.4.26 Script data double escape start state
        /// 8.2.4.30 Script data double escaped less-than sign state
        /// 8.2.4.31 Script data double escape end state
        ///
        /// 8.2.4.72 Character reference state
        /// 8.2.4.73 Numeric character reference state
        /// 8.2.4.78 Numeric character reference end state
        /// 8.2.4.79 Character reference end state
        /// </remarks>
        private readonly StringBuilder TempBuffer = new StringBuilder(1024);

        /// <summary>
        /// The <see cref="StateEnum.CharacterReference"/> state uses a return state to return to the state it was invoked from.
        /// See: http://www.w3.org/TR/html52/syntax.html#return-state
        /// </summary>
        private StateEnum ReturnState;

        private char ConsumeNextInputCharacter()
        {
            // Most states consume a single character, which may have various side-effects, 
            // and either switches the state machine to a new state to reconsume the current input character, 
            // or switches it to a new state to consume the next character, or stays in the same state to consume the next character.
            //
            // When a state says to reconsume a matched character in a specified state, that means to switch to that state,
            // but when it attempts to consume the next input character, provide it with the current input character instead.
            // See: http://www.w3.org/TR/html52/syntax.html#tokenization
            
            /*
            8.2.2.5 Preprocessing the input stream
            See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#preprocessing-the-input-stream

            The input stream consists of the characters pushed into it as the input byte stream is
            decoded or from the various APIs that directly manipulate the input stream.

            One leading U+FEFF BYTE ORDER MARK character must be ignored if any are present in the input stream.

            NOTE: The requirement to strip a U+FEFF BYTE ORDER MARK character regardless of whether that character
            was used to determine the byte order is a willful violation of Unicode, motivated by a desire to increase the
            resilience of user agents in the face of naïve transcoders.

            IMPLEMENTATION NOTE: Stripping BOMs is done by the HTML stream.
  
            Any occurrences of any characters in the ranges U+0001 to U+0008, U+000E to U+001F, U+007F to U+009F,
            U+FDD0 to U+FDEF, and characters U+000B, U+FFFE, U+FFFF, U+1FFFE, U+1FFFF, U+2FFFE, U+2FFFF, U+3FFFE,
            U+3FFFF, U+4FFFE, U+4FFFF, U+5FFFE, U+5FFFF, U+6FFFE, U+6FFFF, U+7FFFE, U+7FFFF, U+8FFFE, U+8FFFF,
            U+9FFFE, U+9FFFF, U+AFFFE, U+AFFFF, U+BFFFE, U+BFFFF, U+CFFFE, U+CFFFF, U+DFFFE, U+DFFFF, U+EFFFE,
            U+EFFFF, U+FFFFE, U+FFFFF, U+10FFFE, and U+10FFFF are parse errors. These are all control characters or
            permanently undefined Unicode characters (noncharacters).

            IMPLEMENTATION NOTE: Too expensive. We don't check characters. It's only a parse error and corrected by the parser.
            
            Any character that is a not a Unicode character, i.e. any isolated surrogate, is a parse error.
            (These can only find their way into the input stream via script APIs such as document.write().)

            IMPLEMENTATION NOTE: Checking for surrogates or characters over U+10000 is too expensive. We don't!

            "CR" (U+000D) characters and "LF" (U+000A) characters are treated specially. All CR characters must be converted
            to LF characters, and any LF characters that immediately follow a CR character must be ignored. Thus, newlines in
            HTML DOMs are represented by LF characters, and there are never any CR characters in the input to the tokenization stage.
            */

            char ch = this.ConsumeNextInputCharacterWorker();
            if (ch == Characters.Cr)
            {
                // Is the CR followed by an LF? If so, just skip the CR and use the LF instead.
                ch = this.HtmlStream.ReadChar();
                if (ch != Characters.Lf)
                {
                    // If not, put back the not-LF character and pretend the CR is an LF.
                    // NB: CharactersToReconsume is empty at this point.
                    // NB: We may return a CR for re-consumption, but this will be handled next time.
                    this.CharactersToReconsume[0] = ch;
                    this.CharactersToReconsumeIndex = 0;
                    ch = Characters.Lf;
                }
            }

            if (ch == Characters.Lf)
            {
                this.CurrentLine++;
                this.CurrentColumn = 1;
            }
            else
            {
                this.CurrentColumn++;
            }

            this.CurrentInputCharacter = ch;
            return ch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char ConsumeNextInputCharacterWorker()
        {
            if (this.CharactersToReconsumeIndex >= 0)
            {
                // Take the first character
                char ch = this.CharactersToReconsume[this.CharactersToReconsumeIndex];

                // Decrement the index in the list
                this.CharactersToReconsumeIndex--;
                
                return ch;
            }
            else
            {
                char ch = this.HtmlStream.ReadChar();
                return ch;
            }
        }

        private string ConsumeNextInputCharacters(int count, bool replaceNulls)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            char[] buffer = new char[count];

            for (int i = 0; i < count; i++)
            {
                char ch = this.ConsumeNextInputCharacter();
                buffer[i] = ch;
                if (ch == Characters.EOF)
                    return new string(buffer, 0, i + 1);
                if ((ch == Characters.Null) && replaceNulls)
                    buffer[i] = Characters.ReplacementCharacter;
            }

            return new string(buffer);
        }

        /// <summary>
        /// When a state says to reconsume a matched character in a specified state, that means to switch to that state,
        /// but when it attempts to consume the next input character, provide it with the current input character instead.
        /// See: http://www.w3.org/TR/html52/syntax.html#reconsume
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReconsumeIn(StateEnum state)
        {
            this.SwitchTo(state);
            this.ReconsumeInputCharacter(this.CurrentInputCharacter);
        }

        /// <summary>
        /// When a state says to reconsume a matched character in a specified state, that means to switch to that state,
        /// but when it attempts to consume the next input character, provide it with the current input character instead.
        /// See: http://www.w3.org/TR/html52/syntax.html#reconsume
        /// </summary>
        private void ReconsumeInputCharacter(char ch)
        {
            this.CharactersToReconsumeIndex++;
            if (this.CharactersToReconsumeIndex >= this.CharactersToReconsume.Length)
                Array.Resize(ref this.CharactersToReconsume, this.CharactersToReconsume.Length * 2);
            this.CharactersToReconsume[this.CharactersToReconsumeIndex] = ch;

            if (ch == Characters.Lf)
            {
                this.CurrentLine--;
                this.CurrentColumn = 1;
            }
        }

        /// <summary>
        /// When a state says to reconsume a matched character in a specified state, that means to switch to that state,
        /// but when it attempts to consume the next input character, provide it with the current input character instead.
        /// See: http://www.w3.org/TR/html52/syntax.html#reconsume
        /// </summary>
        private void ReconsumeInputCharacters(string chars)
        {
            if (String.IsNullOrEmpty(chars))
                return;

            // NB: Must be pushed in reverse order, because this is a stack
            for (int i = chars.Length - 1; i >= 0; i--)
                this.ReconsumeInputCharacter(chars[i]);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EmitCharacter(char ch)
        {
            if (this.CurrentToken.Type == TokenType.Unknown)
                this.CurrentToken.SetCharacter(ch);
            else
                this.EmitTokenQueue.Enqueue(() => this.EmitCharacter(ch));
        }

        private void EmitEndOfFile()
        {
            if (this.CurrentToken.Type == TokenType.Unknown)
                this.CurrentToken.SetEndOfFile();
            else
                this.EmitTokenQueue.Enqueue(this.EmitEndOfFile);
        }

        #region DocType related logic

        /// <summary>
        /// Buffer for constructing the doc type names.
        /// </summary>
        private StringBuilder DocTypeName;

        /// <summary>
        /// Buffer for constructing the doc type public identifier.
        /// </summary>
        private StringBuilder DocTypePublicIdentifier;

        /// <summary>
        /// Buffer for constructing the doc type system identifier.
        /// </summary>
        private StringBuilder DocTypeSystemIdentifier;

        private bool DocTypeForceQuirks = false;

        private void NewDocType()
        {
            this.DocTypeForceQuirks = false;
            this.DocTypeName = null;
            this.DocTypePublicIdentifier = null;
            this.DocTypeSystemIdentifier = null;
        }

        private void EmitDocType()
        {
            string name = this.DocTypeName?.ToString();
            string publicId = this.DocTypePublicIdentifier?.ToString();
            string systemId = this.DocTypeSystemIdentifier?.ToString();
            bool quirks = this.DocTypeForceQuirks;
            if (this.CurrentToken.Type == TokenType.Unknown)
                this.CurrentToken.SetDocType(name, publicId, systemId, quirks);
            else
                this.EmitTokenQueue.Enqueue(() => this.CurrentToken.SetDocType(name, publicId, systemId, quirks));
        }

        #endregion

        #region Comment related logic

        /// <summary>
        /// Keeps the comment data while reading it.
        /// </summary>
        private readonly StringBuilder CommentData = new StringBuilder();

        private void NewComment()
        {
            this.CommentData.Clear();
        }

        private void EmitComment()
        {
            // NB: We pass the ToString as function, because it is not sure they need
            // the string data and that way we save memory garbage.
            if (this.CurrentToken.Type == TokenType.Unknown)
            {
                this.CurrentToken.SetComment(this.CommentData.ToString);
            }
            else
            {
                string data = this.CommentData.ToString();
                this.EmitTokenQueue.Enqueue(() => this.CurrentToken.SetComment(() => data));
            }
        }

        #endregion

        #region Tag related logic

        /// <summary>
        /// Buffer for constructing the tag names.
        /// </summary>
        private readonly StringBuilder TagName = new StringBuilder(32);

        private bool TagIsSelfClosing = false;

        private bool IsCurrentTagEndTag = false;

        private void NewStartTag()
        {
            this.TagName.Clear();
            this.TagIsSelfClosing = false;
            this.IsCurrentTagEndTag = false;
            this.CurrentTagAttributes.Clear();
            this.HasCurrentAttribute = false;
        }

        private void NewEndTag()
        {
            this.TagName.Clear();
            this.TagIsSelfClosing = false;
            this.IsCurrentTagEndTag = true;
            this.CurrentTagAttributes.Clear();
            this.HasCurrentAttribute = false;
        }

        private void EmitTag()
        {
            this.AddCurrentAttributeToList();
            Attribute[] attributes = (this.CurrentTagAttributes.Count == 0) ? Attribute.None : this.CurrentTagAttributes.ToArray();

            if (this.IsCurrentTagEndTag)
            {
                // When an end tag token is emitted with attributes, that is a parse error.
                if (attributes.Length != 0)
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                // When an end tag token is emitted with its self-closing flag set, that is a parse error.
                if (this.TagIsSelfClosing)
                    this.InformParseError(Parsing.ParseError.InvalidTag);

                this.EmitEndTag(this.TagName.ToString(), this.TagIsSelfClosing, attributes);
            }
            else
            {
                this.EmitStartTag(this.TagName.ToString(), this.TagIsSelfClosing, attributes);
            }
        }

        private void EmitStartTag(string tagName, bool isSelfClosing, Attribute[] attributes)
        {
            if (this.CurrentToken.Type == TokenType.Unknown)
            {
                this.LastStartTagName = tagName;
                this.CurrentToken.SetStartTag(tagName, isSelfClosing, attributes);
            }
            else
            {
                this.EmitTokenQueue.Enqueue(() => this.EmitStartTag(tagName, isSelfClosing, attributes));
            }
        }

        private void EmitEndTag(string tagName, bool isSelfClosing, Attribute[] attributes)
        {
            if (this.CurrentToken.Type == TokenType.Unknown)
                this.CurrentToken.SetEndTag(tagName, isSelfClosing, attributes);
            else
                this.EmitTokenQueue.Enqueue(() => this.CurrentToken.SetEndTag(tagName, isSelfClosing, attributes));
        }

        private string LastStartTagName;

        /// <summary>
        /// An appropriate end tag token is an end tag token whose tag name matches the tag name of the
        /// last start tag to have been emitted from this tokenizer, if any. If no start tag has been
        /// emitted from this tokenizer, then no end tag token is appropriate.
        /// </summary>
        private bool IsCurrentEndTagAppropriateEndTag()
        {
            if (this.IsCurrentTagEndTag && (this.LastStartTagName == null))
                return false;
            if (this.TagName.Length != this.LastStartTagName.Length)
                return false;
            return this.TagName.ToString().Equals(this.LastStartTagName, StringComparison.Ordinal);
        }

        #endregion

        #region Attribute related logic

        /// <summary>
        /// Buffer for constructing the attribute names.
        /// </summary>
        private readonly StringBuilder AttributeName = new StringBuilder(32);

        /// <summary>
        /// Buffer for constructing the attribute values.
        /// </summary>
        private readonly StringBuilder AttributeValue = new StringBuilder(32);

        /// <summary>
        /// Indicates that there's currently a new attribute.
        /// </summary>
        private bool HasCurrentAttribute;

        private void NewAttribute()
        {
            this.AddCurrentAttributeToList();
            this.HasCurrentAttribute = true;
        }

        private void AddCurrentAttributeToList()
        {
            this.CheckForDuplicateName();

            if (!this.HasCurrentAttribute)
                return;

            this.CurrentTagAttributes.Add(new Attribute(this.AttributeName.ToString(), this.AttributeValue.ToString()));
            this.HasCurrentAttribute = false;
        }

        private void CheckForDuplicateName()
        {
            if (!this.HasCurrentAttribute)
                return;

            /*
            When the user agent leaves the attribute name state (and before emitting the tag token, if appropriate),
            the complete attribute's name must be compared to the other attributes on the same token; if there is
            already an attribute on the token with the exact same name, then this is a parse error and the new attribute
            must be removed from the token.

            NOTE: If an attribute is so removed from a token, it, along with the value that gets associated with it,
            if any, are never subsequently used by the parser, and are therefore effectively discarded. Removing the
            attribute in this way does not change its status as the "current attribute" for the purposes of the tokenizer,
            however.
            */

            string name = this.AttributeName.ToString();
            if (this.CurrentTagAttributes.Any(attr => attr.Name.Equals(name, StringComparison.Ordinal)))
            {
                // Skip the current attribute.
                // Setting HasCurrentAttribute to false will continue parsing,
                // but the result will not be added to the current tag's attributes.
                this.HasCurrentAttribute = false;
            }
        }

        /// <summary>
        /// Determines if there is an adjusted current node and it is not an element in the HTML namespace.
        /// </summary>
        /// <returns></returns>
        private bool HasAdjustedNonHtmlElementCurrentNode()
        {
            // .. if there is an adjusted current node and it is ** NOT ** an element in the HTML namespace and ...
            // TO-DO ... we currently do not support other elements EXCEPT ones in the HTML namespace,
            // THEREFORE the current node will always be in the HTML namespace and this will always be FALSE!
            return false;
        }

        private readonly List<Parsing.Attribute> CurrentTagAttributes = new List<Parsing.Attribute>();

        #endregion

        #endregion

        #region State Handlers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateData()
        {
            /*
            8.2.4.1 Data state
            See: http://www.w3.org/TR/html52/syntax.html#tokenizer-data-state

            Consume the next input character:
                U+0026 AMPERSAND (&)
                    Set the return state to the data state. Switch to the character reference state.
                "<" (U+003C)
                    Switch to the tag open state.
                U+0000 NULL
                    Parse error. Emit the current input character as a character token.
                EOF
                    Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
             */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == Characters.Ampersand)
            {
                this.ReturnState = StateEnum.Data;
                this.SwitchTo(StateEnum.CharacterReference);
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.TagOpen);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.NullCharacter);
                this.EmitCharacter(ch);
            }
            else if (ch == Characters.EOF)
            {
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcData()
        {
            /*
            8.2.4.2 RCDATA state
            See: http://www.w3.org/TR/html52/syntax.html#tokenizer-rcdata-state

             Consume the next input character:
                U+0026 AMPERSAND (&)
                    Set the return state to the RCDATA state. Switch to the character reference state.
                "<" (U+003C)
                    Switch to the RCDATA less-than sign state.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == Characters.Ampersand)
            {
                this.ReturnState = StateEnum.RcData;
                this.SwitchTo(StateEnum.CharacterReference);
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.RcDataLessThanSign);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.NullCharacter);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawText()
        {
            /*
            8.2.4.3 RAWTEXT state
            See: http://www.w3.org/TR/html52/syntax.html#tokenizer-rawtext-state

            Consume the next input character:
                "<" (U+003C)
                    Switch to the RAWTEXT less-than sign state.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '<')
            {
                this.SwitchTo(StateEnum.RawTextLessThanSign);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.NullCharacter);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptData()
        {
            /*
            8.2.4.4 Script data state
            See: http://www.w3.org/TR/html52/syntax.html#tokenizer-script-data-state

            Consume the next input character:
                "<" (U+003C)
                    Switch to the script data less-than sign state.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataLessThanSign);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.NullCharacter);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStatePlainText()
        {
            /*
            8.2.4.5 PLAINTEXT state
            See: http://www.w3.org/TR/html52/syntax.html#plaintext-state

            Consume the next input character:
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.NullCharacter);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateTagOpen()
        {
            /*
            8.2.4.6 Tag open state
            See: http://www.w3.org/TR/html52/syntax.html#tokenizer-tag-open-state

            Consume the next input character:
                "!" (U+0021)
                    Switch to the markup declaration open state.
                "/" (U+002F)
                    Switch to the end tag open state.
                ASCII letter 
                    Create a new start tag token, set its tag name to the empty string. 
                    Reconsume in the tag name state.
                "?" (U+003F)
                    Parse error. Create a comment token whose data is the empty string. 
                    Reconsume in the bogus comment state.
                Anything else
                    Parse error.Emit a U+003C LESS-THAN SIGN character token. 
                    Reconsume in the data state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '!')
            {
                this.SwitchTo(StateEnum.MarkupDeclarationOpen);
            }
            else if (ch == '/')
            {
                this.SwitchTo(StateEnum.EndTagOpen);
            }
            else if (ch.IsAsciiLetter())
            {
                this.NewStartTag();
                this.TagName.Clear();
                this.ReconsumeIn(StateEnum.TagName);
            }
            else if (ch == '?')
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.NewComment();
                this.CommentData.Clear();
                this.ReconsumeIn(StateEnum.BogusComment);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.EmitCharacter('<');
                this.ReconsumeIn(StateEnum.Data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateEndTagOpen()
        {
            /*
            8.2.4.7 End tag open state
            See: http://www.w3.org/TR/html52/syntax.html#end-tag-open-state

            Consume the next input character:
                ASCII letter 
                    Create a new end tag token, set its tag name to the empty string. 
                    Reconsume in the tag name state.
                ">" (U+003E)
                    Parse error. Switch to the data state.
                EOF
                    Parse error. Emit a U+003C LESS-THAN SIGN character token, 
                    a U+002F SOLIDUS character token and an end-of-file token.
                Anything else
                    Parse error. Switch to the bogus comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.ReconsumeIn(StateEnum.TagName);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.Data);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.BogusComment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateTagName()
        {
            /*
            8.2.4.8 Tag name state
            See: http://www.w3.org/TR/html52/syntax.html#tag-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before attribute name state.
                "/" (U+002F)
                    Switch to the self-closing start tag state.
                ">" (U+003E)
                    Switch to the data state. Emit the current tag token.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current tag token's tag name.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current tag token's tag name.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current tag token's tag name.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if (ch == '/')
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.NullCharacter);
                this.TagName.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.PrematureEndOfFile);
                this.EmitEndOfFile();
            }
            else
            {
                this.TagName.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcDataLessThanSign()
        {
            /*
            8.2.4.9 RCDATA less-than sign state
            See: http://www.w3.org/TR/html52/syntax.html#RCDATA-less-than-sign-state

            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the RCDATA end tag open state.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token. Reconsume in the RCDATA state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.RcDataEndTagOpen);
            }
            else
            {
                this.EmitCharacter('<');
                this.ReconsumeIn(StateEnum.RcData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcDataEndTagOpen()
        {
            /*
            8.2.4.10 RCDATA end tag open state
            See: http://www.w3.org/TR/html52/syntax.html#RCDATA-end-tag-open-state

            Consume the next input character:
                ASCII letter 
                    Create a new end tag token, set its tag name to the empty string. 
                    Reconsume in RCDATA end tag name state.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume in the RCDATA state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.ReconsumeIn(StateEnum.RcDataEndTagName);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeIn(StateEnum.RcData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcDataEndTagName()
        {
            /*
            8.2.4.11 RCDATA end tag name state
            See: http://www.w3.org/TR/html52/syntax.html#RCDATA-end-tag-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    If the current end tag token is an appropriate end tag token, then switch to the before attribute name
                    state. Otherwise, treat it as per the "anything else" entry below.
                "/" (U+002F)
                    If the current end tag token is an appropriate end tag token, then switch to the self-closing start tag
                    state. Otherwise, treat it as per the "anything else" entry below.
                ">" (U+003E)
                    If the current end tag token is an appropriate end tag token, then switch to the data state and emit
                    the current tag token. Otherwise, treat it as per the "anything else" entry below.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current tag token's tag name. Append the current input character to the temporary buffer.
                Lowercase ASCII letter
                    Append the current input character to the current tag token's tag name. Append the current input
                    character to the temporary buffer.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS character token,
                    and a character token for each of the characters in the temporary buffer
                    (in the order they were added to the buffer). Reconsume in the RCDATA state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if ((ch == '/') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if ((ch == '>') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeIn(StateEnum.RcData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawTextLessThanSign()
        {
            /*
            8.2.4.12 RAWTEXT less-than sign state
            See: http://www.w3.org/TR/html52/syntax.html#rawtext-less-than-sign-state

            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the RAWTEXT end tag open state.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token. Reconsume in the RAWTEXT state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.RawTextEndTagOpen);
            }
            else
            {
                this.EmitCharacter('<');
                this.ReconsumeIn(StateEnum.RawText);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawTextEndTagOpen()
        {
            /*
            8.2.4.13 RAWTEXT end tag open state
            See: http://www.w3.org/TR/html52/syntax.html#rawtext-end-tag-open-state

            Consume the next input character:
                ASCII letter 
                    Create a new end tag token, set its tag name to the empty string. 
                    Reconsume in the RAWTEXT end tag name state.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume in the RAWTEXT state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.ReconsumeIn(StateEnum.RawTextEndTagName);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeIn(StateEnum.RawText);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawTextEndTagName()
        {
            /*
            8.2.4.14 RAWTEXT end tag name state
            See: http://www.w3.org/TR/html52/syntax.html#rawtext-end-tag-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    If the current end tag token is an appropriate end tag token, then switch to the before attribute
                    name state. Otherwise, treat it as per the "anything else" entry below.
                "/" (U+002F)
                    If the current end tag token is an appropriate end tag token, then switch to the self-closing start
                    tag state. Otherwise, treat it as per the "anything else" entry below.
                ">" (U+003E)
                    If the current end tag token is an appropriate end tag token, then switch to the data state and emit
                    the current tag token. Otherwise, treat it as per the "anything else" entry below.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current tag token's tag name. Append the current input character to the temporary buffer.
                Lowercase ASCII letter
                    Append the current input character to the current tag token's tag name. Append the current input
                    character to the temporary buffer.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS character token,
                    and a character token for each of the characters in the temporary buffer
                    (in the order they were added to the buffer). Reconsume in the RAWTEXT state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if ((ch == '/') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if ((ch == '>') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeIn(StateEnum.RawText);
            }
        }

        #region Scripts

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataLessThanSign()
        {
            /*
            8.2.4.15 Script data less-than sign state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-less-than-sign-state

            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the script data end tag open state.
                "!" (U+0021)
                    Switch to the script data escape start state. Emit a U+003C LESS-THAN SIGN character token and a
                    U+0021 EXCLAMATION MARK character token.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token. Reconsume in the script data state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.ScriptDataEndTagOpen);
            }
            else if (ch == '!')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapeStart);
                this.EmitCharacter('<');
                this.EmitCharacter('!');
            }
            else
            {
                this.EmitCharacter('<');
                this.ReconsumeIn(StateEnum.ScriptData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEndTagOpen()
        {
            /*
            8.2.4.16 Script data end tag open state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-end-tag-open-state

            Consume the next input character:
                ASCII letter 
                    Create a new end tag token, set its tag name to the empty string. 
                    Reconsume in the script data end tag name state.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume in the script data state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.ReconsumeIn(StateEnum.ScriptDataEndTagName);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeIn(StateEnum.ScriptData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEndTagName()
        {
            /*
            8.2.4.17 Script data end tag name state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-end-tag-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    If the current end tag token is an appropriate end tag token, then switch to the before attribute name
                    state. Otherwise, treat it as per the "anything else" entry below.
                "/" (U+002F)
                    If the current end tag token is an appropriate end tag token, then switch to the self-closing start tag
                    state. Otherwise, treat it as per the "anything else" entry below.
                ">" (U+003E)
                    If the current end tag token is an appropriate end tag token, then switch to the data state and emit
                    the current tag token. Otherwise, treat it as per the "anything else" entry below.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current tag token's tag name. Append the current input character to the temporary buffer.
                Lowercase ASCII letter
                    Append the current input character to the current tag token's tag name. Append the current input
                    character to the temporary buffer.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS character token,
                    and a character token for each of the characters in the temporary buffer
                    (in the order they were added to the buffer). Reconsume in the script data state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if ((ch == '/') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if ((ch == '>') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeIn(StateEnum.ScriptData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapeStart()
        {
            /*
            8.2.4.18 Script data escape start state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escape-start-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escape start dash state. Emit a U+002D HYPHEN-MINUS character token.
                Anything else
                    Reconsume in the script data state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapeStartDash);
                this.EmitCharacter('\u002D');
            }
            else
            {
                this.ReconsumeIn(StateEnum.ScriptData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapeStartDash()
        {
            /*
            8.2.4.19 Script data escape start dash state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escape-start-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escaped dash dash state. Emit a U+002D HYPHEN-MINUS character token.
                Anything else
                Reconsume in the script data state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedDashDash);
                this.EmitCharacter('\u002D');
            }
            else
            {
                this.ReconsumeIn(StateEnum.ScriptData); 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscaped()
        {
            /*
            8.2.4.20 Script data escaped state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escaped-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escaped dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data escaped less-than sign state.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedDash);
                this.EmitCharacter('\u002D');
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedLessThanSign);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedDash()
        {
            /*
            8.2.4.21 Script data escaped dash state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escaped-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escaped dash dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data escaped less-than sign state.
                U+0000 NULL
                    Parse error. Switch to the script data escaped state. Emit a U+FFFD REPLACEMENT CHARACTER
                    character token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Switch to the script data escaped state. Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedDashDash);
                this.EmitCharacter('\u002D');
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedLessThanSign);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitEndOfFile();
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedDashDash()
        {
            /*
            8.2.4.22 Script data escaped dash dash state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escaped-dash-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data escaped less-than sign state.
                ">" (U+003E)
                    Switch to the script data state. Emit a U+003E GREATER-THAN SIGN character token.
                U+0000 NULL
                    Parse error. Switch to the script data escaped state. Emit a U+FFFD REPLACEMENT CHARACTER
                    character token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Switch to the script data escaped state. Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.EmitCharacter('\u002D');
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedLessThanSign);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.ScriptData);
                this.EmitCharacter('>');
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitEndOfFile();
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedLessThanSign()
        {
            /*
            8.2.4.23 Script data escaped less-than sign state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escaped-less-than-sign-state

            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the script data escaped end tag open state.
                ASCII letter 
                    Set the temporary buffer to the empty string. 
                    Emit a U+003C LESS-THAN SIGN character token. 
                    Reconsume in the script data double escape start state.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token. Reconsume in the script data escaped state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.ScriptDataEscapedEndTagOpen);
            }
            else if (ch.IsAsciiLetter())
            {
                this.TempBuffer.Clear();
                this.EmitCharacter('<');
                this.ReconsumeIn(StateEnum.ScriptDataDoubleEscapeStart);
            }
            else
            {
                this.EmitCharacter('<');
                this.ReconsumeIn(StateEnum.ScriptDataEscaped);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedEndTagOpen()
        {
            /*
            8.2.4.24 Script data escaped end tag open state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escaped-end-tag-open-state

            Consume the next input character:
                ASCII letter 
                    Create a new end tag token. 
                    Reconsume in the script data escaped end tag name state. 
                    (Don’t emit the token yet; further details will be filled in
                    before it is emitted.)
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume in the script data escaped state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.ReconsumeIn(StateEnum.ScriptDataEscapedEndTagName);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeIn(StateEnum.ScriptDataEscaped);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedEndTagName()
        {
            /*
            8.2.4.25 Script data escaped end tag name state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-escaped-end-tag-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    If the current end tag token is an appropriate end tag token, then switch to the before attribute name
                    state. Otherwise, treat it as per the "anything else" entry below.
                "/" (U+002F)
                    If the current end tag token is an appropriate end tag token, then switch to the self-closing start tag
                    state. Otherwise, treat it as per the "anything else" entry below.
                ">" (U+003E)
                    If the current end tag token is an appropriate end tag token, then switch to the data state and emit
                    the current tag token. Otherwise, treat it as per the "anything else" entry below.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current tag token's tag name. Append the current input character to the temporary buffer.
                Lowercase ASCII letter
                    Append the current input character to the current tag token's tag name. Append the current input
                    character to the temporary buffer.
                Anything else
                    Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS character token,
                    and a character token for each of the characters in the temporary buffer
                    (in the order they were added to the buffer). Reconsume in the script data escaped state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if ((ch == '/') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if ((ch == '>') && this.IsCurrentEndTagAppropriateEndTag())
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
            }
            else
            {
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeIn(StateEnum.ScriptDataEscaped);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapeStart()
        {
            /*
            8.2.4.26 Script data double escape start state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-double-escape-start-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                "/" (U+002F)
                ">" (U+003E)
                    If the temporary buffer is the string "script", then switch to the script data double escaped state.
                    Otherwise, switch to the script data escaped state. Emit the current input character as a character
                    token.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the temporary buffer. Emit the current input character as a character token.
                Lowercase ASCII letter
                    Append the current input character to the temporary buffer. Emit the current input character as a
                    character token.
                Anything else
                    Reconsume in the script data escaped state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() || (ch == '/') || (ch == '>'))
            {
                if (this.TempBuffer.ToString().Equals("script", StringComparison.OrdinalIgnoreCase))
                    this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                else
                    this.SwitchTo(StateEnum.ScriptDataEscaped);

                this.EmitCharacter(ch);
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TempBuffer.Append(ch.ToLowercaseAsciiLetter());
                this.EmitCharacter(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TempBuffer.Append(ch);
                this.EmitCharacter(ch);
            }
            else
            {
                this.ReconsumeIn(StateEnum.ScriptDataEscaped);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscaped()
        {
            /*
            8.2.4.27 Script data double escaped state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data double escaped dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data double escaped less-than sign state. Emit a U+003C LESS-THAN SIGN
                    character token.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapedDash);
                this.EmitCharacter('-');
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapedLessThanSign);
                this.EmitCharacter('<');
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapedDash()
        {
            /*
            8.2.4.28 Script data double escaped dash state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data double escaped dash dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data double escaped less-than sign state. Emit a U+003C LESS-THAN SIGN
                    character token.
                U+0000 NULL
                    Parse error. Switch to the script data double escaped state. Emit a U+FFFD REPLACEMENT CHARACTER
                    character token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Switch to the script data double escaped state. Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapedDashDash);
                this.EmitCharacter('-');
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapedLessThanSign);
                this.EmitCharacter('<');
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitEndOfFile();
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapedDashDash()
        {
            /*
            8.2.4.29 Script data double escaped dash dash state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-dash-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data double escaped less-than sign state. Emit a U+003C LESS-THAN SIGN character
                    token.
                ">" (U+003E)
                    Switch to the script data state. Emit a U+003E GREATER-THAN SIGN character token.
                U+0000 NULL
                    Parse error. Switch to the script data double escaped state. Emit a U+FFFD REPLACEMENT CHARACTER
                    character token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Switch to the script data double escaped state. Emit the current input character as a character token.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.EmitCharacter('-');
            }
            else if (ch == '<')
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapedLessThanSign);
                this.EmitCharacter('<');
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.ScriptData);
                this.EmitCharacter('>');
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                this.EmitCharacter(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.EmitEndOfFile();
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapedLessThanSign()
        {
            /*
            8.2.4.30 Script data double escaped less-than sign state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-double-escaped-less-than-sign-state

            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the script data double escape end state.
                    Emit a U+002F SOLIDUS character token.
                Anything else
                    Reconsume in the script data double escaped state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapeEnd);
                this.EmitCharacter('\u002F');
            }
            else
            {
                this.ReconsumeIn(StateEnum.ScriptDataDoubleEscaped);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapeEnd()
        {
            /*
            8.2.4.31 Script data double escape end state
            See: http://www.w3.org/TR/html52/syntax.html#script-data-double-escape-end-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                "/" (U+002F)
                ">" (U+003E)
                    If the temporary buffer is the string "script", then switch to the script data escaped state.
                    Otherwise, switch to the script data double escaped state. Emit the current input character as a
                    character token.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the temporary buffer. Emit the current input character as a character token.
                Lowercase ASCII letter
                    Append the current input character to the temporary buffer. Emit the current input character as a
                    character token.
                Anything else
                    Reconsume in the script data double escaped state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() || (ch == '/') || (ch == '>'))
            {
                if (String.Equals(this.TempBuffer.ToString(), "script", StringComparison.Ordinal))
                    this.SwitchTo(StateEnum.ScriptDataEscaped);
                else
                    this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);

                this.EmitCharacter(ch);
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TempBuffer.Append(ch.ToLowercaseAsciiLetter());
                this.EmitCharacter(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TempBuffer.Append(ch);
                this.EmitCharacter(ch);
            }
            else
            {
                this.ReconsumeIn(StateEnum.ScriptDataDoubleEscaped);
            }
        }

        #endregion

        #region Attributes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeAttributeName()
        {
            /*
            8.2.4.32 Before attribute name state
            See: http://www.w3.org/TR/html52/syntax.html#before-attribute-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                "/" (U+002F)
                ">" (U+003E)
                EOF
                    Reconsume in the after attribute name state.
                "=" (U+003D)
                    Parse error. Start a new attribute in the current tag token. 
                    Set that attribute’s name to the current input character, and 
                    its value to the empty string. Switch to the attribute name state.
                Anything else
                    Start a new attribute in the current tag token. 
                    Set that attribute’s name and value to the empty string. 
                    Reconsume in the attribute name state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if ((ch == '/') || (ch == '>') || (ch == Characters.EOF))
            {
                this.ReconsumeIn(StateEnum.AfterAttributeName);
            }
            else if (ch == '=')
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(ch);
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
            else
            {
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeValue.Clear();
                this.ReconsumeIn(StateEnum.AttributeName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAttributeName()
        {
            /*
            8.2.4.33 Attribute name state
            See: http://www.w3.org/TR/html52/syntax.html#attribute-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                "/" (U+002F)
                ">" (U+003E)
                EOF
                    Reconsume in the after attribute name state.
                "=" (U+003D)
                    Switch to the before attribute value state.

                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current attribute's name.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's name.
                U+0022 QUOTATION MARK (")
                "'" (U+0027)
                "<" (U+003C)
                    Parse error. Treat it as per the "anything else" entry below.
                Anything else
                    Append the current input character to the current attribute's name.

            When the user agent leaves the attribute name state (and before emitting the tag token, if appropriate),
            the complete attribute's name must be compared to the other attributes on the same token; if there is
            already an attribute on the token with the exact same name, then this is a parse error and the new attribute
            must be removed from the token.

            NOTE: If an attribute is so removed from a token, it, along with the value that gets associated with it,
            if any, are never subsequently used by the parser, and are therefore effectively discarded. Removing the
            attribute in this way does not change its status as the "current attribute" for the purposes of the tokenizer,
            however.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() || (ch == '/') || (ch == '>') || (ch == Characters.EOF))
            {
                this.ReconsumeIn(StateEnum.AfterAttributeName);
            }
            else if (ch == '=')
            {
                this.SwitchTo(StateEnum.BeforeAttributeValue);
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.AttributeName.Append(ch.ToLowercaseAsciiLetter());
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeName.Append(Characters.ReplacementCharacter);
            }
            else
            {
                if ((ch == '\u0022') || (ch == '\u0027') || (ch == '<'))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.AttributeName.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterAttributeName()
        {
            /*
            8.2.4.34 After attribute name state
            See: http://www.w3.org/TR/html52/syntax.html#after-attribute-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                "/" (U+002F)
                    Switch to the self-closing start tag state.
                "=" (U+003D)
                    Switch to the before attribute value state.
                ">" (U+003E)
                    Switch to the data state. Emit the current tag token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Start a new attribute in the current tag token.
                    Set that attribute’s name and value to the empty string.
                    Reconsume in the attribute name state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '/')
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if (ch == '=')
            {
                this.SwitchTo(StateEnum.BeforeAttributeValue);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.EmitEndOfFile();
            }
            else
            {
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeValue.Clear();
                this.ReconsumeIn(StateEnum.AttributeName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeAttributeValue()
        {
            /*
            8.2.4.35 Before attribute value state
            See: http://www.w3.org/TR/html52/syntax.html#before-attribute-value-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                U+0022 QUOTATION MARK (")
                    Switch to the attribute value (double-quoted) state.
                "'" (U+0027)
                    Switch to the attribute value (single-quoted) state.
                ">" (U+003E)
                    Parse error. Treat it as per the "anything else" entry below.
                Anything else
                    Reconsume in the attribute value (unquoted) state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '\u0022')
            {
                this.SwitchTo(StateEnum.AttributeValueDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.SwitchTo(StateEnum.AttributeValueSingleQuoted);
            }
            else
            {
                if (ch == '>')
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.ReconsumeIn(StateEnum.AttributeValueUnquoted);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAttributeValueDoubleQuoted()
        {
            /*
            8.2.4.36 Attribute value (double-quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#attribute-value-double-quoted-state

            Consume the next input character:
                U+0022 QUOTATION MARK (")
                    Switch to the after attribute value (quoted) state.
                U+0026 AMPERSAND (&)
                    Set the return state to the attribute value (double-quoted) state.
                    Switch to the character reference state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current attribute's value.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '\u0022')
            {
                this.SwitchTo(StateEnum.AfterAttributeValueQuoted);
            }
            else if (ch == '&')
            {
                this.ReturnState = StateEnum.AttributeValueDoubleQuoted;
                this.SwitchTo(StateEnum.CharacterReference);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeValue.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.EmitEndOfFile();
            }
            else
            {
                this.AttributeValue.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAttributeValueSingleQuoted()
        {
            /*
            8.2.4.37 Attribute value (single-quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#attribute-value-single-quoted-state

            Consume the next input character:
                "'" (U+0027)
                    Switch to the after attribute value (quoted) state.
                U+0026 AMPERSAND (&)
                    Set the return state to the attribute value (single-quoted) state.
                    Switch to the character reference state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current attribute's value.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '\u0027')
            {
                this.SwitchTo(StateEnum.AfterAttributeValueQuoted);
            }
            else if (ch == '&')
            {
                this.ReturnState = StateEnum.AttributeValueSingleQuoted;
                this.SwitchTo(StateEnum.CharacterReference);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeValue.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.EmitEndOfFile();
            }
            else
            {
                this.AttributeValue.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAttributeValueUnquoted()
        {
            /*
            8.2.4.38 Attribute value (unquoted) state
            See: http://www.w3.org/TR/html52/syntax.html#attribute-value-unquoted-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before attribute name state.
                U+0026 AMPERSAND (&)
                    Set the return state to the attribute value (unquoted) state.
                    Switch to the character reference state.
                ">" (U+003E)
                    Switch to the data state. Emit the current tag token.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                U+0022 QUOTATION MARK (")
                "'" (U+0027)
                "<" (U+003C)
                "=" (U+003D)
                "`" (U+0060)
                    Parse error. Treat it as per the "anything else" entry below.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current attribute's value.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if (ch == '&')
            {
                this.ReturnState = StateEnum.AttributeValueUnquoted;
                this.SwitchTo(StateEnum.CharacterReference);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeValue.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.EmitEndOfFile();
            }
            else
            {
                if ((ch == '\u0022') || (ch == '\u0027') || (ch == '<') || (ch == '=') || (ch == '\u0060'))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.AttributeValue.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterAttributeValueQuoted()
        {
            /*
            8.2.4.39 After attribute value (quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#after-attribute-value-quoted-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before attribute name state.
                "/" (U+002F)
                    Switch to the self-closing start tag state.
                ">" (U+003E)
                    Switch to the data state. Emit the current tag token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Parse error. Reconsume in the before attribute name state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeAttributeName);
            }
            else if (ch == '/')
            {
                this.SwitchTo(StateEnum.SelfClosingStartTag);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.ReconsumeIn(StateEnum.BeforeAttributeName);
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateSelfClosingStartTag()
        {
            /*
            8.2.4.40 Self-closing start tag state
            See: http://www.w3.org/TR/html52/syntax.html#self-closing-start-tag-state

            Consume the next input character:
                ">" (U+003E)
                    Set the self-closing flag of the current tag token. 
                    Switch to the data state.
                    Emit the current tag token.
                EOF
                    Parse error. Emit an end-of-file token.
                Anything else
                    Parse error. Reconsume in the before attribute name state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '>')
            {
                this.TagIsSelfClosing = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.ReconsumeIn(StateEnum.BeforeAttributeName);
            }
        }

        #region Comments

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBogusComment()
        {
            /*
            8.2.4.41 Bogus comment state
            See: http://www.w3.org/TR/html52/syntax.html#bogus-comment-state

            Consume the next input character:
                ">" (U+003E)
                    Switch to the data state. Emit the comment token.
                EOF 
                    Emit the comment. Emit an end-of-file token.
                NULL (U+0000)
                    Append a U+FFFD REPLACEMENT CHARACTER character to the comment token’s data.
                Anything else 
                    Append the current input character to the comment token’s data.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else if (ch == Characters.EOF)
            {
                this.EmitComment();
                this.EmitEndOfFile();
            }
            else if (ch == Characters.Null)
            {
                this.CommentData.Append(Characters.ReplacementCharacter);
            }
            else
            {
                this.CommentData.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateMarkupDeclarationOpen()
        {
            /*
            8.2.4.42 Markup declaration open state
            See: http://www.w3.org/TR/html52/syntax.html#markup-declaration-open-state

            If the next two characters are both "-" (U+002D) characters, consume those two characters, create a comment
            token whose data is the empty string, and switch to the comment start state.

            Otherwise, if the next seven characters are an ASCII case-insensitive match for the word "DOCTYPE", then
            consume those characters and switch to the DOCTYPE state.

            Otherwise, if there is an adjusted current node and it is not an element in the HTML namespace and the next
            seven characters are a case-sensitive match for the string "[CDATA[" (the five uppercase letters "CDATA" with
            a U+005B LEFT SQUARE BRACKET character before and after), then consume those characters and switch to the
            CDATA section state.

            Otherwise, this is a parse error. Create a comment token whose data is the empty string. 
            Switch to the bogus comment state (don’t consume anything in the current state).
            */
            string tmp = this.ConsumeNextInputCharacters(7, false);
            if (tmp.StartsWith("--", StringComparison.Ordinal))
            {
                // We only need the first two chars. Reconsume the rest.
                this.ReconsumeInputCharacters(tmp.Substring(2));

                this.NewComment();
                this.CommentData.Clear();
                this.SwitchTo(StateEnum.CommentStart);
            }
            else if (tmp.Equals("DOCTYPE", StringComparison.OrdinalIgnoreCase))
            {
                this.SwitchTo(StateEnum.DocType);
            }
            else if (this.HasAdjustedNonHtmlElementCurrentNode() && tmp.Equals("[CDATA[", StringComparison.Ordinal))
            {
                this.SwitchTo(StateEnum.CDataSection);
            }
            else
            {
                // Return the characters
                this.ReconsumeInputCharacters(tmp);

                this.InformParseError(Parsing.ParseError.InvalidMarkup);
                this.NewComment();
                this.CommentData.Clear();
                this.SwitchTo(StateEnum.BogusComment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentStart()
        {
            /*
            8.2.4.43 Comment start state
            See: http://www.w3.org/TR/html52/syntax.html#comment-start-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment start dash state.
                ">" (U+003E)
                    Parse error. Switch to the data state. Emit the comment token.
                Anything else
                    Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentStartDash);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else
            {
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentStartDash()
        {
            /*
            8.2.4.44 Comment start dash state
            See: http://www.w3.org/TR/html52/syntax.html#comment-start-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment end state
                ">" (U+003E)
                    Parse error. Switch to the data state. Emit the comment token.
                EOF
                    Parse error. Emit the comment token. Emit an end-of-file token.
                Anything else
                    Append a U+002D HYPHEN-MINUS character (-) to the comment token’s data. 
                    Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentEnd);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.EmitComment();
                this.EmitEndOfFile();
            }
            else
            {
                this.CommentData.Append('-');
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateComment()
        {
            /*
            8.2.4.45 Comment state
            See: http://www.w3.org/TR/html52/syntax.html#comment-state

            Consume the next input character:
                "<" (U+003C)
                    Append the current input character to the comment token’s data.
                    Switch to the comment less-than sign state.
                "-" (U+002D)
                    Switch to the comment end dash state
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                EOF
                    Parse error. Emit the comment token. Emit an end-of-file token.
                Anything else
                    Append the current input character to the comment token's data.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '<')
            {
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.CommentLessThanSign);
            }
            else if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentEndDash);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.EmitComment();
                this.EmitEndOfFile();
            }
            else
            {
                this.CommentData.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentLessThanSign()
        {
            /*
            8.2.4.46 Comment less-than sign state
            See: http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-state

            Consume the next input character:
                "!" (U+0021)
                    Append the current input character to the comment token’s data. 
                    Switch to the comment less-than sign bang state.
                "<" (U+003C)
                    Append the current input character to the comment token’s data.
                Anything else 
                    Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '!')
            {
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.CommentLessThanSignBang);
            }
            else if (ch == '<')
            {
                this.CommentData.Append(ch);
            }
            else
            {
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentLessThanSignBang()
        {
            /*
            8.2.4.47 Comment less-than sign bang state
            See: http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-bang-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment less-than sign bang dash state.
                Anything else 
                    Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentLessThanSignBangDash);
            }
            else
            {
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentLessThanSignBangDash()
        {
            /*
            8.2.4.48 Comment less-than sign bang dash state
            See: http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-bang-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment less-than sign bang dash dash state.
                Anything else 
                    Reconsume in the comment end dash state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentLessThanSignBangDashDash);
            }
            else
            {
                this.ReconsumeIn(StateEnum.CommentEndDash);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentLessThanSignBangDashDash()
        {
            /*
            8.2.4.49 Comment less-than sign bang dash dash state
            See: http://www.w3.org/TR/html52/syntax.html#comment-less-than-sign-bang-dash-dash-state

            Consume the next input character:
                ">" (U+003E)
                EOF 
                    Reconsume in the comment end state.
                Anything else 
                    Parse error. Reconsume in the comment end state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if ((ch == '>') || (ch == Characters.EOF))
            {
                this.ReconsumeIn(StateEnum.CommentEnd);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.ReconsumeIn(StateEnum.CommentEnd);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentEndDash()
        {
            /*
            8.2.4.50 Comment end dash state
            See: http://www.w3.org/TR/html52/syntax.html#comment-end-dash-state

            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment end state
                EOF
                    Parse error. Emit the comment token. Emit an end-of-file token.
                Anything else
                    Append a U+002D HYPHEN-MINUS character (-) to the comment token’s data.
                    Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentEnd);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.EmitComment();
                this.EmitEndOfFile();
            }
            else
            {
                this.CommentData.Append('-');
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentEnd()
        {
            /*
            8.2.4.51 Comment end state
            See: http://www.w3.org/TR/html52/syntax.html#comment-end-state

            Consume the next input character:
                ">" (U+003E)
                    Switch to the data state. Emit the comment token.
                "!" (U+0021)
                    Switch to the comment end bang state.
                "-" (U+002D)
                    Append a U+002D HYPHEN-MINUS character (-) to the comment token’s data.
                EOF
                    Parse error. Emit the comment token. Emit an end-of-file token.
                Anything else
                    Append two U+002D HYPHEN-MINUS characters (-) to the comment token’s data.
                    Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else if (ch == '!')
            {
                this.SwitchTo(StateEnum.CommentEndBang);
            }
            else if (ch == '-')
            {
                this.CommentData.Append('-');
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.EmitComment();
                this.EmitEndOfFile();
            }
            else
            {
                this.CommentData.Append('-');
                this.CommentData.Append('-');
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentEndBang()
        {
            /*
            8.2.4.52 Comment end bang state
            See: http://www.w3.org/TR/html52/syntax.html#comment-end-bang-state

            Consume the next input character:
                "-" (U+002D)
                    Append two "-" (U+002D) characters and a "!" (U+0021) character to the comment token's data. Switch
                    to the comment end dash state.
                ">" (U+003E)
                    Parse error. Switch to the data state. Emit the comment token.
                EOF
                    Parse error. Emit the comment token. Emit an end-of-file token.
                Anything else
                    Append two U+002D HYPHEN-MINUS characters (-) and a U+0021 EXCLAMATION MARK character (!)
                    to the comment token’s data. Reconsume in the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.CommentData.Append("--!");
                this.SwitchTo(StateEnum.CommentEndDash);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.EmitComment();
                this.EmitEndOfFile();
            }
            else
            {
                this.CommentData.Append("--!");
                this.ReconsumeIn(StateEnum.Comment);
            }
        }

        #endregion

        #region DocTypes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocType()
        {
            /*
            8.2.4.53 DOCTYPE state
            See: http://www.w3.org/TR/html52/syntax.html#doctype-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before DOCTYPE name state.
                EOF
                    Parse error. Create a new DOCTYPE token. 
                    Set its force-quirks flag to on. 
                    Emit the token. 
                    Emit an end-of-file token.
                Anything else
                    Parse error. Reconsume in the before DOCTYPE name state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeDocTypeName);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.NewDocType();
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.ReconsumeIn(StateEnum.BeforeDocTypeName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeDocTypeName()
        {
            /*
            8.2.4.54 Before DOCTYPE name state
            See: http://www.w3.org/TR/html52/syntax.html#before-doctype-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                Uppercase ASCII letter
                    Create a new DOCTYPE token. Set the token's name to the lowercase version of the current input
                    character (add 0x0020 to the character's code point). Switch to the DOCTYPE name state.
                U+0000 NULL
                    Parse error. Create a new DOCTYPE token. Set the token's name to a U+FFFD REPLACEMENT CHARACTER
                    character. Switch to the DOCTYPE name state.
                ">" (U+003E)
                    Parse error. Create a new DOCTYPE token. Set its force-quirks flag to on. Switch to the data state.
                    Emit the token.
                EOF
                    Parse error. Create a new DOCTYPE token. 
                    Set its force-quirks flag to on. 
                    Emit the token. 
                    Emit an end-of-file token.
                Anything else
                    Create a new DOCTYPE token. Set the token's name to the current input character. Switch to the
                    DOCTYPE name state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.NewDocType();
                this.DocTypeName = new StringBuilder();
                this.DocTypeName.Append(ch.ToLowercaseAsciiLetter());
                this.SwitchTo(StateEnum.DocTypeName);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.NewDocType();
                this.DocTypeName = new StringBuilder();
                this.DocTypeName.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.DocTypeName);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.NewDocType();
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.NewDocType();
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.NewDocType();
                this.DocTypeName = new StringBuilder();
                this.DocTypeName.Append(ch);
                this.SwitchTo(StateEnum.DocTypeName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocTypeName()
        {
            /*
            8.2.4.55 DOCTYPE name state
            See: http://www.w3.org/TR/html52/syntax.html#doctype-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the after DOCTYPE name state.
                ">" (U+003E)
                    Switch to the data state. Emit the current DOCTYPE token.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current DOCTYPE token's name.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current DOCTYPE token's name.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current DOCTYPE token's name.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.AfterDocTypeName);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.DocTypeName.Append(ch.ToLowercaseAsciiLetter());
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeName.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.DocTypeName.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterDocTypeName()
        {
            /*
            8.2.4.56 After DOCTYPE name state
            See: http://www.w3.org/TR/html52/syntax.html#after-doctype-name-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                ">" (U+003E)
                    Switch to the data state. Emit the current DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    If the six characters starting from the current input character are an ASCII case-insensitive match
                    for the word "PUBLIC", then consume those characters and switch to the after DOCTYPE public keyword
                    state.

                    Otherwise, if the six characters starting from the current input character are an ASCII
                    case-insensitive match for the word "SYSTEM", then consume those characters and switch to the
                    after DOCTYPE system keyword state.

                    Otherwise, this is a parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the
                    bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                string tmp = this.ConsumeNextInputCharacters(5, false);
                if (((ch == 'P') || (ch == 'p')) && tmp.Equals("UBLIC", StringComparison.OrdinalIgnoreCase))
                {
                    this.SwitchTo(StateEnum.AfterDocTypePublicKeyword);
                }
                else if (((ch == 'S') || (ch == 's')) && tmp.Equals("YSTEM", StringComparison.OrdinalIgnoreCase))
                {
                    this.SwitchTo(StateEnum.AfterDocTypeSystemKeyword);
                }
                else
                {
                    this.InformParseError(Parsing.ParseError.InvalidDocType);
                    this.DocTypeForceQuirks = true;
                    this.SwitchTo(StateEnum.BogusDocType);

                    // Return the 5 characters we sampled, but consume still consume the one character at the start.
                    this.ReconsumeInputCharacters(tmp);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterDocTypePublicKeyword()
        {
            /*
            8.2.4.57 After DOCTYPE public keyword state
            See: http://www.w3.org/TR/html52/syntax.html#after-doctype-public-keyword-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before DOCTYPE public identifier state.
                U+0022 QUOTATION MARK (")
                    Parse error. Set the DOCTYPE token's public identifier to the empty string (not missing), then switch
                    to the DOCTYPE public identifier (double-quoted) state.
                "'" (U+0027)
                    Parse error. Set the DOCTYPE token's public identifier to the empty string (not missing), then switch
                    to the DOCTYPE public identifier (single-quoted) state.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeDocTypePublicIdentifier);
            }
            else if (ch == '\u0022')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypePublicIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypePublicIdentifierDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypePublicIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypePublicIdentifierSingleQuoted);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeDocTypePublicIdentifier()
        {
            /*
            8.2.4.58 Before DOCTYPE public identifier state
            See: http://www.w3.org/TR/html52/syntax.html#before-doctype-public-identifier-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                U+0022 QUOTATION MARK (")
                    Set the DOCTYPE token's public identifier to the empty string (not missing), then switch to the
                    DOCTYPE public identifier (double-quoted) state.
                "'" (U+0027)
                    Set the DOCTYPE token's public identifier to the empty string (not missing), then switch to the
                    DOCTYPE public identifier (single-quoted) state.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '\u0022')
            {
                this.DocTypePublicIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypePublicIdentifierDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.DocTypePublicIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypePublicIdentifierSingleQuoted);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocTypePublicIdentifierDoubleQuoted()
        {
            /*
            8.2.4.59 DOCTYPE public identifier (double-quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#doctype-public-identifier-double-quoted-state

            Consume the next input character:
                U+0022 QUOTATION MARK (")
                    Switch to the after DOCTYPE public identifier state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current DOCTYPE token's public
                    identifier.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current DOCTYPE token's public identifier.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '\u0022')
            {
                this.SwitchTo(StateEnum.AfterDocTypePublicIdentifier);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypePublicIdentifier.Append(Characters.ReplacementCharacter);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.DocTypePublicIdentifier.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocTypePublicIdentifierSingleQuoted()
        {
            /*
            8.2.4.60 DOCTYPE public identifier (single-quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#doctype-public-identifier-single-quoted-state

            Consume the next input character:
                "'" (U+0027)
                    Switch to the after DOCTYPE public identifier state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current DOCTYPE token's
                    public identifier.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current DOCTYPE token's public identifier.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '\u0027')
            {
                this.SwitchTo(StateEnum.AfterDocTypePublicIdentifier);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypePublicIdentifier.Append(Characters.ReplacementCharacter);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.DocTypePublicIdentifier.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterDocTypePublicIdentifier()
        {
            /*
            8.2.4.61 After DOCTYPE public identifier state
            See: http://www.w3.org/TR/html52/syntax.html#after-doctype-public-identifier-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the between DOCTYPE public and system identifiers state.
                ">" (U+003E)
                    Switch to the data state. Emit the current DOCTYPE token.
                U+0022 QUOTATION MARK (")
                    Parse error. Set the DOCTYPE token's system identifier to the empty string (not missing), then
                    switch to the DOCTYPE system identifier (double-quoted) state.
                "'" (U+0027)
                    Parse error. Set the DOCTYPE token's system identifier to the empty string (not missing), then
                    switch to the DOCTYPE system identifier (single-quoted) state.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BetweenDocTypePublicAndSystemIdentifiers);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == '\u0022')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierSingleQuoted);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBetweenDocTypePublicAndSystemIdentifiers()
        {
            /*
            8.2.4.62 Between DOCTYPE public and system identifiers state
            See: http://www.w3.org/TR/html52/syntax.html#between-doctype-public-and-system-identifiers-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                ">" (U+003E)
                    Switch to the data state. Emit the current DOCTYPE token.
                U+0022 QUOTATION MARK (")
                    Set the DOCTYPE token's system identifier to the empty string (not missing), then switch to the
                    DOCTYPE system identifier (double-quoted) state.
                "'" (U+0027)
                    Set the DOCTYPE token's system identifier to the empty string (not missing), then switch to the
                    DOCTYPE system identifier (single-quoted) state.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == '\u0022')
            {
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierSingleQuoted);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterDocTypeSystemKeyword()
        {
            /*
            8.2.4.63 After DOCTYPE system keyword state
            See: http://www.w3.org/TR/html52/syntax.html#after-doctype-system-keyword-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before DOCTYPE system identifier state.
                U+0022 QUOTATION MARK (")
                    Parse error. Set the DOCTYPE token's system identifier to the empty string (not missing), then
                    switch to the DOCTYPE system identifier (double-quoted) state.
                "'" (U+0027)
                    Parse error. Set the DOCTYPE token's system identifier to the empty string (not missing), then
                    switch to the DOCTYPE system identifier (single-quoted) state.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeDocTypeSystemIdentifier);
            }
            else if (ch == '\u0022')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierSingleQuoted);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeDocTypeSystemIdentifier()
        {
            /*
            8.2.4.64 Before DOCTYPE system identifier state
            See: http://www.w3.org/TR/html52/syntax.html#before-doctype-system-identifier-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                U+0022 QUOTATION MARK (")
                    Set the DOCTYPE token's system identifier to the empty string (not missing), then switch to the
                    DOCTYPE system identifier (double-quoted) state.
                "'" (U+0027)
                    Set the DOCTYPE token's system identifier to the empty string (not missing), then switch to the
                    DOCTYPE system identifier (single-quoted) state.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the bogus DOCTYPE state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '\u0022')
            {
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierDoubleQuoted);
            }
            else if (ch == '\u0027')
            {
                this.DocTypeSystemIdentifier = new StringBuilder();
                this.SwitchTo(StateEnum.DocTypeSystemIdentifierSingleQuoted);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocTypeSystemIdentifierDoubleQuoted()
        {
            /*
            8.2.4.65 DOCTYPE system identifier (double-quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#doctype-system-identifier-double-quoted-state

            Consume the next input character:
                U+0022 QUOTATION MARK (")
                    Switch to the after DOCTYPE system identifier state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current DOCTYPE token's
                    system identifier.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current DOCTYPE token's system identifier.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '\u0022')
            {
                this.SwitchTo(StateEnum.AfterDocTypeSystemIdentifier);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeSystemIdentifier.Append(Characters.ReplacementCharacter);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.DocTypeSystemIdentifier.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocTypeSystemIdentifierSingleQuoted()
        {
            /*
            8.2.4.66 DOCTYPE system identifier (single-quoted) state
            See: http://www.w3.org/TR/html52/syntax.html#doctype-system-identifier-single-quoted-state

            Consume the next input character:
                "'" (U+0027)
                    Switch to the after DOCTYPE system identifier state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current DOCTYPE token's system
                    identifier.
                ">" (U+003E)
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Switch to the data state. Emit that
                    DOCTYPE token.
                EOF
                    Parse error. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Append the current input character to the current DOCTYPE token's system identifier.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '\u0027')
            {
                this.SwitchTo(StateEnum.AfterDocTypeSystemIdentifier);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeSystemIdentifier.Append(Characters.ReplacementCharacter);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.DocTypeSystemIdentifier.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterDocTypeSystemIdentifier()
        {
            /*
            8.2.4.67 After DOCTYPE system identifier state
            See: http://www.w3.org/TR/html52/syntax.html#after-doctype-system-identifier-state

            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                ">" (U+003E)
                    Switch to the data state. Emit the current DOCTYPE token.
                EOF
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on.
                    Emit that DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Parse error. Switch to the bogus DOCTYPE state. (This does not set the DOCTYPE token's
                    force-quirks flag to on.)
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignore
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.SwitchTo(StateEnum.BogusDocType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBogusDocType()
        {
            /*
            8.2.4.68 Bogus DOCTYPE state
            See: http://www.w3.org/TR/html52/syntax.html#bogus-doctype-state

            Consume the next input character:
                ">" (U+003E)
                    Switch to the data state. Emit the DOCTYPE token.
                EOF
                    Emit the DOCTYPE token. Emit an end-of-file token.
                Anything else
                    Ignore the character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
            }
            else if (ch == Characters.EOF)
            {
                this.EmitDocType();
                this.EmitEndOfFile();
            }
            else
            {
                // Ignore
            }
        }

        #endregion

        #region CDATA

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCDataSection()
        {
            /*
            8.2.4.69 CDATA section state
            See: http://www.w3.org/TR/html52/syntax.html#CDATA-section-state

            Consume the next input character:
                "]" (U+005D)
                    Switch to the CDATA section bracket state.
                EOF 
                    Parse error. Emit an end-of-file token.
                Anything else 
                    Emit the current input character as a character token.

                NOTE: U+0000 NULL characters are handled in the tree construction stage,
                as part of the in foreign content insertion mode, which is the only place
                where CDATA sections can appear.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == ']')
            {
                this.SwitchTo(StateEnum.CDataSectionBracket);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterData);
                this.EmitEndOfFile();
            }
            else
            {
                this.EmitCharacter(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCDataSectionBracket()
        {
            /*
            8.2.4.70 CDATA section bracket state
            See: http://www.w3.org/TR/html52/syntax.html#CDATA-section-bracket-state

            Consume the next input character:
                "]" (U+005D)
                    Switch to the CDATA section end state.
                Anything else 
                    Emit a U+005D RIGHT SQUARE BRACKET character token. 
                    Reconsume in the CDATA section state
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == ']')
            {
                this.SwitchTo(StateEnum.CDataSectionEnd);
            }
            else
            {
                this.EmitCharacter(']');
                this.ReconsumeIn(StateEnum.CDataSection);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCDataSectionEnd()
        {
            /*
            8.2.4.71 CDATA section end state
            See: http://www.w3.org/TR/html52/syntax.html#CDATA-section-end-state

            Consume the next input character:
                "]" (U+005D)
                    Emit a U+005D RIGHT SQUARE BRACKET character token.
                ">" (U+003E)
                    Switch to the data state.
                Anything else
                    Emit two U+005D RIGHT SQUARE BRACKET character tokens. 
                    Reconsume in the CDATA section state
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == ']')
            {
                this.EmitCharacter(']');
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
            }
            else
            {
                this.EmitCharacter(']');
                this.EmitCharacter(']');
                this.ReconsumeIn(StateEnum.CDataSection);
            }
        }

        #endregion

        #region Character Reference
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCharacterReference()
        {
            /*
            8.2.4.72 Character reference state
            See: http://www.w3.org/TR/html52/syntax.html#character-reference-state

            Set the temporary buffer to the empty string. 
            Append a U+0026 AMPERSAND (&) character to the temporary buffer.

            Consume the next input character:
                U+0009 CHARACTER TABULATION (tab) 
                U+000A LINE FEED (LF) 
                U+000C FORM FEED (FF) 
                U+0020 SPACE 
                U+003C LESS-THAN SIGN 
                U+0026 AMPERSAND 
                EOF 
                    Reconsume in the character reference end state.
                U+0023 NUMBER SIGN (#) 
                    Append the current input character to the temporary buffer. Switch to the numeric character reference state.
                Anything else 
                    *** See comments inline ***
                    
            */
            this.TempBuffer.Clear();
            this.TempBuffer.Append('&');

            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter() || (ch == '<') || (ch == '&') || (ch == Characters.EOF))
            {
                this.ReconsumeIn(StateEnum.CharacterReferenceEnd);
            }
            else if (ch == '#')
            {
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.NumericCharacterReference);
            }
            else
            {
                // IMPLEMENTATION: This is the worst description in the whole HTML5 specification. Some comments are needed.
                // The <TempBuffer> instance variable keeps track of what we've read so far of characters. It contains all
                // characters read incl. the "&", "#", "X" and all other chars we may encounter. 
                // * If all ends good, then we switch to "8.2.4.78 Numeric character reference end state", which clears the
                //   temp buffer and replaces the contents with the character reference.
                // * In any case, we end up in "8.2.4.79 Character reference end state", which emits the contents of the 
                //   temp buffer. If 8.2.4.78 was not called, then the temp buffer contains whatever raw chars we may have
                //   read so far; If 8.2.4.78 was called, then the temp buffer will contain the character.

                // Consume the maximum number of characters possible, with the consumed characters matching one of the identifiers
                // in the first column of the §8.5 Named character references table(in a case-sensitive manner).
                // Append each character to the temporary buffer when it’s consumed.

                StringBuilder characterName = new StringBuilder();
                // IMPLEMENTATION: No where does it was to use the current char, but it makes no sense to throw it away. Add it to the name.
                characterName.Append(ch);
                NamedCharacters.Match match = NamedCharacters.Match.NoMatch;
                do
                {
                    // IMPLEMENTATION: Because we need to "consume the maximum number of characters possible",
                    // we continue consuming until we hit a no-match. This means we will get one of the following matches:
                    // * None
                    // * Partial, Partial ... Partial, None
                    // * Partial, Partial ... Partial, Full, None
                    NamedCharacters.Match tmpMatch = NamedCharacters.TryGetNamedCharacter(characterName.ToString());
                    if (!tmpMatch.HasMatch)
                        break;

                    // IMPLEMENTATION: Remember the last match.
                    match = tmpMatch;

                    // IMPLEMENTATION: Add the last consumed char to the temp buffer.
                    this.TempBuffer.Append(ch);
                    // IMPLEMENTATION: Consume another character.
                    ch = this.ConsumeNextInputCharacter();
                    if (ch == Characters.EOF)
                        break;
                    characterName.Append(ch);
                }
                while (true);

                // IMPLEMENTATION: Always return the last consumed char, because it was never used for a match and it is not for us.
                this.ReconsumeInputCharacter(ch);

                // A partial match is not a match!
                if (match.HasMatch && !match.IsFullMatch)
                    match = NamedCharacters.Match.NoMatch; 

                // IMPLEMENTATION: It is unclear from the HTML5 specification how to read the remaining clauses. We assume they are independent.

                if (!match.HasMatch)
                {
                    // If no match can be made and the temporary buffer consists of a U+0026 AMPERSAND character(&) followed by a
                    // sequence of one or more alphanumeric ASCII characters and a U+003B SEMICOLON character(;), then this is a parse error.
                    string tmp = this.TempBuffer.ToString();
                    if (tmp.Length >= 3)
                    {
                        if ((tmp[0] == '&') && (tmp[tmp.Length - 1] == ';') && tmp.Skip(1).Take(tmp.Length - 2).All(c => c.IsAlphaNumericAsciiCharacter()))
                        {
                            // IMPLEMENTATION: This is a case like "&foo;", which is an unrecognized named character reference.
                            this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                        }
                    }
                

                    // If no match can be made, switch to the character reference end state.
                    this.SwitchTo(StateEnum.CharacterReferenceEnd);

                    // IMPLEMENTATION: We are done! Return here! "8.2.4.79 Character reference end" outputs the temp buffer.
                    return;
                }
                else
                {
                    // If the character reference was consumed as part of an attribute (return state is either attribute value(double - quoted)
                    //    state, attribute value(single-quoted) state or attribute value(unquoted) state), 
                    // and the last character matched is not a U + 003B SEMICOLON character(;), 
                    // and the next input character is either a U + 003D EQUALS SIGN character(=) 
                    //    or an alphanumeric ASCII character, 
                    // then, for historical reasons, switch to the character reference end state.
                    bool attrib = (this.ReturnState == StateEnum.AttributeValueDoubleQuoted) || (this.ReturnState == StateEnum.AttributeValueSingleQuoted) || (this.ReturnState == StateEnum.AttributeValueUnquoted);
                    if (attrib && (match.LastMatchedCharacter != ';') && ((ch == '=') || ch.IsAlphaNumericAsciiCharacter()))
                    {
                        // IMPLEMENTATION: This is a case like "&amp=" inside an attribute, which does not qualify
                        // for one of the *hack names* listed in "8.5. Named character references". 
                        // Return here and let "8.2.4.79 Character reference end" outputs the temp buffer.
                        this.SwitchTo(StateEnum.CharacterReferenceEnd);
                        return;
                    }

                    // If the last character matched is not a U + 003B SEMICOLON character(;), this is a parse error.
                    if (match.LastMatchedCharacter != ';')
                    {
                        // IMPLEMENTATION: This is a case like "&amp", which is a character reference is missing an ending semicolon ";".
                        // We still output the character. Only few named characters allow this hack and are listed in "8.5. Named character references"
                        this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                    }

                    // Set the temporary buffer to the empty string.
                    // Append one or two characters corresponding to the character reference name
                    // (as given by the second column of the §8.5 Named character references table) to the temporary buffer.
                    this.TempBuffer.Clear();
                    this.TempBuffer.Append(match.Character);

                    // Switch to the character reference end state.
                    // IMPLEMENTATION: Let "8.2.4.79 Character reference end" outputs the temp buffer.
                    this.SwitchTo(StateEnum.CharacterReferenceEnd);
                }
            }
        }

        /// <summary>
        /// See: http://www.w3.org/TR/html52/syntax.html#character-reference-code
        /// </summary>
        private int CharacterReferenceCode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateNumericCharacterReference()
        {
            /*
            8.2.4.73 Numeric character reference state
            See: http://www.w3.org/TR/html52/syntax.html#numeric-character-reference-state

            Set the character reference code to zero (0).
            
            Consume the next input character:
                U+0078 LATIN SMALL LETTER X 
                U+0058 LATIN CAPITAL LETTER X 
                    Append the current input character to the temporary buffer. Switch to the hexadecimal character reference start state.
                Anything else 
                    Reconsume in the decimal character reference start state.
            */
            this.CharacterReferenceCode = 0;

            char ch = this.ConsumeNextInputCharacter();
            if ((ch == 'x') || (ch == 'X'))
            {
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.HexadecimalCharacterReferenceStart);
            }
            else
            {
                this.ReconsumeIn(StateEnum.DecimalCharacterReferenceStart);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateHexadecimalCharacterReferenceStart()
        {
            /*
            8.2.4.74 Hexadecimal character reference start state
            See: http://www.w3.org/TR/html52/syntax.html#hexadecimal-character-reference-start-state

            Consume the next input character:
                ASCII hex digit 
                    Reconsume in the hexadecimal character reference state.
                Anything else 
                    Parse error. Reconsume in the character reference end state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiHexDigit())
            {
                this.ReconsumeIn(StateEnum.HexadecimalCharacterReference);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                this.ReconsumeIn(StateEnum.CharacterReferenceEnd);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDecimalCharacterReferenceStart()
        {
            /*
            8.2.4.75 Decimal character reference start state
            See: http://www.w3.org/TR/html52/syntax.html#decimal-character-reference-start-state

            Consume the next input character:
                ASCII digit 
                    Reconsume in the decimal character reference state.
                Anything else 
                    Parse error. Reconsume in the character reference end state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiDigit())
            {
                this.ReconsumeIn(StateEnum.DecimalCharacterReference);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                this.ReconsumeIn(StateEnum.CharacterReferenceEnd);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateHexadecimalCharacterReference()
        {
            /*
            8.2.4.76 Hexadecimal character reference state
            See: http://www.w3.org/TR/html52/syntax.html#hexadecimal-character-reference-state

            Consume the next input character:
                Uppercase ASCII hex digit 
                    Multiply the character reference code by 16. Add a numeric version of the current input character
                    as a hexademical digit (subtract 0x0037 from the character’s code point) to the character reference code.
                Lowercase ASCII hex digit 
                    Multiply the character reference code by 16. Add a numeric version of the current input character
                    as a hexademical digit (subtract 0x0057 from the character’s code point) to the character reference code.
                ASCII digit 
                    Multiply the character reference code by 16. Add a numeric version of the current input character
                    (subtract 0x0030 from the character’s code point) to the character reference code.
                U+003B SEMICOLON character (;) 
                    Switch to the numeric character reference end state.
                Anything else 
                    Parse error. Reconsume in the numeric character reference end state.
            */
            char ch = this.ConsumeNextInputCharacter();

            // IMPLEMENTATION: There is a BUG in the HTML5 specification. Must handle ASCII-Digit *BEFORE* ASCII-Hex-Digits.

            if (ch.IsAsciiDigit())
            {
                this.CharacterReferenceCode = this.CharacterReferenceCode * 16;
                this.CharacterReferenceCode = this.CharacterReferenceCode + (ch - 0x0030);
            }
            else if (ch.IsUppercaseAsciiHexDigit())
            {
                this.CharacterReferenceCode = this.CharacterReferenceCode * 16;
                this.CharacterReferenceCode = this.CharacterReferenceCode + (ch - 0x0037);
            }
            else if (ch.IsLowercaseAsciiHexDigit())
            {
                this.CharacterReferenceCode = this.CharacterReferenceCode * 16;
                this.CharacterReferenceCode = this.CharacterReferenceCode + (ch - 0x0057);
            }
            else if (ch == ';')
            {
                this.SwitchTo(StateEnum.NumericCharacterReferenceEnd);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                this.ReconsumeIn(StateEnum.NumericCharacterReferenceEnd);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDecimalCharacterReference()
        {
            /*
            8.2.4.77 Decimal character reference state
            See: http://www.w3.org/TR/html52/syntax.html#decimal-character-reference-state

            Consume the next input character:
                ASCII digit 
                    Multiply the character reference code by 16. Add a numeric version of the current input character
                    (subtract 0x0030 from the character’s code point) to the character reference code.
                U+003B SEMICOLON character (;) 
                    Switch to the numeric character reference end state.
                Anything else 
                    Parse error. Reconsume in the numeric character reference end state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsAsciiDigit())
            {
                // IMPLEMENTATION: There is a BUG in the HTML5 specification. Must multiply by 10 (not by 16).
                this.CharacterReferenceCode = this.CharacterReferenceCode * 10;
                this.CharacterReferenceCode = this.CharacterReferenceCode + (ch - 0x0030);
            }
            else if (ch == ';')
            {
                this.SwitchTo(StateEnum.NumericCharacterReferenceEnd);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                this.ReconsumeIn(StateEnum.NumericCharacterReferenceEnd);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateNumericCharacterReferenceEnd()
        {
            /*
            8.2.4.78 Numeric character reference end state
            See: http://www.w3.org/TR/html52/syntax.html#numeric-character-reference-end-state
            */

            // Check the character reference code:
            
            // If that number is one of the numbers in the first column of the following table, then this is a parse error. 
            // Find the row with that number in the first column, and set the character reference code to the number in the
            // second column of that row.
            char replacementChar = '\u0000';
            if (this.CharacterReferenceCode < Tokenizer.CharacterReferenceReplacetCharacters.Length)
                replacementChar = Tokenizer.CharacterReferenceReplacetCharacters[this.CharacterReferenceCode];
            if (replacementChar != '\u0000')
            {
                // The code-point is defined in the table.
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                this.CharacterReferenceCode = replacementChar;
            }

            // If the number is in the range 0xD800 to 0xDFFF or is greater than 0x10FFFF, then this is a parse error.
            // Set the character reference code to 0xFFFD.
            else if (((0xD800 <= this.CharacterReferenceCode) && (this.CharacterReferenceCode <= 0xDFFF)) || (this.CharacterReferenceCode > 0x10FFFF))
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                this.CharacterReferenceCode = 0xFFFD;
            }

            // If the number is in the range 0x0001 to 0x0008, 0x000D to 0x001F, 0x007F to 0x009F, 0xFDD0 to 0xFDEF, 
            // or is one of 0x000B, 0xFFFE, 0xFFFF, 0x1FFFE, 0x1FFFF, 0x2FFFE, 0x2FFFF, 0x3FFFE, 0x3FFFF, 0x4FFFE, 
            // 0x4FFFF, 0x5FFFE, 0x5FFFF, 0x6FFFE, 0x6FFFF, 0x7FFFE, 0x7FFFF, 0x8FFFE, 0x8FFFF, 0x9FFFE, 0x9FFFF, 0xAFFFE, 
            // 0xAFFFF, 0xBFFFE, 0xBFFFF, 0xCFFFE, 0xCFFFF, 0xDFFFE, 0xDFFFF, 0xEFFFE, 0xEFFFF, 0xFFFFE, 0xFFFFF, 0x10FFFE,
            // or 0x10FFFF, then this is a parse error.

            else if (
#pragma warning disable SA1117
                    this.CharacterReferenceCode.IsInRange(0x0001, 0x0008) ||
                    this.CharacterReferenceCode.IsInRange(0x000D, 0x001F) ||
                    this.CharacterReferenceCode.IsInRange(0x007F, 0x009F) ||
                    this.CharacterReferenceCode.IsInRange(0xFDD0, 0xFDEF) ||
                    this.CharacterReferenceCode.IsOneOf(
                        0x000B, 0xFFFE, 0xFFFF, 0x1FFFE, 0x1FFFF, 0x2FFFE, 0x2FFFF, 0x3FFFE,
                        0x3FFFF, 0x4FFFE, 0x4FFFF, 0x5FFFE, 0x5FFFF, 0x6FFFE, 0x6FFFF, 0x7FFFE, 0x7FFFF, 0x8FFFE, 0x8FFFF,
                        0x9FFFE, 0x9FFFF, 0xAFFFE, 0xAFFFF, 0xBFFFE, 0xBFFFF, 0xCFFFE, 0xCFFFF, 0xDFFFE, 0xDFFFF, 0xEFFFE,
                        0xEFFFF, 0xFFFFE, 0xFFFFF, 0x10FFFE, 0x10FFFF))
#pragma warning restore SA1117 // Parameters must be on same line or separate lines
            {
                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
            }

            // Set the temporary buffer to the empty string. 
            // Append the Unicode character with code point equal to the character reference code to the temporary buffer. 
            // Switch to the character reference end state.
            this.TempBuffer.Clear();
            this.TempBuffer.Append(Char.ConvertFromUtf32(this.CharacterReferenceCode));
            this.SwitchTo(StateEnum.CharacterReferenceEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCharacterReferenceEnd()
        {
            /*
            8.2.4.79 Character reference end state
            See: http://www.w3.org/TR/html52/syntax.html#character-reference-end-state

            Consume the next input character.

            Check the return state:
                attribute value (double-quoted) state 
                attribute value (single-quoted) state 
                attribute value (unquoted) state 
                    Append each character in the temporary buffer (in the order they were added to the buffer) to the current attribute’s value.
                Anything else 
                    For each of the characters in the temporary buffer (in the order they were added to the buffer), emit the character as a character token.

            Reconsume in the return state.
            */
            char ch = this.ConsumeNextInputCharacter();

            if ((this.ReturnState == StateEnum.AttributeValueDoubleQuoted) || (this.ReturnState == StateEnum.AttributeValueSingleQuoted) || (this.ReturnState == StateEnum.AttributeValueUnquoted))
            {
                this.AttributeValue.Append(this.TempBuffer.ToString());
            }
            else
            {
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
            }

            this.ReconsumeIn(this.ReturnState);
        }

        private static readonly char[] CharacterReferenceReplacetCharacters = Tokenizer.GetCharacterReferenceReplacetCharacters();

        private static char[] GetCharacterReferenceReplacetCharacters()
        {
            char[] chars = new char[256];

            // List found in 8.2.4.78. Numeric character reference end state
            // See: http://www.w3.org/TR/html52/syntax.html#numeric-character-reference-end-state
            // Every array index not set to a value (i.e. \u0000) is not defined in the table.
            chars[0x00] = '\uFFFD';
            chars[0x80] = '\u20AC';
            chars[0x82] = '\u201A';
            chars[0x83] = '\u0192';
            chars[0x84] = '\u201E';
            chars[0x85] = '\u2026';
            chars[0x86] = '\u2020';
            chars[0x87] = '\u2021';
            chars[0x88] = '\u02C6';
            chars[0x89] = '\u2030';
            chars[0x8A] = '\u0160';
            chars[0x8B] = '\u2039';
            chars[0x8C] = '\u0152';
            chars[0x8E] = '\u017D';
            chars[0x91] = '\u2018';
            chars[0x92] = '\u2019';
            chars[0x93] = '\u201C';
            chars[0x94] = '\u201D';
            chars[0x95] = '\u2022';
            chars[0x96] = '\u2013';
            chars[0x97] = '\u2014';
            chars[0x98] = '\u02DC';
            chars[0x99] = '\u2122';
            chars[0x9A] = '\u0161';
            chars[0x9B] = '\u203A';
            chars[0x9C] = '\u0153';
            chars[0x9E] = '\u017E';
            chars[0x9F] = '\u0178';

            return chars;
        }

        #endregion

        #endregion
    }
}
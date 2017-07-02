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
            /// See: 8.2.4.1 Data state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            Data = 1,

            /// <summary>
            /// Character reference in data state
            /// See: 8.2.4.2 Character reference in data state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CharacterReferenceInData = 2,

            /// <summary>
            /// RCDATA state
            /// See: 8.2.4.3 RCDATA state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RcData = 3,

            /// <summary>
            /// Character reference in RCDATA state
            /// See: 8.2.4.4 Character reference in RCDATA state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CharacterReferenceInRcData = 4,

            /// <summary>
            /// RAWTEXT state
            /// See: 8.2.4.5 RAWTEXT state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RawText = 5,

            /// <summary>
            /// Script data state
            /// See: 8.2.4.6 Script data state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptData = 6,

            /// <summary>
            /// PLAINTEXT state
            /// See: 8.2.4.7 PLAINTEXT state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            PlainText = 7,

            /// <summary>
            /// Tag open state
            /// See: 8.2.4.8 Tag open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            TagOpen = 8,

            /// <summary>
            /// End tag open state
            /// See: 8.2.4.9 End tag open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            EndTagOpen = 9,

            /// <summary>
            /// Tag name state
            /// See: 8.2.4.10 Tag name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            TagName = 10,

            /// <summary>
            /// RCDATA less-than sign state
            /// See: 8.2.4.11 RCDATA less-than sign state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RcDataLessThanSign = 11,

            /// <summary>
            /// RCDATA end tag open state
            /// See: 8.2.4.12 RCDATA end tag open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RcDataEndTagOpen = 12,

            /// <summary>
            /// RCDATA end tag name state
            /// See: 8.2.4.13 RCDATA end tag name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RcDataEndTagName = 13,

            /// <summary>
            /// RAWTEXT less-than sign state
            /// See: 8.2.4.14 RAWTEXT less-than sign state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RawTextLessThanSign = 14,

            /// <summary>
            /// RAWTEXT end tag open state
            /// See: 8.2.4.15 RAWTEXT end tag open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RawTextEndTagOpen = 15,

            /// <summary>
            /// RAWTEXT end tag name state
            /// See: 8.2.4.16 RAWTEXT end tag name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            RawTextEndTagName = 16,

            /// <summary>
            /// Script data less-than sign state
            /// See: 8.2.4.17 Script data less-than sign state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataLessThanSign = 17,

            /// <summary>
            /// Script data end tag open state
            /// See: 8.2.4.18 Script data end tag open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEndTagOpen = 18,

            /// <summary>
            /// Script data end tag name state
            /// See: 8.2.4.19 Script data end tag name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEndTagName = 19,

            /// <summary>
            /// Script data escape start state
            /// See: 8.2.4.20 Script data escape start state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapeStart = 20,

            /// <summary>
            /// Script data escape start dash state
            /// See: 8.2.4.21 Script data escape start dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapeStartDash = 21,

            /// <summary>
            /// Script data escaped state
            /// See: 8.2.4.22 Script data escaped state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscaped = 22,

            /// <summary>
            /// Script data escaped dash state
            /// See: 8.2.4.23 Script data escaped dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapedDash = 23,

            /// <summary>
            /// Script data escaped dash dash state
            /// See: 8.2.4.24 Script data escaped dash dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapedDashDash = 24,

            /// <summary>
            /// Script data escaped less-than sign state
            /// See: 8.2.4.25 Script data escaped less-than sign state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapedLessThanSign = 25,

            /// <summary>
            /// Script data escaped end tag open state
            /// See: 8.2.4.26 Script data escaped end tag open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapedEndTagOpen = 26,

            /// <summary>
            /// Script data escaped end tag name state
            /// See: 8.2.4.27 Script data escaped end tag name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataEscapedEndTagName = 27,

            /// <summary>
            /// Script data double escape start state
            /// See: 8.2.4.28 Script data double escape start state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataDoubleEscapeStart = 28,

            /// <summary>
            /// Script data double escaped state
            /// See: 8.2.4.29 Script data double escaped state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataDoubleEscaped = 29,

            /// <summary>
            /// Script data double escaped dash state
            /// See: 8.2.4.30 Script data double escaped dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataDoubleEscapedDash = 30,

            /// <summary>
            /// Script data double escaped dash dash state
            /// See: 8.2.4.31 Script data double escaped dash dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataDoubleEscapedDashDash = 31,

            /// <summary>
            /// Script data double escaped less-than sign state
            /// See: 8.2.4.32 Script data double escaped less-than sign state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataDoubleEscapedLessThanSign = 32,

            /// <summary>
            /// Script data double escape end state
            /// See: 8.2.4.33 Script data double escape end state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            ScriptDataDoubleEscapeEnd = 33,

            /// <summary>
            /// Before attribute name state
            /// See: 8.2.4.34 Before attribute name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BeforeAttributeName = 34,

            /// <summary>
            /// Attribute name state
            /// See: 8.2.4.35 Attribute name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AttributeName = 35,

            /// <summary>
            /// After attribute name state
            /// See: 8.2.4.36 After attribute name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterAttributeName = 36,

            /// <summary>
            /// Before attribute value state
            /// See: 8.2.4.37 Before attribute value state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BeforeAttributeValue = 37,

            /// <summary>
            /// Attribute value (double-quoted) state
            /// See: 8.2.4.38 Attribute value (double-quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AttributeValueDoubleQuoted = 38,

            /// <summary>
            /// Attribute value (single-quoted) state
            /// See: 8.2.4.39 Attribute value (single-quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AttributeValueSingleQuoted = 39,

            /// <summary>
            /// Attribute value (unquoted) state
            /// See: 8.2.4.40 Attribute value (unquoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AttributeValueUnquoted = 40,

            /// <summary>
            /// Character reference in attribute value state
            /// See: 8.2.4.41 Character reference in attribute value state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CharacterReferenceInAttributeValue = 41,

            /// <summary>
            /// After attribute value (quoted) state
            /// See: 8.2.4.42 After attribute value (quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterAttributeValueQuoted = 42,

            /// <summary>
            /// Self-closing start tag state
            /// See: 8.2.4.43 Self-closing start tag state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            SelfClosingStartTag = 43,

            /// <summary>
            /// Bogus comment state
            /// See: 8.2.4.44 Bogus comment state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BogusComment = 44,

            /// <summary>
            /// Markup declaration open state
            /// See: 8.2.4.45 Markup declaration open state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            MarkupDeclarationOpen = 45,

            /// <summary>
            /// Comment start state
            /// See: 8.2.4.46 Comment start state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CommentStart = 46,

            /// <summary>
            /// Comment start dash state
            /// See: 8.2.4.47 Comment start dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CommentStartDash = 47,

            /// <summary>
            /// Comment state
            /// See: 8.2.4.48 Comment state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            Comment = 48,

            /// <summary>
            /// Comment end dash state
            /// See: 8.2.4.49 Comment end dash state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CommentEndDash = 49,

            /// <summary>
            /// Comment end state
            /// See: 8.2.4.50 Comment end state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CommentEnd = 50,

            /// <summary>
            /// Comment end bang state
            /// See: 8.2.4.51 Comment end bang state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CommentEndBang = 51,

            /// <summary>
            /// DOCTYPE state
            /// See: 8.2.4.52 DOCTYPE state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            DocType = 52,

            /// <summary>
            /// Before DOCTYPE name state
            /// See: 8.2.4.53 Before DOCTYPE name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BeforeDocTypeName = 53,

            /// <summary>
            /// DOCTYPE name state
            /// See: 8.2.4.54 DOCTYPE name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            DocTypeName = 54,

            /// <summary>
            /// After DOCTYPE name state
            /// See: 8.2.4.55 After DOCTYPE name state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterDocTypeName = 55,

            /// <summary>
            /// After DOCTYPE public keyword state
            /// See: 8.2.4.56 After DOCTYPE public keyword state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterDocTypePublicKeyword = 56,

            /// <summary>
            /// Before DOCTYPE public identifier state
            /// See: 8.2.4.57 Before DOCTYPE public identifier state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BeforeDocTypePublicIdentifier = 57,

            /// <summary>
            /// DOCTYPE public identifier (double-quoted) state
            /// See: 8.2.4.58 DOCTYPE public identifier (double-quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            DocTypePublicIdentifierDoubleQuoted = 58,

            /// <summary>
            /// DOCTYPE public identifier (single-quoted) state
            /// See: 8.2.4.59 DOCTYPE public identifier (single-quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            DocTypePublicIdentifierSingleQuoted = 59,

            /// <summary>
            /// After DOCTYPE public identifier state
            /// See: 8.2.4.60 After DOCTYPE public identifier state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterDocTypePublicIdentifier = 60,

            /// <summary>
            /// Between DOCTYPE public and system identifiers state
            /// See: 8.2.4.61 Between DOCTYPE public and system identifiers state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BetweenDocTypePublicAndSystemIdentifiers = 61,

            /// <summary>
            /// After DOCTYPE system keyword state
            /// See: 8.2.4.62 After DOCTYPE system keyword state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterDocTypeSystemKeyword = 62,

            /// <summary>
            /// Before DOCTYPE system identifier state
            /// See: 8.2.4.63 Before DOCTYPE system identifier state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BeforeDocTypeSystemIdentifier = 63,

            /// <summary>
            /// DOCTYPE system identifier (double-quoted) state
            /// See: 8.2.4.64 DOCTYPE system identifier (double-quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            DocTypeSystemIdentifierDoubleQuoted = 64,

            /// <summary>
            /// DOCTYPE system identifier (single-quoted) state
            /// See: 8.2.4.65 DOCTYPE system identifier (single-quoted) state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            DocTypeSystemIdentifierSingleQuoted = 65,

            /// <summary>
            /// After DOCTYPE system identifier state
            /// See: 8.2.4.66 After DOCTYPE system identifier state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            AfterDocTypeSystemIdentifier = 66,

            /// <summary>
            /// Bogus DOCTYPE state
            /// See: 8.2.4.67 Bogus DOCTYPE state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            BogusDocType = 67,

            /// <summary>
            /// CDATA section state
            /// See: 8.2.4.68 CDATA section state (http://www.w3.org/TR/html5/syntax.html#tokenization)
            /// </summary>
            CDataSection = 68
        }

        public Tokenizer(HtmlStream stream)
        {
            Contract.RequiresNotNull(stream, nameof(stream));

            this.HtmlStream = stream;
        }

        #region Public Interface

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

            // Continue tokenizing until a token has been emitted.
            while (this.CurrentToken.Type == TokenType.Unknown)
            {
                this.Tokenize();
            }

            return this.CurrentToken;
        }

        /// <summary>
        /// Used by the tree parsing stage to inform the tokinzer that a
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
                case StateEnum.CharacterReferenceInData:
                    this.HandleStateCharacterReferenceInData();
                    break;
                case StateEnum.RcData:
                    this.HandleStateRcData();
                    break;
                case StateEnum.CharacterReferenceInRcData:
                    this.HandleStateCharacterReferenceInRcData();
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
                case StateEnum.CharacterReferenceInAttributeValue:
                    this.HandleStateCharacterReferenceInAttributeValue();
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
                default:
                    throw new NotImplementedException();
            }
        }

        internal void SwitchTo(StateEnum state)
        {
            switch (state)
            {
                case StateEnum.Data:
                    break;
                case StateEnum.CharacterReferenceInData:
                    break;
                case StateEnum.RcData:
                    break;
                case StateEnum.CharacterReferenceInRcData:
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
                case StateEnum.CharacterReferenceInAttributeValue:
                    break;
                case StateEnum.AfterAttributeValueQuoted:
                    break;
                case StateEnum.SelfClosingStartTag:
                    break;
                case StateEnum.BogusComment:
                    // ... a comment token whose data is the concatenation of all the characters starting
                    // from and including the character that caused the state machine to switch into the bogus comment state ...
                    this.CommentData.Clear();
                    this.CommentData.Append((this.CurrentInputCharacter == Characters.Null) ? Characters.ReplacementCharacter : this.CurrentInputCharacter);
                    break;
                case StateEnum.MarkupDeclarationOpen:
                    break;
                case StateEnum.CommentStart:
                    break;
                case StateEnum.CommentStartDash:
                    break;
                case StateEnum.Comment:
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
                default:
                    break;
            }

            this.State = state;
        }

        private void InformParseError(ParseError error)
        {
            this.ParseError?.Invoke(this, new Parsing.ParseErrorEventArgs(error));
        }

        #endregion

        #region Character reading related logic

        /// <summary>
        /// The source HTML stream, where we read characters from.
        /// </summary>
        private readonly HtmlStream HtmlStream;

        /// <summary>
        /// The current input character is the last character to have been consumed.
        /// </summary>
        private char CurrentInputCharacter;

        /// <summary>
        /// Buffer of characters that are to be reconsumed. This may grow as needed.
        /// </summary>
        private char[] CharactersToReconsume = new char[10];

        /// <summary>
        /// Current count of valid characters in <see cref="CharactersToReconsume"/>.
        /// </summary>
        private int CharactersToReconsumeCount = 0;

        /// <summary>
        /// Buffer for constructing temporary strings (used by misc states).
        /// </summary>
        private readonly StringBuilder TempBuffer = new StringBuilder(1024);

        private char ConsumeNextInputCharacter()
        {
            if (this.CharactersToReconsumeCount > 0)
            {
                // Take the first character
                this.CurrentInputCharacter = this.CharactersToReconsume[0];

                // Decrement the count in the list
                this.CharactersToReconsumeCount--;

                // And shift the remaining characters to the left (if anything left in the array).
                // We may need to optimize this algorithm if the performance penalty is too high.
                if (this.CharactersToReconsumeCount > 0)
                    Array.Copy(this.CharactersToReconsume, 1, this.CharactersToReconsume, 0, this.CharactersToReconsumeCount);

                return this.CurrentInputCharacter;
            }

            char ch = this.HtmlStream.ReadChar();
            this.CurrentInputCharacter = ch;
            return ch;
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

        private void ReconsumeInputCharacter()
        {
            this.ReconsumeInputCharacter(this.CurrentInputCharacter);
        }

        private void ReconsumeInputCharacter(char ch)
        {
            if (this.CharactersToReconsumeCount >= this.CharactersToReconsume.Length)
                Array.Resize(ref this.CharactersToReconsume, this.CharactersToReconsume.Length * 2);
            this.CharactersToReconsume[this.CharactersToReconsumeCount] = ch;
            this.CharactersToReconsumeCount++;
        }

        private void ReconsumeInputCharacters(string chars)
        {
            if (String.IsNullOrEmpty(chars))
                return;

            foreach (char ch in chars)
                this.ReconsumeInputCharacter(ch);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EmitCharacter(char ch)
        {
            this.CurrentToken.SetCharacter(ch);
        }

        private void EmitEndOfFile()
        {
            this.CurrentToken.SetEndOfFile();
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
            this.CurrentToken.SetDocType(
                this.DocTypeName?.ToString(),
                this.DocTypePublicIdentifier?.ToString(),
                this.DocTypeSystemIdentifier?.ToString(),
                this.DocTypeForceQuirks);
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
            this.CurrentToken.SetComment(this.CommentData.ToString);
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

                this.CurrentToken.SetEndTag(this.TagName.ToString(), this.TagIsSelfClosing, attributes);
            }
            else
            {
                this.LastStartTagName = this.TagName.ToString();
                bool isSelfClosing = this.TagIsSelfClosing;
                this.CurrentToken.SetStartTag(this.LastStartTagName, isSelfClosing, attributes);
            }
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
        /// Used when parsing attribute values
        /// </summary>
        private char AdditionalAllowedChar = Characters.EOF;

        /// <summary>
        /// The attribute value state that switched into the CharacterReferenceInAttributeValue state.
        /// </summary>
        private StateEnum BeforeCharacterReferenceInAttributeValueState;

        /// <summary>
        /// Indicates that there's currenty a new attribute.
        /// </summary>
        private bool HasCurrentAttribute;

        private void NewAttribute()
        {
            this.AddCurrentAttributeToList();
            this.HasCurrentAttribute = true;
            this.AdditionalAllowedChar = Characters.EOF;
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
            Consume the next input character:
                U+0026 AMPERSAND (&)
                    Switch to the character reference in data state.
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
                this.SwitchTo(StateEnum.CharacterReferenceInData);
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
        private void HandleStateCharacterReferenceInData()
        {
            /*
            8.2.4.2 Character reference in data state
            Switch to the data state.
            Attempt to consume a character reference, with no additional allowed character.
            If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
            Otherwise, emit the character tokens that were returned.
            */
            this.SwitchTo(StateEnum.Data);
            string ch = this.ConsumeCharacterReference(false);
            if (ch == null)
            {
                this.EmitCharacter('&');
            }
            else if (ch.Length == 1)
            {
                this.EmitCharacter(ch[0]);
            }
            else
            {
                for (int i = 0; i < ch.Length; i++)
                {
                    this.EmitCharacter(ch[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcData()
        {
            /*
            Consume the next input character:
                U+0026 AMPERSAND (&)
                    Switch to the character reference in RCDATA state.
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
                this.SwitchTo(StateEnum.CharacterReferenceInRcData);
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
        private void HandleStateCharacterReferenceInRcData()
        {
            /*
            8.2.4.4 Character reference in RCDATA state
            Switch to the RCDATA state.
            Attempt to consume a character reference, with no additional allowed character.
            If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
            Otherwise, emit the character tokens that were returned.
            */
            this.SwitchTo(StateEnum.RcData);
            string ch = this.ConsumeCharacterReference(false);
            if (ch == null)
            {
                this.EmitCharacter('&');
            }
            else if (ch.Length == 1)
            {
                this.EmitCharacter(ch[0]);
            }
            else
            {
                for (int i = 0; i < ch.Length; i++)
                {
                    this.EmitCharacter(ch[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawText()
        {
            /*
            8.2.4.5 RAWTEXT state
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
            8.2.4.6 Script data state
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
            8.2.4.7 PLAINTEXT state
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
            8.2.4.8 Tag open state
            Consume the next input character:
                "!" (U+0021)
                    Switch to the markup declaration open state.
                "/" (U+002F)
                    Switch to the end tag open state.
                Uppercase ASCII letter
                    Create a new start tag token, set its tag name to the lowercase version of the current input character
                    (add 0x0020 to the character's code point), then switch to the tag name state. (Don't emit the token
                    yet; further details will be filled in before it is emitted.)
                Lowercase ASCII letter
                    Create a new start tag token, set its tag name to the current input character, then switch to the tag
                    name state. (Don't emit the token yet; further details will be filled in before it is emitted.)
                "?" (U+003F)
                    Parse error. Switch to the bogus comment state.
                Anything else
                    Parse error. Switch to the data state. Emit a U+003C LESS-THAN SIGN character token. Reconsume the
                    current input character.
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
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.NewStartTag();
                this.TagName.Clear();
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.SwitchTo(StateEnum.TagName);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.NewStartTag();
                this.TagName.Clear();
                this.TagName.Append(ch);
                this.SwitchTo(StateEnum.TagName);
            }
            else if (ch == '?')
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.BogusComment);
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.Data);
                this.EmitCharacter('<');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateEndTagOpen()
        {
            /*
            8.2.4.9 End tag open state
            Consume the next input character:
                Uppercase ASCII letter
                    Create a new end tag token, set its tag name to the lowercase version of the current input character
                    (add 0x0020 to the character's code point), then switch to the tag name state. (Don't emit the token
                    yet; further details will be filled in before it is emitted.)
                Lowercase ASCII letter
                    Create a new end tag token, set its tag name to the current input character, then switch to the tag
                    name state. (Don't emit the token yet; further details will be filled in before it is emitted.)
                ">" (U+003E)
                    Parse error. Switch to the data state.
                EOF
                    Parse error. Switch to the data state. Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume the EOF character.
                Anything else
                    Parse error. Switch to the bogus comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsUppercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.SwitchTo(StateEnum.TagName);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch);
                this.SwitchTo(StateEnum.TagName);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.Data);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.Data);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeInputCharacter();
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
            8.2.4.10 Tag name state
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
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.ReconsumeInputCharacter();
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
            8.2.4.11 RCDATA less-than sign state
            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the RCDATA end tag open state.
                Anything else
                    Switch to the RCDATA state. Emit a U+003C LESS-THAN SIGN character token. Reconsume the current
                    input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.RcDataEndTagOpen);
            }
            else
            {
                this.SwitchTo(StateEnum.RcData);
                this.EmitCharacter('<');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcDataEndTagOpen()
        {
            /*
            8.2.4.12 RCDATA end tag open state
            Consume the next input character:
                Uppercase ASCII letter
                    Create a new end tag token, and set its tag name to the lowercase version of the current input
                    character (add 0x0020 to the character's code point). Append the current input character to the
                    temporary buffer. Finally, switch to the RCDATA end tag name state. (Don't emit the token yet;
                    further details will be filled in before it is emitted.)
                Lowercase ASCII letter
                    Create a new end tag token, and set its tag name to the current input character. Append the current
                    input character to the temporary buffer. Finally, switch to the RCDATA end tag name state. (Don't emit
                    the token yet; further details will be filled in before it is emitted.)
                Anything else
                    Switch to the RCDATA state. Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS character
                    token. Reconsume the current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsUppercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.RcDataEndTagName);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.RcDataEndTagName);
            }
            else
            {
                this.SwitchTo(StateEnum.RcData);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRcDataEndTagName()
        {
            /*
            8.2.4.13 RCDATA end tag name state
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
                    Switch to the RCDATA state. Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS character
                    token, and a character token for each of the characters in the temporary buffer (in the order they
                    were added to the buffer). Reconsume the current input character.
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
                this.SwitchTo(StateEnum.RcData);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawTextLessThanSign()
        {
            /*
            8.2.4.14 RAWTEXT less-than sign state
            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the RAWTEXT end tag open state.
                Anything else
                    Switch to the RAWTEXT state. Emit a U+003C LESS-THAN SIGN character token. Reconsume the
                    current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.RawTextEndTagOpen);
            }
            else
            {
                this.SwitchTo(StateEnum.RawText);
                this.EmitCharacter('<');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawTextEndTagOpen()
        {
            /*
            8.2.4.15 RAWTEXT end tag open state
            Consume the next input character:
                Uppercase ASCII letter
                    Create a new end tag token, and set its tag name to the lowercase version of the current input
                    character (add 0x0020 to the character's code point). Append the current input character to the
                    temporary buffer. Finally, switch to the RAWTEXT end tag name state. (Don't emit the token yet;
                    further details will be filled in before it is emitted.)
                Lowercase ASCII letter
                    Create a new end tag token, and set its tag name to the current input character. Append the current
                    input character to the temporary buffer. Finally, switch to the RAWTEXT end tag name state.
                    (Don't emit the token yet; further details will be filled in before it is emitted.)
                Anything else
                    Switch to the RAWTEXT state. Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume the current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsUppercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.RawTextEndTagName);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.RawTextEndTagName);
            }
            else
            {
                this.SwitchTo(StateEnum.RawText);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateRawTextEndTagName()
        {
            /*
            8.2.4.16 RAWTEXT end tag name state
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
                    Switch to the RAWTEXT state. Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS character
                    token, and a character token for each of the characters in the temporary buffer (in the order they
                    were added to the buffer). Reconsume the current input character.
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
                this.SwitchTo(StateEnum.RawText);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeInputCharacter();
            }
        }

        #region Scripts

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataLessThanSign()
        {
            /*
            8.2.4.17 Script data less-than sign state
            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the script data end tag open state.
                "!" (U+0021)
                    Switch to the script data escape start state. Emit a U+003C LESS-THAN SIGN character token and a
                    U+0021 EXCLAMATION MARK character token.
                Anything else
                    Switch to the script data state. Emit a U+003C LESS-THAN SIGN character token. Reconsume the current
                    input character.
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
                this.SwitchTo(StateEnum.ScriptData);
                this.EmitCharacter('<');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEndTagOpen()
        {
            /*
            8.2.4.18 Script data end tag open state
            Consume the next input character:
                Uppercase ASCII letter
                    Create a new end tag token, and set its tag name to the lowercase version of the current input
                    character (add 0x0020 to the character's code point). Append the current input character to the
                    temporary buffer. Finally, switch to the script data end tag name state. (Don't emit the token yet;
                    further details will be filled in before it is emitted.)
                Lowercase ASCII letter
                    Create a new end tag token, and set its tag name to the current input character. Append the current
                    input character to the temporary buffer. Finally, switch to the script data end tag name state.
                    (Don't emit the token yet; further details will be filled in before it is emitted.)
                Anything else
                    Switch to the script data state. Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
                    character token. Reconsume the current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsUppercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.ScriptDataEndTagName);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.ScriptDataEndTagName);
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptData);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEndTagName()
        {
            /*
            8.2.4.19 Script data end tag name state
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
                    Switch to the script data state. Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
                    character token, and a character token for each of the characters in the temporary buffer (in the
                    order they were added to the buffer). Reconsume the current input character.
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
                this.SwitchTo(StateEnum.ScriptData);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapeStart()
        {
            /*
            8.2.4.20 Script data escape start state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escape start dash state. Emit a U+002D HYPHEN-MINUS character token.
                Anything else
                    Switch to the script data state. Reconsume the current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapeStartDash);
                this.EmitCharacter('\u002D');
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptData);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapeStartDash()
        {
            /*
            8.2.4.21 Script data escape start dash state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escaped dash dash state. Emit a U+002D HYPHEN-MINUS character token.
                Anything else
                    Switch to the script data state. Reconsume the current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.ScriptDataEscapedDashDash);
                this.EmitCharacter('\u002D');
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptData);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscaped()
        {
            /*
            8.2.4.22 Script data escaped state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escaped dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data escaped less-than sign state.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Switch to the data state. Parse error. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.ScriptData);
                this.InformParseError(Parsing.ParseError.InvalidScript);
                this.ReconsumeInputCharacter();
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
            8.2.4.23 Script data escaped dash state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data escaped dash dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data escaped less-than sign state.
                U+0000 NULL
                    Parse error. Switch to the script data escaped state. Emit a U+FFFD REPLACEMENT CHARACTER
                    character token.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
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
            8.2.4.24 Script data escaped dash dash state
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
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.EmitCharacter(ch);
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
            8.2.4.25 Script data escaped less-than sign state
            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the script data escaped end tag open state.
                Uppercase ASCII letter
                    Set the temporary buffer to the empty string. Append the lowercase version of the current input
                    character (add 0x0020 to the character's code point) to the temporary buffer. Switch to the script
                    data double escape start state. Emit a U+003C LESS-THAN SIGN character token and the current input
                    character as a character token.
                Lowercase ASCII letter
                    Set the temporary buffer to the empty string. Append the current input character to the temporary
                    buffer. Switch to the script data double escape start state. Emit a U+003C LESS-THAN SIGN character
                    token and the current input character as a character token.
                Anything else
                    Switch to the script data escaped state. Emit a U+003C LESS-THAN SIGN character token. Reconsume the
                    current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '/')
            {
                this.TempBuffer.Clear();
                this.SwitchTo(StateEnum.ScriptDataEscapedEndTagOpen);
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.TempBuffer.Clear();
                this.TempBuffer.Append(ch.ToLowercaseAsciiLetter());
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapeStart);
                this.EmitCharacter('<');
                this.EmitCharacter(ch);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.TempBuffer.Clear();
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.ScriptDataDoubleEscapeStart);
                this.EmitCharacter('<');
                this.EmitCharacter(ch);
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter('<');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedEndTagOpen()
        {
            /*
            8.2.4.26 Script data escaped end tag open state
            Consume the next input character:
                Uppercase ASCII letter
                    Create a new end tag token, and set its tag name to the lowercase version of the current input
                    character (add 0x0020 to the character's code point). Append the current input character to the
                    temporary buffer. Finally, switch to the script data escaped end tag name state. (Don't emit the
                    token yet; further details will be filled in before it is emitted.)
                Lowercase ASCII letter
                    Create a new end tag token, and set its tag name to the current input character. Append the current
                    input character to the temporary buffer. Finally, switch to the script data escaped end tag name state.
                    (Don't emit the token yet; further details will be filled in before it is emitted.)
                Anything else
                    Switch to the script data escaped state. Emit a U+003C LESS-THAN SIGN character token and a
                    U+002F SOLIDUS character token. Reconsume the current input character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsUppercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch.ToLowercaseAsciiLetter());
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.ScriptDataEscapedEndTagName);
            }
            else if (ch.IsLowercaseAsciiLetter())
            {
                this.NewEndTag();
                this.TagName.Clear();
                this.TagName.Append(ch);
                this.TempBuffer.Append(ch);
                this.SwitchTo(StateEnum.ScriptDataEscapedEndTagName);
            }
            else
            {
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataEscapedEndTagName()
        {
            /*
            8.2.4.27 Script data escaped end tag name state
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
                    Switch to the script data escaped state. Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
                    character token, and a character token for each of the characters in the temporary buffer (in the order
                    they were added to the buffer). Reconsume the current input character.
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
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.EmitCharacter('<');
                this.EmitCharacter('\u002F');
                foreach (char c in this.TempBuffer.ToString())
                    this.EmitCharacter(c);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapeStart()
        {
            /*
            8.2.4.28 Script data double escape start state
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
                    Switch to the script data escaped state. Reconsume the current input character.
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
                this.SwitchTo(StateEnum.ScriptDataEscaped);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscaped()
        {
            /*
            8.2.4.29 Script data double escaped state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the script data double escaped dash state. Emit a U+002D HYPHEN-MINUS character token.
                "<" (U+003C)
                    Switch to the script data double escaped less-than sign state. Emit a U+003C LESS-THAN SIGN
                    character token.
                U+0000 NULL
                    Parse error. Emit a U+FFFD REPLACEMENT CHARACTER character token.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
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
            8.2.4.30 Script data double escaped dash state
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
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
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
            8.2.4.31 Script data double escaped dash dash state
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
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
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
            8.2.4.32 Script data double escaped less-than sign state
            Consume the next input character:
                "/" (U+002F)
                    Set the temporary buffer to the empty string. Switch to the script data double escape end state.
                    Emit a U+002F SOLIDUS character token.
                Anything else
                    Switch to the script data double escaped state. Reconsume the current input character.
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
                this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateScriptDataDoubleEscapeEnd()
        {
            /*
            8.2.4.33 Script data double escape end state
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
                    Switch to the script data double escaped state. Reconsume the current input character.
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
                this.SwitchTo(StateEnum.ScriptDataDoubleEscaped);
                this.ReconsumeInputCharacter();
            }
        }

        #endregion

        #region Attributes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeAttributeName()
        {
            /*
            8.2.4.34 Before attribute name state
            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                "/" (U+002F)
                    Switch to the self-closing start tag state.
                ">" (U+003E)
                    Switch to the data state. Emit the current tag token.
                Uppercase ASCII letter
                    Start a new attribute in the current tag token. Set that attribute's name to the lowercase version
                    of the current input character (add 0x0020 to the character's code point), and its value to the empty
                    string. Switch to the attribute name state.
                U+0000 NULL
                    Parse error. Start a new attribute in the current tag token. Set that attribute's name to a
                    U+FFFD REPLACEMENT CHARACTER character, and its value to the empty string. Switch to the attribute
                    name state.
                U+0022 QUOTATION MARK (")
                "'" (U+0027)
                "<" (U+003C)
                "=" (U+003D)
                    Parse error. Treat it as per the "anything else" entry below.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
                Anything else
                    Start a new attribute in the current tag token. Set that attribute's name to the current input
                    character, and its value to the empty string. Switch to the attribute name state.
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
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(ch.ToLowercaseAsciiLetter());
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(Characters.ReplacementCharacter);
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                if ((ch == '\u0022') || (ch == '\u0027') || (ch == '<') || (ch == '='))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(ch);
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAttributeName()
        {
            /*
            8.2.4.35 Attribute name state
            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the after attribute name state.
                "/" (U+002F)
                    Switch to the self-closing start tag state.
                "=" (U+003D)
                    Switch to the before attribute value state.
                ">" (U+003E)
                    Switch to the data state. Emit the current tag token.
                Uppercase ASCII letter
                    Append the lowercase version of the current input character (add 0x0020 to the character's code point)
                    to the current attribute's name.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's name.
                U+0022 QUOTATION MARK (")
                "'" (U+0027)
                "<" (U+003C)
                    Parse error. Treat it as per the "anything else" entry below.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.AfterAttributeName);
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
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.AttributeName.Append(ch.ToLowercaseAsciiLetter());
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeName.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                if ((ch == '\u0027') || (ch == '<'))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.AttributeName.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterAttributeName()
        {
            /*
            8.2.4.36 After attribute name state
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
                Uppercase ASCII letter
                    Start a new attribute in the current tag token. Set that attribute's name to the lowercase version of
                    the current input character (add 0x0020 to the character's code point), and its value to the empty
                    string. Switch to the attribute name state.
                U+0000 NULL
                    Parse error. Start a new attribute in the current tag token. Set that attribute's name to a
                    U+FFFD REPLACEMENT CHARACTER character, and its value to the empty string. Switch to the attribute
                    name state.
                U+0022 QUOTATION MARK (")
                "'" (U+0027)
                "<" (U+003C)
                    Parse error. Treat it as per the "anything else" entry below.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
                Anything else
                    Start a new attribute in the current tag token. Set that attribute's name to the current input
                    character, and its value to the empty string. Switch to the attribute name state.
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
            else if (ch.IsUppercaseAsciiLetter())
            {
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(ch.ToLowercaseAsciiLetter());
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(Characters.ReplacementCharacter);
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                if ((ch == '\u0022') || (ch == '\u0027') || (ch == '<'))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.NewAttribute();
                this.AttributeName.Clear();
                this.AttributeName.Append(ch);
                this.AttributeValue.Clear();
                this.SwitchTo(StateEnum.AttributeName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeAttributeValue()
        {
            /*
            8.2.4.37 Before attribute value state
            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                U+0022 QUOTATION MARK (")
                    Switch to the attribute value (double-quoted) state.
                U+0026 AMPERSAND (&)
                    Switch to the attribute value (unquoted) state. Reconsume the current input character.
                "'" (U+0027)
                    Switch to the attribute value (single-quoted) state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                    Switch to the attribute value (unquoted) state.
                ">" (U+003E)
                    Parse error. Switch to the data state. Emit the current tag token.
                "<" (U+003C)
                "=" (U+003D)
                "`" (U+0060)
                    Parse error. Treat it as per the "anything else" entry below.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
                Anything else
                    Append the current input character to the current attribute's value. Switch to the attribute value
                    (unquoted) state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                // Ignote
            }
            else if (ch == '\u0022')
            {
                this.SwitchTo(StateEnum.AttributeValueDoubleQuoted);
            }
            else if (ch == '&')
            {
                this.SwitchTo(StateEnum.AttributeValueUnquoted);
                this.ReconsumeInputCharacter();
            }
            else if (ch == '\u0027')
            {
                this.SwitchTo(StateEnum.AttributeValueSingleQuoted);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeValue.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.AttributeValueUnquoted);
            }
            else if (ch == '>')
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.EmitTag();
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                if ((ch == '<') || (ch == '=') || (ch == '\u0060'))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.AttributeValue.Append(ch);
                this.SwitchTo(StateEnum.AttributeValueUnquoted);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAttributeValueDoubleQuoted()
        {
            /*
            8.2.4.38 Attribute value (double-quoted) state
            Consume the next input character:
                U+0022 QUOTATION MARK (")
                    Switch to the after attribute value (quoted) state.
                U+0026 AMPERSAND (&)
                    Switch to the character reference in attribute value state, with the additional allowed character
                    being U+0022 QUOTATION MARK (").
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.CharacterReferenceInAttributeValue);
                this.AdditionalAllowedChar = '\u0022';
                this.BeforeCharacterReferenceInAttributeValueState = StateEnum.AttributeValueDoubleQuoted;
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeValue.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
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
            8.2.4.39 Attribute value (single-quoted) state
            Consume the next input character:
                "'" (U+0027)
                    Switch to the after attribute value (quoted) state.
                U+0026 AMPERSAND (&)
                    Switch to the character reference in attribute value state, with the additional allowed character
                    being "'" (U+0027).
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.CharacterReferenceInAttributeValue);
                this.AdditionalAllowedChar = '\u0027';
                this.BeforeCharacterReferenceInAttributeValueState = StateEnum.AttributeValueSingleQuoted;
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.AttributeValue.Append(Characters.ReplacementCharacter);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
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
            8.2.4.40 Attribute value (unquoted) state
            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before attribute name state.
                U+0026 AMPERSAND (&)
                    Switch to the character reference in attribute value state, with the additional allowed character
                    being ">" (U+003E).
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
                    Parse error. Switch to the data state. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.CharacterReferenceInAttributeValue);
                this.AdditionalAllowedChar = '>';
                this.BeforeCharacterReferenceInAttributeValueState = StateEnum.AttributeValueUnquoted;
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                if ((ch == '\u0022') || (ch == '\u0027') || (ch == '<') || (ch == '=') || (ch == '\u0060'))
                    this.InformParseError(Parsing.ParseError.InvalidAttribute);

                this.AttributeValue.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCharacterReferenceInAttributeValue()
        {
            /*
            8.2.4.41 Character reference in attribute value state
            Attempt to consume a character reference.
            If nothing is returned, append a U+0026 AMPERSAND character (&) to the current attribute's value.
            Otherwise, append the returned character tokens to the current attribute's value.
            Finally, switch back to the attribute value state that switched into this state.
            */
            string ch = this.ConsumeCharacterReference(this.AdditionalAllowedChar, true);
            if (ch == null)
                this.AttributeValue.Append('&');
            else
                this.AttributeValue.Append(ch);

            this.SwitchTo(this.BeforeCharacterReferenceInAttributeValueState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateAfterAttributeValueQuoted()
        {
            /*
            8.2.4.42 After attribute value (quoted) state
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
                    Parse error. Switch to the data state. Reconsume the EOF character.
                Anything else
                    Parse error. Switch to the before attribute name state. Reconsume the character.
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidAttribute);
                this.SwitchTo(StateEnum.BeforeAttributeName);
                this.ReconsumeInputCharacter();
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateSelfClosingStartTag()
        {
            /*
            8.2.4.43 Self-closing start tag state
            Consume the next input character:
                ">" (U+003E)
                    Set the self-closing flag of the current tag token. Switch to the data state.
                    Emit the current tag token.
                EOF
                    Parse error. Switch to the data state. Reconsume the EOF character.
                Anything else
                    Parse error. Switch to the before attribute name state. Reconsume the character.
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
                this.SwitchTo(StateEnum.Data);
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidTag);
                this.SwitchTo(StateEnum.BeforeAttributeName);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBogusComment()
        {
            /*
            8.2.4.44 Bogus comment state
            Consume every character up to and including the first ">" (U+003E) character or the end of the file (EOF),
            whichever comes first. Emit a comment token whose data is the concatenation of all the characters starting
            from and including the character that caused the state machine to switch into the bogus comment state, up to
            and including the character immediately before the last consumed character (i.e. up to the character just
            before the U+003E or EOF character), but with any U+0000 NULL characters replaced by U+FFFD REPLACEMENT
            CHARACTER characters. (If the comment was started by the end of the file (EOF), the token is empty. Similarly,
            the token is empty if it was generated by the string "<!>".)

            Switch to the data state.

            If the end of the file was reached, reconsume the EOF character.
            */
            while (true)
            {
                char ch = this.ConsumeNextInputCharacter();
                if ((ch == '>') || (ch == Characters.EOF))
                {
                    this.EmitComment();
                    return;
                }

                if (ch == Characters.Null)
                    this.CommentData.Append(Characters.ReplacementCharacter);
                else
                    this.CommentData.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateMarkupDeclarationOpen()
        {
            /*
            8.2.4.45 Markup declaration open state
            If the next two characters are both "-" (U+002D) characters, consume those two characters, create a comment
            token whose data is the empty string, and switch to the comment start state.

            Otherwise, if the next seven characters are an ASCII case-insensitive match for the word "DOCTYPE", then
            consume those characters and switch to the DOCTYPE state.

            Otherwise, if there is an adjusted current node and it is not an element in the HTML namespace and the next
            seven characters are a case-sensitive match for the string "[CDATA[" (the five uppercase letters "CDATA" with
            a U+005B LEFT SQUARE BRACKET character before and after), then consume those characters and switch to the
            CDATA section state.
            Otherwise, this is a parse error. Switch to the bogus comment state. The next character that is consumed,
            if any, is the first character that will be in the comment.
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
                this.SwitchTo(StateEnum.BogusComment);
                this.NewComment();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentStart()
        {
            /*
            8.2.4.46 Comment start state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment start dash state.
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                    Switch to the comment state.
                ">" (U+003E)
                    Parse error. Switch to the data state. Emit the comment token.
                EOF
                    Parse error. Switch to the data state. Emit the comment token. Reconsume the EOF character.
                Anything else
                    Append the current input character to the comment token's data. Switch to the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentStartDash);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.Comment);
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
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentStartDash()
        {
            /*
            8.2.4.47 Comment start dash state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment end state
                U+0000 NULL
                    Parse error. Append a "-" (U+002D) character and a U+FFFD REPLACEMENT CHARACTER character to
                    the comment token's data. Switch to the comment state.
                ">" (U+003E)
                    Parse error. Switch to the data state. Emit the comment token.
                EOF
                    Parse error. Switch to the data state. Emit the comment token. Reconsume the EOF character.
                Anything else
                    Append a "-" (U+002D) character and the current input character to the comment token's data.
                    Switch to the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentEnd);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append('-');
                this.CommentData.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.Comment);
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
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.CommentData.Append('-');
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateComment()
        {
            /*
            8.2.4.48 Comment state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment end dash state
                U+0000 NULL
                    Parse error. Append a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                EOF
                    Parse error. Switch to the data state. Emit the comment token. Reconsume the EOF character.
                Anything else
                    Append the current input character to the comment token's data.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
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
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.CommentData.Append(ch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentEndDash()
        {
            /*
            8.2.4.49 Comment end dash state
            Consume the next input character:
                "-" (U+002D)
                    Switch to the comment end state
                U+0000 NULL
                    Parse error. Append a "-" (U+002D) character and a U+FFFD REPLACEMENT CHARACTER character to the
                    comment token's data. Switch to the comment state.
                EOF
                    Parse error. Switch to the data state. Emit the comment token. Reconsume the EOF character.
                Anything else
                    Append a "-" (U+002D) character and the current input character to the comment token's data.
                    Switch to the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.SwitchTo(StateEnum.CommentEnd);
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append('-');
                this.CommentData.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.Comment);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.CommentData.Append('-');
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentEnd()
        {
            /*
            8.2.4.50 Comment end state
            Consume the next input character:
                ">" (U+003E)
                    Switch to the data state. Emit the comment token.
                U+0000 NULL
                    Parse error. Append two "-" (U+002D) characters and a U+FFFD REPLACEMENT CHARACTER character to
                    the comment token's data. Switch to the comment state.
                "!" (U+0021)
                    Parse error. Switch to the comment end bang state.
                "-" (U+002D)
                    Parse error. Append a "-" (U+002D) character to the comment token's data.
                EOF
                    Parse error. Switch to the data state. Emit the comment token. Reconsume the EOF character.
                Anything else
                    Parse error. Append two "-" (U+002D) characters and the current input character to the comment
                    token's data. Switch to the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append('-');
                this.CommentData.Append('-');
                this.CommentData.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.Comment);
            }
            else if (ch == '!')
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.CommentEndBang);
            }
            else if (ch == '-')
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append('-');
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append('-');
                this.CommentData.Append('-');
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.Comment);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCommentEndBang()
        {
            /*
            8.2.4.51 Comment end bang state
            Consume the next input character:
                "-" (U+002D)
                    Append two "-" (U+002D) characters and a "!" (U+0021) character to the comment token's data. Switch
                    to the comment end dash state.
                ">" (U+003E)
                    Switch to the data state. Emit the comment token.
                U+0000 NULL
                    Parse error. Append two "-" (U+002D) characters, a "!" (U+0021) character, and a U+FFFD REPLACEMENT
                    CHARACTER character to the comment token's data. Switch to the comment state.
                EOF
                    Parse error. Switch to the data state. Emit the comment token. Reconsume the EOF character.
                Anything else
                    Append two "-" (U+002D) characters, a "!" (U+0021) character, and the current input character to the
                    comment token's data. Switch to the comment state.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch == '-')
            {
                this.CommentData.Append("--!");
                this.SwitchTo(StateEnum.CommentEndDash);
            }
            else if (ch == '>')
            {
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
            }
            else if (ch == Characters.Null)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.CommentData.Append("--!");
                this.CommentData.Append(Characters.ReplacementCharacter);
                this.SwitchTo(StateEnum.Comment);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidComment);
                this.SwitchTo(StateEnum.Data);
                this.EmitComment();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.CommentData.Append("--!");
                this.CommentData.Append(ch);
                this.SwitchTo(StateEnum.Comment);
            }
        }

        #region DocTypes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateDocType()
        {
            /*
            8.2.4.52 DOCTYPE state
            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Switch to the before DOCTYPE name state.
                EOF
                    Parse error. Switch to the data state. Create a new DOCTYPE token. Set its force-quirks flag to on.
                    Emit the token. Reconsume the EOF character.
                Anything else
                    Parse error. Switch to the before DOCTYPE name state. Reconsume the character.
            */
            char ch = this.ConsumeNextInputCharacter();
            if (ch.IsSpaceCharacter())
            {
                this.SwitchTo(StateEnum.BeforeDocTypeName);
            }
            else if (ch == Characters.EOF)
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.SwitchTo(StateEnum.Data);
                this.NewDocType();
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
            }
            else
            {
                this.InformParseError(Parsing.ParseError.InvalidDocType);
                this.SwitchTo(StateEnum.BeforeDocTypeName);
                this.ReconsumeInputCharacter();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateBeforeDocTypeName()
        {
            /*
            8.2.4.53 Before DOCTYPE name state
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
                    Parse error. Switch to the data state. Create a new DOCTYPE token. Set its force-quirks flag to on.
                    Emit the token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.NewDocType();
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.54 DOCTYPE name state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.55 After DOCTYPE name state
            Consume the next input character:
                "tab" (U+0009)
                "LF" (U+000A)
                "FF" (U+000C)
                U+0020 SPACE
                    Ignore the character.
                ">" (U+003E)
                    Switch to the data state. Emit the current DOCTYPE token.
                EOF
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.56 After DOCTYPE public keyword state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.57 Before DOCTYPE public identifier state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.58 DOCTYPE public identifier (double-quoted) state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.59 DOCTYPE public identifier (single-quoted) state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.60 After DOCTYPE public identifier state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.61 Between DOCTYPE public and system identifiers state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.62 After DOCTYPE system keyword state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.63 Before DOCTYPE system identifier state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.64 DOCTYPE system identifier (double-quoted) state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.65 DOCTYPE system identifier (single-quoted) state
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
                    Parse error. Switch to the data state. Set the DOCTYPE token's force-quirks flag to on. Emit that
                    DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.66 After DOCTYPE system identifier state
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
                    Emit that DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.DocTypeForceQuirks = true;
                this.EmitDocType();
                this.ReconsumeInputCharacter();
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
            8.2.4.67 Bogus DOCTYPE state
            Consume the next input character:
                ">" (U+003E)
                    Switch to the data state. Emit the DOCTYPE token.
                EOF
                    Switch to the data state. Emit the DOCTYPE token. Reconsume the EOF character.
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
                this.SwitchTo(StateEnum.Data);
                this.EmitDocType();
                this.ReconsumeInputCharacter();
            }
            else
            {
                // Ignore
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleStateCDataSection()
        {
            /*
            8.2.4.68 CDATA section state
            Switch to the data state.

            Consume every character up to the next occurrence of the three character sequence U+005D RIGHT SQUARE BRACKET
            U+005D RIGHT SQUARE BRACKET U+003E GREATER-THAN SIGN (]]>), or the end of the file (EOF), whichever comes
            first. Emit a series of character tokens consisting of all the characters consumed except the matching three
            character sequence at the end (if one was found before the end of the file).

            If the end of the file was reached, reconsume the EOF character.
            */
            this.SwitchTo(StateEnum.Data);

            while (true)
            {
                char ch = this.ConsumeNextInputCharacter();

                if (ch == ']')
                {
                    ch = this.ConsumeNextInputCharacter();
                    if (ch == ']')
                    {
                        ch = this.ConsumeNextInputCharacter();
                        if (ch == '>')
                            return;

                        this.EmitCharacter(']');
                    }

                    this.EmitCharacter(']');
                }

                if (ch == Characters.EOF)
                {
                    this.ReconsumeInputCharacter();
                    return;
                }

                this.EmitCharacter(ch);
            }
        }

        #region Character References

        private string ConsumeCharacterReference(bool partOfAnAttribute)
        {
            // EOF is already in the list of special characters, therefore using EOF has no effect.
            return this.ConsumeCharacterReference(Characters.EOF, partOfAnAttribute);
        }

        private string ConsumeCharacterReference(char additionalCharacter, bool partOfAnAttribute)
        {
            // The behavior depends on the identity of the next character (the one immediately after the U+0026 AMPERSAND character):
            char ch = this.ConsumeNextInputCharacter();

            // "tab" (U+0009)
            // "LF" (U+000A)
            // "FF" (U+000C)
            // U+0020 SPACE
            // U+003C LESS-THAN SIGN
            // U+0026 AMPERSAND
            // EOF
            // The additional allowed character, if there is one
            if (ch.IsSpaceCharacter() || (ch == '<') || (ch == '&') || (ch == Characters.EOF) || (ch == additionalCharacter))
            {
                // Not a character reference. No characters are consumed, and nothing is returned. (This is not an error, either.)
                this.ReconsumeInputCharacter();
                return null;
            }

            StringBuilder consumedCharacters = new StringBuilder();
            if (ch == '#')
            {
                // Consume the U+0023 NUMBER SIGN.
                consumedCharacters.Append(ch);

                // The behavior further depends on the character after the U+0023 NUMBER SIGN:
                ch = this.ConsumeNextInputCharacter();

                bool isHex;

                // U+0078 LATIN SMALL LETTER X
                // U+0058 LATIN CAPITAL LETTER X
                if ((ch == 'x') || (ch == 'X'))
                {
                    // Consume the X.
                    consumedCharacters.Append(ch);

                    // Follow the steps below, but using the range of characters ASCII digits, U +0061 LATIN SMALL LETTER A
                    // to U+0066 LATIN SMALL LETTER F, and U+0041 LATIN CAPITAL LETTER A to U+0046 LATIN CAPITAL LETTER F
                    // (in other words, 0-9, A-F, a-f).

                    // When it comes to interpreting the number, interpret it as a hexadecimal number.
                    isHex = true;
                }
                else
                {
                    // Anything else

                    // Follow the steps below, but using the range of characters ASCII digits.
                    // When it comes to interpreting the number, interpret it as a decimal number.
                    isHex = false;
                }

                // Consume as many characters as match the range of characters given above (ASCII hex digits or ASCII digits).
                StringBuilder number = new StringBuilder();
                ch = this.ConsumeNextInputCharacter();
                while ((isHex && ch.IsAsciiHexDigit()) || (!isHex && ch.IsAsciiDigit()))
                {
                    consumedCharacters.Append(ch);
                    number.Append(ch);

                    ch = this.ConsumeNextInputCharacter();
                }

                // If no characters match the range, then don't consume any characters
                // (and unconsume the U+0023 NUMBER SIGN character and, if appropriate, the X character).
                // This is a parse error; nothing is returned.
                if (number.Length == 0)
                {
                    consumedCharacters.Append(ch); // Reconsume last char.
                    this.ReconsumeInputCharacters(consumedCharacters.ToString());
                    this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                    return null;
                }

                // Otherwise, if the next character is a U + 003B SEMICOLON, consume that too.
                if (ch == ';')
                {
                    consumedCharacters.Append(ch);
                }
                else
                {
                    // If it isn't, there is a parse error.
                    this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                }

                // If one or more characters match the range, then take them all and interpret the
                // string of characters as a number(either hexadecimal or decimal as appropriate).
                int codePoint = Tokenizer.ParseNumber(number.ToString(), isHex);

                // If that number is one of the numbers in the first column of the following table,
                // then this is a parse error.
                char replacementChar = '\u0000';
                if (codePoint < Tokenizer.CharacterReferenceReplacetCharacters.Length)
                    replacementChar = Tokenizer.CharacterReferenceReplacetCharacters[codePoint];
                if (replacementChar != '\u0000')
                {
                    // The code-point is defined in the table.
                    this.InformParseError(Parsing.ParseError.InvalidCharacterReference);

                    // Find the row with that number in the first column, and return a
                    // character token for the Unicode character given in the second column of that row.
                    return replacementChar.ToString();
                }
                else if (((0xD800 <= codePoint) && (codePoint <= 0xDFFF)) || (codePoint > 0x10FFFF))
                {
                    // Otherwise, if the number is in the range 0xD800 to 0xDFFF or is greater than 0x10FFFF,
                    // then this is a parse error. Return a U + FFFD REPLACEMENT CHARACTER character token.
                    this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                    return Characters.ReplacementCharacter.ToString();
                }
                else
                {
                    // Otherwise, return a character token for the Unicode character whose code point is that number.

                    // Parameters must be on same line or separate lines
                    // Additionally, if the number is in the range 0x0001 to 0x0008, 0x000D to 0x001F, 0x007F to 0x009F,
                    // 0xFDD0 to 0xFDEF, or is one of 0x000B, 0xFFFE, 0xFFFF, 0x1FFFE, 0x1FFFF, 0x2FFFE, 0x2FFFF, 0x3FFFE,
                    // 0x3FFFF, 0x4FFFE, 0x4FFFF, 0x5FFFE, 0x5FFFF, 0x6FFFE, 0x6FFFF, 0x7FFFE, 0x7FFFF, 0x8FFFE, 0x8FFFF,
                    // 0x9FFFE, 0x9FFFF, 0xAFFFE, 0xAFFFF, 0xBFFFE, 0xBFFFF, 0xCFFFE, 0xCFFFF, 0xDFFFE, 0xDFFFF, 0xEFFFE,
                    // 0xEFFFF, 0xFFFFE, 0xFFFFF, 0x10FFFE, or 0x10FFFF, then this is a parse error.
                    if (
#pragma warning disable SA1117
                        codePoint.IsInRange(0x0001, 0x0008) ||
                        codePoint.IsInRange(0x000D, 0x001F) ||
                        codePoint.IsInRange(0x007F, 0x009F) ||
                        codePoint.IsInRange(0xFDD0, 0xFDEF) ||
                        codePoint.IsOneOf(
                            0x000B, 0xFFFE, 0xFFFF, 0x1FFFE, 0x1FFFF, 0x2FFFE, 0x2FFFF, 0x3FFFE,
                            0x3FFFF, 0x4FFFE, 0x4FFFF, 0x5FFFE, 0x5FFFF, 0x6FFFE, 0x6FFFF, 0x7FFFE, 0x7FFFF, 0x8FFFE, 0x8FFFF,
                            0x9FFFE, 0x9FFFF, 0xAFFFE, 0xAFFFF, 0xBFFFE, 0xBFFFF, 0xCFFFE, 0xCFFFF, 0xDFFFE, 0xDFFFF, 0xEFFFE,
                            0xEFFFF, 0xFFFFE, 0xFFFFF, 0x10FFFE, 0x10FFFF))
#pragma warning restore SA1117 // Parameters must be on same line or separate lines
                    {
                        this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                    }

                    return Char.ConvertFromUtf32(codePoint);
                }
            }
            else
            {
                // Anything else (not a #)

                // Consume the maximum number of characters possible, with the consumed characters
                // matching one of the identifiers in the first column of the named character
                // references table (in a case-sensitive manner).
                StringBuilder characterName = new StringBuilder();
                for (int i = 0; i < NamedCharacters.MaxNameLength; i++)
                {
                    // In reality, the above means: Read characters until one of the above is true:
                    // * We encounter a ";" (semicolon). No names contain semicolon, and we can perform the lookup immedeately.
                    // * We reach the "NamedCharacters.MaxNameLength", in which case, we'll need to brute-force lookup the name.
                    // * We reach EOF ... we have to stop.
                    if (i != 0)
                        ch = this.ConsumeNextInputCharacter();
                    consumedCharacters.Append(ch);
                    if (ch == Characters.EOF)
                        break;
                    characterName.Append(ch);
                    if (ch == ';')
                        break; // No need for further lookup
                }

                string actualName;
                string namedCharacter = NamedCharacters.TryGetNamedCharacter(characterName.ToString(), out actualName);

                if (namedCharacter == null)
                {
                    // If no match can be made, then no characters are consumed, and nothing is returned.
                    this.ReconsumeInputCharacters(consumedCharacters.ToString());

                    // In this case, if the characters after the U+0026 AMPERSAND character (&) consist of a
                    // sequence of one or more alphanumeric ASCII characters followed by a U + 003B SEMICOLON character(;),
                    // then this is a parse error.
                    if ((characterName.Length >= 2) && characterName.ToString().Substring(0, characterName.Length - 1).All(c => c.IsAsciiHexDigit()) && (characterName.ToString()[characterName.Length - 1] == ';'))
                        this.InformParseError(Parsing.ParseError.InvalidCharacterReference);

                    return null;
                }
                else
                {
                    // If the character reference is being consumed as part of an attribute, and the last character matched
                    // is not a ";" (U + 003B) character, and the next character is either a "=" (U + 003D) character or an
                    // alphanumeric ASCII character, then, for historical reasons ...
                    if (partOfAnAttribute && (actualName[actualName.Length - 1] != ';'))
                    {
                        // We need to figure out what the next char is.
                        if (ch != Characters.EOF)
                        {
                            // If the last read char was EOF, no need to do this.
                            ch = this.ConsumeNextInputCharacter();
                            consumedCharacters.Append(ch);
                        }

                        if ((ch == '=') || ch.IsAlphaNumericAsciiCharacter())
                        {
                            // ... all the characters that were matched after
                            // the U + 0026 AMPERSAND character(&) must be unconsumed, and nothing is returned.
                            // However, if this next character is in fact a "=" (U + 003D) character, then this is a parse error,
                            // because some legacy user agents will misinterpret the markup in those cases.
                            if (ch == '=')
                                this.InformParseError(Parsing.ParseError.InvalidCharacterReference);
                            this.ReconsumeInputCharacters(consumedCharacters.ToString());
                        }
                    }

                    // Otherwise, a character reference is parsed. If the last character matched is not a ";"(U + 003B) character,
                    // there is a parse error.
                    if (actualName[actualName.Length - 1] != ';')
                        this.InformParseError(Parsing.ParseError.InvalidCharacterReference);

                    // Return one or two character tokens for the character(s) corresponding to the character reference name
                    // (as given by the second column of the named character references table).

                    // Return whatever characters we did not need.
                    consumedCharacters.Remove(0, actualName.Length);
                    this.ReconsumeInputCharacters(consumedCharacters.ToString());
                    return namedCharacter;
                }
            }
        }

        private static readonly char[] CharacterReferenceReplacetCharacters = Tokenizer.GetCharacterReferenceReplacetCharacters();

        private static char[] GetCharacterReferenceReplacetCharacters()
        {
            char[] chars = new char[256];

            // List found in 8.2.4.69 Tokenizing character references.
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

        private static int ParseNumber(string digits, bool isHex)
        {
            if ((digits == null) || (digits.Length == 0))
                throw new ArgumentOutOfRangeException(nameof(digits));
            if (isHex && (digits.Length > 7))
                throw new ArgumentOutOfRangeException(nameof(digits));
            if (!isHex && (digits.Length > 9))
                throw new ArgumentOutOfRangeException(nameof(digits));

            if (isHex)
            {
                int number = 0;
                foreach (char digit in digits)
                {
                    int value;
                    if (('0' <= digit) && (digit <= '9'))
                        value = digit - '0';
                    else if (('A' <= digit) && (digit <= 'F'))
                        value = (digit - 'A') + 10;
                    else if (('a' <= digit) && (digit <= 'f'))
                        value = (digit - 'a') + 10;
                    else
                        throw new ArgumentOutOfRangeException(nameof(digits)); // Garbage digit

                    number = (number * 16) + value;
                }

                return number;
            }
            else
            {
                int number = 0;
                foreach (char digit in digits)
                {
                    int value;
                    if (('0' <= digit) && (digit <= '9'))
                        value = digit - '0';
                    else
                        throw new ArgumentOutOfRangeException(nameof(digits)); // Garbage digit

                    number = (number * 10) + value;
                }

                return number;
            }
        }

        #endregion

        #endregion
    }
}
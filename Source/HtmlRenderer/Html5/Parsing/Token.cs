using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    /// <summary>
    /// Contains information about a token from the <see cref="Tokenizer"/>.
    /// </summary>
    internal struct Token
    {
        #region Fields - keep this compact, so it can fit in a wide (16 byte) register.

        private char _Character;

        private TokenType _Type;

        private string Name;

        private Attribute[] _TagAttributes;

        // This could contain anything and is dependent on the token type.
        private object Data;

        private static readonly object TagIsSelfClosingMarker = new object();

        #endregion

        #region Token Properties

        /// <summary>
        /// Indicates the type of token.
        /// </summary>
        public TokenType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return this._Type; }
        }

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.Character"/>,
        /// this contains the character value of the token.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.Character"/>.
        /// </exception>
        public char Character
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if (this._Type != TokenType.Character)
                    throw new InvalidOperationException();
#endif
                return this._Character;
            }
        }

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.StartTag"/> or <see cref="TokenType.EndTag"/>,
        /// this contains the name of the tag. It is always in lower case according to HTML5 spec.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.StartTag"/>
        /// or <see cref="TokenType.EndTag"/>.
        /// </exception>
        public string TagName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if ((this._Type != TokenType.StartTag) && (this._Type != TokenType.EndTag))
                    throw new InvalidOperationException();
#endif
                return this.Name;
            }
        }

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.StartTag"/> or <see cref="TokenType.EndTag"/>,
        /// this contains information if the tag is self-closing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.StartTag"/>
        /// or <see cref="TokenType.EndTag"/>.
        /// </exception>
        public bool TagIsSelfClosing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if ((this._Type != TokenType.StartTag) && (this._Type != TokenType.EndTag))
                    throw new InvalidOperationException();
#endif
                return this.Data == Token.TagIsSelfClosingMarker;
            }
        }

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.StartTag"/> or <see cref="TokenType.EndTag"/>,
        /// this contains the attributes of the tag. This will never be null.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.StartTag"/>
        /// or <see cref="TokenType.EndTag"/>.
        /// </exception>
        public Attribute[] TagAttributes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if ((this._Type != TokenType.StartTag) && (this._Type != TokenType.EndTag))
                    throw new InvalidOperationException();
#endif
                return this._TagAttributes;
            }
        }

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.DocType"/>,
        /// this contains the name of the document type declaration.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.DocType"/>.
        /// </exception>
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

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.DocType"/>,
        /// this contains the public identifier of the document type declaration.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.DocType"/>.
        /// </exception>
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

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.DocType"/>,
        /// this contains the system identifier of the document type declaration.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.DocType"/>.
        /// </exception>
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

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.DocType"/>,
        /// this contains the "force quirks" flag as set by the <see cref="Tokenizer"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.DocType"/>.
        /// </exception>
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

        /// <summary>
        /// If <see cref="Type"/> is <see cref="TokenType.Comment"/>,
        /// this contains the text data of the comment.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Only in debug builds - if <see cref="Type"/> is not <see cref="TokenType.Character"/>.
        /// </exception>
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

        #endregion

        #region Internal helpers for setting the token's contents.

        /// <summary>
        /// Resets this token instance to an uninitialized state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResetToken()
        {
            this._Type = TokenType.Unknown;
            this.Data = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetCharacter(char ch)
        {
            this._Type = TokenType.Character;
            this._Character = ch;
        }

        internal void SetEndOfFile()
        {
            this._Type = TokenType.EndOfFile;
        }

        internal void SetDocType(string name, string publicIdentifier, string systemIdentifier, bool forceQuirks)
        {
            this._Type = TokenType.DocType;
            this.Name = name;
            this.Data = new Tuple<string, string, bool>(publicIdentifier, systemIdentifier, forceQuirks);
        }

        internal void SetComment(Func<string> data)
        {
            this._Type = TokenType.Comment;
            this.Data = data;
        }

        internal void SetStartTag(string tagName, bool isSelfClosing, Attribute[] attributes)
        {
            this._Type = TokenType.StartTag;
            this.Name = tagName;
            this.Data = isSelfClosing ? Token.TagIsSelfClosingMarker : null;
            this._TagAttributes = attributes ?? Attribute.None;
        }

        internal void SetEndTag(string tagName, bool isSelfClosing, Attribute[] attributes)
        {
            this._Type = TokenType.EndTag;
            this.Name = tagName;
            this.Data = isSelfClosing ? Token.TagIsSelfClosingMarker : null;
            this._TagAttributes = attributes ?? Attribute.None;
        }

        #endregion

        #region Helper Methods ... for often used tests.

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

        /// <summary>
        /// A character token that is U+0000 NULL
        /// </summary>
        /// <returns></returns>
        public bool IsCharacterNull()
        {
            return (this._Type == TokenType.Character) && (this._Character == '\u0000');
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

        #endregion

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
}

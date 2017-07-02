// FROM: base64js.min.js
(function (r) { if (typeof exports === "object" && typeof module !== "undefined") { module.exports = r() } else if (typeof define === "function" && define.amd) { define([], r) } else { var e; if (typeof window !== "undefined") { e = window } else if (typeof global !== "undefined") { e = global } else if (typeof self !== "undefined") { e = self } else { e = this } e.base64js = r() } })(function () { var r, e, t; return function r(e, t, n) { function o(i, a) { if (!t[i]) { if (!e[i]) { var u = typeof require == "function" && require; if (!a && u) return u(i, !0); if (f) return f(i, !0); var d = new Error("Cannot find module '" + i + "'"); throw d.code = "MODULE_NOT_FOUND", d } var c = t[i] = { exports: {} }; e[i][0].call(c.exports, function (r) { var t = e[i][1][r]; return o(t ? t : r) }, c, c.exports, r, e, t, n) } return t[i].exports } var f = typeof require == "function" && require; for (var i = 0; i < n.length; i++) o(n[i]); return o }({ "/": [function (r, e, t) { "use strict"; t.byteLength = c; t.toByteArray = v; t.fromByteArray = s; var n = []; var o = []; var f = typeof Uint8Array !== "undefined" ? Uint8Array : Array; var i = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"; for (var a = 0, u = i.length; a < u; ++a) { n[a] = i[a]; o[i.charCodeAt(a)] = a } o["-".charCodeAt(0)] = 62; o["_".charCodeAt(0)] = 63; function d(r) { var e = r.length; if (e % 4 > 0) { throw new Error("Invalid string. Length must be a multiple of 4") } return r[e - 2] === "=" ? 2 : r[e - 1] === "=" ? 1 : 0 } function c(r) { return r.length * 3 / 4 - d(r) } function v(r) { var e, t, n, i, a, u; var c = r.length; a = d(r); u = new f(c * 3 / 4 - a); n = a > 0 ? c - 4 : c; var v = 0; for (e = 0, t = 0; e < n; e += 4, t += 3) { i = o[r.charCodeAt(e)] << 18 | o[r.charCodeAt(e + 1)] << 12 | o[r.charCodeAt(e + 2)] << 6 | o[r.charCodeAt(e + 3)]; u[v++] = i >> 16 & 255; u[v++] = i >> 8 & 255; u[v++] = i & 255 } if (a === 2) { i = o[r.charCodeAt(e)] << 2 | o[r.charCodeAt(e + 1)] >> 4; u[v++] = i & 255 } else if (a === 1) { i = o[r.charCodeAt(e)] << 10 | o[r.charCodeAt(e + 1)] << 4 | o[r.charCodeAt(e + 2)] >> 2; u[v++] = i >> 8 & 255; u[v++] = i & 255 } return u } function l(r) { return n[r >> 18 & 63] + n[r >> 12 & 63] + n[r >> 6 & 63] + n[r & 63] } function h(r, e, t) { var n; var o = []; for (var f = e; f < t; f += 3) { n = (r[f] << 16) + (r[f + 1] << 8) + r[f + 2]; o.push(l(n)) } return o.join("") } function s(r) { var e; var t = r.length; var o = t % 3; var f = ""; var i = []; var a = 16383; for (var u = 0, d = t - o; u < d; u += a) { i.push(h(r, u, u + a > d ? d : u + a)) } if (o === 1) { e = r[t - 1]; f += n[e >> 2]; f += n[e << 4 & 63]; f += "==" } else if (o === 2) { e = (r[t - 2] << 8) + r[t - 1]; f += n[e >> 10]; f += n[e >> 4 & 63]; f += n[e << 2 & 63]; f += "=" } i.push(f); return i.join("") } }, {}] }, {}, [])("/") });

// FROM TextEncode.js
function TextEncoderLite() {
}
function TextDecoderLite() {
}

(function () {
    'use strict';

    // Taken from https://github.com/feross/buffer/blob/master/index.js
    // Thanks Feross et al! :-)

    function utf8ToBytes(string, units) {
        units = units || Infinity
        var codePoint
        var length = string.length
        var leadSurrogate = null
        var bytes = []
        var i = 0

        for (; i < length; i++) {
            codePoint = string.charCodeAt(i)

            // is surrogate component
            if (codePoint > 0xD7FF && codePoint < 0xE000) {
                // last char was a lead
                if (leadSurrogate) {
                    // 2 leads in a row
                    if (codePoint < 0xDC00) {
                        if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD)
                        leadSurrogate = codePoint
                        continue
                    } else {
                        // valid surrogate pair
                        codePoint = leadSurrogate - 0xD800 << 10 | codePoint - 0xDC00 | 0x10000
                        leadSurrogate = null
                    }
                } else {
                    // no lead yet

                    if (codePoint > 0xDBFF) {
                        // unexpected trail
                        if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD)
                        continue
                    } else if (i + 1 === length) {
                        // unpaired lead
                        if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD)
                        continue
                    } else {
                        // valid lead
                        leadSurrogate = codePoint
                        continue
                    }
                }
            } else if (leadSurrogate) {
                // valid bmp char, but last char was a lead
                if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD)
                leadSurrogate = null
            }

            // encode utf8
            if (codePoint < 0x80) {
                if ((units -= 1) < 0) break
                bytes.push(codePoint)
            } else if (codePoint < 0x800) {
                if ((units -= 2) < 0) break
                bytes.push(
                  codePoint >> 0x6 | 0xC0,
                  codePoint & 0x3F | 0x80
                )
            } else if (codePoint < 0x10000) {
                if ((units -= 3) < 0) break
                bytes.push(
                  codePoint >> 0xC | 0xE0,
                  codePoint >> 0x6 & 0x3F | 0x80,
                  codePoint & 0x3F | 0x80
                )
            } else if (codePoint < 0x200000) {
                if ((units -= 4) < 0) break
                bytes.push(
                  codePoint >> 0x12 | 0xF0,
                  codePoint >> 0xC & 0x3F | 0x80,
                  codePoint >> 0x6 & 0x3F | 0x80,
                  codePoint & 0x3F | 0x80
                )
            } else {
                throw new Error('Invalid code point')
            }
        }

        return bytes
    }

    function utf8Slice(buf, start, end) {
        var res = ''
        var tmp = ''
        end = Math.min(buf.length, end || Infinity)
        start = start || 0;

        for (var i = start; i < end; i++) {
            if (buf[i] <= 0x7F) {
                res += decodeUtf8Char(tmp) + String.fromCharCode(buf[i])
                tmp = ''
            } else {
                tmp += '%' + buf[i].toString(16)
            }
        }

        return res + decodeUtf8Char(tmp)
    }

    function decodeUtf8Char(str) {
        try {
            return decodeURIComponent(str)
        } catch (err) {
            return String.fromCharCode(0xFFFD) // UTF 8 invalid char
        }
    }

    TextEncoderLite.prototype.encode = function (str) {
        var result;

        if ('undefined' === typeof Uint8Array) {
            result = utf8ToBytes(str);
        } else {
            result = new Uint8Array(utf8ToBytes(str));
        }

        return result;
    };

    TextDecoderLite.prototype.decode = function (bytes) {
        return utf8Slice(bytes, 0, bytes.length);
    }

}());


// OWN CODE

DomTools = (function () {
    "use strict";

    function DomAnalyzer() {
        this.output = "";

        this.analyzeDocument = function (doc) {
            // Give each node an ID.
            var i = 0;
            this.walkNodes(doc, function (node) {
                node.___analyzerId = i;
                i++;
            });

            var self = this;
            this.walkNodes(doc, function (node) {
                self.analyzeNode(node);
            });
        }

        this.walkNodesXX = function (doc, callback) {
            // Create a tree walker
            var treeWalker = doc.createTreeWalker(
                doc,
                NodeFilter.SHOW_ALL,
                { acceptNode: function (node) { return NodeFilter.FILTER_ACCEPT; } },
                false
            );

            callback(doc);

            var node;
            while (node = treeWalker.nextNode()) {
                callback(node);
            }
        }

        this.walkNodes = function (node, callback) {
            if (node === null)
                return;

            callback(node);
            this.walkNodes(node.firstChild, callback);
            this.walkNodes(node.nextSibling, callback);
        }

        this.analyzeNode = function (node) {
            this.visitNode(node);

            var type = node.nodeType;
            if (type == Node.ELEMENT_NODE)
                this.visitElement(node);
            else if (type == Node.TEXT_NODE)
                this.visitText(node);
            else if (type == Node.PROCESSING_INSTRUCTION_NODE)
                this.visitProcessingInstruction(node);
            else if (type == Node.COMMENT_NODE)
                this.visitComment(node);
            else if (type == Node.DOCUMENT_NODE)
                this.visitDocument(node);
            else if (type == Node.DOCUMENT_TYPE_NODE)
                this.visitDocumentType(node);
            else if (type == Node.DOCUMENT_FRAGMENT_NODE)
                this.visitDocumentFragment(node);
            else
                this.visitUnknownNode(node);

            this.print('\n');
        }

        this.visitNode = function (node) {
            this.printProperty('NodeId', node.___analyzerId);
            this.printProperty('NodeType', node.nodeType);
            this.printString('NodeName', node.nodeName);

            this.printString('BaseUri', node.baseURI);
            this.printNode('OwnerDocument', node.ownerDocument);
            this.printNode('ParentNode', node.parentNode);
            this.printNode('ParentElement', node.parentElement);
            this.printNode('FirstChild', node.firstChild);
            this.printNode('LastChild', node.lastChild);
            this.printNode('PreviousSibling', node.previousSibling);
            this.printNode('NextSibling', node.nextSibling);
            this.printString('NodeValue', node.nodeValue);
            this.printString('TextContent', node.textContent);
            this.printNodeList('ChildNodes', node.childNodes);
        }

        this.visitParentNode = function (node) {
            this.printProperty('ChildElementCount', node.childElementCount);
            this.printNode('FirstElementChild', node.firstElementChild);
            this.printNode('LastElementChild', node.lastElementChild);
            this.printNodeList('Children', node.children);
        }

        this.visitCharacterData = function (node) {
            this.printNode('PreviousElementSibling', node.previousElementSibling);
            this.printNode('NextElementSibling', node.nextElementSibling);
            this.printString('Data', node.data);
            this.printProperty('Length', node.length);
        }

        this.visitElement = function (node) {
            this.visitParentNode(node);

            this.printNode('PreviousElementSibling', node.previousElementSibling);
            this.printNode('NextElementSibling', node.nextElementSibling);

            this.printString('NamespaceURI', node.namespaceURI);
            this.printString('Prefix', node.prefix);
            this.printString('LocalName', node.localName);
            this.printString('TagName', node.tagName);
            this.printString('Id', node.id);
            this.printString('ClassName', node.className);
            this.printStringList('ClassList', node.classList);

            this.printProperty('Attributes', node.attributes.length);
            for (var i = 0; i < node.attributes.length; i++) {
                this.visitAttribute(node.attributes[i]);
            }
        }

        this.visitText = function (node) {
            this.visitCharacterData(node);

            this.printString('WholeText', node.wholeText);
        }

        this.visitProcessingInstruction = function (node) {
            this.visitCharacterData(node);

            this.printString('Target', node.target);
        }

        this.visitComment = function (node) {
            this.visitCharacterData(node);
        }

        this.visitDocument = function (node) {
            this.visitParentNode(node);

            this.printString('URL', node.URL);
            this.printString('DocumentURI', node.documentURI);
            this.printString('Origin', node.origin);
            this.printString('CompatMode', node.compatMode);
            this.printString('CharacterSet', node.characterSet);
            this.printString('ContentType', node.contentType);

            this.printNode('DocType', node.doctype);
            this.printNode('DocumentElement', node.documentElement);
        }

        this.visitDocumentType = function (node) {
            this.printString('Name', node.name);
            this.printString('PublicId', node.publicId);
            this.printString('SystemId', node.systemId);
        }

        this.visitDocumentFragment = function (node) {
            this.visitParentNode(node);
        }

        this.visitUnknownNode = function (node) {

        }

        this.visitAttribute = function (attr) {
            this.print('\n');
            this.printString('NamespaceURI', attr.namespaceURI);
            this.printString('Prefix', attr.prefix);
            this.printString('LocalName', attr.localName);
            this.printString('Name', attr.name);
            this.printString('Value', attr.value);
        }

        this.print = function (text) {
            this.output += text;
        }

        this.printProperty = function (name, value) {
            this.print(name + ' : ' + value + '\n');
        }

        this.printString = function (name, value) {
            this.print(name + ' : ');
            this.printEncodedTextIfNeeded(value);
            this.print('\n');
        }

        this.printNode = function (name, node) {
            if (node === null)
                this.print(name + ' : null \n');
            else if (node === undefined)
                this.print(name + ' : undefined \n');
            else
                this.print(name + ' : ' + node.___analyzerId + '\n');
        }

        this.printEncodedTextIfNeeded = function (text) {
            if (text === null)
                this.print('=== null');
            else if (text === undefined)
                this.print('=== undefined');
            else if (text.length == 0)
                this.print('=== empty');
            else if (this.needsEncoding(text))
                this.printEncodedText(text);
            else
                this.print(text);
        }

        this.printEncodedText = function (text) {
            if (text === null) {
                this.print('=== null');
            }
            else if (text === undefined) {
                this.print('=== undefined');
            }
            else if (text.length == 0) {
                this.print('=== empty');
            }
            else {
                var uint8array = new TextEncoderLite('utf-8').encode(text);
                var base64 = base64js.fromByteArray(uint8array);
                this.print('== ' + base64);
            }
        }

        this.printNodeList = function (name, list)
        {
            if (list === null)
            {
                this.print(name + ' : null \n');
                return;
            }
            if (list === undefined)
            {
                this.print(name + ' : undefined \n');
                return;
            }

            this.print(name + ' : ' + list.length + ' : ');
            for (var i = 0; i < list.length; i++) {
                if (i == 0)
                    this.print(list[i].___analyzerId);
                else
                    this.print(', ' + list[i].___analyzerId);
            }
            this.print('\n');
        }

        this.printStringList = function (name, list)
        {
            if (list === null) {
                this.print(name + ' : null \n');
                return;
            }
            if (list === undefined) {
                this.print(name + ' : undefined \n');
                return;
            }

            this.print(name + ' : ' + list.length + ' : ');
            for (var i = 0; i < list.length; i++) {
                if (i == 0)
                    this.printEncodedText(list[i]);
                else
                    this.printEncodedText(', ' + list[i]);
            }
            this.print('\n');
        }

        this.needsEncoding = function (text) {
            // If any ASCII 0-31 and 128-65535 we need encoding
            return /[\x00-\x1F\x80-\xFFFF]/.test(text) || ((text.length >= 2) && (text[0] === '=') && (text[1] === '=')) || (text[0] === ' ') || (text[text.length - 1] == ' ');
        }
    }

    var loadDocument = function (url) {
        // See: https://developer.mozilla.org/en-US/Add-ons/Code_snippets/HTML_to_DOM
        var request = XMLHttpRequest();
        request.open("GET", url, false);
        request.send(null);

        var doc = document.implementation.createHTMLDocument("example");
        doc.documentElement.innerHTML = request.responseText;

        return doc;
    }

    var analyzeDocument = function (doc) {
        var analyzer = new DomAnalyzer();
        analyzer.analyzeDocument(doc);
        return analyzer.output;
    }

    return {
        LoadDocument: loadDocument,
        AnalyzeDocument: analyzeDocument
    }
})();


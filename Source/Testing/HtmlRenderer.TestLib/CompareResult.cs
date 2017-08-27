using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib
{
    [Flags]
    public enum CompareResult : long
    {
        Equal = 0,
        Node_BaseUri = 0x00000001,
        Node_NodeName = 0x00000002,
        Node_NodeType = 0x00000004,
        Node_NodeValue = 0x00000008,
        Node_TextContent = 0x00000010,
        CharacterData_Data = 0x00000020,
        CharacterData_Length = 0x00000040,
        Text_WholeText = 0x00000080,
        DocumentType_Name = 0x00000100,
        DocumentType_PublicId = 0x00000200,
        DocumentType_SystemId = 0x00000400,
        Document_CharacterSet = 0x00000800,
        Document_CompatMode = 0x00001000,
        Document_ContentType = 0x00002000,
        Document_DocumentType = 0x00004000,
        Document_DocumentElement = 0x00008000,
        Document_DocumentUri = 0x00010000,
        Document_Origin = 0x00020000,
        Document_QuirksMode = 0x00040000,
        Document_Url = 0x00080000,
        Element_AttributeCount = 0x00100000,
        Element_AttributeValue = 0x00200000,
        Element_ClassList = 0x00400000,
        Element_ClassName = 0x00800000,
        Element_Id = 0x01000000,
        Element_LocalName = 0x02000000,
        Element_NamespaceUri = 0x04000000,
        Element_Prefix = 0x08000000,
        Element_TagName = 0x10000000,
        ProcessingInstruction_Target = 0x20000000,
        NodeTypeMismatch = 0x40000000,
        ChildCountMismatch = 0x80000000,

        InvalidChild = unchecked((long)0x8000000000000000)
    }
}

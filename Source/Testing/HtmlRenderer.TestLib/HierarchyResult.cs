using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib
{
    [Flags]
    public enum HierarchyResult
    {
        Valid = 0x00000000,

        Node_ParentNode_MustBeNull = 0x00000001,
        Node_ParentElement_MustBeNull = 0x00000002,
        Node_OwnerDocument_MustBeNull = 0x00000004,

        Node_ParentNode_Invalid = 0x00000008,
        Node_ParentElement_Invalid = 0x00000010,
        Node_OwnerDocument_Invalid = 0x00000020,

        Node_PreviousSibling_Invalid = 0x00000040,
        Node_NextSibling_Invalid = 0x00000080,
        Node_FirstChild_Invalid = 0x00000100,
        Node_LastChild_Invalid = 0x00000200,
        Node_ChildNodes_NotUnique = 0x00000400,
        Node_ChildNodes_NotEmptyForNonParent = 0x00000800,

        NonDocumentTypeChildNode_PreviousElementSibling_Invalid = 0x00004000,
        NonDocumentTypeChildNode_NextElementSibling_Invalid = 0x00008000,

        Document_DocTypeOrElement_Invalid = 0x00020000,

        Element_PreviousElementSibling_Invalid = 0x00040000,
        Element_NextElementSibling_Invalid = 0x00080000,

        ParentNode_FirstChild_Invalid = 0x00100000,
        ParentNode_LastChild_Invalid = 0x00200000,
        ParentNode_Children_NotUnique = 0x00400000,

        ParentNode_ChildElementCount_Invalid = 0x00800000,
        ParentNode_Children_Inconsistent = 0x01000000,

        InvalidChild = unchecked((int)0x80000000)
    }
}

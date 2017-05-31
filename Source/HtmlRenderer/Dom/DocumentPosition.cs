using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-node
    /// </summary>
    [Flags]
    public enum DocumentPosition
    {
        None = 0,
        Disconnected = 1,
        Preceding = 2,
        Following = 4,
        Contains = 8,
        ContainedBy = 16,
        ImplementationSpecific = 32
    }
}

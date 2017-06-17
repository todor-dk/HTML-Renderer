using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal struct ElementAttributes
    {
        private object Content;

        internal int GetCount()
        {
            if (this.Content == null)
                return 0;
            if (this.Content is Attr)
                return 1;
            return ((IList)this.Content).Count;
        }

        internal IEnumerator<Dom.Attr> GetEnumerator()
        {
            if (this.Content == null)
                yield break;

            Attr attr = this.Content as Attr;
            if (attr != null)
            {
                yield return attr;
                yield break;
            }

            IEnumerator<Attr> enumerator = ((IReadOnlyList<Attr>)this.Content).GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        internal Attr GetItem(int index)
        {
            if (index < 0)
                return null;

            if (this.Content == null)
                return null;

            Attr attr = this.Content as Attr;
            if (attr != null)
            {
                if (index == 0)
                    return attr;
                else
                    return null;
            }

            IReadOnlyList<Attr> attrs = (IReadOnlyList<Attr>)this.Content;
            if (index < attrs.Count)
                return attrs[index];
            else
                return null;
        }

        internal IdAttr GetIdAttr()
        {
            if (this.Content == null)
                return null;

            Attr attr = this.Content as Attr;
            if (attr != null)
                return attr as IdAttr;

            return ((IReadOnlyList<Attr>)this.Content)[0] as IdAttr;
        }

        internal ClassAttr GetClassAttr()
        {
            if (this.Content == null)
                return null;

            Attr attr = this.Content as Attr;
            if (attr != null)
                return attr as ClassAttr;

            IReadOnlyList<Attr> list = (IReadOnlyList<Attr>)this.Content;
            return (list[0] as ClassAttr) ?? (list[1] as ClassAttr);
        }

        internal void Append(Element ownerElement, Attr attribute)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-element-attributes-append
            // To append an attribute attribute to an element element, run these steps:

            // 1. Queue a mutation record of "attributes" for element with name attribute's local name,
            //    namespace attribute's namespace, and oldValue null.
            // TODO: Implement observers

            // 2. Append the attribute to the element's attribute list.
            attribute.OwnerElement = ownerElement;
            this.Add(attribute);

            // 3. An attribute is set and an attribute is added.
            ownerElement.AttributeIsSetAndAdded(attribute);
        }

        private const int ComplexThreshold = 10;

        private void Add(Attr attribute)
        {
            if (this.Content == null)
            {
                this.Content = attribute;
                return;
            }

            Attr existing = this.Content as Attr;
            if (existing != null)
            {
                if (attribute is IdAttr)
                    this.Content = new Attr[] { attribute, existing };
                else if ((attribute is ClassAttr) && !(existing is IdAttr))
                    this.Content = new Attr[] { attribute, existing };
                else
                    this.Content = new Attr[] { existing, attribute };
                return;
            }

            Attr[] existingAttrs = this.Content as Attr[];
            if (existingAttrs != null)
            {
                if (existingAttrs.Length == ElementAttributes.ComplexThreshold)
                {
                    // Switch to complex
                    this.Content = new List<Attr>(existingAttrs);

                    // fall trough
                }
                else
                {
                    Attr[] newContents = new Attr[existingAttrs.Length + 1];
                    if (attribute is IdAttr)
                    {
                        newContents[0] = attribute;
                        Array.Copy(existingAttrs, 0, newContents, 1, existingAttrs.Length);
                    }
                    else if (attribute is ClassAttr)
                    {
                        if (existingAttrs[0] is IdAttr)
                        {
                            newContents[0] = existingAttrs[0];
                            newContents[1] = attribute;
                            Array.Copy(existingAttrs, 1, newContents, 2, existingAttrs.Length);
                        }
                        else
                        {
                            newContents[0] = attribute;
                            Array.Copy(existingAttrs, 0, newContents, 1, existingAttrs.Length);
                        }
                    }
                    else
                    {
                        Array.Copy(existingAttrs, newContents, existingAttrs.Length);
                        newContents[existingAttrs.Length] = attribute;
                    }

                    this.Content = newContents;
                    return;
                }
            }

            List<Attr> list = (List<Attr>)this.Content;
            if (attribute is IdAttr)
            {
                list.Insert(0, attribute);
            }
            else if (attribute is ClassAttr)
            {
                if (list[0] is IdAttr)
                    list.Insert(1, attribute);
                else
                    list.Insert(0, attribute);
            }
            else
            {
                list.Add(attribute);
            }
        }

        internal void RemoveAttribute(Predicate<Attr> test)
        {
            if (this.Content == null)
                return;

            Attr existing = this.Content as Attr;
            if (existing != null)
            {
                if (test(existing))
                {
                    this.Content = null;
                    existing.OwnerElement = null;
                }

                return;
            }

            Attr[] existingAttrs = this.Content as Attr[];
            if (existingAttrs != null)
            {
                for (int idx = 0; idx < existingAttrs.Length; idx++)
                {
                    if (test(existingAttrs[idx]))
                    {
                        Attr[] newContents = new Attr[existingAttrs.Length - 1];
                        if (idx != 0)
                            Array.Copy(existingAttrs, newContents, idx);
                        for (int i = idx; i < newContents.Length; i++)
                            newContents[i] = existingAttrs[i + 1];

                        if (newContents.Length == 1)
                            this.Content = newContents[0];
                        else
                            this.Content = newContents;

                        existingAttrs[idx].OwnerElement = null;

                        return;
                    }
                }

                return;
            }

            List<Attr> list = (List<Attr>)this.Content;
            for (int idx = 0; idx < list.Count; idx++)
            {
                if (test(list[idx]))
                {
                    list[idx].OwnerElement = null;
                    list.RemoveAt(idx);

                    if (list.Count < ElementAttributes.ComplexThreshold)
                        this.Content = list.ToArray();

                    return;
                }
            }
        }
    }
}

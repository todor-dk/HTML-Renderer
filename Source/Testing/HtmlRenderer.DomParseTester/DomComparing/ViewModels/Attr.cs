using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class Attr : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Context Context { get; private set; }

        public ReferenceAttr Model { get; private set; }

        public Attr(Context context, ReferenceAttr model)
        {
            this.Context = context;
            this.Model = model;
        }
    }
}

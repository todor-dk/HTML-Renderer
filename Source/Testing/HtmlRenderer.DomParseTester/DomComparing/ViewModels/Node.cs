using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{

    public abstract class Node : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Node> ChildNodes { get; private set; }

        public Context Context { get; private set; }

        public CompareResult CompareResult
        {
            get
            {
                return this._CompareResult;
            }
            set
            {
                if (this._CompareResult == value)
                    return;
                this._CompareResult = value;
                this.OnPropertyChanged(nameof(this.CompareResult));
            }
        }

        private CompareResult _CompareResult;

        public Node(Context context)
        {
            this.ChildNodes = new List<Node>();
            this.Context = context;
        }

        public abstract CompareResult Compare(Node other);

        public static Node FromReferenceNode(Context context, ReferenceNode node)
        {
            Node model = CreateFromReferenceNode(context, node);
            foreach (ReferenceNode child in node.ChildNodes)
                model.ChildNodes.Add(FromReferenceNode(context, child));
            return model;
        }

        private static Node CreateFromReferenceNode(Context context, ReferenceNode node)
        {
            if (node is ReferenceElement)
                return new Element(context, (ReferenceElement)node);
            if (node is ReferenceText)
                return new Text(context, (ReferenceText)node);
            if (node is ReferenceComment)
                return new Comment(context, (ReferenceComment)node);
            if (node is ReferenceDocument)
                return new Document(context, (ReferenceDocument)node);
            if (node is ReferenceDocumentFragment)
                return new DocumentFragment(context, (ReferenceDocumentFragment)node);
            if (node is ReferenceDocumentType)
                return new DocumentType(context, (ReferenceDocumentType)node);
            throw new AggregateException();
        }
    }
    
    public abstract class Node<TReferenceNode> : Node
        where TReferenceNode : ReferenceNode
    {
        public TReferenceNode Model { get; private set; }

        public Node(Context context, TReferenceNode model)
            : base(context)
        {
            this.Model = model;
        }

        protected CompareResult CompareWithNode(TReferenceNode otherModel)
        {
            CompareResult result = CompareResult.Equal;

            if (this.Model.BaseUri != otherModel.BaseUri)
                result = result | CompareResult.Node_BaseUri;

            if (this.Model.NodeName != otherModel.NodeName)
                result = result | CompareResult.Node_NodeName;

            if (this.Model.NodeType != otherModel.NodeType)
                result = result | CompareResult.Node_NodeType;

            if (this.Model.NodeValue != otherModel.NodeValue)
                result = result | CompareResult.Node_NodeValue;

            if (this.Model.TextContent != otherModel.TextContent)
                result = result | CompareResult.Node_TextContent;

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WinReporter
{

    public class TextNode
    {
        public static TextNode Empty { get => new(TextNode.Empty, string.Empty, string.Empty); }

        private TextNode? _Parent;
        public TextNode? Parent { get => this._Parent; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int ImageIndex { get; set; }
        public TextNodeCollection TextNodes { get; set; }
        public TextNode(TextNode? parent, string name, string text)
        {
            this.TextNodes = new(this);
            this.Name = name;
            this.Text = text;
            this.ImageIndex = -1;
            this._Parent = parent;
        }
        public TextNode(string name, string text)
        {
            this.TextNodes = new(this);
            this.Name = name;
            this.Text = text;
            this.ImageIndex = -1;
            this._Parent = null;
        }
    }
    public class SortedTextNodeComparer : IComparer<TextNode>
    {
        public int Compare(TextNode? x, TextNode? y)
        {
            int result = 0;
            if (x != null && y != null)
            {
                result = x.Name.CompareTo(y.Name);
                if (result == 0)
                {
                    result = x.Text.CompareTo(y.Text);
                }
            }
            return (result);
        }
    }
    
    public class TextNodeCollection : List<TextNode>
    {
        public static TextNodeCollection Empty { get => new(TextNode.Empty); }
        private TextNode? Parent { get; set; }
        public virtual TextNode this[string name] => this.GetTextNode(name);
        public TextNode Add(string name, string text)
        {
            TextNode textNode = new TextNode(this.Parent, name, text);

            this.Add(textNode);

            return (textNode);
        }
        public TextNodeCollection(TextNode? parent)
        {
            this.Parent = parent;
        }
        internal new void Add(TextNode textNode)
        {
            int i = this.BinarySearch(textNode, new SortedTextNodeComparer());
            base.Insert(~i, textNode);
        }
        
        internal TextNode GetTextNode(string name)
        {
            if (this.Count == 0) { return (TextNode.Empty); }

            int i = ~this.BinarySearch(new TextNode(TextNode.Empty, name, string.Empty), new SortedTextNodeComparer());

            TextNode? textNode = this[i];
            if (textNode != null)
            {
                return (textNode);
            }
            else
            {
                return (TextNode.Empty);
            }
        }
    }

    public class TextTree
    {
        public TextNodeCollection TextNodes { get; set; }
        public TextTree()
        {
            this.TextNodes = new(null);
        }
    }
}

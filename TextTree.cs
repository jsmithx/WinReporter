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
        public string Name { get; set; }
        public string Text { get; set; }
        public int ImageIndex { get; set; }
        public TextNodeCollection TextNodes { get; set; }
        public TextNode(string name, string text)
        {
            this.TextNodes = new();
            this.Name = name;
            this.Text = text;
            this.ImageIndex = -1;
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
        public TextNode Empty { get => new(string.Empty, string.Empty); }

        public virtual TextNode this[string name] => this.GetTextNode(name);
        public TextNode Add(string name, string text)
        {
            TextNode textNode = new TextNode(name, text);
            
            this.Add(textNode);

            return (textNode);
        }

        internal new void Add(TextNode textNode)
        {
            int i = this.BinarySearch(textNode, new SortedTextNodeComparer());
            base.Insert(~i, textNode);
        }

        
        internal TextNode GetTextNode(string name)
        {
            if (this.Count == 0) { return (this.Empty); }

            int i = ~this.BinarySearch(new TextNode(name, string.Empty), new SortedTextNodeComparer());

            TextNode? textNode = this[i];
            if (textNode != null)
            {
                return (textNode);
            }
            else
            {
                return (this.Empty);
            }
        }
    }

    public class TextTree
    {
        public TextNodeCollection TextNodes { get; set; }
        public TextTree()
        {
            this.TextNodes = new();
        }
    }

}

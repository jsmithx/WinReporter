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
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Reflection;

namespace WinReporter
{

    public class TextNode
    {
        public static TextNode? Empty { get => null; }

        private TextNode? _Parent;
        public TextNode? Parent { get => this._Parent; }
        private int _Level;
        public int Level { get => this._Level; }
        private string _Name = string.Empty;
        public string Name { get { return this._Name; } set { this._Name = value; } }
        private string _Text = string.Empty;
        public string Text { get { return this._Text; } set { this._Text = value; } }
        string _ImageKey = string.Empty;
        public string ImageKey { get { return this._ImageKey; } set { this._ImageKey= value; } }
        string _SelectedImageKey = string.Empty;
        public string SelectedImageKey { get { return this._SelectedImageKey; } set { this._SelectedImageKey = value; } }
        TextNodeCollection _TextNodes;
        public TextNodeCollection TextNodes { get { return this._TextNodes; } set{ this._TextNodes = value; } }
        public TextNode(TextNode? parent, string name, string text)
        {
            this._TextNodes = new(this);
            this._Parent = parent;
            this.Initialize(name, text, string.Empty);
        }
        public TextNode(string name, string text)
        {
            this._TextNodes = new(this);
            this._Parent = null;
            this.Initialize(name, text, string.Empty);
        }
        public TextNode(string name, string text, string imageKey)
        {
            this._TextNodes = new(this);
            this._Parent = null;
            this.Initialize(name, text, imageKey);
        }
        private void Initialize(string name, string text, string imageKey)
        {
            this._Level = this._Parent != null ? this._Parent.Level + 1 : 0;
            this.Name = name;
            this.Text = text;
            this.ImageKey = imageKey;
            this.SelectedImageKey = imageKey;
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
    public class OrderedHashSet : KeyedCollection<string, TextNode>
    {
        protected override string GetKeyForItem(TextNode item)
        {
            return item.Name;
        }
    }
    public class TextNodeCollection : OrderedHashSet //HashSet<TextNode>
    {
        private TextNode? _Parent;
        private TextNode? Parent { get => this._Parent; }

        public virtual TextNode? this[TextNode textNode] => this.GetSimilarTextNode(textNode);
        public TextNodeCollection(TextNode? parent)
        {
            this._Parent = parent;
        }
        public TextNode Add(string name, string text)
        {
            TextNode textNode;
            textNode = new TextNode(this.Parent, name, text);
            this.Add(textNode);

            return (textNode);
        }
        internal new void Add(TextNode textNode)
        {
            base.Add(textNode);
        }
        
        internal TextNode? GetSimilarTextNode(TextNode textNode)
        {
            if (this.Count == 0) { return (TextNode.Empty); }

            TextNode? result = this.Where(w => w.Name == textNode.Name && w.Text == textNode.Text).FirstOrDefault();
            return (result);
        }

        public string Serialize()
        {
            StringBuilder lines = new();

            for (int i = 0; i < this.Items.Count; i++)
            {
                TextNode node = this.Items[i];
                lines.Append(
                    "<" +
                    "Level=" + node.Level + " " +
                    "Name=(" + node.Name + ") " +
                    (node.ImageKey.Length > 0 ? "ImageKey=" + node.ImageKey + " " : "") +
                    (node.SelectedImageKey.Length > 0 ? "SelectedImageKey=" + node.SelectedImageKey + " " : "") +
                    "Text=(" + node.Text + ") " +
                    ">" +
                    Environment.NewLine
                    );
                lines.Append(node.TextNodes.Serialize());
            }
            return (lines.ToString());
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

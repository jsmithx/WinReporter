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

namespace WinReporter
{

    public class TextNode
    {
        public static TextNode Empty { get => new(TextNode.Empty, string.Empty, string.Empty); }

        private TextNode? _Parent = null;
        public TextNode? Parent { get => this._Parent; }
        private int _Level = 0;
        public int Level { get => this._Level; }
        public string Name { get { return string.Empty; } set { Name = value; } }
        public string Text { get { return string.Empty; } set { Text = value; } }
        public string ImageKey { get { return string.Empty; } set { ImageKey = value; } }
        public TextNodeCollection TextNodes { get { return TextNodeCollection.Empty; } set { TextNodes = value; } }
        public TextNode(TextNode? parent, string name, string text)
        {
            this.Initialize(parent, name, text, string.Empty);
        }
        public TextNode(string name, string text)
        {
            this.Initialize(null, name, text, string.Empty);
        }
        public TextNode(string name, string text, string imageKey)
        {
            this.Initialize(null, name, text, imageKey);
        }
        private void Initialize(TextNode? parent, string name, string text, string imageKey)
        {
            this.TextNodes = new(this);
            this.Name = name;
            this.Text = text;
            this.ImageKey = ImageKey;
            this._Parent = parent;
            if (this._Parent != null)
            {
                this._Level = this._Parent._Level + 1;
            }
            else
            {
                this._Level = 0;
            }
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
    public class TextNodeTable : Hashtable
    {
        Hashtable a = new();
        public void b()
        {
            
        }
    }
    public class TextNodeCollection : HashSet<TextNode>
    {
        public static TextNodeCollection Empty { get => new(TextNode.Empty); }
        private TextNode? Parent { get; set; }

        public virtual TextNode this[string name] => this.GetTextNode(name);
        public TextNodeCollection(TextNode? parent)
        {
            this.Parent = parent;
        }
        public TextNode Add(string name, string text)
        {
            TextNode textNode = new TextNode(this.Parent, name, text);
            
            this.Add(textNode);

            return (textNode);
        }
        internal new void Add(TextNode textNode)
        {
            base.Add(textNode);
        }
        
        internal TextNode GetTextNode(string name)
        {
            if (this.Count == 0) { return (TextNode.Empty); }

            TextNode? textNode = this.Where(w => w.Name == name).FirstOrDefault();
            
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

        public string SerializeTextTree()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            foreach(TextNode textNode in this.TextNodes)
            {

            }

            return (stringBuilder.ToString());
        }
    }
}

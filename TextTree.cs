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
using System.CodeDom;

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
        private object? _Value = string.Empty;
        public object? Value { get { return this._Value; } set { this._Value = value; } }
        public string ValueStr
        {
            get
            {
                if (this.Value != null && this.Value.GetType() == typeof(byte[]))
                {
                    return (((byte[])this.Value).ToText());
                }
                else
                {
                    return ("{ TextObject }");
                }
            }
        }

        string _ImageKey = string.Empty;
        public string ImageKey { get { return this._ImageKey; } set { this._ImageKey= value; } }
        string _SelectedImageKey = string.Empty;
        public string SelectedImageKey { get { return this._SelectedImageKey; } set { this._SelectedImageKey = value; } }
        TextNodeCollection _TextNodes;
        public TextNodeCollection TextNodes { get { return this._TextNodes; } set{ this._TextNodes = value; } }
        public long _NodePosition = 0;
        public long NodePosition { get=>this._NodePosition; set { this._NodePosition = value; } }
        public long? _ParentPosition;
        public long? ParentPosition { get => this._ParentPosition; set { this._ParentPosition = value; } }
        public TextNode(TextNode? parent, string name, object? value)
        {
            this._TextNodes = new(this);
            this._Parent = parent;
            this.Initialize(name, value, string.Empty);
        }
        public TextNode(string name, object? value)
        {
            this._TextNodes = new(this);
            this._Parent = null;
            this.Initialize(name, value, string.Empty);
        }
        public TextNode(string name, object? value, string imageKey)
        {
            this._TextNodes = new(this);
            this._Parent = null;
            this.Initialize(name, value, imageKey);
        }
        private void Initialize(string name, object? value, string imageKey)
        {
            this._Level = this._Parent != null ? this._Parent.Level + 1 : 0;
            this.Name = name;
            this.Value = value;
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
                    if (x != null && y != null && x.GetType() == typeof(string) && y.GetType() == typeof(string))
                    {
                        string xStr = x.Value.ToBytes().ToText();
                        string yStr = y.Value.ToBytes().ToText();

                        result = (xStr).CompareTo(yStr);
                    }
                }
            }
            return (result);
        }
    }
    public class TextNodeCollection : KeyedCollection<string, TextNode>
    {
        protected override string GetKeyForItem(TextNode item)
        {
            return item.Name;
        }

        private TextNode? _Parent;
        private TextNode? Parent { get => this._Parent; }

        public virtual TextNode? this[TextNode textNode] => this.GetSimilarTextNode(textNode);
        public TextNodeCollection(TextNode? parent)
        {
            this._Parent = parent;
        }
        public TextNode Add(string name, object? value)
        {
            TextNode textNode;
            textNode = new TextNode(this.Parent, name, value);
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

            TextNode? result = this.Where(w => w.Name == textNode.Name && w.Value == textNode.Value).FirstOrDefault();
            return (result);
        }

        public static TextNodeCollection Deserialize(ref Stream stream, long basePosition = 0)
        {
            byte[] data = new byte[sizeof(long)];
            stream.Position = basePosition + stream.Length - data.Length;
            stream.Read(data);

            long XrefPosition = BitConverter.ToInt64(data, 0);

            return (new(null));
        }
        public Stream Serialize(ref Stream stream, long basePosition = 0)
        {
            return (this.Serialize(ref stream, 0, true));
        }
        private Stream Serialize(ref Stream stream, long basePosition, bool IsRoot)
        {
            long[] parentPositionTable = new long[this.Items.Count];

            string res;
            for (int i = 0; i < this.Items.Count; i++)
            {
                TextNode node = this.Items[i];

                node.NodePosition = basePosition + stream.Position;
                if (IsRoot == true)
                {
                    parentPositionTable[i] = node.NodePosition;
                }

                stream.Write("<<".ToBytes());

                stream.Write("/Type ".ToBytes());
                stream.Write("/Node".ToBytes());
                stream.Write(" ".ToBytes());

                stream.Write("/Name (".ToBytes());
                stream.Write(node.Name.ToBytes());
                res = node.Name.ToBytes().ToText();
                stream.Write(") ".ToBytes());

                stream.Write("/Level ".ToBytes());
                stream.Write(node.Level.ToBytes());
                res = node.Level.ToBytes().ToText();
                stream.Write(" ".ToBytes());

                if (node.Parent != null)
                {
                    stream.Write("/ParentPosition ".ToBytes());
                    node.ParentPosition = node.Parent.NodePosition;
                    stream.Write(node.ParentPosition.ToBytes());
                    res = node.ParentPosition.ToBytes().ToText();
                    stream.Write(" ".ToBytes());
                }

                stream.Write("/Text (".ToBytes());
                stream.Write(node.Value.ToBytes());
                res = node.Value.ToBytes().ToText();
                stream.Write(")".ToBytes());
                stream.Write(">>".ToBytes());

                stream.Write(Environment.NewLine.ToBytes());

                node.TextNodes.Serialize(ref stream, basePosition, false);
            }

            if (IsRoot == true)
            {
                long XrefPosition = basePosition + stream.Position;

                stream.Write("<<".ToBytes());

                stream.Write("/Type=".ToBytes());
                stream.Write("/Xref".ToBytes());
                stream.Write(" ".ToBytes());
                stream.Write("/Count=".ToBytes());

                stream.Write(parentPositionTable.Length.ToBytes());

                stream.Write(">>".ToBytes());

                for (int i = 0; i < parentPositionTable.Length; i++)
                {
                    string pos = parentPositionTable[i].ToBytes().ToText();
                    stream.Write(parentPositionTable[i].ToBytes());
                }
                stream.Write(BitConverter.GetBytes(XrefPosition));
            }

            return (stream);
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

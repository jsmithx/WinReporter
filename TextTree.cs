using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace WinReporter
{
    public class TextNode
    {
        public Dictionary<string, TextNode> TextNodes { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }

        public TextNode(string text, string name)
        {
            this.TextNodes = new();
            this.Text = text;
            this.Name = name;
        }
    }
    public class TextTree
    {
        Dictionary<string, TextNode> TextNodes;
        public TextTree()
        {
            this.TextNodes = new();
        }

        public string ToTreeString()
        {
            string lines = string.Empty;
            foreach (KeyValuePair<string, TextNode> keyValuePair in this.TextNodes)
            {
                //string line = "<" + "level=" + treeNode.Level + " name=(" + treeNode.Name + ") text=(" + treeNode.Text + ")" + ">" + Environment.NewLine;
                //lines += line;
                //lines += TreeToText(treeNode.Nodes);
            }
            return (lines);
        }
    }
}

using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Data;
using Microsoft.Data.Sqlite;

namespace WinReporter
{
    public partial class fMain : Form
    {
        public fMain()
        {
            InitializeComponent();
        }

        // The root node.
        private void fMain_Load(object sender, EventArgs e)
        {
            byte[] data = Encoding.UTF8.GetBytes("Publisher\tMarvel Comics[a]\r\nFirst appearance\tCaptain America Comics #1 (December 20, 1940)[b]\r\nCreated by\tJoe Simon\r\nJack Kirby");
            string[] keys = new string[] { "Publisher", "First appearance", "Created by" };
            TextParser a = new(ref data, ref keys);

            byte[] testdata = Encoding.Default.GetBytes("a//bb//ccc//dddd//eeeee//");
            byte[] delimiter = Encoding.Default.GetBytes("/");
            byte[][] testdatachunks = testdata.Split(delimiter);
            byte[][] testdatachunks2 = testdata.Split(delimiter, true);
            string[] testdatachunkstxt = testdatachunks.ToStringArray();
            string[] testdatachunkstxt2 = testdatachunks2.ToStringArray();
        }
        
        private string TreeToText(TreeNodeCollection nodes)
        {
            string lines = "";

            foreach (TreeNode treeNode in nodes)
            {
                string line = "<" + "level=" + treeNode.Level + " name=(" + treeNode.Name + ") text=(" + treeNode.Text + ")" + ">" + Environment.NewLine;
                lines += line;
                lines += TreeToText(treeNode.Nodes);
            }
            return (lines);
        }

        private void TextToTree(byte[] textData)
        {
            int pos = 0;
            int bracketBalance = 0;
            int posDataStart = -1;
            int posDataEnd = -1;

            byte[] dataKey = new byte[0];
            byte[] dataValue = new byte[0];
            int dataKeyValueStart = 0;
            int dataKeyValueEnd = 0;

            string textDataStr = Encoding.UTF8.GetString(textData);
            while (pos < textData.Length)
            {
                if (textData[pos] == '<')
                {
                    if (bracketBalance == 0)
                    {
                        posDataStart = pos + 1;
                    }
                    bracketBalance++;
                }
                else if (textData[pos] == '>')
                {
                    bracketBalance--;
                    if (bracketBalance == 0)
                    {
                        posDataEnd = pos;
                    }
                }
                else if (textData[pos] == '(')
                {
                    while (pos < textData.Length)
                    {
                        if (textData[pos] == ')')
                        {
                            break;
                        }
                        pos++;
                    }
                }
                
                if (bracketBalance == 0 && posDataStart > -1 && posDataEnd > -1)
                {
                    byte[] textDataSelection = textData.Skip(posDataStart).Take(posDataEnd - posDataStart).ToArray();
                    string textDataSelectionStr = Encoding.UTF8.GetString(textDataSelection);
                    TextToTree(textDataSelection);
                    posDataStart = -1;
                    posDataEnd = -1;
                }
                else if (bracketBalance == 0 && posDataStart < 0 && posDataEnd < 0)
                {
                    if (textData[pos] == ' ' || textData[pos] == '=')
                    {
                        dataKeyValueEnd = pos;
                        if (dataKey.Length == 0)
                        {
                            dataKey = textData.Skip(dataKeyValueStart).Take(dataKeyValueEnd - dataKeyValueStart).ToArray();
                            dataKeyValueStart = dataKeyValueEnd + 1;
                            dataKeyValueEnd = -1;

                            dataValue = new byte[0];
                            string dataKeyStr = Encoding.UTF8.GetString(dataKey);
                        }
                        else if (dataKey.Length > 0)
                        {
                            dataValue = textData.Skip(dataKeyValueStart).Take(dataKeyValueEnd - dataKeyValueStart).ToArray();
                            dataKeyValueStart = dataKeyValueEnd + 1;
                            dataKeyValueEnd = -1;
                            dataKey = new byte[0];

                            string dataValueStr = Encoding.UTF8.GetString(dataValue);
                        }
                    }
                }

                pos++;
            }
        }
        private void GenerateRandomTree()
        {
            this.GenerateTree(5);
        }
        private void GenerateTree(int nodesCount = 5)
        {
            Random r = new Random();
            for (int i = 0; i < nodesCount; i++)
            {
                string nodeName = i.ToString();
                TreeNode childNode = this.ctlTreeOriginal.Nodes.Add(key: nodeName, text: Generator.LoremIpsum(1, 3, 0, 0, 1));
                GenerateTree(childNode, nodesCount - 1);
            }
        }
        private string GetKeyPath(TreeNode node)
        {
            if (node.Parent == null)
            {
                return node.Name;
            }

            return GetKeyPath(node.Parent) + "/" + node.Name;
        }

        private void GenerateTree(TreeNode parentNode, int nodesCount)
        {
            Random r = new Random();

            for (int i = 0; i < nodesCount; i++)
            {
                string nodeName = i.ToString();
                TreeNode childNode = parentNode.Nodes.Add(key: nodeName, text: Generator.LoremIpsum(1, 3, 0, 0, 1));
                GenerateTree(childNode, nodesCount - 1);
            }
        }

        private void btDemo_Click(object sender, EventArgs e)
        {
            this.ctlTreeOriginal.Nodes.Clear();
            this.ctlTreeText.Text = string.Empty;
            this.ctlTreeCopy.Nodes.Clear();

            this.GenerateRandomTree();
            this.ctlTreeText.Text = TreeToText(this.ctlTreeOriginal.Nodes);
            TextToTree(Encoding.UTF8.GetBytes(this.ctlTreeText.Text));
        }
    }
}
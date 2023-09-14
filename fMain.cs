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
            byte[] data = "Publisher\tMarvel Comics[a]\r\nFirst appearance\tCaptain America Comics #1 (December 20, 1940)[b]\r\nCreated by\tJoe Simon\r\nJack Kirby".ToBytes();
            string[] keysStr = new string[] { "[0]Publisher|test", "First appearance", "Created by" };
            TextKey[] textKeys = TextParser.GetKeys(keysStr, "|", "[0]");
            
            TextParser parser = new(ref data, textKeys);

            byte[] testdata = "a//bb//ccc//dddd//eeeee//".ToBytes();
            byte[] separator = "/".ToBytes();
            byte[][] testdatachunks = testdata.Split(separator, false);
            byte[][] testdatachunks2 = testdata.Split(separator, true);
            string[] testdatachunkstxt = testdatachunks.ToTextArray();
            string[] testdatachunkstxt2 = testdatachunks2.ToTextArray();

            testdata = "a/b!c!".ToBytes();
            char[] specialChars = new char[2] { '/', '!' };
            byte[] testDataResult = testdata.EncodeSpecialChars(specialChars);
            string testDataResultStr = testDataResult.ToText();
            byte[] decodedTestData = testDataResult.DecodeSpecialChars(new char[] { '!' });
            string decodedTestDataStr = decodedTestData.ToText();

            TextTree tree = new();
            TextNode node1 = tree.TextNodes.Add("nameA", "textA");
            TextNode node2 = tree.TextNodes.Add("nameD", "textD");
            TextNode node3 = tree.TextNodes.Add("nameB", "textB");

            TextNode node4 = node1.TextNodes.Add("name1", "text1");
            TextNode node5 = node1.TextNodes.Add("name2", "text2");

            node4.TextNodes.Add("name11", "text11");

            TextNode node1Copy = tree.TextNodes["nameAB"].TextNodes["name2"];

            foreach (TextNode node in tree.TextNodes)
            {
                string name = node.Name;
                TextNodeCollection textNodes = node.TextNodes;
            }

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

            string textDataStr = textData.ToText();
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
                    string textDataSelectionStr = textDataSelection.ToText();
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
                            string dataKeyStr = dataKey.ToText();
                        }
                        else if (dataKey.Length > 0)
                        {
                            dataValue = textData.Skip(dataKeyValueStart).Take(dataKeyValueEnd - dataKeyValueStart).ToArray();
                            dataKeyValueStart = dataKeyValueEnd + 1;
                            dataKeyValueEnd = -1;
                            dataKey = new byte[0];

                            string dataValueStr = dataValue.ToText();
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
            TextToTree(this.ctlTreeText.Text.ToBytes());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinReporter
{
    public struct Item
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TextParser
    {
        private byte[] DataSource { get; set; }
        public string[] Keys { get; set; }

        public List<Item> Items;
        public TextParser(ref byte[] dataSource, ref string[] keys)
        {
            this.Items = new();

            this.Keys = keys;
            this.DataSource = dataSource;
            this.Parse();
        }
        private void Parse()
        {
            List<string> LKeys = new(this.Keys);
            List<string> LResults = new();
            int resultPos = -1;
            string resultKey = string.Empty;

            int pos = 0;
            while (pos < this.DataSource.Length)
            {
                string matchedKey;
                if (this.IsLetterOrDigit(pos - 1) == false && this.IsEqual(pos, LKeys.ToArray(), out matchedKey) == true)
                {
                    if (resultPos > -1)
                    {
                        string? resultStr = Encoding.Default.GetString(this.DataSource.Skip(resultPos).Take(pos - resultPos).ToArray());
                        if (resultStr != null)
                        {
                            LResults.Add(resultStr);
                            this.Items.Add(new() { Name = resultKey, Value = resultStr.Substring(resultKey.Length).Trim() });
                        }

                        // Get the last Key
                        if (LKeys.Count == 1)
                        {
                            resultKey = matchedKey;
                            resultStr = Encoding.Default.GetString(this.DataSource.Skip(pos).Take(this.DataSource.Length - pos).ToArray());
                            if (resultStr != null)
                            {
                                LResults.Add(resultStr);
                                this.Items.Add(new() { Name = resultKey, Value = resultStr.Substring(resultKey.Length).Trim() });
                            }
                        }
                    }

                    resultPos = pos;
                    resultKey = matchedKey;
                    LKeys.Remove(matchedKey);
                }
                pos++;
            }
        }
        
        public bool IsEqual(int sourcePos, byte[] key)
        {
            bool isValid = false;
            int n = 0;
            for (int i = sourcePos; i < this.DataSource.Length; i++)
            {
                if (n < key.Length)
                {
                    if (key[n] != this.DataSource[i])
                    {
                        isValid = false;
                        break;
                    }
                }
                else
                {
                    isValid = true;
                    break;
                }
                n++;
            }
            return (isValid);
        }
        public bool IsEqual(int sourcePos, string[] keys, out string matchedKey)
        {
            matchedKey = string.Empty;

            bool isValid = false;

            for (int k = 0; k < keys.Length; k++)
            {
                byte[] key = Encoding.Default.GetBytes(keys[k]);

                isValid = this.IsEqual(sourcePos, key);
                if (isValid == true)
                {
                    matchedKey = keys[k];
                    break;
                }
            }
            
            return (isValid);
        }

        public bool IsLetterOrDigit(int sourcePos)
        {
            if (sourcePos > -1 && sourcePos < this.DataSource.Length)
            {
                if (char.IsLetterOrDigit(Convert.ToChar(this.DataSource[sourcePos])))
                {
                    return (true);
                }
            }
            return (false);
        }
    }
}

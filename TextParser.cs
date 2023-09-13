using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinReporter
{
    public struct Item
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Key
    {
        public byte[][] Subkeys { get; set; }
        public string[] SubkeysStr {
            get => Subkeys.ToTextArray();
        }
        public byte[] SelectedSubkey { get; set; }
        public string SelectedSubkeyStr
        {
            get => SelectedSubkey.ToText();
        }
        public Key(byte[] keys, byte[] subkeySeparator)
        {
            this.Subkeys = keys.Split(subkeySeparator, true);
            this.SelectedSubkey = new byte[0];
        }
    }
   
    public class TextParser
    {
        public List<Item> Items;
        public TextParser(ref byte[] dataSource, Key[] keys)
        {
            this.Items = new();
            this.Parse(ref dataSource, keys, false);
        }
        private void Parse(ref byte[] dataSource, Key[] keys, bool trimValues)
        {
            List<Key> LKeys = new(keys);
            List<string> LResults = new();
            int resultPos = -1;
            Key resultKey = new(new byte[0], new byte[0]);

            int pos = 0;
            while (pos < dataSource.Length)
            {
                Key matchedKey;

                if (dataSource.IsLetterOrDigit(pos - 1) == false && dataSource.IsEqual(pos, LKeys.ToArray(), out matchedKey) == true)
                {
                    if (resultPos > -1)
                    {
                        string? resultStr = dataSource.SelectTextRange(resultPos, pos - resultPos);
                        if (resultStr != null)
                        {
                            LResults.Add(resultStr);
                            this.Items.Add(new()
                            {
                                Name = resultKey.SelectedSubkey.ToText(),
                                Value = trimValues == false ? resultStr.Substring(resultKey.SelectedSubkey.ToText().Length) : resultStr.Substring(resultKey.SelectedSubkey.ToText().Length).Trim()
                            });
                        }

                        // Get the last Key
                        if (LKeys.Count == 1)
                        {
                            resultKey = matchedKey;
                            resultStr = dataSource.SelectTextRange(pos, dataSource.Length - pos);
                            if (resultStr != null)
                            {
                                LResults.Add(resultStr);
                                this.Items.Add(new()
                                {
                                    Name = matchedKey.SelectedSubkey.ToText(),
                                    Value = trimValues == false ? resultStr.Substring(matchedKey.SelectedSubkey.ToText().Length) : resultStr.Substring(matchedKey.SelectedSubkey.ToText().Length).Trim()
                                });
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
        public static Key[] GetKeys(string[] keys, string subkeySeparator)
        {
            List<Key> LKeys = new();
            for (int i = 0; i < keys.Length; i++)
            {
                LKeys.Add(new(keys[i].ToBytes(), subkeySeparator.ToBytes()));
            }
            return (LKeys.ToArray());
        }
    }
}

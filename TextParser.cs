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
        public bool Enabled { get; set; }
        public byte[][] Subkeys { get; set; }
        public string[] SubkeysStr {
            get => Subkeys.ToTextArray();
        }
        public byte[] SelectedSubkey { get; set; }
        public string SelectedSubkeyStr
        {
            get => SelectedSubkey.ToText();
        }
        public Key(byte[] keys, byte[] subkeySeparator, bool enabled)
        {
            this.Subkeys = keys.Split(subkeySeparator, true);
            this.SelectedSubkey = new byte[0];
            this.Enabled = enabled;
        }
        public Key(byte[] keys, byte[][] subkeySeparators, bool enabled)
        {
            byte[] matchedSeparator = new byte[0];
            this.Subkeys = keys.Split(subkeySeparators, true, out matchedSeparator);
            this.SelectedSubkey = new byte[0];
            this.Enabled = enabled;
        }
    }
    
    public class KeyList
    {
        Key[] Keys { get; set; }
        public KeyList(Key[] keys)
        {
            this.Keys = keys;
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
            Key resultKey = new(new byte[0], new byte[0], false);

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
        public static Key[] GetKeys(string[] keys, string subkeySeparator, string tagToDisable)
        {
            List<Key> LKeys = new();
            bool enabled;

            for (int i = 0; i < keys.Length; i++)
            {
                byte[] keyBytes = keys[i].ToBytes();
                byte[] keyBytesModified; 
                byte[] tagToDisableBytes = tagToDisable.ToBytes();
                int sourceStart = tagToDisableBytes.Length;

                if (keyBytes.IsEqual(0, tagToDisableBytes))
                {
                    keyBytesModified = new byte[keyBytes.Length - tagToDisableBytes.Length];
                    sourceStart = tagToDisableBytes.Length;
                    enabled = false;
                }
                else
                {
                    keyBytesModified = new byte[keyBytes.Length];
                    sourceStart = 0;
                    enabled = true;
                }

                Array.Copy(keyBytes, sourceStart, keyBytesModified, 0, keyBytesModified.Length);

                LKeys.Add(new(keyBytesModified, new byte[][] { subkeySeparator.ToBytes() }, enabled));
            }
            return (LKeys.ToArray());
        }
    }
}

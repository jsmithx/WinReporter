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
    public struct TextItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TextKey
    {
        public bool Enabled { get; set; }
        public byte[][] Subkeys { get; set; }
        public string[] SubkeysStr {
            get => Subkeys.ToTextArray();
        }
        public static TextKey Empty { get => new TextKey(new byte[0], new byte[0], false); }

        public byte[] SelectedSubkey { get; set; }
        public string SelectedSubkeyStr
        {
            get => SelectedSubkey.ToText();
        }
        public TextKey(byte[] keys, byte[] subkeySeparator, bool enabled)
        {
            this.Subkeys = keys.Split(subkeySeparator, true);
            this.SelectedSubkey = new byte[0];
            this.Enabled = enabled;
        }
        public TextKey(byte[] keys, byte[][] subkeySeparators, bool enabled)
        {
            byte[] matchedSeparator = new byte[0];
            this.Subkeys = keys.Split(subkeySeparators, true, out matchedSeparator);
            this.SelectedSubkey = new byte[0];
            this.Enabled = enabled;
        }
    }
    
    public class KeyList
    {
        TextKey[] TextKeys { get; set; }
        public KeyList(TextKey[] textKeys)
        {
            this.TextKeys = textKeys;
        }
    }
        

    public class TextParser
    {
        public List<TextItem> TextItems;
        public TextParser(ref byte[] dataSource, TextKey[] textKeys)
        {
            this.TextItems = new();
            this.Parse(ref dataSource, textKeys, false);
        }
        private void Parse(ref byte[] dataSource, TextKey[] textKeys, bool trimValues)
        {
            List<TextKey> LKeys = new(textKeys);
            List<string> LResults = new();
            int resultPos = -1;
            TextKey resultKey = TextKey.Empty;

            int pos = 0;
            while (pos < dataSource.Length)
            {
                TextKey matchedKey;

                if (dataSource.IsLetterOrDigit(pos - 1) == false && dataSource.IsEqual(pos, LKeys.ToArray(), out matchedKey) == true)
                {
                    if (resultPos > -1)
                    {
                        string? resultStr = dataSource.SelectTextRange(resultPos, pos - resultPos);
                        if (resultStr != null && resultKey.Enabled == true)
                        {
                            LResults.Add(resultStr);
                            this.TextItems.Add(new()
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
                            if (resultStr != null && resultKey.Enabled == true)
                            {
                                LResults.Add(resultStr);
                                this.TextItems.Add(new()
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
        public static TextKey[] GetKeys(string[] keys, string subkeySeparator, string tagToDisable)
        {
            List<TextKey> LKeys = new();
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

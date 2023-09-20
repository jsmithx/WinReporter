using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinReporter
{
    public class TextObjectItem
    {
        public string Name { get; set; }
        public object? Value { get; set; }
        public TextObjectItem(string name, object? value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
    public class TextObject : KeyedCollection<string, TextObject>
    {
        private byte[] TextData { get; set; }
        public string Name { get; set; }
        public List<TextObjectItem> TextObjectItems;
        public TextObject(byte[] textData)
        {
            this.TextData = textData;
            this.TextObjectItems = new();
            this.Name = string.Empty;
            this.Parse(this.TextData);
        }
        private TextObject? Parse(byte[] data)
        {
            int pos = 0;
            bool escapeChar = false;
            int bracketBalance = 0;
            int bracketStart = -1;
            int bracketEnd = -1;
            int itemStart = -1;
            int itemEnd = -1;
            string itemName = string.Empty;
            object? itemValue = null;

            byte[] openBracket = new byte[] { Convert.ToByte('<'), Convert.ToByte('<') };
            byte[] closeBracket = new byte[] { Convert.ToByte('>'), Convert.ToByte('>') };

            while (pos < data.Length)
            {
                if (data[pos] == '\\' && escapeChar == false)
                {
                    escapeChar = true;
                }
                else
                {
                    if (escapeChar == true) { escapeChar = false; }

                    if (data.IsEqual(pos, openBracket))
                    {
                        if (bracketBalance == 0)
                        {
                            bracketStart = pos;
                        }
                        bracketBalance++;
                        pos += openBracket.Length - 1;
                    }
                    else if (data.IsEqual(pos,closeBracket))
                    {
                        bracketBalance--;
                        if (bracketBalance == 0)
                        {
                            bracketEnd = pos;
                        }
                        pos += closeBracket.Length - 1;
                    }

                    if (bracketBalance == 0)
                    {
                        this.TextObjectItems.Add(this.Parse(data.SelectByteRange(bracketStart, bracketEnd - bracketStart)));
                    }

                    if (data[pos] == '/')
                    {
                        if (itemName == string.Empty)
                        {
                            if (itemStart < 0)
                            {
                                itemStart = pos;
                            }
                            else
                            {
                                itemEnd = pos;
                                itemName = data.SelectTextRange(itemStart, itemEnd - itemStart);
                                
                                itemStart = -1;
                                itemEnd = -1;
                            }
                        }
                        else
                        {
                            if (itemStart < 0)
                            {
                                itemStart = pos;
                            }
                            else
                            {
                                itemEnd = pos;
                                itemValue = data.SelectByteRange(itemStart, itemEnd - itemStart);
                                string itemValueStr = ((byte[])itemValue).ToText();
                                this.TextObjectItems.Add(new(itemName, this.Parse((byte[])itemValue)));

                                itemStart = -1;
                                itemEnd = -1;
                            }
                        }
                    }
                }
            }
            return (null);
        }

        protected override string GetKeyForItem(TextObject item)
        {
            return (item.Name);
        }
    }
}

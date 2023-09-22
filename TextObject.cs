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

        private bool NextToken(int pos, byte[] data, out byte[] token)
        {

            string dataStr = data.ToText();

            int objectBracketBalance = 0;
            int objectBracketStart = -1;
            int objectBracketEnd = -1;
            int itemStart = -1;
            int itemEnd = -1;

            //itemName = new byte[0];
            //itemValue = new byte[0];
            token = new byte[0];

            byte[] openObjectBracket = "<<".ToBytes();
            byte[] closeObjectBracket = ">>".ToBytes();
            byte[][] objectBrackets = new byte[][] { openObjectBracket, closeObjectBracket };

            byte[] openParenthesisBracket = "(".ToBytes();
            byte[] closeParenthesisBracket = ")".ToBytes();
            
            byte[] openSquareBracket = "[".ToBytes();
            byte[] closeSquareBracket = "]".ToBytes();

            byte[] openCurlyBracket = "{".ToBytes();
            byte[] closeCurlyBracket = "}".ToBytes();

            byte[] controlSlash = "/".ToBytes();
            byte[] escapeChar = "\\".ToBytes();

            byte[][] tokens = new byte[][] {
                controlSlash,
                openObjectBracket, //closeObjectBracket,
                openParenthesisBracket, //closeParenthesisBracket,
                openSquareBracket, //closeSquareBracket,
                openCurlyBracket, //closeCurlyBracket,
            };
            
            short bytePerChar = 2; //unicode length is 2 bytes per char

            byte[] matchedToken;
            int startPos = data.IndexOf(pos, tokens, out matchedToken);
            if (matchedToken.IsEqual(openObjectBracket))
            {
                objectBracketBalance++;

                byte[] matchedObjectBracket;
                pos = data.IndexOf(pos + matchedToken.Length, objectBrackets, out matchedObjectBracket);
                while (pos > -1)
                {
                    string matchedObjectBracketStr = matchedObjectBracket.ToText();
                    if (matchedObjectBracket.IsEqual(openObjectBracket))
                    {
                        objectBracketBalance++;
                    }
                    else if (matchedObjectBracket.IsEqual(closeObjectBracket))
                    {
                        objectBracketBalance--;
                    }
                    if (objectBracketBalance == 0)
                    {
                        byte[] item;
                        //item = data.SelectByteRange(startPos + openObjectBracket.Length, pos - startPos - openObjectBracket.Length);
                        item = data.SelectByteRange(startPos, pos - startPos + matchedObjectBracket.Length);
                        string itemStr = item.ToText();
                        token = item;
                        return (true);
                    }
                    pos = data.IndexOf(pos + matchedObjectBracket.Length, objectBrackets, out matchedObjectBracket);
                }
            }
            else
            {
                pos = data.IndexOf(pos + matchedToken.Length, tokens, out matchedToken);
                byte[] item = data.SelectByteRange(startPos, pos - startPos);
                string itemStr = item.ToText();
                token = item;
                return (true);
            }

            return (false);
        }
        private TextObject? Parse(byte[] data)
        {
            string dataStr = data.ToText();
            string itemName = string.Empty;
            object? itemValue = null;
            byte[] item;

            int pos = 0;

            while(this.NextToken(pos, data, out item))
            {
                string itemStr = item.ToText();
                if (data.IsLeftEqual(pos, "<<".ToBytes()) == true)
                {
                    return (this.Parse(item.SelectByteRange("<<".ToBytes().Length, item.Length - ">>".ToBytes().Length)));
                }
                else
                {
                    if (itemName == string.Empty)
                    {
                        itemName = item.ToText();
                    }
                    else
                    {
                        itemValue = item;
                    }
                }
                pos += item.Length;
            }
            return (this);
        }

        protected override string GetKeyForItem(TextObject item)
        {
            return (item.Name);
        }
    }
}

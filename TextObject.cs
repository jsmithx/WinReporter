using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinReporter
{
    public struct TextObjectDelimiters
    {
        public byte[] OpenObjectBracket;
        public byte[] CloseObjectBracket;
        public byte[][] ObjectBrackets;

        public byte[] OpenParenthesisBracket;
        public byte[] CloseParenthesisBracket;

        public byte[] OpenSquareBracket;
        public byte[] CloseSquareBracket;

        public byte[] OpenCurlyBracket;
        public byte[] CloseCurlyBracket;

        public byte[] ControlSlash;
        public byte[] EscapeChar;
        public TextObjectDelimiters()
        {
            this.OpenObjectBracket = "<<".ToBytes();
            this.CloseObjectBracket = ">>".ToBytes();
            this.ObjectBrackets = new byte[][] { OpenObjectBracket, CloseObjectBracket };

            this.OpenParenthesisBracket = "(".ToBytes();
            this.CloseParenthesisBracket = ")".ToBytes();

            this.OpenSquareBracket = "[".ToBytes();
            this.CloseSquareBracket = "]".ToBytes();

            this.OpenCurlyBracket = "{".ToBytes();
            this.CloseCurlyBracket = "}".ToBytes();

            this.ControlSlash = "/".ToBytes();
            this.EscapeChar = "\\".ToBytes();
        }
    }
    /*
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
    */

    public class TextObject : TextNodeCollection
    {
        private byte[] TextData { get; set; }
        public string Name { get; set; }
        public TextObjectDelimiters TextDelimiters { get; set; }
        public TextObject(TextNode? parent, byte[] textData) : base(parent)
        {
            this.TextDelimiters = new();
            this.TextData = textData;
            this.Name = string.Empty;
            this.Parse(this.TextData);
        }

        private int NextToken(int pos, byte[] data, out byte[] matchedDelimiter)
        {
            string dataStr = data.ToText();
            matchedDelimiter = new byte[0];

            byte[][] tokens = new byte[][] {
                this.TextDelimiters.ControlSlash,
                this.TextDelimiters.OpenObjectBracket, this.TextDelimiters.CloseObjectBracket,
                this.TextDelimiters.OpenParenthesisBracket, this.TextDelimiters.CloseParenthesisBracket,
                this.TextDelimiters.OpenSquareBracket, this.TextDelimiters.CloseSquareBracket,
                this.TextDelimiters.OpenCurlyBracket, this.TextDelimiters.CloseCurlyBracket,
            };
            
            return(data.IndexOf(pos, tokens, out matchedDelimiter));
        }
        private TextObject? Parse(byte[] data)
        {
            string dataStr = data.ToText();
            string itemName = string.Empty;
            object? itemValue = null;
            byte[] item;

            int objectBalanceBracket = 0;
            byte[] matchedDelimiter = new byte[0];
            int pos = this.NextToken(0, data, out matchedDelimiter);
            short bytePerChar = 2; //unicode length is 2 bytes per char

            int nextPos = pos;
            while (pos > -1 && pos < data.Length)
            {
                if (data.IsLeftEqual(nextPos, this.TextDelimiters.OpenObjectBracket))
                {
                    objectBalanceBracket++;
                }
                else if (data.IsLeftEqual(nextPos, this.TextDelimiters.CloseObjectBracket))
                {
                    objectBalanceBracket--;
                }

                int oldNextPos = nextPos;
                byte[] oldMatchedDelimiter = matchedDelimiter;
                nextPos = this.NextToken(nextPos + matchedDelimiter.Length, data, out matchedDelimiter);

                item = data.SelectByteRange(pos, nextPos - pos);
                string itemStr = item.ToText(); 
                
                if (objectBalanceBracket == 0)
                {
                    if (nextPos < 0)
                    {
                        nextPos = oldNextPos + oldMatchedDelimiter.Length;
                    }
                    item = data.SelectByteRange(pos, nextPos - pos);
                    itemStr = item.ToText();

                    if (itemName.Length == 0)
                    {
                        if(data.IsLeftEqual(pos, this.TextDelimiters.OpenObjectBracket))
                        {
                            item = item.SelectByteRange(
                                pos + this.TextDelimiters.OpenObjectBracket.Length,
                                nextPos - pos - this.TextDelimiters.OpenObjectBracket.Length - this.TextDelimiters.CloseObjectBracket.Length
                                );
                            itemStr = item.ToText();
                            this.Parse(item);
                            return(this);
                        }
                        else
                        {
                            itemName = item.ToText();
                        }
                    }
                    else if (itemName.Length > 0)
                    {
                        itemValue = item;
                        
                        this.Add(itemName, this.Parse((byte[])itemValue));

                        itemName = string.Empty;
                        itemValue = null;
                    }

                    pos = nextPos;
                }
                
            }

            return(this);
        }
    }
}

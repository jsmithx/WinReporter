using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
    public static class ArrayExtension
    {
        public static string[] ToStringArray(this byte[][] source)
        {
            List<string> chunks = new();
            for (int i = 0; i < source.Length; i++)
            {
                string chunk = Encoding.Default.GetString(source[i]);
                chunks.Add(chunk);
            }
            return (chunks.ToArray());
        }
        public static byte[][] Split(this byte[] source, byte[] separator, bool removeEmptyEntries = false)
        {
            List<byte[]> chunks = new();
            int posEnd = 0;
            int posStart = 0;

            while(posEnd < source.Length)
            {
                bool isEqual = source.IsEqual(posEnd, separator);
                bool isEndPos = posEnd == source.Length - 1;
                bool isEndPosDelimiter = false;

                if (isEqual || isEndPos)
                {
                    if (isEndPos)
                    {
                        if(!isEqual)
                        {
                            posEnd++;
                        }
                        else
                        {
                            isEndPosDelimiter = true;
                        }
                    }
                    byte[] chunk = new byte[posEnd - posStart];
                    Array.Copy(source, posStart, chunk, 0, chunk.Length);
                    string chunkStr = Encoding.Default.GetString(chunk);
                    if (chunk.Length > 0 || removeEmptyEntries == false)
                    {
                        chunks.Add(chunk);
                    }

                    if (isEndPosDelimiter == true && removeEmptyEntries == false)
                    {
                        chunks.Add(new byte[0] { });
                    }
                    posEnd += separator.Length;
                    posStart = posEnd;
                }
                else
                {
                    posEnd++;
                }
            }
            return (chunks.ToArray());
        }
        public static bool IsEqual(this byte[] source, int sourcePos, byte[] key)
        {
            bool isValid = true;
            int n = 0;
            for (int i = sourcePos; i < source.Length; i++)
            {
                if (n < key.Length)
                {
                    if (key[n] != source[i])
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
        public static bool IsEqual(this byte[] source, int sourcePos, string[] keys, out string matchedKey)
        {
            matchedKey = string.Empty;

            bool isValid = false;

            for (int k = 0; k < keys.Length; k++)
            {
                byte[] key = Encoding.Default.GetBytes(keys[k]);

                isValid = source.IsEqual(sourcePos, key);
                if (isValid == true)
                {
                    matchedKey = keys[k];
                    break;
                }
            }

            return (isValid);
        }

        public static bool IsLetterOrDigit(this byte[] source, int sourcePos)
        {
            if (sourcePos > -1 && sourcePos < source.Length)
            {
                if (char.IsLetterOrDigit(Convert.ToChar(source[sourcePos])))
                {
                    return (true);
                }
            }
            return (false);
        }
    }

    public class Key
    {
        public byte[][] Subkeys { get; set; }
        public Key(byte[] keys, byte[] subkeySeparator)
        {
            this.Subkeys = keys.Split(subkeySeparator, true);
        }
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
                if (this.DataSource.IsLetterOrDigit(pos - 1) == false && this.DataSource.IsEqual(pos, LKeys.ToArray(), out matchedKey) == true)
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
        
        
    }
}

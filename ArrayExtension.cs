﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WinReporter
{
    public static class ArrayExtension
    {
        public static Encoding TextEncoder = Encoding.Unicode;
        public static string[] ToTextArray(this byte[][] source)
        {
            List<string> chunks = new();
            for (int i = 0; i < source.Length; i++)
            {
                string chunk = source[i].ToText();
                chunks.Add(chunk);
            }
            return (chunks.ToArray());
        }
        public static string ToText(this byte[] source)
        {
            return(TextEncoder.GetString(source));
        }
        public static byte[] EncodeSpecialChars(this byte[] source, char[] specialChars)
        {
            int charsCount = source.CountSpecialChars(specialChars);

            byte[] escapeChar = BitConverter.GetBytes('/');
            int escapeCharSize = escapeChar.Length;
            byte[] EncodedSource = new byte[source.Length + (charsCount * escapeCharSize)];

            int i = 0;
            int encodedPos = 0;
            char matchedChar;
            string EncodedSourceStr;
            while (i < source.Length)
            {
                if (source.IsEqual(i, specialChars, out matchedChar) == true)
                {
                    byte[] matchedCharBytes = BitConverter.GetBytes(matchedChar);

                    Array.Copy(escapeChar, 0, EncodedSource, encodedPos, escapeChar.Length);
                    encodedPos += escapeChar.Length;
                    EncodedSourceStr = EncodedSource.ToText();
                    Array.Copy(matchedCharBytes, 0, EncodedSource, encodedPos, matchedCharBytes.Length);
                    EncodedSourceStr = EncodedSource.ToText();
                    encodedPos += matchedCharBytes.Length;

                    i += matchedCharBytes.Length;
                }
                else
                {
                    EncodedSource[encodedPos] = source[i];
                    EncodedSourceStr = EncodedSource.ToText();
                    encodedPos++;
                    i++;
                }
            }
            return (EncodedSource);
        }
        public static byte[] DecodeSpecialChars(this byte[] source, char[] specialChars)
        {
            string sourceStr = source.ToText();
            char escapeChar = '/';
            byte[] escapeCharBytes = BitConverter.GetBytes(escapeChar);
            int escapeCharSize = escapeCharBytes.Length;

            int escapeCharCount = source.CountEscapeChars(escapeChar);

            byte[] DecodedSource = new byte[source.Length - (escapeCharCount * escapeCharSize)];
            int i = 0;
            int decodedPos = 0;
            while (i < source.Length)
            {
                if (source.IsLeftEqual(i, escapeChar) == true)
                {
                    i += escapeCharBytes.Length;

                    if (i < source.Length)
                    {
                        Array.Copy(source, i, DecodedSource, decodedPos, 2);
                        string DecodedSourceStr = DecodedSource.ToText();
                        decodedPos += 2;
                        i += 2;
                    }
                }
                else
                {
                    DecodedSource[decodedPos] = source[i];
                    string DecodedSourceStr = DecodedSource.ToText();
                    decodedPos++;
                    i++;
                }
            }

            return (DecodedSource);
        }
        public static int CountSpecialChars(this byte[] source, char[] specialChars)
        {
            int count = 0;
            int i = 0;
            char escapeChar = '/';
            byte[] slashBytes = BitConverter.GetBytes(escapeChar);
            int slashSize = slashBytes.Length;

            char matchedChar;
            while (i < source.Length)
            {
                if (source.IsEqual(i, specialChars, out matchedChar) == true)
                {
                    byte[] charByteArray = BitConverter.GetBytes(matchedChar);
                    count++;
                    i += charByteArray.Length;
                }
                else
                {
                    i++;
                }
            }
            return (count);
        }
        public static int CountEscapeChars(this byte[] source, char escapeChar)
        {
            int count = 0;
            int i = 0;
            byte[] escapeCharBytes = BitConverter.GetBytes(escapeChar);
            int escapeCharSize = escapeCharBytes.Length;

            while (i < source.Length)
            {
                if (source.IsLeftEqual(i, escapeChar) == true)
                {
                    count++;
                    i += escapeCharBytes.Length;

                    i += 2;
                }
                else
                {
                    i++;
                }
            }
            return (count);
        }

        public static byte[] ToBytes(this string source)
        {
            return(TextEncoder.GetBytes(source));
        }
        public static byte[] ToBytes(this object? source)
        {
            string? result = Convert.ToString(source);

            return (result == null ? new byte[0] : result.ToBytes());
        }
        public static byte[] SelectByteRange(this byte[] source, int start, int length)
        {
            return (source.Skip(start).Take(length).ToArray());
        }
        public static string SelectTextRange(this byte[] source, int start, int length)
        {
            return (source.SelectByteRange(start, length).ToText());
        }
        public static byte[][] Split(this byte[] source, byte[][] separators, bool removeEmptyEntries, out byte[] matchedSeparator)
        {
            matchedSeparator = new byte[0];

            List<byte[]> chunks = new();
            int posEnd = 0;
            int posStart = 0;

            while (posEnd < source.Length)
            {
                bool isEqual = source.IsLeftEqual(posEnd, separators, out matchedSeparator);
                bool isEndPos = posEnd == source.Length - 1;
                bool isEndPosDelimiter = false;

                if (isEqual || isEndPos)
                {
                    if (isEndPos)
                    {
                        if (!isEqual)
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
                    string chunkStr = chunk.ToText();
                    if (chunk.Length > 0 || removeEmptyEntries == false)
                    {
                        chunks.Add(chunk);
                    }

                    if (isEndPosDelimiter == true && removeEmptyEntries == false)
                    {
                        chunks.Add(new byte[0] { });
                    }
                    posEnd += matchedSeparator.Length;
                    posStart = posEnd;
                }
                else
                {
                    posEnd++;
                }
            }
            return (chunks.ToArray());
        }

        public static byte[][] Split(this byte[] source, byte[] separator, bool removeEmptyEntries)
        {
            return (source.Split(new byte[][] { separator }, removeEmptyEntries, out _));
        }
        
        public static bool IsEqual(this byte[] source, int sourcePos, char[] keyChars, out char matchedChar)
        {
            matchedChar = '\u0000';

            for (int i = 0; i < keyChars.Length; i++)
            {
                if (source.IsLeftEqual(sourcePos, keyChars[i]) == true)
                {
                    matchedChar = keyChars[i];
                    return (true);
                }
            }

            return (false);
        }
        public static bool IsLeftEqual(this byte[] source, int sourcePos, char keyChar)
        {
            return(source.IsLeftEqual(sourcePos, BitConverter.GetBytes(keyChar)));
        }
        public static bool IsEqual(this byte[] source, byte[] key)
        {
            return (source.Length == key.Length && source.IsLeftEqual(0, key));
        }
        public static bool IsLeftEqual(this byte[] source, byte[] key)
        {
            return (source.IsLeftEqual(0, key));

        }
        public static bool IsLeftEqual(this byte[] source, int sourcePos, byte[] key)
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
        public static bool IsLeftEqual(this byte[] source, int sourcePos, byte[][] keys, out byte[] matchedKey)
        {
            matchedKey = new byte[0];
            bool isValid = true;

            for (int k = 0; k < keys.Length; k++)
            {
                isValid = true;
                int n = 0;

                for (int i = sourcePos; i < source.Length; i++)
                {
                    if (n < keys[k].Length)
                    {
                        if (keys[k][n] != source[i])
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
                if (isValid == true)
                {
                    matchedKey = keys[k];
                    break;
                }
            }

            return (isValid);
        }
        public static bool IsLeftEqual(this byte[] source, int sourcePos, TextKey[] keys, out TextKey matchedTextKey)
        {
            matchedTextKey = TextKey.Empty;

            bool isValid = false;

            for (int k = 0; k < keys.Length; k++)
            {
                for (int n = 0; n < keys[k].Subkeys.Length; n++)
                {
                    byte[] key = keys[k].Subkeys[n];
                    string keyStr = key.ToText();

                    isValid = source.IsLeftEqual(sourcePos, key);
                    if (isValid == true)
                    {
                        matchedTextKey = keys[k];
                        matchedTextKey.SelectedSubkey = keys[k].Subkeys[n];
                        break;
                    }
                }
                if (isValid == true)
                {
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

        public static int IndexOf(this byte[] source, int startPos, byte[] key)
        {
            return (source.IndexOf(startPos, new byte[][] { key }, out _));
        }
        public static int IndexOf(this byte[] source, int startPos, byte[][] keys, out byte[] matchedKey)
        {
            int pos = startPos;
            matchedKey = new byte[0];

            while(pos < source.Length)
            {
                int k = 0;
                while (k < keys.Length)
                {
                    if (source.IsLeftEqual(pos, keys[k]))
                    {
                        matchedKey = keys[k];
                        return (pos);
                    }
                    k++;
                }
                pos++;
            }

            return (-1);
        }
    }
}

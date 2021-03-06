using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using KTrie;

namespace FileArchiver
{
    public static class Lzw
    {
        public static readonly Encoding Encoding;

        private static readonly char[] Chars;
        
        private const int MaxDictionarySize = ushort.MaxValue + 1;
        
        static Lzw()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding = Encoding.GetEncoding(1251);
            Chars = Encoding.GetChars(Enumerable.Range(0, 256).Select(n => (byte) n).ToArray());
        }
        
        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH")]
        public static void Compress(FileStream input, FileStream output)
        {
            var dictionary = InitDictionary();
            var str = string.Empty;

            var readBuffer = new char[1];

            using var reader = new StreamReader(new BufferedStream(input), Encoding);
            using var writer = new BinaryWriter(output);
            while (!reader.EndOfStream)
            {
                reader.Read(readBuffer);
                var ch = readBuffer[0];
                if (dictionary.ContainsKey(str + ch))
                    str += ch;
                else
                {
                    writer.Write(dictionary[str]);
                    if (dictionary.Count != MaxDictionarySize)
                        dictionary[str + ch] = (ushort) dictionary.Count;
                    str = ch.ToString();
                }
            }

            writer.Write(dictionary[str]);
        }

        public static void Decompress(FileStream input, FileStream output)
        {
            var inverseDictionary = InitInverseDictionary();
            var entry = string.Empty;


            using var reader = new BinaryReader(new BufferedStream(input));
            using var writer = new StreamWriter(output, Encoding);

            var prevCode = reader.ReadUInt16();
            writer.Write(inverseDictionary[prevCode]);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var currCode = reader.ReadUInt16();
                if (inverseDictionary.ContainsKey(currCode))
                    entry = inverseDictionary[currCode];
                else
                    entry += entry[0];
                writer.Write(entry);
                var ch = entry[0];
                
                if (inverseDictionary.Count != MaxDictionarySize)
                    inverseDictionary[(ushort) inverseDictionary.Count] = inverseDictionary[prevCode] + ch;
                prevCode = currCode;
            }
        }

        private static StringTrie<ushort> InitDictionary()
        {
            var dictionary = new StringTrie<ushort>();
            dictionary.AddRange(Chars.Select((c, i) => new StringEntry<ushort>(c.ToString(), (ushort)i)));
            return dictionary;
        }

        private static Dictionary<ushort, string> InitInverseDictionary()
        {
            var dictionary = new Dictionary<ushort, string>(MaxDictionarySize);
            for (int i = 0; i < Chars.Length; i++)
            {
                dictionary[(ushort) i] = Chars[i].ToString();
            }

            return dictionary;
        }
    }
}
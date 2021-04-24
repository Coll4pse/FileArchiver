using System;
using System.IO;
using System.Linq;
using System.Text;
using KTrie;

namespace FileArchiver
{
    public class Lzw
    {
        public static readonly Encoding Encoding;
        
        private const int WordSize = 2;
        
        private const int MaxDictionarySize = ushort.MaxValue + 1;
        
        private readonly StringTrie<ushort> dictionary = new ();

        static Lzw()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding = Encoding.GetEncoding(1251);
        }
        
        public Lzw()
        {
            InitDictionary(dictionary);
        }

        public void Compress(FileStream input, FileStream output)
        {
            var str = string.Empty;
            char ch;

            var readBuffer = new char[1];

            using var reader = new StreamReader(new BufferedStream(input), Encoding);
            using var writer = new BinaryWriter(output);
            while (!reader.EndOfStream)
            {
                reader.Read(readBuffer);
                ch = readBuffer[0];
                if (dictionary.ContainsKey(str + ch))
                    str += ch;
                else
                {
                    writer.Write(BitConverter.GetBytes(dictionary[str]));
                    dictionary[str + ch] = (ushort)dictionary.Count;
                    str = ch.ToString();
                }
            }
            
            writer.Write(BitConverter.GetBytes(dictionary[str]));
        }

        private static void InitDictionary(StringTrie<ushort> dictionary)
        {
            var chars = Lzw.Encoding.GetChars(Enumerable.Range(0, 256).Select(n => (byte) n).ToArray());
            dictionary.AddRange(chars.Select((c, i) => new StringEntry<ushort>(c.ToString(), (ushort)i)));
        }
    }
}
using System;
using System.IO;
using FileArchiver;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        private static readonly string Path = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\")) + @"\Files\";
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("bib", "bib.lzw", "bib.dec", TestName = "On small file (bib)")]
        [TestCase("paper1", "paper1.lzw", "paper1.dec", TestName = "On small file (paper1)")]
        [TestCase("paper2", "paper2.lzw", "paper2.dec", TestName = "On small file (paper2)")]
        [TestCase("russian", "russian.lzw", "russian.dec", TestName = "On small file (russian)")]
        [TestCase("news", "news.lzw", "news.dec", TestName = "On medium file (news)")]
        [TestCase("book1", "book1.lzw", "book1.dec", TestName = "On medium file (book1)")]
        [TestCase("book2", "book2.lzw", "book2.dec", TestName = "On medium file (book2)")]
        [TestCase("E.coli", "E.coli.lzw", "E.coli.dec", TestName = "On large file (E.coli)")]
        [TestCase("bible", "bible.lzw", "bible.dec", TestName = "On large file (bible)")]
        [TestCase("enwik8", "enwik8.lzw", "enwik8.dec", TestName = "On extra large file (enwik8)")]
        public void IsLzwCorrect(string name, string encodeName, string decodeName)
        {
            var (source, encoded, decoded) = InitFiles(name, encodeName, decodeName);
            try
            {
                Lzw.Compress(source.OpenRead(), encoded.OpenWrite());
                Assert.Greater(source.Length, encoded.Length);
                Lzw.Decompress(encoded.OpenRead(), decoded.OpenWrite());
                Assert.That(source.Length, Is.EqualTo(decoded.Length));
                Assert.That(CompareFiles(source.OpenRead(), decoded.OpenRead()), Is.True);
            }
            finally
            {
                encoded.Delete();
                decoded.Delete();
            }
        }

        private static (FileInfo sourceFile, FileInfo encodedFile, FileInfo decodedFile) InitFiles(
            string sourceName,
            string encodedName,
            string decodedName)
        {
            if (File.Exists(Path + encodedName)) File.Delete(Path + encodedName);
            if (File.Exists(Path + decodedName)) File.Delete(Path + decodedName);
            return (new FileInfo(Path + sourceName), new FileInfo(Path + encodedName),
                new FileInfo(Path + decodedName));
        }

        private static bool CompareFiles(Stream f1, Stream f2)
        {
            if (f1.Length != f2.Length) return false;
            using var reader1 = new StreamReader(new BufferedStream(f1));
            using var reader2 = new StreamReader(new BufferedStream(f2));
            while (!reader1.EndOfStream)
            {
                if (reader1.Read() != reader2.Read()) return false;
            }

            return true;
        }
    }
}
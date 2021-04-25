using System;
using System.Collections.Generic;
using System.IO;

namespace FileArchiver
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Type command: compress <path>, decompress <path>, exit");
                    Console.ResetColor();
                    var commands = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    switch (commands[0].ToLowerInvariant())
                    {
                        case "compress":
                        {
                            var (path, name, ext) = GetFullPathAndName(commands[1]);
                            var sourceFile = new FileInfo(path + name + ext);
                            var encodedFile = new FileInfo(path + name + ".lzw");
                            using var source = sourceFile.Open(FileMode.Open, FileAccess.Read);
                            using var encoded = encodedFile.Open(FileMode.CreateNew, FileAccess.Write);
                            Console.WriteLine("Compressing...");
                            Lzw.Compress(source, encoded);
                            Console.WriteLine("Complete");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Coefficient of compression: {(double)sourceFile.Length / encodedFile.Length:F2}");
                            break;
                        }
                        case "decompress":
                        {
                            var (path, name, ext) = GetFullPathAndName(commands[1]);
                            using var source = File.Open(path + name + ext, FileMode.Open, FileAccess.Read);
                            using var decoded = File.Open(path + name + "_decoded.txt", FileMode.CreateNew,
                                FileAccess.Write);
                            Console.WriteLine("Decompressing...");
                            Lzw.Decompress(source, decoded);
                            Console.WriteLine("Complete");
                            break;
                        }
                        case "exit":
                        {
                            Environment.Exit(0);
                            break;
                        }
                        default: throw new FormatException("Incorrect command");
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Incorrect command");
                    Console.ResetColor();
                }
                catch (FormatException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
                catch (FileNotFoundException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("File not found:" + e.FileName);
                    Console.ResetColor();
                }
                catch (IOException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
                catch (KeyNotFoundException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("File cannot be decompressed");
                    Console.ResetColor();
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ooops! Unknow error");
                    Console.ResetColor();
                }
            }
        }

        private static (string fullPath, string name, string extension) GetFullPathAndName(string path)
        {
            var extension = path.Contains('.') ? path[path.LastIndexOf('.')..] : string.Empty;

            if (!path.Contains('\\') && !path.Contains('/'))
            {
                var name = path.Contains('.') ? path[..path.LastIndexOf('.')] : path;
                return (Directory.GetCurrentDirectory() + '\\', name, extension);
            }
            else
            {
                var name = path.Contains('.')
                    ? path[(Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\')) + 1)..path.LastIndexOf('.')]
                    : path[(Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\')) + 1)..];
                return (path[..(Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\')) + 1)], name, extension);
            }
        }
    }
}
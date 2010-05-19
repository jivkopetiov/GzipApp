using System;
using System.IO;
using System.Linq;
using NDesk.Options;

namespace GzipApp
{
    public class EntryPoint
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                PrintUsage();
                return;
            }

            int compressionLevel = 5;
            bool decompress = false;
            bool forceCompression = false;

            var p = new OptionSet() {
   	            { "l|level=", v => compressionLevel = int.Parse(v) },
                { "d|decompress", v => decompress = v != null },
                { "f|force", v => forceCompression = v != null }
            };
            var extra = p.Parse(args);
            string filePath = extra.First();

            if (compressionLevel < 0 || compressionLevel > 9)
            {
                PrintError("Compression level must be between 0 and 9");
                return;
            }

            if (!File.Exists(filePath))
            {
                PrintError("File doesn't exist: " + filePath);
                return;
            }

            try
            {
                if (decompress)
                    Decompress(filePath);
                else
                    Compress(compressionLevel, forceCompression, filePath);
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }
        }

        private static void Compress(int compressionLevel, bool forceCompression, string filePath)
        {
            if (ZipService.IsFileGzipCompressed(filePath) && !forceCompression)
            {
                throw new InvalidOperationException("File seems to be already gzip compressed. Cannot compress again. Use the -f switch to force compression.");
            }

            string tempPath = Path.GetTempFileName();
            ZipService.GzipCompressFile(filePath, tempPath, compressionLevel);
            File.Copy(tempPath, filePath, true);
            File.Delete(tempPath);
            PrintSuccess("Successfully compressed file " + filePath);
        }

        private static void Decompress(string filePath)
        {
            if (!ZipService.IsFileGzipCompressed(filePath))
                throw new InvalidOperationException("File is not gzip compressed. Cannot decompress.");

            string tempPath = Path.GetTempFileName();
            ZipService.GzipDecompressFile(filePath, tempPath);
            File.Copy(tempPath, filePath, true);
            File.Delete(tempPath);
            PrintSuccess("Successfully decompressed file " + filePath);
        }

        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("gzip <filepath> [-l=<level>] [-d] [-f]");
            Console.WriteLine("-filepath - the path of the file to do gzip on.");
            Console.WriteLine("-l - the gzip compression level. Possible values from 0 to 9.");
            Console.WriteLine("-f - forces compression even if file is already compressed.");
            Console.WriteLine("-d - decompresses the file.");
        }
    }
}

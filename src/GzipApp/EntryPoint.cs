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
            //args = new[] { @"D:\do", "-d" };

            if (args == null || args.Length == 0)
            {
                PrintUsage();
                return;
            }

            int compressionLevel = 5;
            bool decompress = false;
            bool forceCompression = false;

            var parameterOptions = new OptionSet() {
   	            { "l|level=", v => compressionLevel = int.Parse(v) },
                { "d|decompress", v => decompress = v != null },
                { "f|force", v => forceCompression = v != null }
            };
            var parameters = parameterOptions.Parse(args);
            string path = parameters.First();

            if (compressionLevel < 0 || compressionLevel > 9)
            {
                PrintError("Compression level must be between 0 and 9");
                return;
            }

            if (File.Exists(path))
            {
                ProcessFile(compressionLevel, decompress, forceCompression, path);
            }
            else if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    ProcessFile(compressionLevel, decompress, forceCompression, file);
                }
            }
            else if (path.Contains("*"))
            {
                string directory = path.Substring(0, path.LastIndexOf("\\") - 1);
                if (!Directory.Exists(directory))
                {
                    PrintError("Directory doesn't exist: " + path);
                    return;
                }

                string searchPattern = path.Substring(path.LastIndexOf("\\") + 1);
                foreach (string file in Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    ProcessFile(compressionLevel, decompress, forceCompression, path);
                }
            }
            else
            {
                PrintError("File / Directory do not exist: " + path);
                return;
            }
        }

        private static void ProcessFile(int compressionLevel, bool decompress, bool forceCompression, string filePath)
        {
            try
            {
                if (decompress)
                    Decompress(filePath);
                else
                    Compress(compressionLevel, forceCompression, filePath);
            }
            catch (Exception ex)
            {
                PrintError("Failed to process file " + filePath + " :: " + ex.Message);
            }
        }

        private static void Compress(int compressionLevel, bool forceCompression, string filePath)
        {
            if (ZipService.IsFileGzipCompressed(filePath) && !forceCompression)
            {
                throw new InvalidOperationException("File seems to already be gzip compressed. Cannot compress again. Use the -f switch to force compression.");
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

            if (filePath.EndsWith(".gz"))
            {
                string outputPath = filePath.Substring(0, filePath.Length - 3);
                File.Copy(tempPath, outputPath, true);
                File.Delete(filePath);
            }
            else
            {
                File.Copy(tempPath, filePath, true);
            }

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
            Console.WriteLine("gzip <path> [-l=<level>] [-d] [-f]");
            Console.WriteLine("-path - the path of the file / directory to do gzip on. also supported is wildcard file matching with *");
            Console.WriteLine("-l - the gzip compression level. Possible values from 0 to 9.");
            Console.WriteLine("-f - forces compression even if file is already compressed.");
            Console.WriteLine("-d - decompresses the file.");
        }
    }
}

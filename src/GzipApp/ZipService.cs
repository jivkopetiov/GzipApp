using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using System;

namespace GzipApp
{
    public static class ZipService
    {
        private const int _defaultBufferSize = 2048;

        public static void GzipCompressFile(string inputPath, string outputPath, int compressionLevel)
        {
            using (var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    using (var gzipStream = new GZipOutputStream(outputStream))
                    {
                        gzipStream.SetLevel(compressionLevel);
                        int size;
                        byte[] data = new byte[_defaultBufferSize];
                        do
                        {
                            size = inputStream.Read(data, 0, data.Length);
                            gzipStream.Write(data, 0, size);
                        } while (size > 0);
                    }
                }
            }
        }

        public static void GzipDecompressFile(string inputFile, string outputFile)
        {
            using (var fileStreamIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var fileStreamOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    using (var zipInStream = new GZipInputStream(fileStreamIn))
                    {
                        int size;
                        byte[] buffer = new byte[_defaultBufferSize];
                        do
                        {
                            size = zipInStream.Read(buffer, 0, buffer.Length);
                            fileStreamOut.Write(buffer, 0, size);
                        } while (size > 0);
                    }
                }
            }
        }

        public static bool IsFileGzipCompressed(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("Must specify a filepath");

            return FileSignatureMatches(filePath, "1F-8B-08");
        }

        public static bool IsFilePkzipCompressed(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("Must specify a filepath");

            return FileSignatureMatches(filePath, "50-4B-03-04");
        }

        public static bool FileSignatureMatches(string filePath, string signature)
        {
            int signatureSize = signature.Split('-', ' ').Length;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (stream.Length < signatureSize)
                    return false;

                byte[] signatureBytes = new byte[signatureSize];
                int bytesRequired = signatureSize;
                int index = 0;

                while (bytesRequired > 0)
                {
                    int bytesRead = stream.Read(signatureBytes, index, bytesRequired);
                    bytesRequired -= bytesRead;
                    index += bytesRead;
                }

                string actualSignature = BitConverter.ToString(signatureBytes);

                if (actualSignature == signature)
                    return true;
                else
                    return false;
            }
        }
    }
}

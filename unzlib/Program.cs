using System.IO.Compression;
using xayrga;
using xayrga.console;
using xayrga.byteglider;

namespace unzlib
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("unzlib - simple zlib decompressor\nCreated by Xayrga!\nhttps://ko-fi.com/xayrga/");
            ConsoleAppHelper.ArgumentList = args;

            var inFile = ConsoleAppHelper.assertArg(0, "Input File");
            ConsoleAppHelper.assert(File.Exists(inFile), $"{inFile} not found.");
            var outFile = ConsoleAppHelper.tryArg(1, "Output File");
            var data = File.ReadAllBytes(inFile);
            var inflated = inflate(data);
            if  (outFile==null)
            {
                var file = Path.GetFileNameWithoutExtension(inFile);
                outFile = $"{file}{getExtensionFromMagic(inflated)}";
            }                 
         
            File.WriteAllBytes(outFile,inflated);
        }

        public static string getExtensionFromMagic(byte[] data)
        {

            using (var ms = new MemoryStream(data))
            using (var br = new bgReader(ms))
            {
                var fourcc = br.ReadInt32();

                switch (fourcc)
                {
                    case 0x20534444:
                        return ".dds";
                    case 0x46464952:
                        return ".wav";
                    default:
                        return ".dat";
                }

            }
        }

        public static byte[] inflate(byte[] data)
        {
            var w = new MemoryStream(data);
            var outStream = new MemoryStream();
            var zlib = new ZLibStream(w, CompressionMode.Decompress);
            zlib.CopyTo(outStream);
            return outStream.ToArray();
        }
    }
}

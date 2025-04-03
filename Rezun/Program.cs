using System;
using System.IO;
using System.Text;

using xayrga.byteglider;
using xayrga.console;
using xayrga.console;
using System.IO.Compression;

//using System.IO.Compression;
namespace Rezun
{

    /*
     0x00 0x00000000
    0x04 int32 entryCount
    0x08 int32 stringTableSize 
    0x0C 0

    entryStructure 
    +0x00 dataBlockOffset
    +0x04 stringTableOffset
    +0x08 dataBlockLength
    +0x0C typeIdentifier
    +0x0E typeIdentifier2

    +(entryCount * 0x10) stringTable 
    +((entryCount * 0x10) + stringTableSize) dataBlock;
*/
     

    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Rezun - Rez Infinite File Extractor\nCreated by Xayrga!\nhttps://ko-fi.com/xayrga/");
            
#if DEBUG
            ConsoleAppHelper.ArgumentList = new string[]
            {
                @"E:\STEAMSLOW\steamapps\common\Rez Infinite\REZ\DATA\RezVR_snd3.bnk"
            };
#else 
            ConsoleAppHelper.ArgumentList = args;  
#endif
            var inFile = ConsoleAppHelper.assertArg(0, "Input File");

            ConsoleAppHelper.assert(File.Exists(inFile),$"{inFile} not found.");
            var outFolder = ConsoleAppHelper.tryArg(1, "Output Folder");

            if (outFolder==null)
                outFolder = $"{Path.GetFileNameWithoutExtension(inFile)}_out";

            Console.WriteLine($"Output folder: {outFolder}");

            Directory.CreateDirectory(outFolder);

            var inStream = File.OpenRead(inFile);
            var reader = new bgReader(inStream);

            var datFile = RezDataFile.createFromStream(reader);

            for (int i = 0; i < datFile.Entries.Count; i++) {
                var nFile = datFile.Entries[i];
                var fileName = Path.GetFileNameWithoutExtension(nFile.Name);
                

                Console.Write($"Extracting {nFile.Name} ... ");
                File.WriteAllBytes($"{outFolder}/{fileName}.raw", nFile.Data);
                try
                {                                 
                    var outData = nFile.Data;                   

                    if (nFile.Flags!=0x0000000) 
                        outData = inflate(nFile.Data);
               
                    var Ext = Path.GetExtension(nFile.Name);           
                    if (Ext==".zip")
                        Ext = getExtensionFromMagic(outData);



                    File.WriteAllBytes($"{outFolder}/{fileName}{Ext}", outData);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("OK");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" ({Ext})");

                } catch (Exception E)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("FAIL");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.WriteLine();
            }
        }


        public static string getExtensionFromMagic(byte[] data)
        {

            using (var ms =  new MemoryStream(data))
            using (var br = new bgReader(ms)) {
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

using System;
using System.IO;
using System.Text;
using Ionic.Zlib;
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

            var inFile = File.OpenRead(args[0]);
            var fileReader = new BinaryReader(inFile);

            fileReader.ReadInt32(); // Skip first 4 bytes 0 
            var fileCount = fileReader.ReadInt32();
            var stringTableSize = fileReader.ReadInt32();
            var stringTableOffset = (fileCount * 0x10) + 0x10;
            var dataBlockOffset = 0x10 + (fileCount * 0x10) + stringTableSize;
            fileReader.ReadInt32(); // 0  

            var anchor = 0x10; // File entries start at 0x10; 

            for (int i = 0; i < fileCount; i++)
            {
                var fileEntryOffset = fileReader.ReadInt32();
                var fileEntrySTOffset = fileReader.ReadInt32();
                var fileDataLength = fileReader.ReadInt32();
                var fileUNK1 = fileReader.ReadInt16();
                var fileUNK2 = fileReader.ReadInt16();

                var FTOAnchor = fileReader.BaseStream.Position;

                fileReader.BaseStream.Position = stringTableOffset + fileEntrySTOffset;
                var fileName = readCString(fileReader);

                fileReader.BaseStream.Position = dataBlockOffset + fileEntryOffset;
                var fileData = fileReader.ReadBytes(fileDataLength);
                var binString = Convert.ToString(fileUNK1, 2);
                var binString2 = Convert.ToString(fileUNK1, 2);
                var decompData = new byte[0];
                var fail = false;
                if (fileUNK1==0 & fileUNK2==0)
                {
                    decompData = fileData;
                    File.WriteAllBytes($"out/{fileName.Substring(0, fileName.Length )}", decompData);
                    Console.WriteLine($"Extraction success: {fileName} 0x{fileUNK1:X} 0x{fileUNK2:X}\t\t\t0b{binString}\t0b{binString2}");
                    fileReader.BaseStream.Position = FTOAnchor;
                    continue;
                }
          
                try
                {
                    decompData = inflate(fileData);
                } catch (Exception E)
                {
                    Console.WriteLine($"Extraction failure: {fileName} 0x{fileUNK1:X} 0x{fileUNK2:X}\t\t\t0b{binString}\t0b{binString2}");
                    decompData = fileData;
                    fail = true;
                }
                File.WriteAllBytes($"out/{fileName.Substring(0,fileName.Length - 4)}", decompData);
           
                if (!fail)
                {
                    Console.WriteLine($"Extraction success: {fileName} 0x{fileUNK1:X} 0x{fileUNK2:X}\t\t\t0b{binString}\t0b{binString2}");
                }
                fileReader.BaseStream.Position = FTOAnchor;
            }

        }

        public static string readCString(BinaryReader aafRead)
        {
            var ofs = aafRead.BaseStream.Position;
            byte nextbyte;
            byte[] name = new byte[0xFF];

            int count = 0;
            while ((nextbyte = aafRead.ReadByte()) != 0x00)
            {
                name[count] = nextbyte;
                count++;
            }
            return Encoding.ASCII.GetString(name, 0, count);
        }


        public static byte[] inflate(byte[] data)
        {


            const int size = 65535;
            int current_size = 0;

            using (ZlibStream stream = new ZlibStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                

                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        //Console.WriteLine("GZ: {0}", count);
                        current_size += count;
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);

                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}

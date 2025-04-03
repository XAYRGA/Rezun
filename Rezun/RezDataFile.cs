using System;
using System.Collections.Generic;
using System.Text;
using xayrga.byteglider;

namespace Rezun
{
    public enum RezFileFlags
    {

    }

    public class RezDataFile
    {
        public List<RezFileEntry> Entries = new();

        private void readFromStream(bgReader br)
        {
            br.Skip(4);
            var fileCount = br.ReadInt32();
            var stringtableSize = br.ReadInt32();
            br.Skip(4);
                     
            for (int i = 0; i < fileCount; i++)
                Entries.Add(RezFileEntry.createFromStream(br));

            var stringtablePosition = (fileCount + 1) * 0x10;
            var datablockPosition = (stringtablePosition + stringtableSize);

            for (int i = 0; i < Entries.Count; i++) 
                Entries[i].loadData(br,stringtablePosition, datablockPosition);
        }

        public static RezDataFile createFromStream(bgReader read)
        {
            var rdf = new RezDataFile();
            rdf.readFromStream(read);
            return rdf;
        }
    }

    public class RezFileEntry
    {
        public string Name;
        public byte[] Data;
        public int Flags;

        private int dataBlockOffset;
        private int stringTableOffset;
        private int dataBlockLength;

        private void readFromStream(bgReader read)
        {
            dataBlockOffset = read.ReadInt32();
            stringTableOffset = read.ReadInt32();
            dataBlockLength = read.ReadInt32();
            read.PushAnchor();  
            Flags = read.ReadInt32();
        }

        public void loadData(bgReader br, int strtable, int datablck)
        {
            br.Seek(datablck + dataBlockOffset);
            Data = br.ReadBytes(dataBlockLength);
            br.Seek(strtable + stringTableOffset);
            Name = br.ReadTerminatedString();
        }

        public static RezFileEntry createFromStream(bgReader br)
        {
            var entry = new RezFileEntry();
            entry.readFromStream(br);
            return entry;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezun
{
    public class RezDataFile
    {
        public int entryCount;
        public int stringtableLength;
        public RezFileEntry[] fileEntries;
    }

    public class RezFileEntry
    {
        public int dataBlockOffset;
        public int stringTableOffset;
        public int dataBlockLength; 

    }
}

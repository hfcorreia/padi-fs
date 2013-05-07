using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    [Serializable]
    public class FileAccessCounter
    {

        public int ReadCounter { get; set; }
        public int ReadVersionCounter { get; set; }
        public int WriteCounter { get; set; }
        public string FileName { get; set; }

        public FileAccessCounter() { }
        public FileAccessCounter(string fileName)
        {
            ReadCounter = 0;
            ReadVersionCounter = 0;
            WriteCounter = 0;
            FileName = fileName;
        }

        public string ToString()
        {
            return "AccessCounter (F,R,RV,W): " + "(" + FileName + "," + ReadCounter + "," + ReadVersionCounter + "," + WriteCounter + ")"; 
        }

    }
}

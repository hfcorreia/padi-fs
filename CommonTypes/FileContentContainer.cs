using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class FileContentContainer
    {
        private List<File> Files { get; set; }
        private int nextFreePosition;
        private int capacity;

        //receives the maximum number of FileMetada's that can be saved
        //and the client in witch they are saved
        public FileContentContainer(int capacity)
        {
            this.capacity = capacity;
            this.nextFreePosition = 0;
            this.Files = new List<File>();
            for (int i = 0; i < capacity; ++i) {
                Files.Add(null);
            }
        }

        //receives a new filemetadata,
        //inserts the file in a roundRobin manner and returns 
        //the position in witch the fileMetadata was saved
        public int addFileContent(File file)
        {

            int filePosition = nextFreePosition;

            Files[filePosition] = file;

            nextFreePosition = ( nextFreePosition + 1 ) % capacity;
            
            return filePosition;
        }

        //receives a position in the structure and returns
        //the file metadata that is saved in that position.
        public File getFileContent(int position)
        {
            return Files[position];
        }

        public File getFileContent(string filename)
        {
            foreach (File file in Files)
            {
                if (file!=null && file.FileName!=null && file.FileName.Equals(filename))
                {
                    return file;
                }
            }
            return null;
        }

        public bool containsFileContent(string filename)
        {
            return getFileContent(filename) != null;
        }

        public void removeFileContent(string filename)
        {
            int position = getPositionOf(filename);
            if (position > -1 && position < capacity)
            {
                Files[position] = null;
            }
        }

        public int getPositionOf(string filename)
        {
            for (int i = 0; i < capacity; ++i)
            {
                if (Files[i] != null &&  Files[i].FileName!=null && Files[i].FileName.Equals(filename))
                {
                    return i;
                }
            }
            return -1;
        }

        public List<string> getAllFileNames()
        {
            List<string> result = new List<string>();
            foreach (File file in Files)
            {
                if (file != null && file.FileName != null)
                {
                    result.Add(file.FileName);
                }
            }
            return result;
        }
    }
}

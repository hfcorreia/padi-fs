using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class FileContentContainer
    {
        private List<File> Files { get; set; }
        private int writePosition;
        private int capacity;

        //receives the maximum number of FileMetada's that can be saved
        //and the client in witch they are saved
        public FileContentContainer(int capacity)
        {
            this.capacity = capacity;
            this.writePosition = 0;
            this.Files = new List<File>();
            for (int i = 0; i < capacity; ++i) {
                Files.Add(null);
            }
        }

        //receives a new filemetadata,
        //inserts the file in a roundRobin manner and returns 
        //the position in witch the fileMetadata was saved
        public void addFileContent(File file)
        {
            //int filePosition = nextFreePosition;
            int filePosition = findRegisterPosition();

            Files[filePosition] = file;

            //nextFreePosition = ( nextFreePosition + 1 ) % capacity;
            
           // return filePosition;
        }

        private int findRegisterPosition()
        {
            for (int i = 0; i < Files.Count; ++i)
            {
                if (hasNullContent(i))
                {
                    return i;
                }
            }

            int position = writePosition;
            writePosition = (writePosition + 1) % capacity;
            return position;
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

        public List<string> getAllFileContentAsString()
        {
            List<string> result = new List<string>();
            foreach (File file in Files)
            {
                if (file != null)
                {
                    string fileContentAsString = System.Text.Encoding.UTF8.GetString(file.Content);
                    result.Add(fileContentAsString);
                }
            }
            return result;
        }

        public bool hasNullContent(int position)
        {
            return position < 0 || position > capacity || Files[position] == null;
        }

        public void setFileContent(int position, File fileContent)
        {
            Files[position] = fileContent;
        }
    }
}

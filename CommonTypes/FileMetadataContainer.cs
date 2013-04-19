using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class FileMetadataContainer
    {
        private List<FileMetadata> Metadata {get; set;}
        private int capacity;
        int writePosition;

        //and the client in witch they are saved
        public FileMetadataContainer(int capacity)
        {
            this.capacity = capacity;
            this.Metadata = new List<FileMetadata>();
            this.writePosition = 0;
            for (int i = 0; i < capacity; ++i) {
                Metadata.Add(null);
            }
        }

        //receives a new filemetadata,
        //inserts the file in a roundRobin manner and returns 
        //the position in witch the fileMetadata was saved
        public int addFileMetadata(FileMetadata fileMetadata){
            int fileMetadataPosition = (containsFileMetadata(fileMetadata.FileName)) ? getPositionOf(fileMetadata.FileName) : findFirstFreePosition();

            fileMetadata.IsOpen = true;
            Metadata[fileMetadataPosition] = fileMetadata;
            
            return fileMetadataPosition;
        }

        private int findFirstFreePosition()
        {
            for (int i=0; i < Metadata.Count; ++i)
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
        public FileMetadata getFileMetadata(int position) {
            return Metadata[position];
        }

        public FileMetadata getFileMetadata(string filename)
        {
            foreach (FileMetadata fileMetadata in Metadata) {
                if (fileMetadata!=null && fileMetadata.FileName!=null && fileMetadata.FileName.Equals(filename)) {
                    return fileMetadata;   
                }
            }
            return null;
        }

        public bool containsFileMetadata(string filename) 
        {
            return getFileMetadata(filename) != null;
        }

        public void removeFileMetadata(string filename)
        {
            int position = getPositionOf(filename);
            if (position > -1 && position < capacity) {
                Metadata[position] = null ;
            }
        }

        public int getPositionOf(string filename) {
            for (int i = 0; i < capacity && i < Metadata.Count ; ++i) {
                if (Metadata[i] != null && Metadata[i].FileName!=null && Metadata[i].FileName.Equals(filename))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool hasNullContent(int position) 
        {
            return position < 0 || position > capacity || Metadata[position] == null;
        }

        public List<string> getAllFileNames() {
            List<string> result = new List<string>();
            foreach (FileMetadata fileMetadata in Metadata)
            {
                if (fileMetadata != null && fileMetadata.FileName != null)
                {
                    result.Add(fileMetadata.FileName);
                }
            }
            return result;
        }

        public void printMetaDataContainer()
        {
            Console.WriteLine("printing metaDataContainer info:");
            foreach (FileMetadata file in Metadata)
            {
                Console.WriteLine(file);
            }
        }

        public void markOpenFile(String filename)
        {
            if (containsFileMetadata(filename)) 
            {
                getFileMetadata(filename).IsOpen = true;
            }
        }

        public void markClosedFile(String filename)
        {
            if (containsFileMetadata(filename))
            {
                getFileMetadata(filename).IsOpen = false;
            }
        }
    }
}

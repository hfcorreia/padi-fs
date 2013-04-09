using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class FileMetadataContainer
    {
        private List<FileMetadata> Metadata {get; set;}
        //private int nextFreePosition;
        private int capacity;

        //and the client in witch they are saved
        public FileMetadataContainer(int capacity)
        {
            this.capacity = capacity;
            //this.nextFreePosition = 0;
            this.Metadata = new List<FileMetadata>();
            for (int i = 0; i < capacity; ++i) {
                Metadata.Add(null);
            }
        }

        //receives a new filemetadata,
        //inserts the file in a roundRobin manner and returns 
        //the position in witch the fileMetadata was saved
        public int addFileMetadata(FileMetadata fileMetadata){

            //int fileMetadataPosition = nextFreePosition;
            int fileMetadataPosition = findFirstFreePosition();

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
            return -1;
        }

        //receives a position in the structure and returns
        //the file metadata that is saved in that position.
        public FileMetadata getFileMetadata(int position) {
            return Metadata[position];
        }

        public FileMetadata getFileMetadata(string filename)
        {
            foreach (FileMetadata fileMetadata in Metadata) {
                Console.WriteLine("percorrer files : " + ((fileMetadata != null) ? fileMetadata.FileName : "Null"));
                if (fileMetadata!=null && fileMetadata.FileName!=null && fileMetadata.FileName.Equals(filename)) {
                    Console.WriteLine("encontrei!! ");
                    return fileMetadata;   
                }
            }
            Console.WriteLine("vou retornar null");
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
    }
}

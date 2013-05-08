using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes.Exceptions;
using CommonTypes;
using System.Threading;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataDeleteOperation : MetaDataOperation
    {

        public String ClientID {get;set;}
        public String Filename { get; set; }

        public FileMetadata Result { get; set; }

        public MetaDataDeleteOperation() { }

        public MetaDataDeleteOperation(String clientID, string filename)
        {
            ClientID = clientID;
            Filename = filename;
        }

        public override string ToString()
        {
            return "Delete Operation";
        }
        public override void execute(MetaDataServer md)
        {
            if (!md.FileMetadata.ContainsKey(Filename))
            {
            throw new CommonTypes.Exceptions.DeleteFileException("#MDS.delete - File " + Filename + " does not exist");
            }

            md.FileMetadata.Remove(Filename);
            Console.WriteLine("#MDS: Deleted file: " + Filename);
            md.makeCheckpoint();
        }      
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes.Exceptions;
using CommonTypes;
using System.Threading;

namespace MetaDataServer
{
    class MetaDataCloseOperation : MetaDataOperation
    {

        String ClientID {get;set;}
        String Filename { get; set; }

        public FileMetadata Result { get; set; }


        public MetaDataCloseOperation(String clientID, string filename)
        {
            ClientID = clientID;
            Filename = filename;
        }

         public override void execute(MetaDataServer md)
        {
            if (!md.FileMetadata.ContainsKey(Filename))
            {
                throw new CloseFileException("#MDS.close - File " + Filename + " does not exist");
            }

            if (md.FileMetadata.ContainsKey(Filename) && !md.FileMetadata[Filename].Clients.Contains(ClientID))
            {
                //throw new CloseFileException("#MDS.close - File " + filename + " is not open for user " + clientID);
                Console.WriteLine("#MDS.close - File " + Filename + " is allready closed for user " + ClientID);
                return;
            }

            Console.WriteLine("#MDS: closing file " + Filename + "...");
            md.FileMetadata[Filename].Clients.Remove(ClientID);
            md.makeCheckpoint();
        }      
    }
}

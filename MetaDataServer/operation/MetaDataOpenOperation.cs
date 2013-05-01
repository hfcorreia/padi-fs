using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes.Exceptions;
using CommonTypes;
using System.Threading;

namespace MetaDataServer
{
    class MetaDataOpenOperation : MetaDataOperation
    {

        String ClientID {get;set;}
        String Filename { get; set; }

        public FileMetadata Result { get; set; }


        public MetaDataOpenOperation(String clientID, string filename)
        {
            ClientID = clientID;
            Filename = filename;
        }

         public override void execute(MetaDataServer md)
        {
            if (!md.FileMetadata.ContainsKey(Filename))
            {
                throw new OpenFileException("#MDS.open - File " + Filename + " does not exist");
            }
            else if (md.FileMetadata[Filename].Clients.Contains(ClientID))
            {
                Console.WriteLine("#MDS.open - File " + Filename + " is already open for user " + ClientID);
                Result = md.FileMetadata[Filename];
                return;
            }
            else
            {
                Console.WriteLine("#MDS: opening file: " + Filename);
                
                md.FileMetadata[Filename].IsOpen = true;
                md.FileMetadata[Filename].Clients.Add(ClientID);

                md.makeCheckpoint();
                
                Result = md.FileMetadata[Filename];
            }
        }      
    }
}

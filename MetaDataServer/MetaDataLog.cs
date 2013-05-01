using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaDataServer
{
    public class MetaDataLog
    {
        private List<MetaDataOperation> log { get; set; }
        private MetaDataServer MetadataServer { get; set; }
        public int Status { get; set; }
        public int MaxId { get; set; }

        public MetaDataLog(MetaDataServer md)
        {
            log = new List<MetaDataOperation>();
            MetadataServer = md;
            Status = 0;
        }

        public void registerOperation(MetaDataOperation operation)
        {
            lock (typeof(MetaDataLog))
            {
                MaxId++;
            }
            log.Add(operation);
        }



        public void loadLog(String filename)
        {
            //TODO - load the log from a xml file and deserialize
        }

        public void saveLog() 
        { 
            //TODO - SERIALIZE THE LOG IN A XML FILE
        }

        public void incrementStatus() 
        {
            lock (typeof(MetaDataLog))
            {
                Status++;
            }
        }

        public void printLog()
        {
            Console.WriteLine("Log from MD " + MetadataServer.Id + " com " + log.Count + " operacoes registadas");
            foreach(MetaDataOperation op in log)
            {
                Console.WriteLine("Operation: " + op);
            }
        }

    }
}

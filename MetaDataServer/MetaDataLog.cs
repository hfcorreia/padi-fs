using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Xml.Serialization;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataLog
    {
        public List<MetaDataOperation> log { get; set; }
        private MetaDataServer MetadataServer { get; set; }

        public int Status { get; set; }
        public int MaxId { get; set; }

        //private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(MetaDataLog));

        public MetaDataLog() { }

        public void init(MetaDataServer md)
        {
            log = new List<MetaDataOperation>();
            MetadataServer = md;
            MaxId = 0;
            Status = 0;
        }

        public void registerOperation(MetaDataServer md, MetaDataOperation operation)
        {
            Console.WriteLine("#MD " + md + "Log - registerOperation - " + operation);

            md.ReplicationHandler.syncOperation(operation);

            log.Add(operation);

            lock (typeof(MetaDataLog))
            {
                MaxId++;
            }
            
        }



        public MetaDataOperation getOperation(int operationId)
        {
            foreach (MetaDataOperation op in log)
            {
                if (op.OperationId == operationId) {
                    return op;
                }
            }
            return null;
        }

        public void loadLog(String filename)
        {
            //TODO - load the log from a xml file and deserialize
        }

        public void saveLog() 
        {

            //lock (typeof(MetaDataLog))
            //{
            //    //GC.Collect();
            //    //GC.WaitForPendingFinalizers();

            //    String metadataServerId = MetadataServer.Id;
            //    Console.WriteLine("#MDS Log: saving log " + Status + " from MD server " + metadataServerId);

            //    string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId;
            //    Util.createDir(dirName);

            //    System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(MetaDataLog));

            //    System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\log.xml");
            //    writer.Serialize(fileWriter, this);

            //    fileWriter.Close();

            //    Console.WriteLine("#MDS Log: log " + Status + " from MD server " + metadataServerId + " saved");
            //}
        }

        public void incrementStatus() 
        {
            lock (typeof(MetaDataLog))
            {
                Status++;
            }
        }

        public void dump()
        {
            Console.WriteLine("Log from MD " + MetadataServer.Id + " com " + log.Count + " operacoes registadas");
            foreach(MetaDataOperation op in log)
            {
                Console.WriteLine("Operation: " + op);
            }
        }


        public List<MetaDataOperation> getOperationsFrom(int status)
        {
            List<MetaDataOperation> logCopy = new List<MetaDataOperation>(log);
            List<MetaDataOperation> operations = new List<MetaDataOperation>();
            while (status < MaxId)
            {
                operations.Add(getOperation(status));
                status++;
            }
            return operations;
        }

        public void registerOperations(List<MetaDataOperation> list)
        {
            log.AddRange(list);
        }
    }
}

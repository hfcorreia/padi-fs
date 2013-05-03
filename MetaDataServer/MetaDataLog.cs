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
        //public List<MetaDataOperation> log { get; set; }
        public SortedSet<MetaDataOperation> log { get; set; }
        private MetaDataServer MetadataServer { get; set; }

        public int Status { get; set; }
        public int NextId { get; set; }

        //private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(MetaDataLog));

        public MetaDataLog() { }

        public void init(MetaDataServer md)
        {
            log = new SortedSet<MetaDataOperation>(new LogOperationComparer());
            MetadataServer = md;
            NextId = 0;
            Status = 0;
        }

        public void registerOperation(MetaDataServer md, MetaDataOperation operation)
        {
            Console.WriteLine("#MD " + md + "Log - registerOperation - " + operation);
            if (md == null || operation == null)
            {
                throw new ArgumentNullException("registerOperation - expected a MDS and an operation, given [ MDS:" + md + "Operation: " + operation + "]");
            }

            int operationId;

            lock (typeof(MetaDataLog))
            {
                operationId = NextId++;
            }

            if (md.ReplicationHandler.IsMaster)
            {
                operation.OperationId = operationId;
            }

            log.Add(operation);

            md.ReplicationHandler.syncOperation(operation);
        }

        public MetaDataOperation getOperation(int operationId)
        {
            try
            {
                return log.First(operation => operation.OperationId == operationId);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
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


        public List<MetaDataOperation> getOperationsFrom(int fromStatus)
        {
            if (Status < fromStatus)
            {
                throw new ArgumentNullException("Log.getOperationsFrom - trying to get operations from " + fromStatus + "but the actual status is " + Status);
            }
            List<MetaDataOperation> logCopy = new List<MetaDataOperation>(log);
            List<MetaDataOperation> operations = new List<MetaDataOperation>();
            operations.AddRange(logCopy.FindAll(operation => operation.OperationId >= fromStatus));
            return operations;
        }

        public void registerOperations(MetaDataServer md, List<MetaDataOperation> list)
        {
            foreach (MetaDataOperation operation in list)
            {
                registerOperation(md, operation);
            }
        }

        public class LogOperationComparer : IComparer<MetaDataOperation>
        {
            public int Compare(MetaDataOperation x, MetaDataOperation y)
            {
                return (x == null) ? 1 : 
                        ((y == null) ? -1 : 
                             x.OperationId - y.OperationId);
            }
        }
    }
}

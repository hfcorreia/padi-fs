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
        public int MaxId { get; set; }

        public MetaDataLog(MetaDataServer md)
        {
            log = new List<MetaDataOperation>();
            MetadataServer = md;
            MaxId = 0;
        }

        public void registerOperationAndExecute(MetaDataOperation operation)
        {
            registerOperation(operation);
            operation.execute(MetadataServer);
        }

        public void registerOperation(MetaDataOperation operation)
        {
            int operationId;
            lock (typeof(MetaDataLog))
            {
                operationId = MaxId++;
            }
            operation.OperationId = operationId;
            log.Add(operation);
        }

        public void recover()
        {
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

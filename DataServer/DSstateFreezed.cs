using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Threading;

namespace DataServer
{
    class DSstateFreezed : DSstate
    {
        public ManualResetEvent monitor = new ManualResetEvent(false);

        public DSstateFreezed(DataServer dataServer) : base(dataServer) { }

        //solucao pa bloquear:
        // quando tiver assincrono, bloqueia, quando passa a normal desbloqueia
        // para garantir ordem, guardar queue de locks ordenados?

        public override void write(File file)
        {
            Console.WriteLine("#DS write " + file.FileName + " is waiting because the server is freezed ");
            monitor.WaitOne();
            Console.WriteLine("#DS write " + file.FileName + " will execute ");
            new DSstateNormal(Ds).write(file);

          //  BufferedWriteRequest request = new BufferedWriteRequest(file);
           // Ds.queue(request);
            //so pode devolver depois de unfreeze
        }
        public override File read(string filename) 
        {
          //  BufferedReadRequest request = new BufferedReadRequest(filename);
           // Ds.queue(request);
            //so pode devolver depois de unfreeze
            Console.WriteLine("#DS read " + filename + " is waiting because the server is freezed ");
            monitor.WaitOne();
            Console.WriteLine("#DS read " + filename + " will execute ");
            return new DSstateNormal(Ds).read(filename); 
        }

        public override int readFileVersion(string filename)
        {
           // BufferedReadVersionRequest request = new BufferedReadVersionRequest(filename);
            //Ds.queue(request);
            //so pode devolver depois de unfreeze
            Console.WriteLine("#DS read file version " + filename + " is waiting because the server is freezed ");
            monitor.WaitOne();
            Console.WriteLine("#DS read file version " + filename + " will execute ");
            return new DSstateNormal(Ds).readFileVersion(filename);
        }

        public override void unfreeze()
        {
            //se nao passar primeiro para normal como mandar executar?
            //se passar primeiro para normal nao vao passar pedidos a frente?
            Console.WriteLine("#DS unfreezing...");

            monitor.Set();
            
            Ds.setState(new DSstateNormal(Ds));

            //foreach (BufferedRequest request in Ds.requestsBuffer)
            //{
            //    if (request.GetType() == typeof(BufferedReadRequest))
            //    {
            //        Ds.read(((BufferedReadRequest)request).Filename);
            //    }

            //    if (request.GetType() == typeof(BufferedWriteRequest))
            //    {
            //        Ds.write(((BufferedWriteRequest)request).File);
            //    }

            //    if (request.GetType() == typeof(BufferedReadVersionRequest))
            //    {
            //        Ds.readFileVersion(((BufferedReadVersionRequest)request).Filename);
            //    }
            //}
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace DataServer
{
    class DSstateFreezed : DSstate
    {
        public DSstateFreezed(DataServer dataServer) : base(dataServer) { }

        //solucao pa bloquear:
        // quando tiver assincrono, bloqueia, quando passa a normal desbloqueia
        // para garantir ordem, guardar queue de locks ordenados?

        public override void write(File file) 
        {
            BufferedWriteRequest request = new BufferedWriteRequest(file);
            Ds.queue(request);
            //so pode devolver depois de unfreeze
        }
        public override File read(string filename) 
        {
            BufferedReadRequest request = new BufferedReadRequest(filename);
            Ds.queue(request);
            //so pode devolver depois de unfreeze
            return null; 
        }

        public override int readFileVersion(string filename)
        {
            BufferedReadVersionRequest request = new BufferedReadVersionRequest(filename);
            Ds.queue(request);
            //so pode devolver depois de unfreeze
            return -1;
        }

        public override void unfreeze()
        {
            //se nao passar primeiro para normal como mandar executar?
            //se passar primeiro para normal nao vao passar pedidos a frente?
            Ds.setState(new DSstateNormal(Ds));

            foreach (BufferedRequest request in Ds.requestsBuffer)
            {
                if (request.GetType() == typeof(BufferedReadRequest))
                {
                    Ds.read(((BufferedReadRequest)request).Filename);
                }

                if (request.GetType() == typeof(BufferedWriteRequest))
                {
                    Ds.write(((BufferedWriteRequest)request).File);
                }

                if (request.GetType() == typeof(BufferedReadVersionRequest))
                {
                    Ds.readFileVersion(((BufferedReadVersionRequest)request).Filename);
                }
            }
        }

    }
}

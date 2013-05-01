using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace DataServer
{
    [Serializable]
    public abstract class DSstate
    {
        public DataServer Ds { get; set; }

        public DSstate(DataServer dataServer) 
        {
            Ds = dataServer;
        }

        public abstract void write(File file);
        public abstract File read(string filename);   
        public abstract int readFileVersion(string filename);

        public virtual void fail() 
        {
            Console.WriteLine("#DS failing..");
            Ds.setState(new DSstateFail(Ds));
            Console.WriteLine("#DS failed");
        }

        public virtual void recover()
        {
            Console.WriteLine("#DS recovering..");
            Ds.setState(new DSstateNormal(Ds));
            Console.WriteLine("#DS recovered");
        }

        public virtual void freeze()
        {
            Console.WriteLine("#DS freezing..");
            Ds.setState(new DSstateFreezed(Ds));
            Console.WriteLine("#DS freezed");
        }

        public virtual void unfreeze()
        {
            Console.WriteLine("#DS unfreezing..");
            Ds.setState(new DSstateNormal(Ds));
            Console.WriteLine("#DS unfreezed");
        }
    }
}

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
            Ds.setState(new DSstateFail(Ds));
        }

        public virtual void recover()
        {
            Ds.setState(new DSstateNormal(Ds));
        }

        public virtual void freeze()
        {
            Ds.setState(new DSstateFreezed(Ds));
        }

        public virtual void unfreeze()
        {
            Ds.setState(new DSstateNormal(Ds));
        }
    }
}

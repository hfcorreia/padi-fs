using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using CommonTypes.Exceptions;

namespace DataServer
{
    class DSstateFail : DSstate
    {
        public DSstateFail(DataServer dataServer) : base(dataServer) { }

        public override void write(File file) 
        {
            throw new ServerDownException("The server " + Ds.Id + " is down");
        }
        public override File read(string filename) 
        {
            throw new ServerDownException("The server " + Ds.Id + " is down"); 
        }
        public override int readFileVersion(string filename) 
        { 
            throw new ServerDownException("The server " + Ds.Id + " is down"); 
        }
    }
}

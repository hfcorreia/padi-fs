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
            Console.WriteLine("#DS write " + file.FileName + " refused because the server is failing ");
            throw new ServerDownException("The server " + Ds.Id + " is down");
        }
        public override File read(string filename) 
        {
            Console.WriteLine("#DS read " + filename + " refused because the server is failing ");
            throw new ServerDownException("The server " + Ds.Id + " is down"); 
        }
        public override int readFileVersion(string filename) 
        {
            Console.WriteLine("#DS read file version " + filename + " refused because the server is failing ");
            throw new ServerDownException("The server " + Ds.Id + " is down"); 
        }
    }
}

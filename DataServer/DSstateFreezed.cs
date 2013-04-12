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

        public override void write(File file)
        {
            Console.WriteLine("#DS write " + file.FileName + " is waiting because the server is freezed ");
            monitor.WaitOne();
            Console.WriteLine("#DS write " + file.FileName + " unfreezed");
            new DSstateNormal(Ds).write(file);

        }
        public override File read(string filename)
        {
            Console.WriteLine("#DS read " + filename + " is waiting because the server is freezed ");
            monitor.WaitOne();
            Console.WriteLine("#DS read " + filename + " unfreezed");
            return new DSstateNormal(Ds).read(filename);
        }

        public override int readFileVersion(string filename)
        {
            Console.WriteLine("#DS read file version " + filename + " is waiting because the server is freezed ");
            monitor.WaitOne();
            Console.WriteLine("#DS read file version " + filename + " unfreezed");
            return new DSstateNormal(Ds).readFileVersion(filename);
        }

        public override void unfreeze()
        {
            Console.WriteLine("#DS unfreezing...");
            monitor.Set();
            Ds.setState(new DSstateNormal(Ds));
        }

    }
}

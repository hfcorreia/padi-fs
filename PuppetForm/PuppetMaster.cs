using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;


namespace PuppetForm
{
    static class PuppetMaster
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new ControlBoard());

        }

        public void fail(string process) { }

        public void recover(string process) { }

        public void freeze(string process) { }

        public void unfreeze(string process) { }

        public void create(string process, string filename, int numberDataServers, int readQuorum, int writeQuorum) { }

        public void open(string process, string filename) { }

        public void close(string process, string filename) { }

        public void read(string process, string fileRegister, string semantics, string stringRegister) { }

        public void write(string process, string fileRegister, byte[] byteArrayRegister) { }

        public void write(string process, string fileRegister, string contents) { }

        public void copy(string process, string fileRegister1, string semantics, string fileRegister2, string salt) { }

        public void dump(string process) { }

        public void exeScript(string process, string filename) { }
    }
}

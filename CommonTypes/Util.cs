using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace CommonTypes
{
    public static class Util
    {
        public static void createDir(String dir)
        {
            if (!System.IO.File.Exists(dir))
            {
                lock (typeof(Util))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
            }
        }

        public static void writeFileToDisk(File file, String clientName)
        {
            string dirName = Properties.Resources.TEMP_DIR + "\\" + clientName;
            Util.createDir(dirName);

            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(File));

            System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + getFileName(file.FileName, file.Version));
         
           
                writer.Serialize(fileWriter, file);
                fileWriter.Close();
        }

        public static File readFileFromDisk(String clientName, String name, Int32 version)
        {
            System.Xml.Serialization.XmlSerializer reader =
                      new System.Xml.Serialization.XmlSerializer(typeof(File));

            string dirName = Properties.Resources.TEMP_DIR + "\\" + clientName + getFileName(name, version) ;
            System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);
            
            File file = new File();
     
            file = (File) reader.Deserialize(fileReader);

            fileReader.Close();

            return file;
        }

        public static String getFileName(String name, Int32 version)
        {
            return "\\" + name + "-V" + version + ".xml";
        }


        public static int getNewPort()
        {
            const int PortStartIndex = 1000;
            const int PortEndIndex = 10000;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            IEnumerable<int> query =
                  (from n in tcpEndPoints.OrderBy(n => n.Port)
                   where (n.Port >= PortStartIndex) && (n.Port <= PortEndIndex)
                   select n.Port).ToArray().Distinct();

            int i = PortStartIndex;
            foreach (int p in query)
            {
                if (p != i)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        public static void IgnoreExceptions(this Task task)
        {
            task.ContinueWith(c => { var ignored = c.Exception; });
        }
    }
}

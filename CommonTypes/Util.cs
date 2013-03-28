using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public static class Util
    {
        public static void createDir(String dir)
        {
            if (!System.IO.File.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }

        public static void writeFileToDisk(File file, String clientName)
        {
            string dirName = Properties.Resources.TEMP_DIR + "\\" + clientName;
            Util.createDir(dirName);

            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(File));

            System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + getFileName(file.FileName, file.Version) );
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
            file = (File)reader.Deserialize(fileReader);

            return file;
        }

        public static String getFileName(String name, Int32 version)
        {
            return "\\" + name + "-V" + version + ".xml";
        }

        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Client
{
    class ClientScriptExecutor
    {
        private Client ClientEntity { get; set; }
        private String ScriptName { get; set; }
        private String ScriptLocation { get; set; }

        public ClientScriptExecutor(Client client, String scriptName)
        {
            ClientEntity = client;
            ScriptName = scriptName;
            ScriptLocation = Environment.CurrentDirectory + Properties.Resources.CLIENT_SCRIPT_DIR + ScriptName;
        }

        public void runScriptFile()
        {
            Console.WriteLine("\r\n#Client: Running Script " + ScriptName);
            System.IO.StreamReader fileReader = new System.IO.StreamReader(ScriptLocation);

            String line = fileReader.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("#"))
                {
                    line = fileReader.ReadLine();
                    continue;
                }
                else
                {
                    try
                    {
                        exeScriptCommand(line);
                    }
                    catch (Exception e) 
                    {
                        Console.WriteLine("#ClientScriptExecutor: " + e.Message + "\n\n" + e.StackTrace);
                    }

                    line = fileReader.ReadLine();
                }
            }
            Console.WriteLine("#Client: End of Script " + ScriptName + "\r\n");
            fileReader.Close();
        }

        private void exeScriptCommand(string line)
        {
            Console.Write("## SCRIPT: ");
            String[] newLine = parseLine(line);

            String command = newLine[0];
            String[] newInput = newLine.Skip(2).ToArray<String>();

           switch (command)
            {
                case "OPEN":
                    open(newInput);
                    break;
                case "CLOSE":
                    close(newInput);
                    break;
                case "CREATE":
                    create(newInput);
                    break;
                case "DELETE":
                    delete(newInput);
                    break;
                case "WRITE":
                    write(newInput);
                    break;
                case "READ":
                    read(newInput);
                    break;
                case "DUMP":
                    dump(newInput);
                    break;
                default:
                    Console.WriteLine("#Client: No such command: " + command + "!");
                    break;
            }
        }

        private String[] parseLine(string line)
        {
            Match mp = Regex.Match(line, "\"(.*)\"");
            String newLine = line.Replace(",", "");
            String[] newInput;
            if (mp.Success)
            {
                String argument = "\"" + mp.Groups[1].Value + "\"";
                newLine = Regex.Replace(newLine, "\"(.*)\"", "temp");
                newInput = newLine.Split(' ');
                newInput[newInput.Length - 1] = argument;
                return newInput;
            }

            newInput = newLine.Split(' ');
            return newInput;
        }

        private void dump(string[] input)
        {
            ClientEntity.dump();
        }

        private void read(string[] input)
        {
          ClientEntity.read(Int32.Parse(input[0]), input[1], Int32.Parse(input[2]));
        }

        private void write(string[] input)
        {
          int fileRegisterId = Int32.Parse(input[0]);
          Match mp = Regex.Match(input[1], "\"(.*)\"");
          if (mp.Success)
          { 
              byte[] content = System.Text.Encoding.UTF8.GetBytes(mp.Groups[1].Value);
              ClientEntity.write(fileRegisterId, content);
          }
          else
          {
              ClientEntity.write(fileRegisterId, Int32.Parse(input[1]));
          }
        }

        private void delete(string[] input)
        {
            ClientEntity.delete(input[0]);
        }

        private void create(string[] input)
        {
     
           ClientEntity.create(input[0], Int32.Parse(input[1]), Int32.Parse(input[2]), Int32.Parse(input[3]));
        }

        private void close(string[] input)
        {
            ClientEntity.close(input[0]);
        }

        private void open(string[] input)
        {
            ClientEntity.open(input[0]);
        }
    }
}

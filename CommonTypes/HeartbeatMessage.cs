using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    [Serializable]
    public class HeartbeatMessage
    {
        public int FileCounter { get; set; }
        public string ServerId { get; set; }

        public int ReadCounter { get; set; }
        public int ReadVersionCounter { get; set; }
        public int WriteCounter { get; set; }

        public Dictionary<string, FileAccessCounter> AccessCounter { get; set; }

        public HeartbeatMessage() { }
        public HeartbeatMessage(string serverID, int fileCounter, int readCounter, int readVersionCounter, int writeCounter, Dictionary<string, FileAccessCounter> accessCounter)
        {
            ServerId = serverID;
            FileCounter = fileCounter;
            ReadCounter = readCounter;
            ReadVersionCounter = readVersionCounter;
            WriteCounter = writeCounter;
            AccessCounter = accessCounter;
        }

        public override string ToString()
        {
            return "HeartBeat from server: " + ServerId + " w/ : (F,R,RV,W): (" + FileCounter + "," + ReadCounter + "," + ReadVersionCounter +"," + WriteCounter + ")";
        }

    }


}

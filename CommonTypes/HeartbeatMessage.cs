using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    [Serializable]
    public class HeartbeatMessage
    {
        public string Message { get; set; }
        public string ServerId { get; set; }

        public int ReadCounter { get; set; }
        public int ReadVersionCounter { get; set; }
        public int WriteCounter { get; set; }

        public HeartbeatMessage(string serverID, string msg, int readCounter, int readVersionCounter, int writeCounter)
        {
            ServerId = serverID;
            Message = msg;
            ReadCounter = readCounter;
            ReadVersionCounter = readVersionCounter;
            WriteCounter = writeCounter;
        }

        public override string ToString()
        {
            return "HeartBeat from server: " + ServerId + " w/ message: " + Message + " and (R,RV,W): (" + ReadCounter + "," + ReadVersionCounter +"," + WriteCounter + ")";
        }

    }


}

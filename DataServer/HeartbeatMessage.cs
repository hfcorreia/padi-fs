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

        public HeartbeatMessage(string serverID, string msg)
        {
            ServerId = serverID;
            Message = msg;
        }

        public override string ToString()
        {
            return "HeartBeat from server: " + ServerId + " w/ message: " + Message;
        }

    }


}

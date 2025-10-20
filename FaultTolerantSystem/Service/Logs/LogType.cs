using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Logs
{
    public enum LogType
    {
        Error,
        HeartbeatReceived,
        ClientStarted,
        ClientRegistered,
        ClientDied,
        SentMessage,
    }
}

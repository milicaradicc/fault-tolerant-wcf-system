using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service
{
    public class ClientData
    {
        Guid id { get; set; }
        ClientStatus status { get; set; }
        DateTime lastHeartbeat { get; set; }
        ICallback callback { get; set; }

    }
}
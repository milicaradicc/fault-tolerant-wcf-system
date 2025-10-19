using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service
{
    public class ClientData
    {
        public ClientData(Guid id, ClientStatus status, DateTime lastHeartbeat)
        {
            this.Id=id;
            this.Status=status;
            this.LastHeartbeat=lastHeartbeat;
        }

        public Guid Id { get; set; }
        public ClientStatus Status { get; set; }
        public DateTime LastHeartbeat { get; set; }


    }
}
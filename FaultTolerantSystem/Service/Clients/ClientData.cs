using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Service.Clients
{
    public class ClientData
    {
        public ClientData() {}
        public ClientData(Guid id, ClientStatus status, DateTime lastHeartbeat)
        {
            this.Id=id;
            this.Status=status;
            this.LastHeartbeat=lastHeartbeat;
        }

        [Key]
        public Guid Id { get; set; }
        [Required]
        public ClientStatus Status { get; set; }
        public DateTime LastHeartbeat { get; set; }
    }
}
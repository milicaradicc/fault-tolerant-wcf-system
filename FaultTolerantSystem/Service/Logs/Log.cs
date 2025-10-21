using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.Clients;

namespace Service.Logs
{
    public class Log
    {
        public Log() {}

        public Log(Guid clientId, LogType type, string message)
        {
            ClientId = clientId;
            Type = type;
            Timestamp = DateTime.Now;
            Message = message;

        }

        public Log(int id, Guid clientId, LogType type, DateTime timestamp, string message)
        {
            Id = id;
            ClientId = clientId;
            Type = type;
            Timestamp = timestamp;
            Message = message;
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid? ClientId { get; set; }

        [Required]
        public LogType Type { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        [StringLength(500)]
        public string Message { get; set; }
    }
}

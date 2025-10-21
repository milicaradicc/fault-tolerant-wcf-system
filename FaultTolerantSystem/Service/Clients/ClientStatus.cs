using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Service.Clients
{
    [DataContract]
    public enum ClientStatus
    {
        [EnumMember]
        Running,
        [EnumMember]
        Standby,
        [EnumMember]
        Dead
    }
}
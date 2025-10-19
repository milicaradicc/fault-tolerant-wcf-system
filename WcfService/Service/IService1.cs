using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Service
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        void RegisterClient(string clientId);

        [OperationContract]
        void Heartbeat(string clientId);

        [OperationContract]
        void MarkAsWorking(string clientId);

        [OperationContract]
        void MarkAsStandby(string clientId);

        [OperationContract]
        string GetClientStatus(string clientId);
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using Service.Clients;

namespace Service
{

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IService1
    {

        [OperationContract]
        Guid RegisterClient();

        [OperationContract]
        void SendHeartbeat(Guid clientId);

        [OperationContract]
        void SetStatus(Guid clientId, ClientStatus status);

        [OperationContract(IsOneWay = true)]
        void SendMessage(Guid fromClientId, Guid toClientId, string encryptedMessage);

    }

    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void OnStart();

        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(Guid fromClientId, string decryptedMessage);
    }

}

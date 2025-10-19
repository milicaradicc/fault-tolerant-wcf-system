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
        Guid RegisterClient();

        [OperationContract]
        void SendHeartbeat(Guid clientId);
    }

    public interface ICallback
    {
       // [OperationContract(IsOneWay = true)]
        
    }



}

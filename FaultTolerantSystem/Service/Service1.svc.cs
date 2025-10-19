using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;

namespace Service
{
    public class Service1 : IService1
    {
        private Dictionary<Guid, ClientData> _clients = new Dictionary<Guid, ClientData>();
        private readonly Dictionary<Guid, ICallback> _callbacks = new Dictionary<Guid, ICallback>();

        public Service1() {
            System.Diagnostics.Debug.WriteLine($"Created service");
        }
        public Guid RegisterClient()
        {
            var callback = OperationContext.Current.GetCallbackChannel<ICallback>();
            Guid clientId = Guid.NewGuid();

            ClientData clientData = new ClientData(clientId, ClientStatus.Standby, DateTime.Now);

            _clients.Add(clientId, clientData);
            //TODO save to database
            _callbacks.Add(clientId, callback);

            System.Diagnostics.Debug.WriteLine($"Registered client {clientId}");

            return clientId;
        }

        public void SendHeartbeat(Guid clientId)
        {
            ClientData clientData = _clients[clientId];
            if (clientData == null)
                return; //TODO decide if exeption should be thrown
            clientData.LastHeartbeat = DateTime.Now;
            _clients[clientId] = clientData; //TODO save to database

            System.Diagnostics.Debug.WriteLine($"Recieved heartbeat from client {clientId}");
        }
    }
}

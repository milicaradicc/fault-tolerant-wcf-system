using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Timers;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        private Dictionary<Guid, ClientData> _clients = new Dictionary<Guid, ClientData>();
        private readonly Dictionary<Guid, ICallback> _callbacks = new Dictionary<Guid, ICallback>();
        private Timer _checkTimer;
        private const int CheckInterval = 5000;
        private const int ClientInactivityTreshold = 30000;
        private const int MaxRunningClients = 2;
        

        public Service1() {
            _checkTimer = new Timer(CheckInterval);
            _checkTimer.Elapsed+=CheckClients;
            _checkTimer.Start();
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

            StartClients();

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

        public void SetStatus(Guid clientId, ClientStatus status)
        {
            throw new NotImplementedException();
        }

        public void StartClients() 
        {
            var runningClients = _clients.Values.Where(c => c.Status == ClientStatus.Running).ToList();
            var standbyClients = _clients.Values.Where(c => c.Status == ClientStatus.Standby).ToList();

            while (runningClients.Count() < MaxRunningClients)
            {
                var client = standbyClients.FirstOrDefault();
                if (client == null) return;
                runningClients.Append(client);
                standbyClients.Remove(client);
                client.Status = ClientStatus.Running;
                //TODO update in database
                _clients[client.Id] = client;
                _callbacks[client.Id].OnStart();
                System.Diagnostics.Debug.WriteLine($"Started client {client.Id}");
            }
        }

        public void CheckClients(object sender, ElapsedEventArgs e) 
        {
            bool clientDied = false;

            foreach (KeyValuePair<Guid, ClientData> kvp in _clients)
            {
                Guid clientId = kvp.Key;
                ClientData client = kvp.Value;

                var timeSinceLastHeartbeat = DateTime.Now - client.LastHeartbeat;

                if (timeSinceLastHeartbeat.TotalMilliseconds > ClientInactivityTreshold && client.Status != ClientStatus.Dead)
                {
                    System.Diagnostics.Debug.WriteLine($"Client {clientId} died.");

                    client.Status = ClientStatus.Dead;
                    clientDied = true;
                }
            }

            if(clientDied)
                StartClients();
        }
        public void SendMessage(Guid fromClientId, Guid toClientId, string encryptedMessage)
        {
            if (_callbacks.ContainsKey(toClientId))
            {
                _callbacks[toClientId].OnMessageReceived(fromClientId, encryptedMessage);
                System.Diagnostics.Debug.WriteLine($"Message from {fromClientId} sent to {toClientId}");
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Timers;
using Service.Clients;
using Service.Logs;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        private readonly ConcurrentDictionary<Guid, ICallback> _callbacks = new ConcurrentDictionary<Guid, ICallback>();
        private readonly Timer _checkTimer;
        private const int CheckInterval = 5000;
        private const int ClientInactivityTreshold = 30000;
        private const int MaxRunningClients = 2;
        private static readonly object StartLock = new object();


        public Service1() {
            ServiceDbContext.ResetDatabase();

            _checkTimer = new Timer(CheckInterval);
            _checkTimer.Elapsed+=CheckClients;
            _checkTimer.Start();
        }

        public Guid RegisterClient()
        {
            var callback = OperationContext.Current.GetCallbackChannel<ICallback>();
            Guid clientId = Guid.NewGuid();

            ClientData clientData = new ClientData(clientId, ClientStatus.Standby, DateTime.Now);

            ClientRepository.AddClient(clientData);
            _callbacks.TryAdd(clientId, callback);

            LogRepository.AddLog(new Log(clientId, LogType.ClientRegistered, $"Registered client {clientId}"));
            System.Diagnostics.Debug.WriteLine($"Registered client {clientId}");

            StartClients();

            return clientId;
        }

        public void SendHeartbeat(Guid clientId)
        {
            ClientData clientData = ClientRepository.GetClient(clientId);
            if (clientData == null)
            {
                LogRepository.AddLog(new Log(clientId, LogType.Error, $"Received heartbeat from unknown client {clientId}"));
                System.Diagnostics.Debug.WriteLine($"Received heartbeat from unknown client {clientId}");
                return;
            }
            clientData.LastHeartbeat = DateTime.Now;
            ClientRepository.UpdateClient(clientId, clientData);

            LogRepository.AddLog(new Log(clientId, LogType.HeartbeatReceived, $"Received heartbeat from client {clientId}"));
            System.Diagnostics.Debug.WriteLine($"Received heartbeat from client {clientId}");
        }

        public void SetStatus(Guid clientId, ClientStatus status)
        {
            throw new NotImplementedException();
        }

        public void StartClients()
        {
            lock (StartLock)
            {
                var runningClients = ClientRepository.GetClientsWithStatus(ClientStatus.Running);
                var standbyClients = ClientRepository.GetClientsWithStatus(ClientStatus.Standby);

                while (runningClients.Count() < MaxRunningClients)
                {
                    var client = standbyClients.FirstOrDefault();
                    if (client == null) return;
                    runningClients.Append(client);
                    standbyClients.Remove(client);
                    client.Status = ClientStatus.Running;
                    ClientRepository.UpdateClient(client.Id, client);
                    _callbacks[client.Id].OnStart();

                    LogRepository.AddLog(new Log(client.Id, LogType.ClientStarted, $"Started client {client.Id}"));
                    System.Diagnostics.Debug.WriteLine($"Started client {client.Id}");
                }
            }
        }

        public void CheckClients(object sender, ElapsedEventArgs e) 
        {
            bool clientDied = false;

            foreach (ClientData client in ClientRepository.GetAllInactiveClients(ClientInactivityTreshold))
            {
                LogRepository.AddLog(new Log(client.Id, LogType.ClientDied, $"Client {client.Id} died."));
                System.Diagnostics.Debug.WriteLine($"Client {client.Id} died.");

                client.Status = ClientStatus.Dead;
                ClientRepository.UpdateClient(client.Id, client);
                clientDied = true;
            }

            if(clientDied)
                StartClients();
        }

        public void SendMessage(Guid fromClientId, Guid toClientId, string encryptedMessage)
        {
            if (!_callbacks.TryGetValue(toClientId, out var callback)) return;
            callback.OnMessageReceived(fromClientId, encryptedMessage);
            LogRepository.AddLog(new Log(fromClientId, LogType.SentMessage, $"Message from {fromClientId} sent to {toClientId}"));
            System.Diagnostics.Debug.WriteLine($"Message from {fromClientId} sent to {toClientId}");
        }
    }
}

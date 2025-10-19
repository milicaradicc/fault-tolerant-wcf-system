
using Client.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Client.Services
{
    internal class Client
    {
        private Service1Client _serviceClient;
        private Timer _heartbeatTimer;
        private Timer _statusTimer;
        private Guid _clientId;
        private ClientStatus _status = ClientStatus.Standby;
        
        public Client(Service1Client serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public void Start() 
        {
            _serviceClient.Open();

            Console.WriteLine("Sending registration to server");

            _clientId = _serviceClient.RegisterClient();

            Console.WriteLine($"Registered to server with id: {_clientId}");

            _heartbeatTimer = new Timer(10000);
            _heartbeatTimer.Elapsed += (s, e) =>
            {
                _serviceClient.SendHeartbeat(_clientId);
            };
            _heartbeatTimer.Start();

            _statusTimer = new Timer(5000);
            _statusTimer.Elapsed += (s, e) =>
            {
                if (_status == ClientStatus.Running)
                {
                    Console.WriteLine($"Working... {DateTime.Now}");
                }
            };
            _statusTimer.Start();
        }

        public void OnStart()
        {
            _status = ClientStatus.Running;
        }
        public void SendMessage(Guid toClientId, string message)
        {
            string encrypted = Crypto.Encrypt(message);
            _serviceClient.SendMessage(_clientId, toClientId, encrypted);
        }
        public void OnMessageReceived(Guid fromClientId, string encryptedMessage)
        {
            string decrypted = Crypto.Decrypt(encryptedMessage);
            Console.WriteLine($"Message from {fromClientId}: {decrypted}");
        }
        public void ShowClientInfo()
        {
            Console.WriteLine("\n┌─────────────────────────────────────────┐");
            Console.WriteLine("│          CLIENT INFORMATION             │");
            Console.WriteLine("├─────────────────────────────────────────┤");
            Console.WriteLine($"│  ID:     {_clientId}  │");
            Console.WriteLine($"│  Status: {_status,-28} │");
            Console.WriteLine($"│  Time:   {DateTime.Now,-28:yyyy-MM-dd HH:mm:ss} │");
            Console.WriteLine("└─────────────────────────────────────────┘\n");
        }
    }
}

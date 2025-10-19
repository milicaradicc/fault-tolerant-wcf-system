
using Client.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Client.Services
{
    internal class Client
    {
        private Service1Client _serviceClient;
        private Timer _heartbeatTimer;
        private Guid _clientId;

        public void Start() 
        {
            _serviceClient = new Service1Client();
            _serviceClient.Open();

            Console.WriteLine("Sending registration to server");

            _clientId = _serviceClient.RegisterClient();

            Console.WriteLine($"Registered to server with id: {_clientId}");

            _heartbeatTimer = new Timer(10000);
            _heartbeatTimer.Elapsed += (s, e) =>
            {
                Console.WriteLine($"Sending heartbeat to server");
                _serviceClient.SendHeartbeat(_clientId);
            };
            _heartbeatTimer.Start();
        }
    }
}

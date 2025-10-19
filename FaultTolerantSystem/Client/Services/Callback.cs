using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.ServiceReference;

namespace Client.Services
{
    internal class Callback : IService1Callback
    {
        private Client _client;

        public void SetClient(Client client)
        {
            _client = client; 
        }

        public void OnStart()
        {
            _client.OnStart();
        }
    }
}

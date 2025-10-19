using Client.ServiceReference;
using Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {

        static void Main(string[] args)
        {
           var callback = new Callback();
           var instanceContext = new InstanceContext(callback);
           Service1Client _serviceClient = new Service1Client(instanceContext);
           Services.Client client = new Services.Client(_serviceClient);
           callback.SetClient(client);
           client.Start();
           while (true) { }

        }
    }
}

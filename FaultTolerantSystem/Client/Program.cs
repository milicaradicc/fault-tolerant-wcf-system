using Client.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {

        static void Main(string[] args)
        {
           Services.Client client = new Services.Client();
           client.Start();
           while (true) { }

        }
    }
}

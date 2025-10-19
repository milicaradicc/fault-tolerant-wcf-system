using ServiceReference;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string clientId = args.Length > 0 ? args[0] : Guid.NewGuid().ToString();
            var client = new Service1Client();

            client.RegisterClient(clientId);

            Task.Run(async () =>
            {
                while (true)
                {
                    client.Heartbeat(clientId);
                    await Task.Delay(10000);
                }
            });

            while (true)
            {
                string status = client.GetClientStatus(clientId);

                if (status == "WORKING")
                {
                    Console.WriteLine($"WORKING... {DateTime.Now}");
                    Thread.Sleep(5000);
                }
                else if (status == "STANDBY")
                {
                    Console.WriteLine($"STANDBY... waiting...");
                    Thread.Sleep(2000);
                }
                else if (status == "DEAD")
                {
                    Console.WriteLine("Client marked as DEAD by server. Exiting...");
                    break;
                }
            }
        }
    }
}

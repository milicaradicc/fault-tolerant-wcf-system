using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Web.Services.Description;

namespace Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {

    private static readonly Dictionary<string, DateTime> clientHeartbeats = new Dictionary<string, DateTime>();
    private static readonly Dictionary<string, string> clientStatus = new Dictionary<string, string>();

        public void RegisterClient(string clientId)
    {
        clientHeartbeats[clientId] = DateTime.Now;
        clientStatus[clientId] = "STANDBY";
        Console.WriteLine($"Client {clientId} registered.");
    }

    public void Heartbeat(string clientId)
    {
        clientHeartbeats[clientId] = DateTime.Now;
    }

    public void MarkAsWorking(string clientId)
    {
        clientStatus[clientId] = "WORKING";
    }

    public void MarkAsStandby(string clientId)
    {
        clientStatus[clientId] = "STANDBY";
    }

    public string GetClientStatus(string clientId)
    {
        return clientStatus.ContainsKey(clientId) ? clientStatus[clientId] : "UNKNOWN";
    }

    public static void MonitorClients()
    {
        while (true)
        {
            foreach (var client in clientHeartbeats.Keys.ToList())
            {
                if ((DateTime.Now - clientHeartbeats[client]).TotalSeconds > 30)
                {
                    Console.WriteLine($"Client {client} is DEAD!");
                    clientStatus[client] = "DEAD";
                    ActivateStandby();
                }
            }
            Thread.Sleep(5000);
        }
    }

        private static void ActivateStandby()
        {
            var standby = clientStatus.FirstOrDefault(c => c.Value == "STANDBY");
            if (!standby.Equals(default(KeyValuePair<string, string>)))
            {
                clientStatus[standby.Key] = "WORKING";
                Console.WriteLine($"Activating standby client: {standby.Key}");
            }
        }
    }
}
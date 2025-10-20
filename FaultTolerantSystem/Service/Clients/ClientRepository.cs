using Service.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Clients
{
    public class ClientRepository
    {
        private ClientRepository() {}
        public static void AddClient(ClientData client)
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                context.Clients.Add(client);
                context.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"(Database) Added client {client.Id}");
        }

        public static ClientData GetClient(Guid clientId)
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                return context.Clients.Find(clientId);
            }
        }

        public static List<ClientData> GetAllClients()
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                return context.Clients.ToList();
            }
        }

        public static List<ClientData> GetAllInactiveClients(int clientInactivityTreshold)
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                return context.Clients
                    .Where(client =>
                        (DateTime.Now - client.LastHeartbeat).TotalMilliseconds > clientInactivityTreshold
                        && client.Status != ClientStatus.Dead).ToList();
            }
        }

        public static List<ClientData> GetClientsWithStatus(ClientStatus status)
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                return context.Clients.Where(c => c.Status == status).ToList();
            }
        }

        public static bool UpdateClient(Guid clientId, ClientData client)
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                ClientData dbClient = context.Clients.Find(client.Id);
                if (dbClient == null)
                    return false;
                dbClient.Status = client.Status;
                dbClient.LastHeartbeat = client.LastHeartbeat;
                context.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"(Database) Updated client {client.Id}");
            return true;
        }
    }
}

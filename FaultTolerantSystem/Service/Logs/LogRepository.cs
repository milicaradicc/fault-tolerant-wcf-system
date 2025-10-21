using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Logs
{
    public class LogRepository
    {
        private LogRepository() { }
        public static void AddLog(Log log)
        {
            using (ServiceDbContext context = new ServiceDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"(Database) {log.Timestamp} [{log.Type}] - Log added ");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using Service.Clients;
using Service.Logs;

namespace Service
{
    public class ServiceDbContext : DbContext
    {
        public ServiceDbContext()
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ServiceDbContext>());
        }

        public DbSet<Log> Logs { get; set; }
        public DbSet<ClientData> Clients { get; set; }

        public static void ResetDatabase()
        {
            using (var db = new ServiceDbContext())
            {
                db.Clients.RemoveRange(db.Clients);
                db.SaveChanges();
            }
        }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>()
                .HasOptional(l => l.ClientData)
                .WithMany()
                .HasForeignKey(l => l.ClientId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
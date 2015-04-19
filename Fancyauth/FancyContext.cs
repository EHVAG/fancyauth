using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Fancyauth.Model;

namespace Fancyauth
{
    public class FancyContext : DbContext
    {
        public FancyContext()
            : base("name=FancyContext")
        {
            Database.Log = x => System.Diagnostics.Trace.WriteLine(x);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogEntry>()
                .Map<LogEntry.Connected>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.Connected))
                .Map<LogEntry.Disconnected>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.Disconnected))
                .Map<LogEntry.ChatMessage>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.ChatMessage));
        }

        public static async Task<FancyContext> Connect()
        {
            var context = new FancyContext();
            await context.Database.Connection.OpenAsync();
            return context;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Invite> Invites { get; set; }
        public virtual DbSet<GuestAssociation> GuestAssociations { get; set; }
        public virtual DbSet<OfflineNotification> OfflineNotifications { get; set; }
        public virtual DbSet<LogEntry> Logs { get; set; }
    }
}


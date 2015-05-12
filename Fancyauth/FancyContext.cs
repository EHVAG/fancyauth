using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Fancyauth.Model;
using Fancyauth.Model.MusiG;
using System.Data.Entity.Infrastructure;

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

        public DbSqlQuery<Song> SearchSong(string search)
        {
            return Songs.SqlQuery(@"
SELECT ""Id"", ""Title"", ""AdditionDate"", ""Album_Id"", ""Interpret_Id"", ""SourceSuggestion_Id"", ""Genre_Id""
FROM dbo.""Songs"", plainto_tsquery(@p0) query
WHERE query @@ ftsvec
ORDER BY ts_rank_cd(ftsvec, query);", search);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Invite> Invites { get; set; }
        public virtual DbSet<GuestAssociation> GuestAssociations { get; set; }
        public virtual DbSet<OfflineNotification> OfflineNotifications { get; set; }
        public virtual DbSet<LogEntry> Logs { get; set; }

        public virtual DbSet<Channel> Channels { get; set; }
        public virtual DbSet<Channel.InfoChange> ChannelInfoChanges { get; set; }

        #region MusiG
        public virtual DbSet<Album> Albums { get; set; }
        public virtual DbSet<Genre> Genres { get; set; }
        public virtual DbSet<Interpret> Interprets { get; set; }
        public virtual DbSet<Song> Songs { get; set; }
        public virtual DbSet<SongRating> SongRatings { get; set; }
        public virtual DbSet<SongSuggestion> SongSuggestions { get; set; }
        #endregion
    }
}


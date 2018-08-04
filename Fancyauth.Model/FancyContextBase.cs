using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Fancyauth.Model;
using Fancyauth.Model.MusiG;
using System.Data.Entity.Infrastructure;
using Fancyauth.Model.UserAttribute;

namespace Fancyauth.Model
{
    public abstract class FancyContextBase : DbContext
    {
        public FancyContextBase(string connectionString)
            : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogEntry>()
                .Map<LogEntry.Connected>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.Connected))
                .Map<LogEntry.ChannelSwitched>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.ChannelSwitched))
                .Map<LogEntry.Disconnected>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.Disconnected))
                .Map<LogEntry.ChatMessage>(x => x.Requires("Discriminator").HasValue((int)LogEntry.Discriminator.ChatMessage));

            modelBuilder.Entity<Song>()
                .HasMany(x => x.AdditionalInterprets).WithMany().Map(x => x.ToTable("Songs_AdditionalInterprets"));

            modelBuilder.Entity<PersistentGuest>()
                .HasMany(x => x.Godfathers).WithMany(x => x.Godfatherships).Map(x => x.ToTable("PG_Godfathers"));

            modelBuilder.Entity<User>().HasOptional(x => x.CertCredentials).WithRequired(x => x.User).WillCascadeOnDelete();
            modelBuilder.Entity<User>().HasOptional(x => x.Membership).WithRequired(x => x.User).WillCascadeOnDelete();
            modelBuilder.Entity<User>().HasOptional(x => x.PersistentGuest).WithRequired(x => x.User).WillCascadeOnDelete();
            modelBuilder.Entity<User>().HasOptional(x => x.GuestInvite).WithMany().WillCascadeOnDelete();
        }

        public DbSqlQuery<Song> SearchSong(string search)
        {
            return Songs.SqlQuery(@"
SELECT s.*
FROM dbo.""Songs"" s, plainto_tsquery(@p0) query
WHERE query @@ dbo.songs_build_ftsvec(s)
ORDER BY ts_rank_cd(dbo.songs_build_ftsvec(s), query); ", search);
        }

        public virtual DbSet<User> Users { get; set; }
        #region UserAttributes
        public virtual DbSet<Membership> Memberships { get; set; }
        public virtual DbSet<PersistentGuest> PersistentGuests { get; set; }
        public virtual DbSet<CertificateCredentials> CertificateCredentials { get; set; }
        #endregion
        public virtual DbSet<Invite> Invites { get; set; }
        public virtual DbSet<OfflineNotification> OfflineNotifications { get; set; }
        public virtual DbSet<LogEntry> Logs { get; set; }

        public virtual DbSet<SteamChatForwardingAssociation> SteamChatForwardingAssociations { get; set; }

        public virtual DbSet<Channel> Channels { get; set; }
        public virtual DbSet<Channel.InfoChange> ChannelInfoChanges { get; set; }

        public virtual DbSet<Rude> Rudes { get; set; }

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


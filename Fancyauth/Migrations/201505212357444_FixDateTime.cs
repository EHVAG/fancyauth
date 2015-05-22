namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FixDateTime : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.LogEntries", new[] { "When" });
            AlterColumn("dbo.ChannelInfoChanges", "When", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.Invites", "ExpirationDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.LogEntries", "When", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.OfflineNotifications", "When", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.Songs", "AdditionDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            CreateIndex("dbo.LogEntries", "When");
        }

        public override void Down()
        {
            DropIndex("dbo.LogEntries", new[] { "When" });
            AlterColumn("dbo.Songs", "AdditionDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.OfflineNotifications", "When", c => c.DateTime(nullable: false));
            AlterColumn("dbo.LogEntries", "When", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Invites", "ExpirationDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ChannelInfoChanges", "When", c => c.DateTime(nullable: false));
            CreateIndex("dbo.LogEntries", "When");
        }
    }
}

namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class SteamChatForwardingAssociations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SteamChatForwardingAssociations",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        AppId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.AppId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.SteamChatForwardingAssociations", "UserId", "dbo.Users");
            DropTable("dbo.SteamChatForwardingAssociations");
        }
    }
}

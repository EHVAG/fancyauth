namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddChannels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChannelInfoChanges",
                c => new
                    {
                        ChannelId = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        When = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ChannelId)
                .ForeignKey("dbo.Channels", t => t.ChannelId)
                .Index(t => t.ChannelId);

            CreateTable(
                "dbo.Channels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Temporary = c.Boolean(nullable: false),
                        ServerId = c.Int(),
                        Parent_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Channels", t => t.Parent_Id)
                .Index(t => t.Parent_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ChannelInfoChanges", "ChannelId", "dbo.Channels");
            DropForeignKey("dbo.Channels", "Parent_Id", "dbo.Channels");
            DropIndex("dbo.Channels", new[] { "Parent_Id" });
            DropIndex("dbo.ChannelInfoChanges", new[] { "ChannelId" });
            DropTable("dbo.Channels");
            DropTable("dbo.ChannelInfoChanges");
        }
    }
}

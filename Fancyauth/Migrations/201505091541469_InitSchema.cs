namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class InitSchema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GuestAssociations",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Session = c.Int(),
                        Invite_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Name)
                .ForeignKey("dbo.Invites", t => t.Invite_Id, cascadeDelete: true)
                .Index(t => t.Invite_Id);

            CreateTable(
                "dbo.Invites",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false),
                        InviterId = c.Int(nullable: false),
                        ExpirationDate = c.DateTime(nullable: false),
                        UseCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.InviterId, cascadeDelete: true)
                .Index(t => t.Code, unique: true)
                .Index(t => t.InviterId);

            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Fingerprint = c.String(nullable: false),
                        CertSerial = c.Long(nullable: false),
                        Texture = c.Binary(nullable: false),
                        Comment = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true)
                .Index(t => t.Fingerprint, unique: true);

            CreateTable(
                "dbo.LogEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        When = c.DateTime(nullable: false),
                        WhoUId = c.Int(),
                        WhoIId = c.Int(),
                        Message = c.String(),
                        Discriminator = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Invites", t => t.WhoIId)
                .ForeignKey("dbo.Users", t => t.WhoUId)
                .Index(t => t.When)
                .Index(t => t.WhoUId)
                .Index(t => t.WhoIId);

            CreateTable(
                "dbo.OfflineNotifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Message = c.String(nullable: false),
                        When = c.DateTime(nullable: false),
                        Recipient_Id = c.Int(nullable: false),
                        Sender_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Recipient_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Sender_Id, cascadeDelete: true)
                .Index(t => t.Recipient_Id)
                .Index(t => t.Sender_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.OfflineNotifications", "Sender_Id", "dbo.Users");
            DropForeignKey("dbo.OfflineNotifications", "Recipient_Id", "dbo.Users");
            DropForeignKey("dbo.LogEntries", "WhoUId", "dbo.Users");
            DropForeignKey("dbo.LogEntries", "WhoIId", "dbo.Invites");
            DropForeignKey("dbo.GuestAssociations", "Invite_Id", "dbo.Invites");
            DropForeignKey("dbo.Invites", "InviterId", "dbo.Users");
            DropIndex("dbo.OfflineNotifications", new[] { "Sender_Id" });
            DropIndex("dbo.OfflineNotifications", new[] { "Recipient_Id" });
            DropIndex("dbo.LogEntries", new[] { "WhoIId" });
            DropIndex("dbo.LogEntries", new[] { "WhoUId" });
            DropIndex("dbo.LogEntries", new[] { "When" });
            DropIndex("dbo.Users", new[] { "Fingerprint" });
            DropIndex("dbo.Users", new[] { "Name" });
            DropIndex("dbo.Invites", new[] { "InviterId" });
            DropIndex("dbo.Invites", new[] { "Code" });
            DropIndex("dbo.GuestAssociations", new[] { "Invite_Id" });
            DropTable("dbo.OfflineNotifications");
            DropTable("dbo.LogEntries");
            DropTable("dbo.Users");
            DropTable("dbo.Invites");
            DropTable("dbo.GuestAssociations");
        }
    }
}

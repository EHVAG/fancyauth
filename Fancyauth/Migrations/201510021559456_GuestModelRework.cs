namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GuestModelRework : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GuestAssociations", "Invite_Id", "dbo.Invites");
            DropForeignKey("dbo.LogEntries", "WhoIId", "dbo.Invites");
            DropForeignKey("dbo.LogEntries", "WhoUId", "dbo.Users");
            DropIndex("dbo.GuestAssociations", new[] { "Invite_Id" });
            DropIndex("dbo.Users", new[] { "Fingerprint" });
            DropIndex("dbo.LogEntries", new[] { "WhoUId" });
            DropIndex("dbo.LogEntries", new[] { "WhoIId" });
            RenameColumn(table: "dbo.LogEntries", name: "WhoUId", newName: "Who_Id");

            CreateTable(
                "dbo.Memberships",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        Texture = c.Binary(nullable: false),
                        Comment = c.String(nullable: false),
                        SteamId = c.Long(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PersistentGuests",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        OriginalInvite_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Invites", t => t.OriginalInvite_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.OriginalInvite_Id);
            
            CreateTable(
                "dbo.PG_Godfathers",
                c => new
                    {
                        PersistentGuest_UserId = c.Int(nullable: false),
                        Membership_UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PersistentGuest_UserId, t.Membership_UserId })
                .ForeignKey("dbo.PersistentGuests", t => t.PersistentGuest_UserId, cascadeDelete: true)
                .ForeignKey("dbo.Memberships", t => t.Membership_UserId, cascadeDelete: true)
                .Index(t => t.PersistentGuest_UserId)
                .Index(t => t.Membership_UserId);
            
            AddColumn("dbo.Users", "GuestInvite_Id", c => c.Int());

            // Delete old guest logs (they were broken anyways)
            Sql(@"DELETE FROM dbo.""LogEntries"" WHERE ""Who_Id"" IS NULL");

            AlterColumn("dbo.LogEntries", "Who_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Users", "GuestInvite_Id");
            CreateIndex("dbo.LogEntries", "Who_Id");
            AddForeignKey("dbo.Users", "GuestInvite_Id", "dbo.Invites", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LogEntries", "Who_Id", "dbo.Users", "Id", cascadeDelete: true);

            // Now move data from dbo.Users to dbo.Memberships
            Sql(@"
INSERT INTO dbo.""Memberships"" (""UserId"", ""Texture"", ""Comment"", ""SteamId"")
SELECT ""Id"", ""Texture"", ""Comment"", ""SteamId""
FROM dbo.""Users""
");

            RenameColumn("dbo.Users", "Fingerprint", "CertFingerprint");
            DropColumn("dbo.Users", "Texture");
            DropColumn("dbo.Users", "Comment");
            DropColumn("dbo.Users", "SteamId");
            DropColumn("dbo.LogEntries", "WhoIId");
            DropTable("dbo.GuestAssociations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.GuestAssociations",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Session = c.Int(),
                        Invite_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Name);
            
            AddColumn("dbo.LogEntries", "WhoIId", c => c.Int());
            AddColumn("dbo.Users", "SteamId", c => c.Long());
            AddColumn("dbo.Users", "Comment", c => c.String(nullable: false));
            AddColumn("dbo.Users", "Texture", c => c.Binary(nullable: false));
            AddColumn("dbo.Users", "CertSerial", c => c.Long(nullable: false));
            AddColumn("dbo.Users", "Fingerprint", c => c.String(nullable: false));
            DropForeignKey("dbo.LogEntries", "Who_Id", "dbo.Users");
            DropForeignKey("dbo.PersistentGuests", "UserId", "dbo.Users");
            DropForeignKey("dbo.Memberships", "UserId", "dbo.Users");
            DropForeignKey("dbo.PersistentGuests", "OriginalInvite_Id", "dbo.Invites");
            DropForeignKey("dbo.PG_Godfathers", "Membership_UserId", "dbo.Memberships");
            DropForeignKey("dbo.PG_Godfathers", "PersistentGuest_UserId", "dbo.PersistentGuests");
            DropForeignKey("dbo.Users", "GuestInvite_Id", "dbo.Invites");
            DropIndex("dbo.PG_Godfathers", new[] { "Membership_UserId" });
            DropIndex("dbo.PG_Godfathers", new[] { "PersistentGuest_UserId" });
            DropIndex("dbo.LogEntries", new[] { "Who_Id" });
            DropIndex("dbo.PersistentGuests", new[] { "OriginalInvite_Id" });
            DropIndex("dbo.PersistentGuests", new[] { "UserId" });
            DropIndex("dbo.Memberships", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "GuestInvite_Id" });
            AlterColumn("dbo.LogEntries", "Who_Id", c => c.Int());
            DropColumn("dbo.Users", "GuestInvite_Id");
            RenameColumn("dbo.Users", "CertFingerprint", "Fingerprint");

            // Move our data back before we drop these
            Sql(@"
UPDATE dbo.""Users""
SET ""Texture"" = m.""Texture"",
    ""Comment"" = m.""Comment"",
    ""SteamId"" = m.""SteamId""
FROM dbo.""Memberships"" m
WHERE ""Id"" = m.""UserId""
");

            DropTable("dbo.PG_Godfathers");
            DropTable("dbo.PersistentGuests");
            DropTable("dbo.Memberships");
            RenameColumn(table: "dbo.LogEntries", name: "Who_Id", newName: "WhoUId");
            CreateIndex("dbo.LogEntries", "WhoIId");
            CreateIndex("dbo.LogEntries", "WhoUId");
            CreateIndex("dbo.Users", "Fingerprint", unique: true);
            CreateIndex("dbo.GuestAssociations", "Invite_Id");
            AddForeignKey("dbo.LogEntries", "WhoUId", "dbo.Users", "Id");
            AddForeignKey("dbo.LogEntries", "WhoIId", "dbo.Invites", "Id");
            AddForeignKey("dbo.GuestAssociations", "Invite_Id", "dbo.Invites", "Id", cascadeDelete: true);
        }
    }
}

namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackUserChannel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LogEntries", "Where_Id", c => c.Int(nullable: false, defaultValue: 39 /* our root channel */));
            CreateIndex("dbo.LogEntries", "Where_Id");
            AddForeignKey("dbo.LogEntries", "Where_Id", "dbo.Channels", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LogEntries", "Where_Id", "dbo.Channels");
            DropIndex("dbo.LogEntries", new[] { "Where_Id" });
            DropColumn("dbo.LogEntries", "Where_Id");
        }
    }
}

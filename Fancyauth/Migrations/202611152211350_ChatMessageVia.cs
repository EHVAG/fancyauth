namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChatMessageVia : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LogEntries", "Via", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LogEntries", "Via");
        }
    }
}

namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChannelInfoChangeFixWrongKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChannelInfoChanges", "ChannelId", "dbo.Channels");
            RenameColumn(table: "dbo.ChannelInfoChanges", name: "ChannelId", newName: "Channel_Id");
            DropIndex(table: "dbo.ChannelInfoChanges", name: "IX_ChannelId");
            CreateIndex(table: "dbo.ChannelInfoChanges", name: "IX_Channel_Id", column: "Channel_Id");
            DropPrimaryKey("dbo.ChannelInfoChanges");
            AddColumn("dbo.ChannelInfoChanges", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.ChannelInfoChanges", "Id");
            AddForeignKey("dbo.ChannelInfoChanges", "Channel_Id", "dbo.Channels", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChannelInfoChanges", "Channel_Id", "dbo.Channels");
            DropPrimaryKey("dbo.ChannelInfoChanges");
            DropColumn("dbo.ChannelInfoChanges", "Id");
            DropIndex(table: "dbo.ChannelInfoChanges", name: "IX_Channel_Id");
            CreateIndex(table: "dbo.ChannelInfoChanges", name: "IX_ChannelId", column: "Channel_Id");
            RenameColumn(table: "dbo.ChannelInfoChanges", name: "Channel_Id", newName: "ChannelId");
            AddPrimaryKey("dbo.ChannelInfoChanges", "ChannelId");
            AddForeignKey("dbo.ChannelInfoChanges", "ChannelId", "dbo.Channels", "Id");
        }
    }
}

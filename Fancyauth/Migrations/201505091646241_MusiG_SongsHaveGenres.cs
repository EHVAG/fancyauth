namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MusiG_SongsHaveGenres : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Songs", "Genre_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Songs", "Genre_Id");
            AddForeignKey("dbo.Songs", "Genre_Id", "dbo.Genres", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Songs", "Genre_Id", "dbo.Genres");
            DropIndex("dbo.Songs", new[] { "Genre_Id" });
            DropColumn("dbo.Songs", "Genre_Id");
        }
    }
}

namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFieldsToSongSuggestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SongSuggestions", "YouTubeURL", c => c.String(nullable: false));
            AddColumn("dbo.SongSuggestions", "AlbumInterpret", c => c.String(nullable: false));
            AlterColumn("dbo.SongSuggestions", "Title", c => c.String(nullable: false));
            AlterColumn("dbo.SongSuggestions", "Interpret", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SongSuggestions", "Interpret", c => c.String());
            AlterColumn("dbo.SongSuggestions", "Title", c => c.String());
            DropColumn("dbo.SongSuggestions", "AlbumInterpret");
            DropColumn("dbo.SongSuggestions", "YouTubeURL");
        }
    }
}

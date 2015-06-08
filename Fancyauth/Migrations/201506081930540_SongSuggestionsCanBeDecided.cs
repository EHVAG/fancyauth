namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class SongSuggestionsCanBeDecided : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SongSuggestions", "Decided", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.SongSuggestions", "Decided");
        }
    }
}

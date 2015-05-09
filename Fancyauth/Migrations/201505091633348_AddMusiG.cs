namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddMusiG : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Albums",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Interpret_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Interprets", t => t.Interpret_Id, cascadeDelete: true)
                .Index(t => t.Interpret_Id);

            CreateTable(
                "dbo.Interprets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.SongRatings",
                c => new
                    {
                        SongId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        QueueModifier = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SongId, t.UserId })
                .ForeignKey("dbo.Songs", t => t.SongId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.SongId)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.Songs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        AdditionDate = c.DateTime(nullable: false),
                        Album_Id = c.Int(nullable: false),
                        Interpret_Id = c.Int(nullable: false),
                        SourceSuggestion_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Albums", t => t.Album_Id, cascadeDelete: true)
                .ForeignKey("dbo.Interprets", t => t.Interpret_Id, cascadeDelete: true)
                .ForeignKey("dbo.SongSuggestions", t => t.SourceSuggestion_Id)
                .Index(t => t.Album_Id)
                .Index(t => t.Interpret_Id)
                .Index(t => t.SourceSuggestion_Id);

            CreateTable(
                "dbo.SongSuggestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Interpret = c.String(),
                        Album = c.String(nullable: false),
                        Genre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SongRatings", "UserId", "dbo.Users");
            DropForeignKey("dbo.SongRatings", "SongId", "dbo.Songs");
            DropForeignKey("dbo.Songs", "SourceSuggestion_Id", "dbo.SongSuggestions");
            DropForeignKey("dbo.Songs", "Interpret_Id", "dbo.Interprets");
            DropForeignKey("dbo.Songs", "Album_Id", "dbo.Albums");
            DropForeignKey("dbo.Albums", "Interpret_Id", "dbo.Interprets");
            DropIndex("dbo.Songs", new[] { "SourceSuggestion_Id" });
            DropIndex("dbo.Songs", new[] { "Interpret_Id" });
            DropIndex("dbo.Songs", new[] { "Album_Id" });
            DropIndex("dbo.SongRatings", new[] { "UserId" });
            DropIndex("dbo.SongRatings", new[] { "SongId" });
            DropIndex("dbo.Albums", new[] { "Interpret_Id" });
            DropTable("dbo.SongSuggestions");
            DropTable("dbo.Songs");
            DropTable("dbo.SongRatings");
            DropTable("dbo.Genres");
            DropTable("dbo.Interprets");
            DropTable("dbo.Albums");
        }
    }
}

namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MusiG_FixAdditionalInterprets : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Songs_AdditionalInterprets",
                c => new
                    {
                        Song_Id = c.Int(nullable: false),
                        Interpret_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Song_Id, t.Interpret_Id })
                .ForeignKey("dbo.Songs", t => t.Song_Id, cascadeDelete: true)
                .ForeignKey("dbo.Interprets", t => t.Interpret_Id, cascadeDelete: true)
                .Index(t => t.Song_Id)
                .Index(t => t.Interpret_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Songs_AdditionalInterprets", "Interpret_Id", "dbo.Interprets");
            DropForeignKey("dbo.Songs_AdditionalInterprets", "Song_Id", "dbo.Songs");
            DropIndex("dbo.Songs_AdditionalInterprets", new[] { "Interpret_Id" });
            DropIndex("dbo.Songs_AdditionalInterprets", new[] { "Song_Id" });
            DropTable("dbo.Songs_AdditionalInterprets");
        }
    }
}

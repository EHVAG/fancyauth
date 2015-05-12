namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MusiG_AddSongFullTextSearch : DbMigration
    {
        public override void Up()
        {
            Sql("ALTER TABLE dbo.\"Songs\" ADD COLUMN ftsvec tsvector NOT NULL");
            Sql(@"
CREATE FUNCTION songs_fts_trigger() RETURNS trigger AS $$
DECLARE
        album text;
        interpret text;
        genre text;
BEGIN
        SELECT ""Name"" INTO STRICT album FROM dbo.""Albums"" WHERE ""Id"" = new.""Album_Id"";
        SELECT ""Name"" INTO STRICT interpret FROM dbo.""Interprets"" WHERE ""Id"" = new.""Interpret_Id"";
        SELECT ""Name"" INTO STRICT genre FROM dbo.""Genres"" WHERE ""Id"" = new.""Genre_Id"";
        new.ftsvec :=
           setweight(to_tsvector('english', new.""Title""), 'A') ||
           setweight(to_tsvector('english', album), 'B') ||
           setweight(to_tsvector('english', interpret), 'C') ||
           setweight(to_tsvector('english', genre), 'D');
        return new;
END
$$ LANGUAGE plpgsql");
            Sql("CREATE TRIGGER ftsvectorupdate BEFORE INSERT OR UPDATE ON dbo.\"Songs\" FOR EACH ROW EXECUTE PROCEDURE songs_fts_trigger()");

            // And create an index to make our fulltext searches fast.
            Sql("CREATE INDEX \"Songs_IXByHand_FTS\" on dbo.\"Songs\" USING gin (ftsvec)");
        }

        public override void Down()
        {
            Sql("DROP INDEX IF EXISTS \"Songs_IXByHand_FTS\"");

            Sql("DROP TRIGGER ftsvectorupdate ON dbo.\"Songs\"");
            Sql("DROP FUNCTION songs_fts_trigger()");
            Sql("ALTER TABLE dbo.\"Songs\" DROP COLUMN ftsvec");
        }
    }
}

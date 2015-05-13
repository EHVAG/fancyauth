namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MusiG_AddSongFullTextSearch : DbMigration
    {
        public override void Up()
        {
            Sql(@"
CREATE FUNCTION dbo.songs_build_ftsvec(s dbo.""Songs"") RETURNS tsvector AS $$
DECLARE
        album text;
        interpret text;
        genre text;
BEGIN
        SELECT ""Name"" INTO STRICT album FROM dbo.""Albums"" WHERE ""Id"" = s.""Album_Id"";
        SELECT ""Name"" INTO STRICT interpret FROM dbo.""Interprets"" WHERE ""Id"" = s.""Interpret_Id"";
        SELECT ""Name"" INTO STRICT genre FROM dbo.""Genres"" WHERE ""Id"" = s.""Genre_Id"";
        return
           setweight(to_tsvector('english', s.""Title""), 'A') ||
           setweight(to_tsvector('english', album), 'B') ||
           setweight(to_tsvector('english', interpret), 'C') ||
           setweight(to_tsvector('english', genre), 'D');
END
$$ LANGUAGE plpgsql IMMUTABLE");

            // And create an index to make our fulltext searches fast.
            Sql("CREATE INDEX \"Songs_IXByHand_FTS\" on dbo.\"Songs\" USING gin (dbo.songs_build_ftsvec(dbo.\"Songs\".*))");
        }

        public override void Down()
        {
            Sql("DROP INDEX dbo.\"Songs_IXByHand_FTS\"");
            Sql("DROP FUNCTION dbo.songs_build_ftsvec(dbo.\"Songs\")");
        }
    }
}

namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UserAddSteamId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "SteamId", c => c.Long());
        }

        public override void Down()
        {
            DropColumn("dbo.Users", "SteamId");
        }
    }
}

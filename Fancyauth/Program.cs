using System;
using System.Data.Entity;
using System.Linq;

namespace Fancyauth
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // EF6 on .NET has no App.config: the provider is configured in code.
            DbConfiguration.SetConfiguration(new Model.NpgSqlConfiguration());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<FancyContext, Migrations.Configuration>());

            using (var ctx = new FancyContext())
                ctx.Albums.ToArray();

            new Fancyauth().ServerMain().Wait();
        }
    }
}

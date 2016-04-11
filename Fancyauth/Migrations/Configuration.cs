using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using Fancyauth.Model;

namespace Fancyauth.Migrations
{
    internal class Configuration : DbMigrationsConfiguration<FancyContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;

            SetSqlGenerator("Npgsql", new Npgsql.NpgsqlMigrationSqlGenerator());
        }
    }
}


using Fancyauth.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth
{
    internal class FancyContext : FancyContextBase
    {
        // An Npgsql connection string, e.g. "Host=…;Database=fancyauth;Username=fancyauth;Password=…;SSL Mode=Require".
        private static readonly string ConnectionString = Settings.Required("FANCYAUTH_DB");

        public FancyContext()
            : base(ConnectionString)
        {
        }

        public static async Task<FancyContext> Connect()
        {
            var context = new FancyContext();
            await context.Database.Connection.OpenAsync();
            return context;
        }
    }
}

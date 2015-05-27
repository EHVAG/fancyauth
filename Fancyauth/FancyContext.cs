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
        public FancyContext()
            : base("name=FancyContext")
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

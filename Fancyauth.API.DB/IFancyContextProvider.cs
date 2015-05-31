using Fancyauth.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API.DB
{
    public interface IFancyContextProvider
    {
        Task<FancyContextBase> Connect();
    }
}

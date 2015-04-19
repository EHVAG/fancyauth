using System;
using System.Threading.Tasks;
using Fancyauth.API;
using System.Collections.Generic;

namespace Fancyauth.API.Commands
{
    public delegate Task CommandFunc(IUser sender, IEnumerable<string> parameters);
}


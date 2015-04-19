using System;
using System.Threading.Tasks;

namespace Fancyauth.API.ContextCallbacks
{
    public interface IServerContextCallback : IContextCallback
    {
        Task Run(IUser usr);
    }
}


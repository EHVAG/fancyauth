using System;
using System.Threading.Tasks;

namespace Fancyauth.API.ContextCallbacks
{
    public interface IUserContextCallback : IContextCallback
    {
        Task Run(IUser usr, IUserShim target);
    }
}


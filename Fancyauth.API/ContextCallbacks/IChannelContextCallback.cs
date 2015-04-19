using System;
using System.Threading.Tasks;

namespace Fancyauth.API.ContextCallbacks
{
    public interface IChannelContextCallback : IContextCallback
    {
        Task Run(IUser usr, IChannelShim channel);
    }
}


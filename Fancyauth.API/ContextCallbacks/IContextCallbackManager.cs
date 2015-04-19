using System;
using System.Threading.Tasks;

namespace Fancyauth.API.ContextCallbacks
{
    public interface IContextCallbackManager
    {
        Task Register<TCallback>(IUserShim user) where TCallback : IContextCallback, new();
        Task Register(IUserShim user, IContextCallback callback);
    }
}


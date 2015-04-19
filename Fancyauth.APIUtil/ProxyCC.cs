using System;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.API.ContextCallbacks;

namespace Fancyauth.APIUtil
{
    internal class ProxyCC : IContextCallback
    {
        public string Name { get; internal set; }
        public string Text { get; internal set; }

        internal class Server : ProxyCC, IServerContextCallback
        {
            internal Func<IUser, Task> Callback { get; set; }
            Task IServerContextCallback.Run(IUser usr) { return Callback(usr); }
        }

        internal class Channel : ProxyCC, IChannelContextCallback
        {
            internal Func<IUser, IChannelShim, Task> Callback { get; set; }
            Task IChannelContextCallback.Run(IUser usr, IChannelShim target) { return Callback(usr, target); }
        }

        internal class User : ProxyCC, IUserContextCallback
        {
            internal Func<IUser, IUserShim, Task> Callback { get; set; }
            Task IUserContextCallback.Run(IUser usr, IUserShim target) { return Callback(usr, target); }
        }
    }
}


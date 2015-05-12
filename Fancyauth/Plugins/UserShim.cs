using System;
using System.Threading.Tasks;
using Fancyauth.API;

namespace Fancyauth.Plugins
{
    public class UserShim : IUserShim
    {
        protected readonly Wrapped.Server Server;
        public int SessionId { get; private set; }

        public UserShim(Wrapped.Server server, int session)
        {
            Server = server;
            SessionId = session;
        }

        async Task<IUser> IShim<IUser>.Load()
        {
            return new UserWrapper(Server, await Server.GetState(SessionId));
        }

        Task IUserShim.Kick(string reason)
        {
            return Server.KickUser(SessionId, reason);
        }

        Task IUserShim.SendMessage(string message)
        {
            return Server.SendMessage(SessionId, message);
        }
    }
}


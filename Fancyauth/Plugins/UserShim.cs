using System;
using System.Threading.Tasks;
using Fancyauth.API;

namespace Fancyauth.Plugins
{
    public class UserShim : IUserShim
    {
        protected readonly Steam.SteamListener SteamListener;
        protected readonly Wrapped.Server Server;
        public int SessionId { get; private set; }

        public UserShim(Steam.SteamListener steamListener, Wrapped.Server server, int session)
        {
            SteamListener = steamListener;
            Server = server;
            SessionId = session;
        }

        async Task<IUser> IShim<IUser>.Load()
        {
            return new UserWrapper(SteamListener, Server, await Server.GetState(SessionId));
        }

        Task IUserShim.Kick(string reason)
        {
            return Server.KickUser(SessionId, reason);
        }

        Task IUserShim.SendMessage(string message)
        {
            return Server.SendMessage(SessionId, message);
        }

        public override bool Equals(object o)
        {
            var userShim = o as UserShim;
            if (null == userShim)
                return false;

            return userShim.SessionId == this.SessionId;
        }

        public override int GetHashCode()
        {
            return SessionId * 37;
        }
    }
}


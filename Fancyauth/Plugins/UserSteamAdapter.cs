using System;
using SteamKit2;
using Fancyauth.API;
using System.Threading.Tasks;
using Fancyauth.Steam;

namespace Fancyauth.Plugins
{
    public class UserSteamAdapter : IUserSteamAdapter
    {
        internal readonly SteamListener SteamListener;
        private readonly SteamID SteamId;

        public UserSteamAdapter(SteamListener steamListener, long steamid)
        {
            SteamListener = steamListener;
            SteamId = new SteamID(unchecked((ulong)steamid));
        }

        Task<uint?> IUserSteamAdapter.GetCurrentGame()
        {
            return Task.FromResult(SteamListener.GetCurrentGameId(SteamId));
        }

        Task IUserSteamAdapter.SendMessage(string message)
        {
            SteamListener.SendMessage(SteamId, message);
            return Task.FromResult<object>(null);
        }
    }
}


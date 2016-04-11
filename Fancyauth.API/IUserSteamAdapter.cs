using System;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    public interface IUserSteamAdapter
    {
        Task<uint?> GetCurrentGame();
        Task SendMessage(string message);
    }
}


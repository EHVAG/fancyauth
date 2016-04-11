using System;
using System.Threading.Tasks;
using SteamKit2;

namespace Fancyauth.Steam
{
    public interface ISteamEventForwarder
    {
        Task OnChatMessage(Steam.SteamListener steamListener, SteamID sender, string message);
    }
}


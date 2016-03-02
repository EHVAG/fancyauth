using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fancyauth.API
{
    public interface IFancyPlugin
    {
        Task Init();

        Task OnUserConnected(IUser user);
        Task OnUserModified(IUser user);
        Task OnUserDisconnected(IUser user);

        Task OnChannelCreated(IChannel channel);
        Task OnChannelModified(IChannel channel);
        Task OnChannelDeleted(IChannel channel);

        Task OnChatMessage(IUser sender, IEnumerable<IChannelShim> targetChannels, string message);
    }
}


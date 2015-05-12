using System;
using System.Threading.Tasks;
using Fancyauth.API;

namespace Fancyauth.Plugins
{
    public class ChannelShim : IChannelShim
    {
        protected readonly Wrapped.Server Server;
        public int ChannelId { get; private set; }

        public ChannelShim(Wrapped.Server server, int id)
        {
            Server = server;
            ChannelId = id;
        }

        async Task<IChannel> IShim<IChannel>.Load()
        {
            return new ChannelWrapper(Server, await Server.GetChannelState(ChannelId));
        }

        Task IChannelShim.SendMessage(string message, bool includingSubchannels)
        {
            return Server.SendMessageChannel(ChannelId, includingSubchannels, message);
        }
    }
}


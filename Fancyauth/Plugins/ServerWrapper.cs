using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fancyauth.API;

namespace Fancyauth.Plugins
{
    public class ServerWrapper : IServer
    {
        private readonly Wrapped.Server Server;

        public ServerWrapper(Wrapped.Server server)
        {
            Server = server;
        }

        async Task<IEnumerable<IUser>> IServer.GetOnlineUsers()
        {
            var userDict = await Server.GetUsers();
            return userDict.Values.Select(u => new UserWrapper(Server, u)); // TODO: we might wanna call ToArray() here
        }

        async Task<IEnumerable<IChannel>> IServer.GetChannels()
        {
            var channelDict = await Server.GetChannels();
            return channelDict.Values.Select(c => new ChannelWrapper(Server, c)); // TODO: we might wanna call ToArray() here
        }

        async Task<TimeSpan> IServer.GetUptime()
        {
            var secs = await Server.GetUptime();
            return TimeSpan.FromSeconds(secs);
        }
    }
}


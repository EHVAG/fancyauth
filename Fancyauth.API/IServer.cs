using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents the mumble server.
    /// </summary>
    public interface IServer
    {
        Task<IEnumerable<IUser>> GetOnlineUsers();
        Task<IEnumerable<IChannel>> GetChannels();
        Task<TimeSpan> GetUptime();
    }
}


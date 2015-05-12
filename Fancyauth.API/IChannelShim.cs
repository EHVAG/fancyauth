using System;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a channel.
    /// </summary>
    public interface IChannelShim : IShim<IChannel>
    {
        int ChannelId { get; }

        Task<IChannelPermissions> GetPermissions();

        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="includingSubchannels">If set to <c>true</c>, the message is sent to subchannels as well.</param>
        Task SendMessage(string message, bool includingSubchannels = false);
    }
}


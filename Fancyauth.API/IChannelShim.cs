using System;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a channel.
    /// </summary>
    public interface IChannelShim
    {
        int ChannelId { get; }

        /// <summary>
        /// Loads information about this channel.
        /// </summary>
        Task<IChannel> Load();
        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="includingSubchannels">If set to <c>true</c>, the message is sent to subchannels as well.</param>
        Task SendMessage(string message, bool includingSubchannels = false);
    }
}


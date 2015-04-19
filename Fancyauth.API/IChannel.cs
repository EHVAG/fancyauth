using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a channel, including information about it.
    /// </summary>
    public interface IChannel : IChannelShim
    {
        string Description { get; set; }
        ISet<IChannelShim> LinkedChannels { get; }
        string Name { get; set; }
        IChannelShim Parent { get; set; }
        int Position { get; set; }
        bool Temporary { get; set; }

        /// <summary>
        /// Saves changes made to this channel.
        /// </summary>
        Task SaveChanges();
        /// <summary>
        /// Reloads this channel's information, discarding all unsaved changes.
        /// </summary>
        Task Refresh();
    }
}


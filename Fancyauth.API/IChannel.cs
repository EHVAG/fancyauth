using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a channel, including information about it. Remember to call SaveChanges()!
    /// </summary>
    public interface IChannel : IChannelShim, IReadModifyWriteObject
    {
        string Description { get; set; }
        ISet<IChannelShim> LinkedChannels { get; }
        string Name { get; set; }
        IChannelShim Parent { get; set; }
        int Position { get; set; }
        bool Temporary { get; set; }
    }
}


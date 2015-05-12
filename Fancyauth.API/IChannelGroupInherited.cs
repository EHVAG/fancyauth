using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    public interface IChannelGroupInherited : IChannelGroup
    {
        /// <summary>
        /// Disable this to get back to a clean and known set of members on this inheritance level.
        /// In other words: As we go down the group inheritance tree, this is where we override EVERYTHING and stop.
        /// </summary>
        bool IncludeMembersFromParentChannels { get; set; }
    }
}

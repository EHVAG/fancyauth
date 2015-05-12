using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    public interface IChannelGroup
    {
        string Name { get; }

        /// <summary>
        /// Whether this group is available in subchannels as well.
        /// </summary>
        bool Inheritable { get; set; }

        /// <summary>
        /// Users to be added to the group.
        /// </summary>
        ISet<int> Members { get; }

        /// <summary>
        /// Users to be removed from the group (because we inherit from a group that includes them, but don't want them in our group).
        /// </summary>
        ISet<int> NegativeMembers { get; }
    }
}

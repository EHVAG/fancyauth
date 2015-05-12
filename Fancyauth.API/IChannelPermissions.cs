using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a channel's permissions.
    /// </summary>
    public interface IChannelPermissions : IReadModifyWriteObject
    {
        /// <summary>
        /// Whether this channel should inherit ACL entries from parent channels.
        /// </summary>
        bool Inherit { get; set; }

        /// <summary>
        /// ACL entries that were inherited from parent channels. Readonly for obvious reasons.
        /// </summary>
        IReadOnlyList<IChannelACLEntryReadonly> InheritedACL { get; }

        /// <summary>
        /// ACL entries that belong to this channel.
        /// </summary>
        IList<IChannelACLEntry> ACL { get; }


        /// <summary>
        /// Groups that were inherited from parent channels. You only inherit a list of members, but you CAN modify that.
        /// </summary>
        IReadOnlyCollection<IChannelGroupInherited> InheritedGroups { get; }

        /// <summary>
        /// New groups created by this channel.
        /// </summary>
        ICollection<IChannelGroup> Groups { get; }

        /// <summary>
        /// Creates a new group. The new group IS automatically added for you.
        /// </summary>
        /// <returns>A new group.</returns>
        IChannelGroup CreateGroup(string name);
    }
}

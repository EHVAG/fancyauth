using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fancyauth.API;

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
        IReadOnlyCollection<IChannelUserGroupInherited> InheritedGroups { get; }

        /// <summary>
        /// New groups created by this channel.
        /// </summary>
        ICollection<IChannelUserGroup> Groups { get; }

        /// <summary>
        /// Creates a new group. The new group IS automatically added for you.
        /// </summary>
        /// <returns>A new group.</returns>
        IChannelUserGroup CreateGroup(string name);

        /// <summary>
        /// Creates a new ACL entry. The new entry IS automatically added for you.
        /// </summary>
        /// <returns>A new ACL entry.</returns>
        /// <param name="targetUser">The user that is going to be subjected to this ACL entry.</param>
        IChannelACLEntry CreateACLEntry(int targetUser);

        /// <summary>
        /// Creates a new ACL entry. The new entry IS automatically added for you.
        /// </summary>
        /// <returns>A new ACL entry.</returns>
        /// <param name="targetGroup">The group that is going to be subjected to this ACL entry.</param>
        IChannelACLEntry CreateACLEntry(IChannelGroup targetGroup);


        /// <summary>
        /// A group that contains every user (even guests).
        /// </summary>
        /// <value>The system group "all".</value>
        IChannelGroup SystemGroup_All { get; }

        /// <summary>
        /// A group that contains every authenticated user (no guests).
        /// </summary>
        /// <value>The system group "auth".</value>
        IChannelGroup SystemGroup_Auth { get; }

        /// <summary>
        /// A group that contains every user in the current channel (even guests).
        /// </summary>
        /// <value>The system group "in".</value>
        IChannelGroup SystemGroup_In { get; }

        /// <summary>
        /// A group that contains every user who is NOT in the current channel (even guests).
        /// </summary>
        /// <value>The system group "out".</value>
        IChannelGroup SystemGroup_Out { get; }

        // TODO: @~sub,...
    }
}

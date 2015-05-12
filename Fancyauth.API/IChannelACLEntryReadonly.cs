using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    public interface IChannelACLEntryReadonly
    {
        bool ApplyOnThisChannel { get; }
        bool ApplyOnSubchannels { get; }

        /// <summary>
        /// The user this ACL entry applies to. <c>null</c> if it applies to a group.
        /// </summary>
        int? TargetUser { get; }

        /// <summary>
        /// The gruop this ACL entry applies to. <c>null</c> if it applies to a user.
        /// </summary>
        IChannelGroup TargetGroup { get; }

        ChannelPermissions Allow { get; }

        ChannelPermissions Deny { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    public interface IChannelACLEntry : IChannelACLEntryReadonly
    {
        // C# doesn't allow you to just add setters. You have to create new properties. :/

        new bool ApplyOnThisChannel { get; set; }
        new bool ApplyOnSubchannels { get; set; }

        /// <summary>
        /// The user this ACL entry applies to. <c>null</c> if it applies to a group.
        /// </summary>
        new int? TargetUser { get; set; }

        /// <summary>
        /// The gruop this ACL entry applies to. <c>null</c> if it applies to a user.
        /// </summary>
        new IChannelGroup TargetGroup { get; set; }

        new ChannelPermissions Allow { get; set; }

        new ChannelPermissions Deny { get; set; }
    }
}

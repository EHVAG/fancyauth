using Fancyauth.APIUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fancyauth.API;

namespace Fancyauth.Plugins.Builtin
{
    public class LockedChannels : PluginBase
    {
        public override async Task OnChannelCreated(IChannel channel)
        {
            if (channel.Temporary && channel.Name.ToLowerInvariant().Contains("lock"))
            {
                var perms = await channel.GetPermissions();

                var allACL = perms.CreateACLEntry(perms.SystemGroup_All);
                allACL.Deny = ChannelPermissions.PermissionEnter | ChannelPermissions.PermissionMove;

                var inACL = perms.CreateACLEntry(perms.SystemGroup_In);
                inACL.Allow = ChannelPermissions.PermissionMove;

                await perms.SaveChanges();
                await channel.SendMessage("locked_chan successfully set up.");
            }
        }
    }
}

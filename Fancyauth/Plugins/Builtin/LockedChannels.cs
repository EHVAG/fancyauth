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
                foreach (var group in perms.InheritedGroups)
                    await channel.SendMessage("iGroup: " + group.Name);
                foreach (var group in perms.Groups)
                    await channel.SendMessage("Group: " + group.Name);
            }
        }
    }
}

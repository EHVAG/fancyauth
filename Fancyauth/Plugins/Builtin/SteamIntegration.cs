using System;
using System.Linq;
using Fancyauth.APIUtil;
using System.Threading.Tasks;
using Fancyauth.API;
using SteamKit2;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Entity;

namespace Fancyauth.Plugins.Builtin
{
    public class SteamIntegration : PluginBase
    {
        [Command]
        public async Task SetSteamId(IUser user, long steam64)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                var dbUser = await context.Users.Include(x => x.Membership).SingleAsync(x => x.Id == user.UserId);
                dbUser.Membership.SteamId = steam64;
                await context.SaveChangesAsync();
                transact.Commit();
            }

            var steamAdapter = await user.GetSteamAdapter();
            ((UserSteamAdapter)steamAdapter).SteamListener.AddFriend(new SteamID(unchecked((ulong)steam64)));

            Trace.WriteLine(steam64, "SteamIntegration added");
        }

        public async override Task OnChatMessage(IUser sender, IEnumerable<IChannelShim> channels, string message)
        {
            var allUsers = await Server.GetOnlineUsers();
            foreach (var user in allUsers.Where(u => channels.Any(c => u.CurrentChannel.ChannelId == c.ChannelId)))
            {
                var steamAdapter = await user.GetSteamAdapter();
                if (steamAdapter != null)
                {
                    var gaim = await steamAdapter.GetCurrentGame();
                    if (gaim.HasValue)
                    {
                        bool shouldForward;
                        using (var context = await FancyContext.Connect())
                            shouldForward = null != await context.SteamChatForwardingAssociations.FindAsync(user.UserId, unchecked((int)gaim.Value));
                        if (shouldForward)
                            await steamAdapter.SendMessage(string.Format("[Chat] {0}: {1}", sender.Name, message));
                    }
                }
            }
        }
    }
}


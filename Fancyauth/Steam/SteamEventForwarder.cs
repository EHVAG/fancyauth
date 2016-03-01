using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using SteamKit2;
using Fancyauth.Steam;

namespace Fancyauth.Steam
{
    public class SteamEventForwarder : ISteamEventForwarder
    {
        async Task ISteamEventForwarder.OnChatMessage(SteamListener steamListener, SteamID sender, string message)
        {
            using (var context = await FancyContext.Connect())
            {
                var steam64 = unchecked((long)sender.ConvertToUInt64());
                var currentGame = unchecked((int)steamListener.GetCurrentGameId(sender).Value);
                var uid = context.Users.Where(x => x.Membership.SteamId == steam64).Single().Id;
                if (message == "@fancy-ng forward")
                    context.SteamChatForwardingAssociations.Add(new Model.SteamChatForwardingAssociation
                    {
                        UserId = uid,
                        AppId = currentGame,
                    });
                else if (message == "@fancy-ng noforward")
                {
                    var assoc = await context.SteamChatForwardingAssociations.FindAsync(uid, currentGame);
                    if (assoc != null)
                        context.SteamChatForwardingAssociations.Remove(assoc);
                }
                else
                    steamListener.SendMessage(sender, "Unknown command");

                await context.SaveChangesAsync();
            }
        }
    }
}


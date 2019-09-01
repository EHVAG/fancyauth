using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using SteamKit2;
using Fancyauth.Steam;
using System.Data;
using Fancyauth.Wrapped;

namespace Fancyauth.Steam
{
    /// <summary>
    /// This class forwards stuff from Steam to Mumble.
    /// </summary>
    public class SteamEventForwarder : ISteamEventForwarder
    {
        public Server Server { get; set; }

        async Task ISteamEventForwarder.OnChatMessage(SteamListener steamListener, SteamID sender, string message)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var steam64 = unchecked((long)sender.ConvertToUInt64());
                var currentGame = unchecked((int)(steamListener.GetCurrentGameId(sender) ?? 0));
                var user = await context.Users.Where(x => x.Membership.SteamId == steam64).SingleAsync();
                if (message == "@fancy-ng forward")
                    context.SteamChatForwardingAssociations.Add(new Model.SteamChatForwardingAssociation
                    {
                        User = user,
                        AppId = currentGame,
                    });
                else if (message == "@fancy-ng noforward")
                {
                    var assoc = await context.SteamChatForwardingAssociations.FindAsync(user.Id, currentGame);
                    if (assoc != null)
                        context.SteamChatForwardingAssociations.Remove(assoc);
                }
                else if (message.StartsWith("@fancy-ng "))
                    steamListener.SendMessage(sender, "Unknown command. Commands are: forward, noforward");
                else
                {
                    // foward to mumble
		    var mumbleUser = (await Server.GetUsers()).Values.Single(x => x.userid == user.Id);
		    await Server.SendMessageChannel(mumbleUser.channel, false, user.Name + "(via Steam): " + message);
                }

                await context.SaveChangesAsync();
                transact.Commit();
            }
        }
    }
}


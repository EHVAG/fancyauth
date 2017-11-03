using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fancyauth.Commands;
using Fancyauth.ContextCallbacks;
using Fancyauth.Model;
using Fancyauth.Plugins;
using Fancyauth.Wrapped;

namespace Fancyauth
{
    public class ServerCallback : Wrapped.ServerCallback
    {
        private readonly Fancyauth Fancyauth;
        private readonly Steam.SteamListener SteamListener;
        private readonly Server Server;
        private readonly CommandManager CommandMgr;

        public ServerCallback(Fancyauth fancyauth, Steam.SteamListener steamListener, Server server, ContextCallbackManager contextCallbackMgr, CommandManager cmdmgr)
            : base(fancyauth.StashCallback)
        {
            Fancyauth = fancyauth;
            SteamListener = steamListener;
            Server = server;
            CommandMgr = cmdmgr;
        }

        private static readonly Regex CommandPattern = new Regex(@"[\""].+?[\""]|[^ ]+", RegexOptions.Compiled);

        public override async Task UserTextMessage(Murmur.User user, Murmur.TextMessage message)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction())
            {
                User senderEntity = null;
                if (user.userid > 0)
                    senderEntity = await context.Users.FindAsync(user.userid);

                var qtext = message.text.Replace("&quot;", "\"");
                var msg = CommandPattern.Matches(qtext).Cast<Match>().Select(m => m.Value).ToArray();
                if (message.channels.Any())
                {
                    if (msg[0] == "@fancy-ng")
                        await CommandMgr.HandleCommand(SteamListener, Server, user, msg.Skip(1));

                    var channel = context.Channels.SingleAsync(a => a.ServerId == message.channels[0]);

                    if (senderEntity != null)
                        context.Logs.Add(new LogEntry.ChatMessage { When = DateTimeOffset.Now, Who = senderEntity, Message = message.text });
                }

                if (senderEntity != null)
                {
                    var messagesInTheLastSeconds = await context.Logs.OfType<LogEntry.ChatMessage>()
                        .Where(x => x.Who.Id == senderEntity.Id && x.When > DbFunctions.AddSeconds(DateTimeOffset.Now, -5)).CountAsync();
                    if (messagesInTheLastSeconds >= 3)
                        await Server.KickUser(user.session, "Who are you, my evil twin?! [stop spamming]");
                }

                await context.SaveChangesAsync();
                transact.Commit();
            }
        }

        public override async Task UserConnected(Murmur.User user)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var userNotificationsQuery = from usr in context.Users.Include(x => x.PersistentGuest.Godfathers)
                                                                      .Include(x => x.GuestInvite.Inviter).Include(x => x.Membership)
                                             where usr.Id == user.userid
                                             join evt in context.Logs.OfType<LogEntry.Connected>() on usr.Id equals evt.Who.Id into connectedEvents
                                             let lastConnection = connectedEvents.Max(x => x.When)
                                             join notific in context.OfflineNotifications on usr.Id equals notific.Recipient.Id into notifications
                                             select new { usr, notifications = notifications.Where(x => x.When > lastConnection) };
                var res = await userNotificationsQuery.SingleAsync();
                foreach (var notify in res.notifications)
                    await Server.SendMessage(user.session, notify.Message);

                if (res.usr.Membership == null)
                {
                    var onlineUsers = await Server.GetUsers();
                    var godfathers = res.usr.PersistentGuest?.Godfathers?.Select(x => x.UserId) ?? new[] { res.usr.GuestInvite.Inviter.Id };
                    if (!godfathers.Intersect(onlineUsers.Select(x => x.Value.userid)).Any())
                    {
                        await Server.KickUser(user.session, "Inviter not online.");
                        return;
                    }
                }

                // TODO: move guests to the guest channel

                context.Logs.Add(new LogEntry.Connected
                {
                    When = DateTimeOffset.Now,
                    Who = res.usr,
                });
                await context.SaveChangesAsync();
                transact.Commit();
            }
        }

        public override async Task UserDisconnected(Murmur.User user)
        {
            Model.UserAttribute.CertificateCredentials cc;
            Fancyauth.GuestCredentials.TryRemove(user.userid, out cc);

            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction())
            {
                context.Logs.Add(new LogEntry.Disconnected
                {
                    When = DateTimeOffset.Now,
                    Who = context.Users.Attach(new User { Id = user.userid }),
                });

                await context.SaveChangesAsync();
                transact.Commit();
            }
        }

        public override async Task ChannelCreated(Murmur.Channel chan)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction())
            {
                var dbchan = context.Channels.Add(new Channel
                {
                    Temporary = chan.temporary,
                    Parent = await context.Channels.Where(x => x.ServerId == chan.parent).SingleAsync(),
                    ServerId = chan.id,
                });
                context.ChannelInfoChanges.Add(new Channel.InfoChange
                {
                    Channel = dbchan,
                    Name = chan.name,
                    Description = chan.description,
                    When = DateTimeOffset.Now
                });

                await context.SaveChangesAsync();
                transact.Commit();
            }
        }

        public override async Task ChannelStateChanged(Murmur.Channel chan)
        {
            using (var context = await FancyContext.Connect())
            {
                var query = from channel in context.Channels
                            where channel.ServerId == chan.id
                            join ichange in context.ChannelInfoChanges on channel.Id equals ichange.Channel.Id into infoChanges
                            select new
                            {
                                channel,
                                parentId = channel.Parent.ServerId,
                                name = infoChanges.OrderByDescending(x => x.When).Select(x => x.Name).Where(x => x != null).FirstOrDefault(),
                                desc = infoChanges.OrderByDescending(x => x.When).Select(x => x.Description).Where(x => x != null).FirstOrDefault(),
                            };
                var res = await query.SingleAsync();
                var infoChange = new Channel.InfoChange
                {
                    Channel = res.channel,
                    Name = chan.name == res.name ? null : chan.name,
                    Description = chan.description == res.desc ? null : chan.description,
                    When = DateTimeOffset.Now,
                };

                if (res.parentId != chan.parent)
                    res.channel.Parent = await context.Channels.Where(x => x.ServerId == chan.parent).SingleAsync();

                if (infoChange.Name != null || infoChange.Description != null)
                    context.ChannelInfoChanges.Add(infoChange);

                await context.SaveChangesAsync();
            }
        }

        public override async Task ChannelRemoved(Murmur.Channel chan)
        {
            using (var context = await FancyContext.Connect())
            {
                var channel = await context.Channels.Where(x => x.ServerId == chan.id).SingleAsync();
                channel.ServerId = null;

                context.ChannelInfoChanges.Add(new Channel.InfoChange
                {
                    Channel = channel,
                    Name = null,
                    Description = null,
                    When = DateTimeOffset.Now,
                });

                await context.SaveChangesAsync();
            }
        }
    }
}

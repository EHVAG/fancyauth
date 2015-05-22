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
        private readonly Server Server;
        private readonly CommandManager CommandMgr;

        public ServerCallback(Server server, ContextCallbackManager contextCallbackMgr, CommandManager cmdmgr, Action<Task> asyncCompleter)
            : base(asyncCompleter)
        {
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
                        await CommandMgr.HandleCommand(Server, user, msg.Skip(1));

                    if (senderEntity != null)
                        context.Logs.Add(new LogEntry.ChatMessage { When = DateTimeOffset.Now, WhoU = senderEntity, Message = message.text });
                }

                if (senderEntity != null)
                {
                    var messagesInTheLastSeconds = await context.Logs.OfType<LogEntry.ChatMessage>()
                        .Where(x => x.WhoUId == senderEntity.Id && x.When > DbFunctions.AddSeconds(DateTimeOffset.Now, -5)).CountAsync();
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
                var logEntry = context.Logs.Add(new LogEntry.Connected { When = DateTimeOffset.Now });

                if (user.userid > 0)
                {
                    var userNotificationsQuery = from usr in context.Users
                                                 where usr.Id == user.userid
                                                 join evt in context.Logs.OfType<LogEntry.Connected>() on usr.Id equals evt.WhoU.Id into connectedEvents
                                                 let lastConnection = connectedEvents.Max(x => x.When)
                                                 join notific in context.OfflineNotifications on usr.Id equals notific.Recipient.Id into notifications
                                                 select new { usr, notifications = notifications.Where(x => x.When > lastConnection) };
                    var res = await userNotificationsQuery.SingleAsync();
                    foreach (var notify in res.notifications)
                        await Server.SendMessage(user.session, notify.Message);

                    logEntry.WhoU = res.usr;
                }
                else
                {
                    // guest handoff
                    var assoc = await context.GuestAssociations.FindAsync(user.name);
                    assoc.Session = user.session;

                    logEntry.WhoI = assoc.Invite;

                    await Server.AddUserToGroup(0, user.session, "Gast"); // add guests to the guest group
                    // TODO: move guests to the guest channel
                }

                context.Logs.Add(logEntry);
                await context.SaveChangesAsync();
                transact.Commit();
            }
        }

        public override async Task UserDisconnected(Murmur.User user)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction())
            {
                var log = context.Logs.Add(new LogEntry.Disconnected { When = DateTimeOffset.Now });
                if (user.userid > 0)
                    log.WhoU = context.Users.Attach(new User { Id = user.userid });
                else
                {
                    // remove guest assoc
                    var assoc = await context.GuestAssociations.FindAsync(user.name);
                    context.GuestAssociations.Remove(assoc);
                    log.WhoI = assoc.Invite;
                }

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


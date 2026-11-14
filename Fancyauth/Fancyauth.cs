using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fancyauth.Commands;
using Fancyauth.ContextCallbacks;
using Fancyauth.Plugins;
using Fancyauth.Wrapped;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Fancyauth.Model;
using System.Data.Entity;
using Fancyauth.Steam;
using System.Collections.Concurrent;

namespace Fancyauth
{
    public class Fancyauth
    {
        public ConcurrentDictionary<int, Model.UserAttribute.CertificateCredentials> GuestCredentials { get; private set; }

        public Fancyauth()
        {
            GuestCredentials = new ConcurrentDictionary<int, Model.UserAttribute.CertificateCredentials>();
        }

        /*
         * TODO: bind callbacks to main thread, just like in AWWS
         * a little weird with ICE's callback threads
         * so right now our temporary solution is ContinueWith
         */

        public async Task ServerMain()
        {
            Ice.Communicator communicator = null;
            try {
                var initdat = new Ice.InitializationData();
                initdat.dispatcher = (act, conn) => Task.Run(act); // prevent weird deadlocks
                var iceProps = Ice.Util.createProperties();
                iceProps.setProperty("Ice.MessageSizeMax", "10240");
                // fancyauth exits when its connection to Mumble closes (below), so Ice must not close it when idle: heartbeats instead.
                iceProps.setProperty("Ice.ACM.Client.Close", "0"); // CloseOff
                iceProps.setProperty("Ice.ACM.Client.Heartbeat", "3"); // HeartbeatAlways
                // While waiting for Mumble, an attempt at an address that's gone gives up after 2 s instead of minutes.
                iceProps.setProperty("Ice.Override.ConnectTimeout", "2000");
                // Mumble's icesecretwrite, sent with every call.
                var iceSecret = Settings.Optional("MUMBLE_ICE_SECRET");
                if (iceSecret != null)
                    iceProps.setProperty("Ice.ImplicitContext", "Shared");
                initdat.properties = iceProps;
                communicator = Ice.Util.initialize(initdat);
                if (iceSecret != null)
                    communicator.getImplicitContext().put("secret", iceSecret);

                // Where Mumble calls back: an address Mumble can reach, as it registers this endpoint.
                var adapter = communicator.createObjectAdapterWithEndpoints("Callback.Client",
                    Settings.Optional("FANCYAUTH_CALLBACK_ENDPOINT", "tcp -h 127.0.1.2 -p 31338"));
                // Mumble may be starting or restarting: wait for it instead of failing, which would only make Kubernetes wait longer.
                MumbleServer.MetaPrx meta;
                MumbleServer.ServerPrx rawServer;
                for (var waiting = false; ; waiting = true)
                {
                    try
                    {
                        meta = MumbleServer.MetaPrxHelper.checkedCast(communicator.stringToProxy(
                            Settings.Optional("MUMBLE_ICE", "Meta:tcp -h 127.0.1.1 -p 6502")));
                        rawServer = await meta.getServerAsync(1);
                        break;
                    }
                    catch (Ice.LocalException e)
                    {
                        if (!waiting)
                            Console.WriteLine($"waiting for Mumble, retrying every 2 s: {e.GetType().Name}");
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }

                var server = new Server(rawServer);

                // Steam is optional: without credentials, or if its login fails, fancyauth runs without it.
                var steamListener = new SteamListener(Settings.Optional("STEAM_USER"), Settings.Optional("STEAM_PASSWORD"),
                    new SteamEventForwarder { Server = server }, StashCallback);
                var steamTask = steamListener.Run();

                var cmdmgr = new CommandManager();
                var contextCallbackMgr = new ContextCallbackManager(steamListener, server, adapter, StashCallback);
                var pluginMan = new PluginManager(this, steamListener, server, contextCallbackMgr, cmdmgr);
                var asci = adapter.addWithUUID(new ServerCallback(this, steamListener, server, contextCallbackMgr, cmdmgr));
                var asci2 = adapter.addWithUUID(pluginMan);
                var authenticator = adapter.addWithUUID(new Authenticator(this));
                adapter.activate();

                await server.AddCallback(MumbleServer.ServerCallbackPrxHelper.uncheckedCast(asci));
                await server.AddCallback(MumbleServer.ServerCallbackPrxHelper.uncheckedCast(asci2));
                await server.SetAuthenticator(MumbleServer.ServerAuthenticatorPrxHelper.uncheckedCast(authenticator));

                // Mumble forgets its callbacks and authenticator when it restarts, and then refuses every login (Mumble 1.5 on
                // Postgres even stops on a known user's login). So exit when the connection closes: Kubernetes starts fancyauth again,
                // which waits for Mumble above and registers anew. Called at once if the connection has closed meanwhile.
                (await rawServer.ice_getConnectionAsync()).setCloseCallback(_ =>
                {
                    Console.WriteLine("connection to Mumble closed: exiting, to register again after the restart");
                    Environment.Exit(1);
                });

                await UpdateChannelModel(server);

                await Task.Yield();
                Console.WriteLine("server up");

                communicator.waitForShutdown();

                await steamTask;
            } finally {
                //communicator.destroy();
            }
        }

        public async Task UpdateChannelModel(Server server)
        {
            var tree = await server.GetTree();
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction())
            {
                var allChanQuery = from channel in context.Channels
                                   where channel.ServerId.HasValue
                                   join ichange in context.ChannelInfoChanges on channel.Id equals ichange.Channel.Id into infoChanges
                                   select new
                                   {
                                       channel,
                                       name = infoChanges.OrderByDescending(x => x.When).Select(x => x.Name).Where(x => x != null).FirstOrDefault(),
                                       desc = infoChanges.OrderByDescending(x => x.When).Select(x => x.Description).Where(x => x != null).FirstOrDefault(),
                                   };
                var allChans = await allChanQuery.ToArrayAsync();

                var hitChans = new List<Channel>();
                var treewalk = new Queue<MumbleServer.Tree>();
                treewalk.Enqueue(tree);
                while (treewalk.Any())
                {
                    var current = treewalk.Dequeue();

                    var dbChanBig = allChans.Where(x => x.channel.ServerId == current.c.id).SingleOrDefault();
                    var dbChan = dbChanBig == null ? null : dbChanBig.channel;
                    if (dbChan == null)
                    {
                        dbChan = context.Channels.Add(new Channel
                        {
                            Temporary = current.c.temporary,
                            Parent = current.c.parent == -1 ? null : hitChans.Where(x => x.ServerId == current.c.parent).Single(),
                            ServerId = current.c.id,
                        });
                        context.ChannelInfoChanges.Add(new Channel.InfoChange
                        {
                            Channel = dbChan,
                            Name = current.c.name,
                            Description = current.c.description,
                            When = DateTimeOffset.Now
                        });
                    }
                    else if ((dbChanBig.name != current.c.name) || (dbChanBig.desc != current.c.description))
                    {
                        // existing, but modified
                        context.ChannelInfoChanges.Add(new Channel.InfoChange
                        {
                            Channel = dbChan,
                            Name = current.c.name == dbChanBig.name ? null : current.c.name,
                            Description = current.c.description == dbChanBig.desc ? null : current.c.description,
                            When = DateTimeOffset.Now,
                        });
                    }

                    hitChans.Add(dbChan);

                    foreach (var child in current.children)
                        treewalk.Enqueue(child);
                }

                foreach (var channel in allChans.Select(x => x.channel).Except(hitChans))
                {
                    channel.ServerId = null;

                    context.ChannelInfoChanges.Add(new Channel.InfoChange
                    {
                        Channel = channel,
                        Name = null,
                        Description = null,
                        When = DateTimeOffset.Now,
                    });
                }

                await context.SaveChangesAsync();

                transact.Commit();
            }
        }

        public void StashCallback(Task t)
        {
            t.ContinueWith(callback => {
                var exceptions = callback.Exception?.Flatten()?.InnerExceptions;
                if (exceptions == null)
                    return;

                foreach (var exception in exceptions)
                {
                    System.Diagnostics.Trace.WriteLine(callback.Exception, "Async callback exception");
                    var dbValidation = exception as System.Data.Entity.Validation.DbEntityValidationException;
                    if (dbValidation != null)
                    {
                        foreach (var result in dbValidation.EntityValidationErrors)
                        {
                            foreach (var error in result.ValidationErrors)
                            {
                                System.Diagnostics.Trace.WriteLine(String.Format("{0} property {1} error: {2}", PrintDbPropertyValues(result.Entry.CurrentValues), error.PropertyName, error.ErrorMessage));
                            }
                        }
                    }
                }
            }, TaskContinuationOptions.None);
        }
        public static string PrintDbPropertyValues(object o)
        {
            var dpv = o as System.Data.Entity.Infrastructure.DbPropertyValues;
            if (dpv != null)
                return "{ " + String.Join(", ", dpv.PropertyNames.Select(x => String.Format("{0} = {1}", x, PrintDbPropertyValues(dpv[x])))) + " }";
            else if (o == null)
                return "null";
            else
                return "\"" + o.ToString() + "\"";
        }
    }
}


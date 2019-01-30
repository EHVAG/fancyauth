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
                communicator = Ice.Util.initialize(initdat);

                #if DEBUG
                var adapter = communicator.createObjectAdapterWithEndpoints("Callback.Client", "tcp -h 127.0.1.1 -p 31338");
                var rawServer = Murmur.ServerPrxHelper.checkedCast(communicator.stringToProxy("s/1 -t -e 1.1:tcp -h 127.0.0.1 -p 31337"));
                #else
                var adapter = communicator.createObjectAdapterWithEndpoints("Callback.Client", "tcp -h 127.0.1.2 -p 31338");
                var meta = Murmur.MetaPrxHelper.checkedCast(communicator.stringToProxy("Meta:tcp -h 127.0.1.1 -p 6502"));
                var rawServer = await FixIce.FromAsync(1, meta.begin_getServer, meta.end_getServer);
                #endif

                Console.Write("Steam user: ");
                var steamUser = Console.ReadLine();
                Console.Write("Steam password: ");
                var steamPassword = Console.ReadLine();
                var steamListener = new SteamListener(steamUser, steamPassword, () =>
                {
                    Console.Write("Steam guard code: ");
                    return Console.ReadLine();
                }, new SteamEventForwarder(), StashCallback);
                var steamTask = steamListener.Run();

                var server = new Server(rawServer);

                var cmdmgr = new CommandManager();
                var contextCallbackMgr = new ContextCallbackManager(steamListener, server, adapter, StashCallback);
                var pluginMan = new PluginManager(this, steamListener, server, contextCallbackMgr, cmdmgr);
                var asci = adapter.addWithUUID(new ServerCallback(this, steamListener, server, contextCallbackMgr, cmdmgr));
                var asci2 = adapter.addWithUUID(pluginMan);
                var authenticator = adapter.addWithUUID(new Authenticator(this));
                adapter.activate();

                await server.AddCallback(Murmur.ServerCallbackPrxHelper.uncheckedCast(asci));
                await server.AddCallback(Murmur.ServerCallbackPrxHelper.uncheckedCast(asci2));
                await server.SetAuthenticator(Murmur.ServerAuthenticatorPrxHelper.uncheckedCast(authenticator));

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
                                   join ichange in context.ChannelInfoChanges on channel.Id equals ichange.Channel.Id into infoChanges
                                   select new
                                   {
                                       channel,
                                       name = infoChanges.OrderByDescending(x => x.When).Select(x => x.Name).Where(x => x != null).FirstOrDefault(),
                                       desc = infoChanges.OrderByDescending(x => x.When).Select(x => x.Description).Where(x => x != null).FirstOrDefault(),
                                   };
                var allChans = await allChanQuery.ToArrayAsync();

                var hitChans = new List<Channel>();
                var treewalk = new Queue<Murmur.Tree>();
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


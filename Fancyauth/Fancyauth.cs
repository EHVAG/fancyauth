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

namespace Fancyauth
{
    public class Fancyauth
    {
        public Fancyauth()
        {
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

                var server = new Server(rawServer);

                var cmdmgr = new CommandManager();
                var contextCallbackMgr = new ContextCallbackManager(server, adapter, StashCallback);
                var pluginMan = new PluginManager(StashCallback, server, contextCallbackMgr, cmdmgr);
                var asci = adapter.addWithUUID(new ServerCallback(server, contextCallbackMgr, cmdmgr, StashCallback));
                var asci2 = adapter.addWithUUID(pluginMan);
                var authenticator = adapter.addWithUUID(new Authenticator());
                adapter.activate();

                await server.AddCallback(Murmur.ServerCallbackPrxHelper.uncheckedCast(asci));
                await server.AddCallback(Murmur.ServerCallbackPrxHelper.uncheckedCast(asci2));
                await server.SetAuthenticator(Murmur.ServerAuthenticatorPrxHelper.uncheckedCast(authenticator));

                await Task.Yield();
                Console.WriteLine("server up");

                communicator.waitForShutdown();
            } finally {
                //communicator.destroy();
            }
        }

        public void StashCallback(Task t)
        {
            t.ContinueWith(callback => System.Diagnostics.Trace.WriteLineIf(callback.Exception != null, callback.Exception, "Async callback exception"), TaskContinuationOptions.None);
        }
    }
}


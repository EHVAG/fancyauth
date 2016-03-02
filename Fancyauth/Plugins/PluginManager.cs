using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.API.Commands;
using Fancyauth.API.ContextCallbacks;
using Fancyauth.Commands;
using Fancyauth.ContextCallbacks;
using Fancyauth.API.DB;
using Fancyauth.Model;

namespace Fancyauth.Plugins
{
    public class PluginManager : Wrapped.ServerCallback
    {
        [Export]
        public IServer Server { get; private set; }

        [Export]
        public IContextCallbackManager ContextCallbackManager { get; private set; }

        [Export]
        public ICommandManager CommandManager { get; private set; }

        [ImportMany]
        public IEnumerable<IFancyPlugin> Plugins { get; private set; }

        private readonly Wrapped.Server WServer;

        public PluginManager(Action<Task> asyncCompleter, Wrapped.Server wserver, ContextCallbackManager ccmgr, CommandManager cmdmgr)
            : base(asyncCompleter)
        {
            WServer = wserver;
            Server = new ServerWrapper(wserver);
            ContextCallbackManager = ccmgr;
            CommandManager = cmdmgr;

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            if (Directory.Exists("plugins"))
                catalog.Catalogs.Add(new DirectoryCatalog("plugins"));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public Task Init() {
            return Task.WhenAll(Plugins.Select(p => p.Init()));
        }

        public override Task UserConnected(Murmur.User user)
        {
            var apiUser = new UserWrapper(WServer, user);
            return Task.WhenAll(Plugins.Select(x => x.OnUserConnected(apiUser)));
        }

        public override Task UserStateChanged(Murmur.User user)
        {
            var apiUser = new UserWrapper(WServer, user);
            return Task.WhenAll(Plugins.Select(x => x.OnUserModified(apiUser)));
        }

        public override Task UserDisconnected(Murmur.User user)
        {
            var apiUser = new UserWrapper(WServer, user);
            return Task.WhenAll(Plugins.Select(x => x.OnUserDisconnected(apiUser)));
        }

        public override Task ChannelCreated(Murmur.Channel chan)
        {
            var apiChannel = new ChannelWrapper(WServer, chan);
            return Task.WhenAll(Plugins.Select(x => x.OnChannelCreated(apiChannel)));
        }

        public override Task ChannelStateChanged(Murmur.Channel chan)
        {
            var apiChannel = new ChannelWrapper(WServer, chan);
            return Task.WhenAll(Plugins.Select(x => x.OnChannelModified(apiChannel)));
        }

        public override Task ChannelRemoved(Murmur.Channel chan)
        {
            var apiChannel = new ChannelWrapper(WServer, chan);
            return Task.WhenAll(Plugins.Select(x => x.OnChannelDeleted(apiChannel)));
        }

        public override Task UserTextMessage(Murmur.User user, Murmur.TextMessage message)
        {
            var apiUser = new UserWrapper(WServer, user);
            var destChans = message.channels.Select(x => new ChannelShim(WServer, x)).ToArray();
            return Task.WhenAll(Plugins.Select(x => x.OnChatMessage(apiUser, destChans, message.text)));
        }

        [Export(typeof(IFancyContextProvider))]
        internal class FancyContextProvider : IFancyContextProvider
        {
            async Task<FancyContextBase> IFancyContextProvider.Connect()
            {
                return await FancyContext.Connect();
            }
        }
    }
}


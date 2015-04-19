using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fancyauth.API.ContextCallbacks;
using Fancyauth.Wrapped;

namespace Fancyauth.ContextCallbacks
{
    public class ContextCallbackManager : ServerContextCallback, IContextCallbackManager
    {
        private readonly Wrapped.Server Server;
        private readonly Murmur.ServerContextCallbackPrx IcePrx;
        private readonly Dictionary<string, IServerContextCallback> ServerCallbacks = new Dictionary<string, IServerContextCallback>();
        private readonly Dictionary<string, IChannelContextCallback> ChannelCallbacks = new Dictionary<string, IChannelContextCallback>();
        private readonly Dictionary<string, IUserContextCallback> UserCallbacks = new Dictionary<string, IUserContextCallback>();

        public ContextCallbackManager(Wrapped.Server server, Ice.ObjectAdapter adapter, Action<Task> asyncCompleter)
            : base(asyncCompleter)
        {
            Server = server;
            IcePrx = Murmur.ServerContextCallbackPrxHelper.uncheckedCast(adapter.addWithUUID(this));
        }

        public override Task ContextAction(string action, Murmur.User usr, int session, int channelid)
        {
            var apiUser = new Plugins.UserWrapper(Server, usr);

            IServerContextCallback server;
            if (ServerCallbacks.TryGetValue(action, out server))
                return server.Run(apiUser);

            IChannelContextCallback channel;
            if (ChannelCallbacks.TryGetValue(action, out channel))
                return channel.Run(apiUser, new Plugins.ChannelShim(Server, channelid));

            IUserContextCallback user;
            if (UserCallbacks.TryGetValue(action, out user))
                return user.Run(apiUser, new Plugins.UserShim(Server, session));

            System.Diagnostics.Trace.WriteLine(action, "Unknown context callback action");
            return Task.FromResult<object>(null);
        }

        public Task Register(API.IUserShim user, IContextCallback callback)
        {
            return RegisterInternal(user, callback.GetType(), callback);
        }

        public Task Register<TCallback>(API.IUserShim ushim) where TCallback : IContextCallback, new()
        {
            return RegisterInternal(ushim, typeof(TCallback), null);
        }

        private async Task RegisterInternal(API.IUserShim ushim, Type cbType, IContextCallback refmatch)
        {
            var session = ushim.SessionId;

            if (typeof(IServerContextCallback).IsAssignableFrom(cbType))
                await RegisterInternalInternal(session, ServerCallbacks, CallbackContext.ContextServer, cbType, (IServerContextCallback)refmatch);

            if (typeof(IChannelContextCallback).IsAssignableFrom(cbType))
                await RegisterInternalInternal(session, ChannelCallbacks, CallbackContext.ContextChannel, cbType, (IChannelContextCallback)refmatch);

            if (typeof(IUserContextCallback).IsAssignableFrom(cbType))
                await RegisterInternalInternal(session, UserCallbacks, CallbackContext.ContextUser, cbType, (IUserContextCallback)refmatch);
        }

        private Task RegisterInternalInternal<TBase>(int session, Dictionary<string, TBase> cbs,
            CallbackContext ictx, Type tbase, TBase refmatch) where TBase : class, IContextCallback
        {
            var existingEntry = cbs.Values.Where(x => (refmatch == null) ? tbase.IsInstanceOfType(x) : (x == refmatch)).SingleOrDefault();
            if (existingEntry == null) {
                existingEntry = (refmatch == null) ? (TBase)Activator.CreateInstance(tbase) : refmatch;
                cbs.Add(existingEntry.Name, existingEntry);
            }

            return Server.AddContextCallback(session, existingEntry.Name, existingEntry.Text, IcePrx, ictx);
        }
    }
}


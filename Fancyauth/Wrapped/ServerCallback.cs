using System;
using System.Threading.Tasks;

namespace Fancyauth.Wrapped
{
    public class ServerCallback : MumbleServer.ServerCallbackDisp_
    {
        private static readonly Task NullTask = Task.FromResult<object>(null);

        private readonly Action<Task> AsyncCompleter;

        public ServerCallback(Action<Task> asyncCompleter)
        {
            AsyncCompleter = asyncCompleter;
        }

        public virtual Task UserConnected(MumbleServer.User user) { return NullTask; }
        public virtual Task UserDisconnected(MumbleServer.User user) { return NullTask; }
        public virtual Task UserStateChanged(MumbleServer.User user) { return NullTask; }
        public virtual Task UserTextMessage(MumbleServer.User user, MumbleServer.TextMessage message) { return NullTask; }
        public virtual Task ChannelCreated(MumbleServer.Channel chan) { return NullTask; }
        public virtual Task ChannelRemoved(MumbleServer.Channel chan) { return NullTask; }
        public virtual Task ChannelStateChanged(MumbleServer.Channel chan) { return NullTask; }

        public sealed override Task userConnectedAsync(MumbleServer.User state, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(UserConnected(state)));
            return Task.CompletedTask;
        }

        public sealed override Task userDisconnectedAsync(MumbleServer.User state, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(UserDisconnected(state)));
            return Task.CompletedTask;
        }

        public sealed override Task userStateChangedAsync(MumbleServer.User state, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(UserStateChanged(state)));
            return Task.CompletedTask;
        }

        public sealed override Task userTextMessageAsync(MumbleServer.User state, MumbleServer.TextMessage message, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(UserTextMessage(state, message)));
            return Task.CompletedTask;
        }

        public sealed override Task channelCreatedAsync(MumbleServer.Channel state, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(ChannelCreated(state)));
            return Task.CompletedTask;
        }

        public sealed override Task channelRemovedAsync(MumbleServer.Channel state, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(ChannelRemoved(state)));
            return Task.CompletedTask;
        }

        public sealed override Task channelStateChangedAsync(MumbleServer.Channel state, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(ChannelStateChanged(state)));
            return Task.CompletedTask;
        }
    }
}


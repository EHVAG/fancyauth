using System;
using System.Threading.Tasks;

namespace Fancyauth.Wrapped
{
    public class ServerCallback : Murmur.ServerCallbackDisp_
    {
        private static readonly Task NullTask = Task.FromResult<object>(null);

        private readonly Action<Task> AsyncCompleter;

        public ServerCallback(Action<Task> asyncCompleter)
        {
            AsyncCompleter = asyncCompleter;
        }

        public virtual Task UserConnected(Murmur.User user) { return NullTask; }
        public virtual Task UserDisconnected(Murmur.User user) { return NullTask; }
        public virtual Task UserStateChanged(Murmur.User user) { return NullTask; }
        public virtual Task UserTextMessage(Murmur.User user, Murmur.TextMessage message) { return NullTask; }
        public virtual Task ChannelCreated(Murmur.Channel chan) { return NullTask; }
        public virtual Task ChannelRemoved(Murmur.Channel chan) { return NullTask; }
        public virtual Task ChannelStateChanged(Murmur.Channel chan) { return NullTask; }

        public sealed override void userConnected_async(Murmur.AMD_ServerCallback_userConnected cb__, Murmur.User state, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(UserConnected(state));
        }

        public sealed override void userDisconnected_async(Murmur.AMD_ServerCallback_userDisconnected cb__, Murmur.User state, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(UserDisconnected(state));
        }

        public sealed override void userStateChanged_async(Murmur.AMD_ServerCallback_userStateChanged cb__, Murmur.User state, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(UserStateChanged(state));
        }

        public sealed override void userTextMessage_async(Murmur.AMD_ServerCallback_userTextMessage cb__, Murmur.User state, Murmur.TextMessage message, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(UserTextMessage(state, message));
        }

        public sealed override void channelCreated_async(Murmur.AMD_ServerCallback_channelCreated cb__, Murmur.Channel state, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(ChannelCreated(state));
        }

        public sealed override void channelRemoved_async(Murmur.AMD_ServerCallback_channelRemoved cb__, Murmur.Channel state, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(ChannelRemoved(state));
        }

        public sealed override void channelStateChanged_async(Murmur.AMD_ServerCallback_channelStateChanged cb__, Murmur.Channel state, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(ChannelStateChanged(state));
        }
    }
}


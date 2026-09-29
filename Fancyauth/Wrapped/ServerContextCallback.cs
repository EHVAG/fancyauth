using System;
using System.Threading.Tasks;

namespace Fancyauth.Wrapped
{
    public class ServerContextCallback : MumbleServer.ServerContextCallbackDisp_
    {
        private static readonly Task NullTask = Task.FromResult<object>(null);

        private readonly Action<Task> AsyncCompleter;

        public ServerContextCallback(Action<Task> asyncCompleter)
        {
            AsyncCompleter = asyncCompleter;
        }

        public virtual Task ContextAction(string action, MumbleServer.User usr, int session, int channelid) { return NullTask; }

        public sealed override Task contextActionAsync(string action, MumbleServer.User usr, int session, int channelid, Ice.Current current__)
        {
            Task.Run(() => AsyncCompleter(ContextAction(action, usr, session, channelid)));
            return Task.CompletedTask;
        }
    }
}


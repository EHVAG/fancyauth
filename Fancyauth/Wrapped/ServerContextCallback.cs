using System;
using System.Threading.Tasks;

namespace Fancyauth.Wrapped
{
    public class ServerContextCallback : Murmur.ServerContextCallbackDisp_
    {
        private static readonly Task NullTask = Task.FromResult<object>(null);

        private readonly Action<Task> AsyncCompleter;

        public ServerContextCallback(Action<Task> asyncCompleter)
        {
            AsyncCompleter = asyncCompleter;
        }

        public virtual Task ContextAction(string action, Murmur.User usr, int session, int channelid) { return NullTask; }

        public sealed override void contextAction_async(Murmur.AMD_ServerContextCallback_contextAction cb__, string action, Murmur.User usr, int session, int channelid, Ice.Current current__)
        {
            cb__.ice_response();
            AsyncCompleter(ContextAction(action, usr, session, channelid));
        }
    }
}


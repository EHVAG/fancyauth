using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.APIUtil;

namespace Fancyauth.Plugins.Builtin
{
    // how it all began
    public class FancyText : PluginBase
    {
        [Command]
        public Task Text(IUser user, IEnumerable<string> rest)
        {
            return user.CurrentChannel.SendMessage(String.Format(@"༼ つ◕_◕༽つ {0} ༼ つ◕_◕༽つ", String.Join(" ", rest).ToUpperInvariant()));
        }
    }
}


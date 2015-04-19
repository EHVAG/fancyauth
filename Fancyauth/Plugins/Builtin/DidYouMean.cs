using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fancyauth.APIUtil;

namespace Fancyauth.Plugins.Builtin
{
    public class DidYouMean : PluginBase
    {
        public override async Task OnChatMessage(API.IUser sender, IEnumerable<API.IChannelShim> channels, string message)
        {
            // TODO: dbize this
            var msgLower = message.ToLowerInvariant();
            if (msgLower.Contains("dignitas"))
                foreach (var chan in channels)
                    await chan.SendMessage("*Dignitrash");
            else if (msgLower.Contains("exploring"))
                foreach (var chan in channels)
                    await chan.SendMessage("*exploiting");
        }
    }
}


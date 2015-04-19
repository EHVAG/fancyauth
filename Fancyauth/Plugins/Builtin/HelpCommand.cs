using System;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.APIUtil;

namespace Fancyauth.Plugins.Builtin
{
    public class HelpCommand : PluginBase
    {
        [Command]
        public Task Help(IUser user)
        {
            return user.SendMessage("Available commands: " + String.Join(", ", CommandManager.Commands.Keys));
        }
    }
}


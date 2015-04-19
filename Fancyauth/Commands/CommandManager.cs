using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fancyauth.API.Commands;

namespace Fancyauth.Commands
{
    public class CommandManager : ICommandManager
    {
        public IReadOnlyDictionary<string, CommandFunc> Commands { get { return _Commands; } }
        private readonly Dictionary<string, CommandFunc> _Commands = new Dictionary<string, CommandFunc>();

        public void RegisterCommand(string name, CommandFunc func)
        {
            _Commands.Add(name, func);
        }

        public Task HandleCommand(Wrapped.Server server, Murmur.User caller, IEnumerable<string> cmd)
        {
            CommandFunc func;
            if (_Commands.TryGetValue(cmd.FirstOrDefault(), out func))
                return func(new Plugins.UserWrapper(server, caller), cmd.Skip(1));
            else
                return server.SendMessage(caller.session, "Unknown command");
        }
    }
}


using System;
using System.Collections.Generic;

namespace Fancyauth.API.Commands
{
    public interface ICommandManager
    {
        IReadOnlyDictionary<string, CommandFunc> Commands { get; }
        void RegisterCommand(string name, CommandFunc func);
    }
}


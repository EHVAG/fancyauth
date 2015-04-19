using System;
using System.Threading.Tasks;

namespace Fancyauth.API.ContextCallbacks
{
    public interface IContextCallback
    {
        string Name { get; }
        string Text { get; }
    }
}


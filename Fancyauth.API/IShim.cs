using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a <see cref="IReadModifyWriteObject"/> about which no information has been loaded yet.
    /// </summary>
    /// <typeparam name="T">The type of this object once it's loaded.</typeparam>
    public interface IShim<T> where T : IReadModifyWriteObject
    {
        /// <summary>
        /// Loads information about this shim.
        /// </summary>
        /// <returns>The associated <see cref="IReadModifyWriteObject"/></returns>
        Task<T> Load();
    }
}

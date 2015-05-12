using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a common pattern in the Murmur API:
    /// Data objects you can load, modify locally and then send back to the server to apply your changes.
    /// </summary>
    /// <remarks>
    /// As most operations only require the object's ID, they are available on the corresponding <see cref="IShim{T}"/>.
    /// </remarks>
    public interface IReadModifyWriteObject
    {
        /// <summary>
        /// Saves changes made to this object. As long as you don't call this, your changes aren't applied.
        /// </summary>
        Task SaveChanges();

        /// <summary>
        /// Reloads information about this user, discarding all unsaved changes.
        /// </summary>
        Task Refresh();
    }
}

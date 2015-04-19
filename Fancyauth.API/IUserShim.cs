using System;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a user.
    /// </summary>
    public interface IUserShim
    {
        int SessionId { get; }

        /// <summary>
        /// Loads information about this user.
        /// </summary>
        Task<IUser> Load();

        /// <summary>
        /// Kicks this user with the specified reason.
        /// </summary>
        /// <param name="reason">Kick message.</param>
        Task Kick(string reason);
        /// <summary>
        /// Sends a message (only) to this user.
        /// </summary>
        /// <param name="message">The message.</param>
        Task SendMessage(string message);
    }
}


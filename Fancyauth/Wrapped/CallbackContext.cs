using System;

namespace Fancyauth.Wrapped
{
    public enum CallbackContext : int
    {
        /// <summary>
        /// Context for actions in the Server menu.
        /// </summary>
        ContextServer = 0x01,

        /// <summary>
        /// Context for actions in the Channel menu.
        /// </summary>
        ContextChannel = 0x02,

        /// <summary>
        /// Context for actions in the User menu.
        /// </summary>
        ContextUser = 0x04,
    }
}


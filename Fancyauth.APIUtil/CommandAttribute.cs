using System;

namespace Fancyauth.APIUtil
{
    /// <summary>
    /// Registers a method as a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; private set; }

        /// <summary>
        /// Registers a method as a command.
        /// The name is generated from the method's name.
        /// </summary>
        public CommandAttribute()
        {
            Name = null;
        }

        /// <summary>
        /// Registers a method as a command.
        /// </summary>
        /// <param name="name">The command's name.</param>
        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
}


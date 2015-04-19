using System;

namespace Fancyauth.APIUtil
{
    /// <summary>
    /// Registers a method as a context menu callback.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class ContextCallbackAttribute : Attribute
    {
        public string Text { get; private set; }

        /// <summary>
        /// Registers a method as a context menu callback.
        /// The method's signature must exactly match the signature of
        /// one of the context callback interfaces in Fancyauth.API.
        /// </summary>
        /// <param name="text">The text that should be shown on the context menu entry.</param>
        public ContextCallbackAttribute(string text)
        {
            Text = text;
        }
    }
}


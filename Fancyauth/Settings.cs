using System;

namespace Fancyauth
{
    // Settings come from the environment, so the container needs no config file.
    internal static class Settings
    {
        public static string Required(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"Environment variable {name} is not set");
            return value;
        }

        public static string Optional(string name, string fallback = null)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}

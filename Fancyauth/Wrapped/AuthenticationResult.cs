using System;

namespace Fancyauth.Wrapped
{
    public sealed class AuthenticationResult
    {
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public string[] Groups { get; private set; }

        private AuthenticationResult()
        {
        }

        public static AuthenticationResult Success(int uid, string username, string[] groups)
        {
            if (uid <= 0)
                throw new ArgumentOutOfRangeException("uid");
            if (String.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username");

            return new AuthenticationResult { UserId = uid, Username = username, Groups = groups };
        }

        public static AuthenticationResult Forbidden()
        {
            return new AuthenticationResult { UserId = -1, Username = null };
        }

        public static AuthenticationResult Fallthrough()
        {
            return new AuthenticationResult { UserId = -2, Username = null };
        }

        public static AuthenticationResult TryAgainLater()
        {
            return new AuthenticationResult { UserId = -3, Username = null };
        }
    }
}


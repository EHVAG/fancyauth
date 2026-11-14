using System;
using System.Threading.Tasks;
using SteamKit2.Authentication;

namespace Fancyauth.Steam
{
    // Nobody is there to type a Steam Guard code, so a login that needs one fails instead of waiting.
    // Steam Guard is off for the bot account. To turn it on, generate the codes here from the authenticator's shared secret.
    internal class NoCodeAuthenticator : IAuthenticator
    {
        public Task<bool> AcceptDeviceConfirmationAsync() => Task.FromResult(false);

        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) =>
            throw new SteamGuardRequiredException("Steam asks for a Steam Guard code from the authenticator app");

        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect) =>
            throw new SteamGuardRequiredException($"Steam asks for a Steam Guard code sent to {email}");
    }

    internal class SteamGuardRequiredException : Exception
    {
        public SteamGuardRequiredException(string message) : base(message) { }
    }
}

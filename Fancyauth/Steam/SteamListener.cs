using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Authentication;

namespace Fancyauth.Steam
{
    public class SteamListener
    {
        private static readonly TimeSpan NormalDelay = TimeSpan.FromMinutes(5);

        private readonly SteamClient SteamClient;
        private readonly CallbackManager CallbackManager;
        private readonly SteamUser SteamUser;
        private readonly SteamFriends SteamFriends;
        private readonly string User;
        private readonly string Pass;
        private readonly ISteamEventForwarder EventForwarder;
        private readonly Action<Task> AsyncCompleter;

        // Set when Steam refuses the credentials: retrying can't help and counts against its rate limit, so Steam stays off
        // until the next restart. Other failures, like an outage, reconnect after ReconnectDelay.
        private bool LoginFailed;
        private TimeSpan ReconnectDelay = NormalDelay;
        private CancellationToken Cancellation;

        public SteamListener(string user, string pass, ISteamEventForwarder forwarder, Action<Task> asyncCompleter)
        {
            User = user;
            Pass = pass;
            EventForwarder = forwarder;
            AsyncCompleter = asyncCompleter;

            SteamClient = new SteamClient();
            CallbackManager = new CallbackManager(SteamClient);
            SteamUser = SteamClient.GetHandler<SteamUser>();
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
        }

        public uint? GetCurrentGameId(SteamID steamid)
        {
            var appId = SteamFriends.GetFriendGamePlayed(steamid).AppID;
            return appId == 0 ? (uint?)null : appId;
        }

        public void SendMessage(SteamID steamid, string message)
        {
            SteamFriends.SendChatMessage(steamid, EChatEntryType.ChatMsg, message);
        }

        public void AddFriend(SteamID steamid)
        {
            SteamFriends.AddFriend(steamid);
        }

        // Handles Steam's callbacks until cancelled. Handlers run one at a time, in this loop.
        public async Task Run(CancellationToken cancellation = default)
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Pass))
            {
                Trace.WriteLine("No credentials (STEAM_USER, STEAM_PASSWORD): not connecting", "Steam");
                return;
            }
            Cancellation = cancellation;

            using var subscriptions = new Subscriptions(
                CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected),
                CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected),
                CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn),
                CallbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff),
                CallbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo),
                CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnChatMessage));

            Trace.WriteLine("Connecting ...", "Steam");
            SteamClient.Connect();
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    await CallbackManager.RunWaitCallbackAsync(cancellation);
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, "Steam callback exception");
                }
            }
        }

        private async Task OnConnected(SteamClient.ConnectedCallback callback)
        {
            Trace.WriteLine("Connected to Steam! Logging in ...", "Steam");
            try
            {
                var authSession = await SteamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                {
                    Username = User,
                    Password = Pass,
                    IsPersistentSession = true,
                    Authenticator = new NoCodeAuthenticator(),
                });
                var pollResponse = await authSession.PollingWaitForResultAsync(Cancellation);

                // The refresh token is what logs on; the result arrives as LoggedOnCallback.
                SteamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = pollResponse.AccountName,
                    AccessToken = pollResponse.RefreshToken,
                    ShouldRememberPassword = true,
                });
            }
            catch (Exception ex) when (ex is SteamGuardRequiredException || ex.InnerException is SteamGuardRequiredException)
            {
                LoginFailure(EResult.AccountLoginDeniedNeedTwoFactor, ex.Message);
                SteamClient.Disconnect();
            }
            catch (Exception ex) when (!Cancellation.IsCancellationRequested)
            {
                LoginFailure((ex as AuthenticationException)?.Result, ex.Message);
                SteamClient.Disconnect();
            }
        }

        private async Task OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            if (LoginFailed || callback.UserInitiated && Cancellation.IsCancellationRequested)
                return;
            var delay = ReconnectDelay;
            ReconnectDelay = NormalDelay;
            Trace.WriteLine($"Disconnected from Steam, reconnecting in {delay}", "Steam");
            await Task.Delay(delay, Cancellation);
            SteamClient.Connect();
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.OK)
                Trace.WriteLine("Successfully logged on!", "Steam");
            else
                LoginFailure(callback.Result, $"{callback.Result} / {callback.ExtendedResult}");
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Trace.WriteLine($"Logged off of Steam: {callback.Result}", "Steam");
        }

        private void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        // Forwarding talks to Mumble, so it runs beside the loop rather than holding it up.
        private void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
                AsyncCompleter(EventForwarder.OnChatMessage(this, callback.Sender, callback.Message));
        }

        // Logs a failed login, and decides whether to try again.
        private void LoginFailure(EResult? result, string reason)
        {
            if (result == EResult.RateLimitExceeded)
                ReconnectDelay = TimeSpan.FromHours(1);
            LoginFailed = result.HasValue && IsPermanent(result.Value);
            Trace.WriteLine(LoginFailed
                ? $"Login refused, Steam stays off until the next restart: {reason}"
                : $"Login failed, trying again in {ReconnectDelay}: {reason}", "Steam");
        }

        private static bool IsPermanent(EResult result) =>
            result == EResult.InvalidPassword || result == EResult.AccessDenied || result == EResult.AccountLogonDenied
            || result == EResult.AccountLoginDeniedNeedTwoFactor || result == EResult.InvalidLoginAuthCode
            || result == EResult.TwoFactorCodeMismatch;

        private sealed class Subscriptions : IDisposable
        {
            private readonly IDisposable[] All;
            public Subscriptions(params IDisposable[] all) => All = all;
            public void Dispose()
            {
                foreach (var subscription in All)
                    subscription.Dispose();
            }
        }
    }
}

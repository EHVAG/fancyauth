using System;
using SteamKit2;
using SteamKit2.Authentication;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Fancyauth.Steam
{
    public class SteamListener
    {
        private readonly SteamClient SteamClient;
        private readonly CallbackManager CallbackManager;
        private readonly SteamUser SteamUser;
        private readonly SteamFriends SteamFriends;
        private readonly string User;
        private readonly string Pass;
        private readonly Func<string> SteamGuardProvider;
        private string SteamGuardCode;
        private readonly ISteamEventForwarder EventForwarder;
        private readonly Action<Task> AsyncCompleter;

        public SteamListener(string user, string pass, Func<string> steamGuardProvider, ISteamEventForwarder forwarder, Action<Task> asyncCompleter)
        {
            User = user;
            Pass = pass;
            SteamGuardProvider = steamGuardProvider;
            EventForwarder = forwarder;
            AsyncCompleter = asyncCompleter;

            SteamClient = new SteamClient();
            CallbackManager = new CallbackManager(SteamClient);
            SteamUser = SteamClient.GetHandler<SteamUser>();
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
        }

        public uint? GetCurrentGameId(SteamID steamid)
        {
            var appId = this.SteamFriends.GetFriendGamePlayed(steamid).AppID;
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

        public Task Run(CancellationToken cancellation = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<object>();
            var steamThread = new Thread(() => {
                CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
                CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                CallbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
                CallbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
                CallbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
                CallbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
                CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnChatMessage);
                Trace.WriteLine("Connecting ...", "Steam");
                SteamClient.Connect();
                while (!cancellation.IsCancellationRequested)
                {
                    try
                    {
                        CallbackManager.RunWaitCallbacks();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine((object) ex, "Steam callback exception");
                    }
                }
                tcs.SetCanceled();
            });
            steamThread.Name = "Steam thread";
            steamThread.Start();
            return tcs.Task;
        }

        private async void OnConnected(SteamClient.ConnectedCallback callback)
        {
            string previouslyStoredGuardData = null; // we have steam guard disabled (hopefully)
            Trace.WriteLine("Connected to Steam! Logging in ...", "Steam");
            var authSession = await SteamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
            {
                Username = User,
                Password = Pass,
                IsPersistentSession = true,

                // See NewGuardData comment below
                GuardData = previouslyStoredGuardData,

                /// <see cref="UserConsoleAuthenticator"/> is the default authenticator implemention provided by SteamKit
                /// for ease of use which blocks the thread and asks for user input to enter the code.
                /// However, if you require special handling (e.g. you have the TOTP secret and can generate codes on the fly),
                /// you can implement your own <see cref="SteamKit2.Authentication.IAuthenticator"/>.
                Authenticator = new UserConsoleAuthenticator(),
            } );

            // Starting polling Steam for authentication response
            var pollResponse = await authSession.PollingWaitForResultAsync();

            if ( pollResponse.NewGuardData != null )
            {
                // When using certain two factor methods (such as email 2fa), guard data may be provided by Steam
                // for use in future authentication sessions to avoid triggering 2FA again (this works similarly to the old sentry file system).
                // Do note that this guard data is also a JWT token and has an expiration date.
                previouslyStoredGuardData = pollResponse.NewGuardData;
            }

            // Logon to Steam with the access token we have received
            // Note that we are using RefreshToken for logging on here
            SteamUser.LogOn( new SteamUser.LogOnDetails
            {
                Username = pollResponse.AccountName,
                AccessToken = pollResponse.RefreshToken,
                ShouldRememberPassword = true,
            } );
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Trace.WriteLine("Disconnected from Steam, reconnecting in 5 minutes...", "Steam");
            Thread.Sleep(TimeSpan.FromMinutes(5));
            SteamClient.Connect();
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
                Trace.WriteLine(string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult), "Steam");
            else
                Trace.WriteLine("Successfully logged on!", "Steam");
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Trace.WriteLine(string.Format("Logged off of Steam: {0}", callback.Result), "Steam");
        }

        private void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        private void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
        }

        private void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
        }

        private void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
                AsyncCompleter(this.EventForwarder.OnChatMessage(this, callback.Sender, callback.Message));
        }
    }
}


using System;
using SteamKit2;
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

            SteamClient = new SteamClient(ProtocolType.Tcp);
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
                CallbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
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

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
                Trace.WriteLine(string.Format("Unable to connect to Steam: {0}", callback.Result), "Steam");
            else
            {
                Trace.WriteLine("Connected to Steam! Logging in ...", "Steam");
                byte[] sentryHash = null;
                if (System.IO.File.Exists("sentry.bin"))
                    sentryHash = CryptoHelper.SHAHash(System.IO.File.ReadAllBytes("sentry.bin"));
                SteamUser.LogOn(new SteamUser.LogOnDetails {
                    Username = User,
                    Password = Pass,
                    AuthCode = SteamGuardCode,
                    SentryFileHash = sentryHash,
                });
            }
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Trace.WriteLine("Disconnected from Steam, reconnecting in 5...", "Steam");
            Thread.Sleep(TimeSpan.FromSeconds(5.0));
            SteamClient.Connect();
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLogonDeniedNeedTwoFactorCode;

            if (isSteamGuard || is2FA)
            {
                Trace.WriteLine("This account is SteamGuard protected!", "Steam");
                SteamGuardCode = SteamGuardProvider();
            }
            else if (callback.Result != EResult.OK)
                Trace.WriteLine(string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult), "Steam");
            else
                Trace.WriteLine("Successfully logged on!", "Steam");
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Trace.WriteLine(string.Format("Logged off of Steam: {0}", callback.Result), "Steam");
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Trace.WriteLine("Updating sentryfile...", "Steam.Sentry");
            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);
            File.WriteAllBytes("sentry.bin", callback.Data);

            SteamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });

            Trace.WriteLine("Done!", "Steam.Sentry");
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


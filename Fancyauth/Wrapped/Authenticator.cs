using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fancyauth.Wrapped
{
    public class Authenticator : MumbleServer.ServerUpdatingAuthenticatorDisp_
    {
        private static readonly Task<AuthenticationResult> NullAuthResult = Task.FromResult(AuthenticationResult.TryAgainLater());
        private static readonly Task<AuthenticationResult> NullRegisterResult = Task.FromResult(AuthenticationResult.Forbidden());

        public virtual Task<AuthenticationResult> Authenticate(string name, string pw, byte[][] certificates, string certhash, bool certstrong) { return NullAuthResult; }
        public virtual Task<AuthenticationResult> RegisterUser(Dictionary<MumbleServer.UserInfo, string> info) { return NullRegisterResult; }
        public virtual Task<AuthenticatorUpdateResult> UnregisterUser(int id) { return Task.FromResult(AuthenticatorUpdateResult.Failure); }
        public virtual Task<int?> NameToId(string name) { return Task.FromResult<int?>(null); }
        public virtual Task<string> IdToName(int id) { return Task.FromResult<string>(null); }
        public virtual Task<byte[]> IdToTexture(int id) { return Task.FromResult<byte[]>(null); }
        public virtual Task<AuthenticatorUpdateResult> SetTexture(int id, byte[] texture) { return Task.FromResult(AuthenticatorUpdateResult.Failure); }
        public virtual Task<Dictionary<MumbleServer.UserInfo, string>> GetInfo(int id) { return Task.FromResult(new Dictionary<MumbleServer.UserInfo, string>()); }
        public virtual Task<AuthenticatorUpdateResult> SetInfo(int id, Dictionary<MumbleServer.UserInfo, string> info) { return Task.FromResult(AuthenticatorUpdateResult.Failure); }
        public virtual Task<Dictionary<int, string>> GetRegisteredUsers(string filter) { return Task.FromResult(new Dictionary<int, string>()); }

        public sealed async override Task<MumbleServer.ServerAuthenticator_AuthenticateResult> authenticateAsync(string name, string pw, byte[][] certificates, string certhash, bool certstrong, Ice.Current current__)
        {
            try {
                return (MumbleServer.ServerAuthenticator_AuthenticateResult)await Authenticate(name, pw, certificates, certhash, certstrong);
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return (MumbleServer.ServerAuthenticator_AuthenticateResult)NullAuthResult.Result;
            }
        }

        public sealed async override Task<MumbleServer.ServerAuthenticator_GetInfoResult> getInfoAsync(int id, Ice.Current current__)
        {
            try {
                var info = await GetInfo(id);
                return new MumbleServer.ServerAuthenticator_GetInfoResult(info != null, info);
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return new MumbleServer.ServerAuthenticator_GetInfoResult(false, null);
            }
        }

        public sealed async override Task<int> nameToIdAsync(string name, Ice.Current current__)
        {
            try {
                return (await NameToId(name)) ?? -2;
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return -2;
            }
        }

        public sealed async override Task<string> idToNameAsync(int id, Ice.Current current__)
        {
            try {
                return (await IdToName(id)) ?? String.Empty;
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e, "Authenticator failed");
                return String.Empty;
            }
        }

        public sealed async override Task<byte[]> idToTextureAsync(int id, Ice.Current current__)
        {
            try {
                return (await IdToTexture(id)) ?? new byte[0];
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return new byte[0];
            }
        }

        public sealed async override Task<int> registerUserAsync(System.Collections.Generic.Dictionary<MumbleServer.UserInfo, string> info, Ice.Current current__)
        {
            try {
                return (await RegisterUser(info)).UserId;
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return NullRegisterResult.Result.UserId;
            }
        }

        public sealed async override Task<int> unregisterUserAsync(int id, Ice.Current current__)
        {
            try {
                return (int)await UnregisterUser(id);
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return (int)AuthenticatorUpdateResult.Failure;
            }
        }

        public sealed async override Task<Dictionary<int, string>> getRegisteredUsersAsync(string filter, Ice.Current current__)
        {
            try {
                return await GetRegisteredUsers(filter);
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return null;
            }
        }

        public sealed async override Task<int> setInfoAsync(int id, System.Collections.Generic.Dictionary<MumbleServer.UserInfo, string> info, Ice.Current current__)
        {
            try {
                return (int)await SetInfo(id, info);
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return (int)AuthenticatorUpdateResult.Failure;
            }
        }

        public sealed async override Task<int> setTextureAsync(int id, byte[] tex, Ice.Current current__)
        {
            try {
                return (int)await SetTexture(id, tex);
            } catch (Exception e) {
                System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                return (int)AuthenticatorUpdateResult.Failure;
            }
        }
    }
}


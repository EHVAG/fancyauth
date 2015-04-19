using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fancyauth.Wrapped
{
    public class Authenticator : Murmur.ServerUpdatingAuthenticatorDisp_
    {
        private static readonly Task<AuthenticationResult> NullAuthResult = Task.FromResult(AuthenticationResult.TryAgainLater());
        private static readonly Task<AuthenticationResult> NullRegisterResult = Task.FromResult(AuthenticationResult.Forbidden());

        public virtual Task<AuthenticationResult> Authenticate(string name, string pw, byte[][] certificates, string certhash, bool certstrong) { return NullAuthResult; }
        public virtual Task<AuthenticationResult> RegisterUser(Dictionary<Murmur.UserInfo, string> info) { return NullRegisterResult; }
        public virtual Task<AuthenticatorUpdateResult> UnregisterUser(int id) { return Task.FromResult(AuthenticatorUpdateResult.Failure); }
        public virtual Task<int?> NameToId(string name) { return Task.FromResult<int?>(null); }
        public virtual Task<string> IdToName(int id) { return Task.FromResult<string>(null); }
        public virtual Task<byte[]> IdToTexture(int id) { return Task.FromResult<byte[]>(null); }
        public virtual Task<AuthenticatorUpdateResult> SetTexture(int id, byte[] texture) { return Task.FromResult(AuthenticatorUpdateResult.Failure); }
        public virtual Task<Dictionary<Murmur.UserInfo, string>> GetInfo(int id) { return Task.FromResult(new Dictionary<Murmur.UserInfo, string>()); }
        public virtual Task<AuthenticatorUpdateResult> SetInfo(int id, Dictionary<Murmur.UserInfo, string> info) { return Task.FromResult(AuthenticatorUpdateResult.Failure); }
        public virtual Task<Dictionary<int, string>> GetRegisteredUsers(string filter) { return Task.FromResult(new Dictionary<int, string>()); }

        public sealed override void authenticate_async(Murmur.AMD_ServerAuthenticator_authenticate cb__, string name, string pw, byte[][] certificates, string certhash, bool certstrong, Ice.Current current__)
        {
            Authenticate(name, pw, certificates, certhash, certstrong).ContinueWith(task => {
                AuthenticationResult res = NullAuthResult.Result;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res.UserId, res.Username, null);
            });
        }

        public sealed override void getInfo_async(Murmur.AMD_ServerAuthenticator_getInfo cb__, int id, Ice.Current current__)
        {
            GetInfo(id).ContinueWith(task => {
                Dictionary<Murmur.UserInfo, string> res = null;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res != null, res);
            });
        }

        public sealed override void nameToId_async(Murmur.AMD_ServerAuthenticator_nameToId cb__, string name, Ice.Current current__)
        {
            NameToId(name).ContinueWith(task => {
                int? res = null;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res ?? -2);
            });
        }

        public sealed override void idToName_async(Murmur.AMD_ServerAuthenticator_idToName cb__, int id, Ice.Current current__)
        {
            IdToName(id).ContinueWith(task => {
                string res = null;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res ?? String.Empty);
            });
        }

        public sealed override void idToTexture_async(Murmur.AMD_ServerAuthenticator_idToTexture cb__, int id, Ice.Current current__)
        {
            IdToTexture(id).ContinueWith(task => {
                byte[] res = null;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res ?? new byte[0]);
            });
        }

        public sealed override void registerUser_async(Murmur.AMD_ServerUpdatingAuthenticator_registerUser cb__, System.Collections.Generic.Dictionary<Murmur.UserInfo, string> info, Ice.Current current__)
        {
            RegisterUser(info).ContinueWith(task => {
                AuthenticationResult res = NullRegisterResult.Result;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res.UserId);
            });
        }

        public sealed override void unregisterUser_async(Murmur.AMD_ServerUpdatingAuthenticator_unregisterUser cb__, int id, Ice.Current current__)
        {
            UnregisterUser(id).ContinueWith(task => {
                var res = AuthenticatorUpdateResult.Failure;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response((int)res);
            });
        }

        public sealed override void getRegisteredUsers_async(Murmur.AMD_ServerUpdatingAuthenticator_getRegisteredUsers cb__, string filter, Ice.Current current__)
        {
            GetRegisteredUsers(filter).ContinueWith(task => {
                Dictionary<int, string> res = null;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response(res);
            });
        }

        public sealed override void setInfo_async(Murmur.AMD_ServerUpdatingAuthenticator_setInfo cb__, int id, System.Collections.Generic.Dictionary<Murmur.UserInfo, string> info, Ice.Current current__)
        {
            SetInfo(id, info).ContinueWith(task => {
                var res = AuthenticatorUpdateResult.Failure;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response((int)res);
            });
        }

        public sealed override void setTexture_async(Murmur.AMD_ServerUpdatingAuthenticator_setTexture cb__, int id, byte[] tex, Ice.Current current__)
        {
            SetTexture(id, tex).ContinueWith(task => {
                var res = AuthenticatorUpdateResult.Failure;
                try {
                    res = task.Result;
                } catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e, "Authenticator failed");
                }

                cb__.ice_response((int)res);
            });
        }
    }
}


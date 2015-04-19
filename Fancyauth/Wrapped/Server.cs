using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fancyauth.Wrapped
{
    public class Server
    {
        /// <summary>
        /// The ICE murmur server proxy.
        /// </summary>
        /// <remarks>
        /// Bad naming, I know. But that's what this class is about
        /// and typing "Inner" all the time was exhausting.
        /// </remarks>
        public readonly Murmur.ServerPrx S;

        public Server(Murmur.ServerPrx inner)
        {
            S = inner;
        }

        public Task<bool> IsRunning() { return FixIce.FromAsync(S.begin_isRunning, S.end_isRunning); }
        public Task Start() { return FixIce.FromAsyncVoid(S.begin_start, S.end_start); }
        public Task Delete() { return FixIce.FromAsyncVoid(S.begin_delete, S.end_delete); }
        public Task<int> GetId() { return FixIce.FromAsync(S.begin_id, S.end_id); }
        public Task AddCallback(Murmur.ServerCallbackPrx sc) { return FixIce.FromAsyncVoid(sc, S.begin_addCallback, S.end_addCallback); }
        public Task RemoveCallback(Murmur.ServerCallbackPrx sc) { return FixIce.FromAsyncVoid(sc, S.begin_removeCallback, S.end_removeCallback); }
        public Task SetAuthenticator(Murmur.ServerAuthenticatorPrx sa) { return FixIce.FromAsyncVoid(sa, S.begin_setAuthenticator, S.end_setAuthenticator); }
        public Task<string> GetConf(string key) { return FixIce.FromAsync(key, S.begin_getConf, S.end_getConf); }
        public Task<Dictionary<string, string>> GetAllConf() { return FixIce.FromAsync(S.begin_getAllConf, S.end_getAllConf); }
        public Task SetConf(string key, string value) { return FixIce.FromAsyncVoid(key, value, S.begin_setConf, S.end_setConf); }
        public Task SetSuperuserPassword(string pw) { return FixIce.FromAsyncVoid(pw, S.begin_setSuperuserPassword, S.end_setSuperuserPassword); }
        public Task<Murmur.LogEntry[]> GetLog(int first, int last) { return FixIce.FromAsync(first, last, S.begin_getLog, S.end_getLog); }
        public Task<int> GetLogLen() { return FixIce.FromAsync(S.begin_getLogLen, S.end_getLogLen); }
        public Task<Dictionary<int, Murmur.User>> GetUsers() { return FixIce.FromAsync(S.begin_getUsers, S.end_getUsers); }
        public Task<Dictionary<int, Murmur.Channel>> GetChannels() { return FixIce.FromAsync(S.begin_getChannels, S.end_getChannels); }
        public Task<byte[][]> GetCertificateList(int session) { return FixIce.FromAsync(session, S.begin_getCertificateList, S.end_getCertificateList); }
        public Task<Murmur.Tree> GetTree() { return FixIce.FromAsync(S.begin_getTree, S.end_getTree); }
        public Task<Murmur.Ban[]> GetBans() { return FixIce.FromAsync(S.begin_getBans, S.end_getBans); }
        public Task SetBans(Murmur.Ban[] bans) { return FixIce.FromAsyncVoid(bans, S.begin_setBans, S.end_setBans); }
        public Task KickUser(int session, string reason) { return FixIce.FromAsyncVoid(session, reason, S.begin_kickUser, S.end_kickUser); }
        public Task<Murmur.User> GetState(int session) { return FixIce.FromAsync(session, S.begin_getState, S.end_getState); }
        public Task SetState(Murmur.User state) { return FixIce.FromAsyncVoid(state, S.begin_setState, S.end_setState); }
        public Task SendMessage(int userSession, string text) { return FixIce.FromAsyncVoid(userSession, text, S.begin_sendMessage, S.end_sendMessage); }
        public Task<bool> HasPermission(int session, int channelid, MumblePermissions perm) { return FixIce.FromAsync(session, channelid, (int)perm, S.begin_hasPermission, S.end_hasPermission); }

        public async Task<MumblePermissions> EffectivePermissions(int session, int channelid)
        {
            int res = await FixIce.FromAsync(session, channelid, S.begin_effectivePermissions, S.end_effectivePermissions);
            return (MumblePermissions)res;
        }

        public Task AddContextCallback(int session, string action, string text, Murmur.ServerContextCallbackPrx cb, CallbackContext ctx)
        {
            return FixIce.FromAsyncVoid(session, action, text, cb, (int)ctx, S.begin_addContextCallback, S.end_addContextCallback);
        }

        public Task RemoveContextCallback(Murmur.ServerContextCallbackPrx cb) { return FixIce.FromAsyncVoid(cb, S.begin_removeContextCallback, S.end_removeContextCallback); }
        public Task<Murmur.Channel> GetChannelState(int chanid) { return FixIce.FromAsync(chanid, S.begin_getChannelState, S.end_getChannelState); }
        public Task SetChannelState(Murmur.Channel state) { return FixIce.FromAsyncVoid(state, S.begin_setChannelState, S.end_setChannelState); }
        public Task RemoveChannel(int chanid) { return FixIce.FromAsyncVoid(chanid, S.begin_removeChannel, S.end_removeChannel); }
        public Task<int> AddChannel(string name, int parent) { return FixIce.FromAsync(name, parent, S.begin_addChannel, S.end_addChannel); }
        public Task SendMessageChannel(int chanid, bool tree, string text) { return FixIce.FromAsyncVoid(chanid, tree, text, S.begin_sendMessageChannel, S.end_sendMessageChannel); }
        // TODO: getACL
        // TODO: setACL
        public Task AddUserToGroup(int chanid, int sess, string grp) { return FixIce.FromAsyncVoid(chanid, sess, grp, S.begin_addUserToGroup, S.end_addUserToGroup); }
        public Task RemoveUserFromGroup(int chanid, int sess, string grp) { return FixIce.FromAsyncVoid(chanid, sess, grp, S.begin_removeUserFromGroup, S.end_removeUserFromGroup); }
        public Task RedirectWhisperGroup(int sess, string src, string target) { return FixIce.FromAsyncVoid(sess, src, target, S.begin_redirectWhisperGroup, S.end_redirectWhisperGroup); }
        public Task<Dictionary<int, string>> GetUserNames(params int[] ids) { return FixIce.FromAsync(ids, S.begin_getUserNames, S.end_getUserNames); }
        public Task<Dictionary<string, int>> GetUserIds(params string[] names) { return FixIce.FromAsync(names, S.begin_getUserIds, S.end_getUserIds); }
        public Task<int> RegisterUser(Dictionary<Murmur.UserInfo, string> ui) { return FixIce.FromAsync(ui, S.begin_registerUser, S.end_registerUser); }
        public Task UnregisterUser(int userid) { return FixIce.FromAsyncVoid(userid, S.begin_unregisterUser, S.end_unregisterUser); }
        public Task UpdateRegistration(int uid, Dictionary<Murmur.UserInfo, string> ui) { return FixIce.FromAsyncVoid(uid, ui, S.begin_updateRegistration, S.end_updateRegistration); }
        public Task<Dictionary<Murmur.UserInfo, string>> GetRegistration(int userid) { return FixIce.FromAsync(userid, S.begin_getRegistration, S.end_getRegistration); }
        public Task<Dictionary<int, string>> GetRegisteredUsers(string filter) { return FixIce.FromAsync(filter, S.begin_getRegisteredUsers, S.end_getRegisteredUsers); }
        public Task<int> VerifyPassword(string name, string pw) { return FixIce.FromAsync(name, pw, S.begin_verifyPassword, S.end_verifyPassword); }
        public Task<byte[]> GetTexture(int userid) { return FixIce.FromAsync(userid, S.begin_getTexture, S.end_getTexture); }
        public Task SetTexture(int userid, byte[] tex) { return FixIce.FromAsyncVoid(userid, tex, S.begin_setTexture, S.end_setTexture); }
        public Task<int> GetUptime() { return FixIce.FromAsync(S.begin_getUptime, S.end_getUptime); }
    }
}


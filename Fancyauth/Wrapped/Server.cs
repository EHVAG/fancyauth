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
        public readonly MumbleServer.ServerPrx S;

        public Server(MumbleServer.ServerPrx inner)
        {
            S = inner;
        }

        public Task<bool> IsRunning() { return S.isRunningAsync(); }
        public Task Start() { return S.startAsync(); }
        public Task Delete() { return S.deleteAsync(); }
        public Task<int> GetId() { return S.idAsync(); }
        public Task AddCallback(MumbleServer.ServerCallbackPrx sc) { return S.addCallbackAsync(sc); }
        public Task RemoveCallback(MumbleServer.ServerCallbackPrx sc) { return S.removeCallbackAsync(sc); }
        public Task SetAuthenticator(MumbleServer.ServerAuthenticatorPrx sa) { return S.setAuthenticatorAsync(sa); }
        public Task<string> GetConf(string key) { return S.getConfAsync(key); }
        public Task<Dictionary<string, string>> GetAllConf() { return S.getAllConfAsync(); }
        public Task SetConf(string key, string value) { return S.setConfAsync(key, value); }
        public Task SetSuperuserPassword(string pw) { return S.setSuperuserPasswordAsync(pw); }
        public Task<MumbleServer.LogEntry[]> GetLog(int first, int last) { return S.getLogAsync(first, last); }
        public Task<int> GetLogLen() { return S.getLogLenAsync(); }
        public Task<Dictionary<int, MumbleServer.User>> GetUsers() { return S.getUsersAsync(); }
        public Task<Dictionary<int, MumbleServer.Channel>> GetChannels() { return S.getChannelsAsync(); }
        public Task<byte[][]> GetCertificateList(int session) { return S.getCertificateListAsync(session); }
        public Task<MumbleServer.Tree> GetTree() { return S.getTreeAsync(); }
        public Task<MumbleServer.Ban[]> GetBans() { return S.getBansAsync(); }
        public Task SetBans(MumbleServer.Ban[] bans) { return S.setBansAsync(bans); }
        public Task KickUser(int session, string reason) { return S.kickUserAsync(session, reason); }
        public Task<MumbleServer.User> GetState(int session) { return S.getStateAsync(session); }
        public Task SetState(MumbleServer.User state) { return S.setStateAsync(state); }
        public Task SendMessage(int userSession, string text) { return S.sendMessageAsync(userSession, text); }
        public Task<bool> HasPermission(int session, int channelid, MumblePermissions perm) { return S.hasPermissionAsync(session, channelid, (int)perm); }

        public async Task<MumblePermissions> EffectivePermissions(int session, int channelid)
        {
            int res = await S.effectivePermissionsAsync(session, channelid);
            return (MumblePermissions)res;
        }

        public Task AddContextCallback(int session, string action, string text, MumbleServer.ServerContextCallbackPrx cb, CallbackContext ctx)
        {
            return S.addContextCallbackAsync(session, action, text, cb, (int)ctx);
        }

        public Task RemoveContextCallback(MumbleServer.ServerContextCallbackPrx cb) { return S.removeContextCallbackAsync(cb); }
        public Task<MumbleServer.Channel> GetChannelState(int chanid) { return S.getChannelStateAsync(chanid); }
        public Task SetChannelState(MumbleServer.Channel state) { return S.setChannelStateAsync(state); }
        public Task RemoveChannel(int chanid) { return S.removeChannelAsync(chanid); }
        public Task<int> AddChannel(string name, int parent) { return S.addChannelAsync(name, parent); }
        public Task SendMessageChannel(int chanid, bool tree, string text) { return S.sendMessageChannelAsync(chanid, tree, text); }
        public Task SetACL(int chanid, ACLData data) { return S.setACLAsync(chanid, data.ACLs, data.Groups, data.Inherit); }

        public async Task<ACLData> GetACL(int chanid)
        {
            var data = new ACLData();
            var acls = await S.getACLAsync(chanid);
            data.ACLs = acls.acls;
            data.Groups = acls.groups;
            data.Inherit = acls.inherit;
            return data;
        }

        public Task AddUserToGroup(int chanid, int sess, string grp) { return S.addUserToGroupAsync(chanid, sess, grp); }
        public Task RemoveUserFromGroup(int chanid, int sess, string grp) { return S.removeUserFromGroupAsync(chanid, sess, grp); }
        public Task RedirectWhisperGroup(int sess, string src, string target) { return S.redirectWhisperGroupAsync(sess, src, target); }
        public Task<Dictionary<int, string>> GetUserNames(params int[] ids) { return S.getUserNamesAsync(ids); }
        public Task<Dictionary<string, int>> GetUserIds(params string[] names) { return S.getUserIdsAsync(names); }
        public Task<int> RegisterUser(Dictionary<MumbleServer.UserInfo, string> ui) { return S.registerUserAsync(ui); }
        public Task UnregisterUser(int userid) { return S.unregisterUserAsync(userid); }
        public Task UpdateRegistration(int uid, Dictionary<MumbleServer.UserInfo, string> ui) { return S.updateRegistrationAsync(uid, ui); }
        public Task<Dictionary<MumbleServer.UserInfo, string>> GetRegistration(int userid) { return S.getRegistrationAsync(userid); }
        public Task<Dictionary<int, string>> GetRegisteredUsers(string filter) { return S.getRegisteredUsersAsync(filter); }
        public Task<int> VerifyPassword(string name, string pw) { return S.verifyPasswordAsync(name, pw); }
        public Task<byte[]> GetTexture(int userid) { return S.getTextureAsync(userid); }
        public Task SetTexture(int userid, byte[] tex) { return S.setTextureAsync(userid, tex); }
        public Task<int> GetUptime() { return S.getUptimeAsync(); }

        public class ACLData
        {
            public MumbleServer.ACL[] ACLs;
            public MumbleServer.Group[] Groups;
            public bool Inherit;
        }
    }
}


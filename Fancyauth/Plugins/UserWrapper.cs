using System;
using System.Net;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.Model;

namespace Fancyauth.Plugins
{
    public class UserWrapper : UserShim, IUser
    {
        private Murmur.User User;

        public UserWrapper(Steam.SteamListener steamListener, Wrapped.Server server, Murmur.User user)
            : base(steamListener, server, user.session)
        {
            User = user;
        }

        async Task<IUserSteamAdapter> IUser.GetSteamAdapter()
        {
            long? steamid;
            using (var context = new FancyContext())
                steamid = await context.Users.Where(x => x.Id == User.userid).Select(x => x.Membership.SteamId).SingleAsync();
            return steamid.HasValue ? new UserSteamAdapter(SteamListener, steamid.Value) : null;
        }

        Task IReadModifyWriteObject.SaveChanges()
        {
            return Server.SetState(User);
        }

        async Task IReadModifyWriteObject.Refresh()
        {
            User = await Server.GetState(User.session);
        }

        int IUser.UserId { get { return User.userid; } }

        bool IUser.ServerMute {
            get { return User.mute; }
            set { User.mute = value; }
        }

        bool IUser.ServerDeaf {
            get { return User.deaf; }
            set { User.deaf = value; }
        }

        bool IUser.ServerSuppress {
            get { return User.suppress; }
            set { User.suppress = value; }
        }

        bool IUser.PrioritySpeaker {
            get { return User.prioritySpeaker; }
            set { User.prioritySpeaker = value; }
        }

        bool IUser.SelfMute { get { return User.selfMute; } }

        bool IUser.SelfDeaf { get { return User.selfDeaf; } }

        bool IUser.Recording { get { return User.recording; } }

        IChannelShim IUser.CurrentChannel {
            get { return new ChannelShim(Server, User.channel); }
            set { User.channel = value.ChannelId; }
        }

        string IUser.Name {
            get { return User.name; }
            set { User.name = value; }
        }

        TimeSpan IUser.OnlineTime { get { return TimeSpan.FromSeconds(User.onlinesecs); } }

        int IUser.TransmissionRateInBytesPerSecond { get { return User.bytespersec; } }

        int IUser.ClientVersion { get { return User.version; } }

        string IUser.ClientRelease { get { return User.release; } }

        string IUser.OperatingSystem { get { return User.os; } }

        string IUser.OperatingSystemVersion { get { return User.osversion; } }

        string IUser.PluginIdentity {
            get { return User.identity; }
            set { User.identity = value; }
        }

        string IUser.PluginContext {
            get { return User.context; }
            set { User.context = value; }
        }

        string IUser.UserComment {
            get { return User.comment; }
            set { User.comment = value; }
        }

        IPAddress IUser.ClientAddress { get { return new IPAddress(User.address); } }

        bool IUser.TcpOnly { get { return User.tcponly; } }

        TimeSpan IUser.IdleTime { get { return TimeSpan.FromSeconds(User.idlesecs); } }

        float IUser.AverageUdpPing { get { return User.udpPing; } }

        float IUser.AverageTcpPing { get { return User.tcpPing; } }

        public override bool Equals(Object o)
        {
            IUser t = this;
            IUser u = o as UserWrapper;

            return base.Equals(u)
                && u.UserId == t.UserId
                && u.ServerMute == t.ServerMute
                && u.ServerDeaf == t.ServerDeaf
                && u.ServerSuppress == t.ServerSuppress
                && u.PrioritySpeaker == t.PrioritySpeaker
                && u.SelfMute == t.SelfMute
                && u.SelfDeaf == t.SelfDeaf
                && u.Recording == t.Recording
                && u.CurrentChannel.Equals(t.CurrentChannel)
                && u.Name == t.Name
                && u.OnlineTime.Equals(t.OnlineTime)
                && u.TransmissionRateInBytesPerSecond == t.TransmissionRateInBytesPerSecond
                && u.ClientVersion == t.ClientVersion
                && u.ClientRelease == t.ClientRelease
                && u.OperatingSystem == t.OperatingSystem
                && u.OperatingSystemVersion == t.OperatingSystemVersion
                && u.PluginIdentity == t.PluginIdentity
                && u.PluginContext == t.PluginContext
                && u.UserComment == t.UserComment
                && u.ClientAddress.Equals(t.ClientAddress)
                && u.TcpOnly == t.TcpOnly
                && u.IdleTime.Equals(t.IdleTime)
                && u.AverageTcpPing == t.AverageTcpPing
                && u.AverageUdpPing == t.AverageUdpPing;
        }
    }
}


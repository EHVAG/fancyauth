using System;
using System.Net;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a user, including information about them.
    /// </summary>
    public interface IUser : IUserShim
    {
        int UserId { get; }
        bool ServerMute { get; set; }
        bool ServerDeaf { get; set; }
        bool ServerSuppress { get; set; }
        bool PrioritySpeaker { get; set; }
        bool SelfMute { get; }
        bool SelfDeaf { get; }
        bool Recording { get; }
        IChannelShim CurrentChannel { get; set; }
        string Name { get; set; }
        TimeSpan OnlineTime { get; }
        int TransmissionRateInBytesPerSecond { get; }
        int ClientVersion { get; }
        string ClientRelease { get; }
        string OperatingSystem { get; }
        string OperatingSystemVersion { get; }
        string PluginIdentity { get; set; }
        string PluginContext { get; set; }
        string UserComment { get; set; }
        IPAddress ClientAddress { get; }
        bool TcpOnly { get; }
        TimeSpan IdleTime { get; }
        float AverageUdpPing { get; }
        float AverageTcpPing { get; }

        /// <summary>
        /// Saves changes made to this user.
        /// </summary>
        Task SaveChanges();
        /// <summary>
        /// Reloads information about this user, discarding all unsaved changes.
        /// </summary>
        Task Refresh();
    }
}


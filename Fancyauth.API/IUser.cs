using System;
using System.Net;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    /// <summary>
    /// Represents a user, including information about them. Remember to call SaveChanges()!
    /// </summary>
    public interface IUser : IUserShim, IReadModifyWriteObject
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
        /// Gets this user's steam adapter or <c>null</c> if the user hasn't enabled steam integration.
        /// </summary>
        /// <returns>The steam adapter.</returns>
        Task<IUserSteamAdapter> GetSteamAdapter();
    }
}


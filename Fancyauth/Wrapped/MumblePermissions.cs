using System;

namespace Fancyauth.Wrapped
{
    [Flags]
    public enum MumblePermissions : int
    {
        /// <summary>
        /// Write access to channel control. Implies all other permissions (except Speak).
        /// </summary>
        PermissionWrite = 0x01,

        /// <summary>
        /// Traverse channel. Without this, a client cannot reach subchannels, no matter which privileges he has there.
        /// </summary>
        PermissionTraverse = 0x02,

        /// <summary>
        /// Enter channel.
        /// </summary>
        PermissionEnter = 0x04,

        /// <summary>
        /// Speak in channel.
        /// </summary>
        PermissionSpeak = 0x08,

        /// <summary>
        /// Whisper to channel. This is different from Speak, so you can set up different permissions.
        /// </summary>
        PermissionWhisper = 0x100,

        /// <summary>
        /// Mute and deafen other users in this channel.
        /// </summary>
        PermissionMuteDeafen = 0x10,

        /// <summary>
        /// Move users from channel. You need this permission in both the source and destination channel to move another user.
        /// </summary>
        PermissionMove = 0x20,

        /// <summary>
        /// Make new channel as a subchannel of this channel.
        /// </summary>
        PermissionMakeChannel = 0x40,

        /// <summary>
        /// Make new temporary channel as a subchannel of this channel.
        /// </summary>
        PermissionMakeTempChannel = 0x400,

        /// <summary>
        /// Link this channel. You need this permission in both the source and destination channel to link channels, or in either channel to unlink them.
        /// </summary>
        PermissionLinkChannel = 0x80,

        /// <summary>
        /// Send text message to channel.
        /// </summary>
        PermissionTextMessage = 0x200,

        /// <summary>
        /// Kick user from server. Only valid on root channel.
        /// </summary>
        PermissionKick = 0x10000,

        /// <summary>
        /// Ban user from server. Only valid on root channel.
        /// </summary>
        PermissionBan = 0x20000,

        /// <summary>
        /// Register and unregister users. Only valid on root channel.
        /// </summary>
        PermissionRegister = 0x40000,

        /// <summary>
        /// Register and unregister users. Only valid on root channel.
        /// </summary>
        PermissionRegisterSelf = 0x80000,
    }
}


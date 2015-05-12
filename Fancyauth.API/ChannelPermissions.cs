using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.API
{
    [Flags]
    public enum ChannelPermissions : int
    {
        PermissionWrite = 1,
        PermissionTraverse = 2,
        PermissionEnter = 4,
        PermissionSpeak = 8,
        PermissionWhisper = 256,
        PermissionMuteDeafen = 16,
        PermissionMove = 32,
        PermissionMakeChannel = 64,
        PermissionMakeTempChannel = 1024,
        PermissionLinkChannel = 128,
        PermissionTextMessage = 512,
        PermissionKick = 65536,
        PermissionBan = 131072,
        PermissionRegister = 262144,
        PermissionRegisterSelf = 524288
    }
}

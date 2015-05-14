using Fancyauth.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Plugins
{
    public class PermissionsWrapper : IChannelPermissions
    {
        private readonly int ChannelId;
        private readonly Wrapped.Server Server;

        public bool Inherit { get; set; }
        public IList<IChannelACLEntry> ACL { get; private set; }
        public ICollection<IChannelUserGroup> Groups { get; private set; }
        public IReadOnlyList<IChannelACLEntryReadonly> InheritedACL { get; private set; }
        public IReadOnlyCollection<IChannelUserGroupInherited> InheritedGroups { get; private set; }

        public PermissionsWrapper(int channelId, Wrapped.Server server)
        {
            ChannelId = channelId;
            Server = server;
        }

        public async Task Refresh()
        {
            var aclData = await Server.GetACL(ChannelId);
            Inherit = aclData.Inherit;
            var aclGroups = aclData.Groups.Select(x => new ChanGroup
            {
                _Inherited = x.inherited,
                IncludeMembersFromParentChannels = x.inherit,
                Inheritable = x.inheritable,
                Name = x.name,
                Members = new HashSet<int>(x.add),
                NegativeMembers = new HashSet<int>(x.remove),
            }).ToArray();

            var aclProxies = aclData.ACLs.Select(x => {
                var acl = new ChanACL
                {
                    _Inherited = x.inherited,
                    ApplyOnSubchannels = x.applySubs,
                    ApplyOnThisChannel = x.applyHere,
                    Allow = (ChannelPermissions)x.allow,
                    Deny = (ChannelPermissions)x.deny,
                };
                if (x.userid == -1)
                    acl.TargetGroup = aclGroups.Where(y => y.Name == x.group).SingleOrDefault() ?? TryLookupSystemGroup(x);
                else
                    acl.TargetUser = x.userid;
                return acl;
            });

            InheritedACL = aclProxies.Where(x => x._Inherited).ToList<IChannelACLEntryReadonly>();
            ACL = aclProxies.Where(x => !x._Inherited).ToList<IChannelACLEntry>();

            InheritedGroups = aclGroups.Where(x => x._Inherited).ToList<IChannelUserGroupInherited>();
            Groups = aclGroups.Where(x => !x._Inherited).ToList<IChannelUserGroup>();
        }

        private SysGroup TryLookupSystemGroup(Murmur.ACL x)
        {
            // Keep this wrapper because I'm lazy and didn't implement ALL system groups
            SysGroup res;
            if (SysGroups.TryGetValue(x.group, out res))
                return res;
            throw new Exception(String.Format("Group not found: '{0}'", x.group));
        }

        Task IReadModifyWriteObject.SaveChanges()
        {
            // Validate that all referenced groups are there.
            // (race condition but fine as this is just a convenience validation)
            // (btw, you could also just implement custom group objects to trick this check)
            if (!ACL.Cast<ChanACL>().All(x => x.TargetGroup == null || SysGroups.Values.Contains(x.TargetGroup)
                || Groups.Concat(InheritedGroups).Any(y => y.Name == ((ChanGroup)x.TargetGroup).Name)))
                throw new InvalidOperationException("Referencing groups that weren't added!");

            return Server.SetACL(ChannelId, new Wrapped.Server.ACLData
            {
                Inherit = Inherit,
                ACLs = ACL.Select(x => new Murmur.ACL
                {
                    allow = (int)x.Allow,
                    deny = (int)x.Deny,
                    applyHere = x.ApplyOnThisChannel,
                    applySubs = x.ApplyOnSubchannels,
                    userid = x.TargetUser ?? -1,
                    group = x.TargetGroup == null ? String.Empty :
                        (x.TargetGroup is ChanGroup ? ((ChanGroup)x.TargetGroup).Name : SysGroups.Single(y => y.Value == x.TargetGroup).Key),
                }).ToArray(),
                Groups = InheritedGroups.Concat(Groups).Cast<ChanGroup>().Select(x => new Murmur.Group
                {
                    name = x.Name,
                    inherited = x._Inherited,
                    inherit = (x is IChannelUserGroupInherited) ? ((IChannelUserGroupInherited)x).IncludeMembersFromParentChannels : false,
                    inheritable = x.Inheritable,
                    add = x.Members.ToArray(),
                    remove = x.NegativeMembers.ToArray(),
                }).ToArray(),
            });
        }

        private const string Alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public IChannelUserGroup CreateGroup(string name)
        {
            if (name.Except(Alphanumeric).Any())
                throw new ArgumentException("Group names must be alphanumeric!", "name");
            if (Groups.Concat(InheritedGroups).Any(x => x.Name == name))
                throw new ArgumentException("A group with that name already exists!", "name");

            return new ChanGroup
            {
                Name = name,
                Inheritable = true,
                Members = new HashSet<int>(),
                NegativeMembers = new HashSet<int>(),
                _Inherited = false,
            };
        }

        public IChannelACLEntry CreateACLEntry(int targetUser)
        {
            var x = new ChanACL
            {
                ApplyOnSubchannels = true,
                ApplyOnThisChannel = true,
                Allow = (ChannelPermissions)0,
                Deny = (ChannelPermissions)0,
                TargetUser = targetUser,
            };
            ACL.Add(x);
            return x;
        }

        public IChannelACLEntry CreateACLEntry(IChannelGroup targetGroup)
        {
            var x = new ChanACL
            {
                ApplyOnSubchannels = true,
                ApplyOnThisChannel = true,
                Allow = (ChannelPermissions)0,
                Deny = (ChannelPermissions)0,
                TargetGroup = targetGroup,
            };
            ACL.Add(x);
            return x;
        }

        private IDictionary<string, SysGroup> SysGroups = new Dictionary<string, SysGroup>
        {
            { "all", new SysGroup() },
            { "auth", new SysGroup() },
            { "in", new SysGroup() },
            { "out", new SysGroup() },
        };

        IChannelGroup IChannelPermissions.SystemGroup_All { get { return SysGroups["all"]; } }
        IChannelGroup IChannelPermissions.SystemGroup_Auth { get { return SysGroups["auth"]; } }
        IChannelGroup IChannelPermissions.SystemGroup_In { get { return SysGroups["in"]; } }
        IChannelGroup IChannelPermissions.SystemGroup_Out { get { return SysGroups["out"]; } }

        private class SysGroup : IChannelGroup
        {
        }

        private class ChanGroup : SysGroup, IChannelUserGroupInherited
        {
            public bool _Inherited { get; set; }

            public bool IncludeMembersFromParentChannels { get; set; }

            public bool Inheritable { get; set; }

            public string Name { get; set; }

            public ISet<int> Members { get; set; }

            public ISet<int> NegativeMembers { get; set; }
        }

        private class ChanACL : IChannelACLEntry
        {
            private IChannelGroup _TargetGroup;
            private int? _TargetUser;

            public bool _Inherited { get; set; }

            public bool ApplyOnSubchannels { get; set; }

            public bool ApplyOnThisChannel { get; set; }

            public IChannelGroup TargetGroup
            {
                get { return _TargetGroup; }
                set
                {
                    _TargetGroup = value;
                    _TargetUser = null;
                }
            }

            public int? TargetUser
            {
                get { return _TargetUser; }
                set
                {
                    _TargetGroup = null;
                    _TargetUser = value;
                }
            }

            public ChannelPermissions Allow { get; set; }

            public ChannelPermissions Deny { get; set; }
        }
    }
}

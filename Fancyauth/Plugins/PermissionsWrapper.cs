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
        public ICollection<IChannelGroup> Groups { get; private set; }
        public IReadOnlyList<IChannelACLEntryReadonly> InheritedACL { get; private set; }
        public IReadOnlyCollection<IChannelGroupInherited> InheritedGroups { get; private set; }

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
                    acl.TargetGroup = aclGroups.Where(y => y.Name == x.group).Single();
                else
                    acl.TargetUser = x.userid;
                return acl;
            });

            InheritedACL = aclProxies.Where(x => x._Inherited).ToList<IChannelACLEntryReadonly>();
            ACL = aclProxies.Where(x => !x._Inherited).ToList<IChannelACLEntry>();

            InheritedGroups = aclGroups.Where(x => x._Inherited).ToList<IChannelGroupInherited>();
            Groups = aclGroups.Where(x => !x._Inherited).ToList<IChannelGroup>();
        }

        Task IReadModifyWriteObject.SaveChanges()
        {
            // Validate that all referenced groups are there.
            // (race condition but fine as this is just a convenience validation)
            // (btw, you could also just implement custom group objects to trick this check)
            if (!ACL.OfType<ChanACL>().All(x => x.TargetGroup == null || Groups.Concat(InheritedGroups).Any(y => y.Name == x.TargetGroup.Name)))
                throw new InvalidOperationException("Referencing groups that weren't added!");

            return Server.SetACL(ChannelId, new Wrapped.Server.ACLData
            {
                Inherit = Inherit,
                ACLs = InheritedACL.Concat(ACL).Select(x => new Murmur.ACL
                {
                    allow = (int)x.Allow,
                    deny = (int)x.Deny,
                    applyHere = x.ApplyOnThisChannel,
                    applySubs = x.ApplyOnSubchannels,
                    userid = x.TargetUser ?? -1,
                    group = x.TargetGroup != null ? x.TargetGroup.Name : "",
                }).ToArray(),
                Groups = InheritedGroups.Concat(Groups).Select(x => new Murmur.Group
                {
                    name = x.Name,
                    inherit = (x is IChannelGroupInherited) ? ((IChannelGroupInherited)x).IncludeMembersFromParentChannels : false,
                    inheritable = x.Inheritable,
                    add = x.Members.ToArray(),
                    remove = x.NegativeMembers.ToArray(),
                }).ToArray(),
            });
        }

        public IChannelGroup CreateGroup(string name)
        {
            if (Groups.Concat(InheritedGroups).Any(x => x.Name == name))
                throw new ArgumentException("A group with that name already exists!");

            return new ChanGroup
            {
                Name = name,
                Inheritable = true,
                Members = new HashSet<int>(),
                NegativeMembers = new HashSet<int>(),
                _Inherited = false,
            };
        }

        private class ChanGroup : IChannelGroupInherited
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

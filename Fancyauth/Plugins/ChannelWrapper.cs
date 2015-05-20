using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fancyauth.API;

namespace Fancyauth.Plugins
{
    public class ChannelWrapper : ChannelShim, IChannel
    {
        private Murmur.Channel Channel;
        private readonly ISet<IChannelShim> LinkedChannels;

        public ChannelWrapper(Wrapped.Server server, Murmur.Channel channel)
            : base(server, channel.id)
        {
            Channel = channel;
            LinkedChannels = new HashSet<IChannelShim>(channel.links.Select(x => new ChannelShim(server, x)));
        }

        Task IReadModifyWriteObject.SaveChanges()
        {
            Channel.links = LinkedChannels.Select(x => x.ChannelId).ToArray();
            return Server.SetChannelState(Channel);
        }

        async Task IReadModifyWriteObject.Refresh()
        {
            Channel = await Server.GetChannelState(Channel.id);
            LinkedChannels.Clear();
            foreach (var chan in Channel.links)
                LinkedChannels.Add(new ChannelShim(Server, chan));
        }

        string IChannel.Description {
            get { return Channel.description; }
            set { Channel.description = value; }
        }

        ISet<IChannelShim> IChannel.LinkedChannels { get { return LinkedChannels; } }

        string IChannel.Name {
            get { return Channel.name; }
            set { Channel.name = value; }
        }

        IChannelShim IChannel.Parent {
            get { return new ChannelShim(Server, Channel.parent); }
            set { Channel.parent = value.ChannelId; }
        }

        int IChannel.Position {
            get { return Channel.position; }
            set { Channel.position = value; }
        }

        bool IChannel.Temporary {
            get { return Channel.temporary; }
            set { Channel.temporary = value; }
        }

        public override bool Equals(object o)
        {
            IChannel t = this;
            IChannel c = o as ChannelWrapper;

            return base.Equals(c)
                && c.Description == t.Description
                && c.LinkedChannels.SetEquals(t.LinkedChannels)
                && c.Name == t.Name
                && c.Parent.Equals(t.Parent)
                && c.Position == t.Position
                && c.Temporary == t.Temporary;
        }
    }
}


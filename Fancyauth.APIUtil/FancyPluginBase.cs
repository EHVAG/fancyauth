using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Fancyauth.API;
using System.Collections.Immutable;

namespace Fancyauth.APIUtil
{
    /// <summary>
    /// FancyPluginBase extends PluginBase in a stateful manner. While Plugins using PluginBase
    /// can only register on events, this FancyPluginBase allows to register on Changes of Objects.
    /// Therefore this class saves states of objects.
    /// </summary>
    public abstract class FancyPluginBase : PluginBase
    {
        private class GlobalListener<T>
        {
            public Func<T, object> Mapper { get; set; }

            public Func<T, Task> Listener { get; set; }

            public GlobalListener(Func<T, object> a, Func<T, Task> b)
            {
                Mapper = a;
                Listener = b;
            }
        }

        private class ActiveListener<T> : GlobalListener<T>
        {
            public object Obj { get; set; }

            public ActiveListener(GlobalListener<T> gl)
                : base(gl.Mapper, gl.Listener)
            {
                Obj = null;
            }
        }

        private readonly ConcurrentBag<GlobalListener<IUser>> GlobalUserListeners = new ConcurrentBag<GlobalListener<IUser>>();
        private readonly ConcurrentDictionary<int, ImmutableList<GlobalListener<IUser>>> SingleUserListeners = new ConcurrentDictionary<int, ImmutableList<GlobalListener<IUser>>>();
        private readonly ConcurrentDictionary<int, ImmutableList<ActiveListener<IUser>>> UserListeners = new ConcurrentDictionary<int, ImmutableList<ActiveListener<IUser>>>();

        private readonly ConcurrentBag<GlobalListener<IChannel>> GlobalChannelListeners = new ConcurrentBag<GlobalListener<IChannel>>();
        private readonly ConcurrentDictionary<int, ImmutableList<GlobalListener<IChannel>>> SingleChannelListeners = new ConcurrentDictionary<int, ImmutableList<GlobalListener<IChannel>>>();
        private readonly ConcurrentDictionary<int, ImmutableList<ActiveListener<IChannel>>> ChannelListeners = new ConcurrentDictionary<int, ImmutableList<ActiveListener<IChannel>>>();
       
        /// <summary>.
        /// Init this instance. It fetches all Users and Channels from the Server and buffers them.
        /// That way only Δ must be applied which saving resources.
        /// </summary>
        public async sealed override Task Init()
        {
            bool success = true;
            foreach (var user in await Server.GetOnlineUsers())
            {
                success &= UserListeners.TryAdd(user.SessionId, ImmutableList<ActiveListener<IUser>>.Empty);
            }
            foreach (var channel in await Server.GetChannels())
            {
                success &= ChannelListeners.TryAdd(channel.ChannelId, ImmutableList<ActiveListener<IChannel>>.Empty);
            }

            if (!success)
            {
                throw new Exception("Init of FancyPluginBase failed due to non-empty listener-lists or duplicte IDs. " +
                    "This should actually not happen - we are so screwed!!!");
            }
        }
        
        /// <summary>
        /// Adds a Global Listener for an IReadModifyWrite-Instance (IChannel/User).
        /// </summary>
        /// <param name="globalListeners">Global listeners of T. Given listener will be added to this.</param>
        /// <param name="listeners">Currently active Listeners of T. Given listener will be added to this.</param>
        /// <param name="mapper">Mapper from T -> T' ⊂ T.</param>
        /// <param name="listener">Listener to be called if T' changes.</param>
        /// <typeparam name="T">IReadModifyWriteObject-Type to register listener on.</typeparam>
        private void AddGlobalListener<T>(ConcurrentBag<GlobalListener<T>> globalListeners,
                                          ConcurrentDictionary<int, ImmutableList<ActiveListener<T>>> listeners,
                                          Func<T, object> mapper, Func<T, Task> listener) where T : IReadModifyWriteObject
        {
            var gl = new GlobalListener<T>(mapper, listener);
            globalListeners.Add(gl);
            foreach (var key in listeners.Keys)
            {
                listeners.AddOrUpdate(key,
                    i => ImmutableList<ActiveListener<T>>.Empty.Add(new ActiveListener<T>(gl)),
                    (i, l) =>
                    {
                        l.Add(new ActiveListener<T>(gl));
                        return l;
                    }
                );
            }
        }
        
        /// <summary>
        /// Adds a single listener for an IReadModifyWriteObject (IUser/IChannel).
        /// </summary>
        /// <param name="singleListeners">Single listeners of T. If `permanent` given listener will be added to this.</param>
        /// <param name="listeners">Currently active Listeners of T. Given listener will be added to this.</param>
        /// <param name="mapper">Mapper from T -> T' ⊂ T.</param>
        /// <param name="listener">Listener to be called whenever T' changes.</param>
        /// <param name="uniqueId">Unique (Session)-Identifier of T. This is how T is identified during changes.</param>
        /// <param name="permanentId">Permanent identifier of T. If T is removed and added again this is still the same.</param>
        /// <param name="permanent">If set to <c>true</c> this listener will be readded if T is removed and readded.</param>
        /// <typeparam name="T">IReadModifyWriteObject-Type to register listener on.</typeparam>
        private void AddSingleListener<T>(ConcurrentDictionary<int, ImmutableList<GlobalListener<T>>> singleListeners,
                                          ConcurrentDictionary<int, ImmutableList<ActiveListener<T>>> listeners,
                                          Func<T, object> mapper, Func<T, Task> listener,
                                          int uniqueId, int permanentId, bool permanent) where T : IReadModifyWriteObject
        {
            var gl = new GlobalListener<T>(mapper, listener);
            if (permanent)
            {
                singleListeners.AddOrUpdate(permanentId,
                    i => ImmutableList<GlobalListener<T>>.Empty.Add(gl),
                    (i, l) =>
                    {
                        l.Add(gl);
                        return l;
                    }
                );
            }

            var al = new ActiveListener<T>(gl);
            listeners.AddOrUpdate(uniqueId,
                i => ImmutableList<ActiveListener<T>>.Empty.Add(al),
                (i, l) =>
                {
                    l.Add(al);
                    return l;
                }
            );
        }

        // Users

        /// <summary>
        /// Adds a Listener for IUsers.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IUser which can be modified. After all functions are finished
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Function taking an IUser which can be modified, returning a Task.</param>
        public void AddListener(Func<IUser, object> mapper, Func<IUser, Task> listener)
        {
            AddGlobalListener(GlobalUserListeners, UserListeners, mapper, listener);
        }

        /// <summary>
        /// Adds a Listener for IUsers.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IUser which can be modified. After all functions are finished
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Action taking an IUser which can be modified.</param>
        public void AddListener(Func<IUser, object> mapper, Action<IUser> listener)
        {
            AddGlobalListener(GlobalUserListeners, UserListeners, mapper, x =>
                {
                    listener(x);
                    return Task.FromResult<object>(null);
                });
        }

        /// <summary>
        /// Adds a Listener for a single IUser identified by it's id.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IUser which can be modified. After all functions are finished
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Function taking an IUser which can be modified and returns a Task.</param>
        /// <param name="user">User to listen on</param>
        /// <param name="permanent">If the Listener should persist after a disconnect of the user</param>
        public void AddSingleListener(Func<IUser, object> mapper, Func<IUser, Task> listener, IUser user, bool permanent)
        {
            AddSingleListener(SingleUserListeners, UserListeners, mapper, listener, user.SessionId, user.UserId, permanent);
        }

        /// <summary>
        /// Adds a Listener for a single IUser identified by it's id.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IUser which can be modified. The modified object will be passed
        /// to all other registered methods. After every function is finished
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Action taking an IUser which can be modified.</param>
        /// <param name="user">User to listen on.</param>
        /// <param name="permanent">If the Listener should persist after a disconnect of the user.</param>
        public void AddSingleListener(Func<IUser, object> mapper, Action<IUser> listener, IUser user, bool permanent)
        {
            AddSingleListener(SingleUserListeners, UserListeners, mapper, x =>
                {
                    listener(x);
                    return Task.FromResult<object>(null);
                }, user.SessionId, user.UserId, permanent);
        }

        // Channels

        /// <summary>
        /// Adds a Listener for IChannels.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IChannel which can be modified. After all functions are finished,
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Function taking an IChannel which can be modified, returning a Task.</param>
        public void AddListener(Func<IChannel, object> mapper, Func<IChannel, Task> listener)
        {
            AddGlobalListener(GlobalChannelListeners, ChannelListeners, mapper, listener);
        }

        /// <summary>
        /// Adds a Listener for IChannels.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IChannel which can be modified. After all functions are finished,
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Action taking an IChannel which can be modified.</param>
        public void AddListener(Func<IChannel, object> mapper, Action<IChannel> listener)
        {
            AddGlobalListener(GlobalChannelListeners, ChannelListeners, mapper, x =>
                {
                    listener(x);
                    return Task.FromResult<object>(null);
                });
        }

        /// <summary>
        /// Adds a Listener for a single IChannel identified by it's id.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IChannel which can be modified. After all functions are finished,
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Function taking an IChannel which can be modified, returning a Task.</param>
        /// <param name="channel">Channel to listen on.</param>
        /// <param name="permanent">If the Listener should persist after removal of the Channel.</param>
        public void AddSingleListener(Func<IChannel, object> mapper, Func<IChannel, Task> listener, IChannel channel, bool permanent)
        {
            AddSingleListener(SingleChannelListeners, ChannelListeners, mapper, listener, channel.ChannelId, channel.ChannelId, permanent);
        }

        /// <summary>
        /// Adds a Listener for a single IChannel identified by it's id.
        /// Every time the result of the first parameter's Func changes,
        /// the second parameter's func is called. That second func receives the
        /// IChannel which can be modified. After all functions are finished,
        /// all changes will be automatically applied.
        /// </summary>
        /// <param name="mapper">Func returning an Anonymous Type containing
        /// all values to register changes on.</param>
        /// <param name="listener">Function taking an IChannel which can be modified, returning a Task.</param>
        /// <param name="channel">Channel to listen on.</param>
        /// <param name="permanent">If the Listener should persist after a removal of the Channel.</param>
        public void AddSingleListener(Func<IChannel, object> mapper, Action<IChannel> listener, IChannel channel, bool permanent)
        {
            AddSingleListener(SingleChannelListeners, ChannelListeners, mapper, x =>
                {
                    listener(x);
                    return Task.FromResult<object>(null);
                }, channel.ChannelId, channel.ChannelId, permanent);
        }
            
        // Events

        public sealed override Task OnUserConnected(IUser user)
        {
            var t0 = OnThingAdded(GlobalUserListeners, SingleUserListeners, UserListeners, user, user.SessionId, user.UserId);
            var t1 = UserConnected(user);
            return Task.WhenAll(t0, t1);
        }

        /// <summary>
        /// Stub method that can be overridden to receive OnUserConnected-Events.
        /// </summary>
        /// <param name="user">User having connected.</param>
        public virtual Task UserConnected(IUser user)
        {
            return Task.FromResult<object>(null);
        }

        public sealed override Task OnChannelCreated(IChannel channel)
        {
            var t0 = OnThingAdded(GlobalChannelListeners, SingleChannelListeners, ChannelListeners, channel, channel.ChannelId, channel.ChannelId);
            var t1 = ChannelCreated(channel);
            return Task.WhenAll(t0, t1);
        }

        /// <summary>
        /// Stub method that can be overridden to receive OnChannelCreated-Events.
        /// </summary>
        /// <param name="channel">Channel having been created.</param>
        public virtual Task ChannelCreated(IChannel channel)
        {
            return Task.FromResult<object>(null);
        }
        
        /// <summary>
        /// Called whenever an IReadModifiedWriteObject has been added.
        /// All globalListeners and all permanent singleListeners being registered on that user
        /// will be added to this new Thing.
        /// </summary>
        /// <param name="globalListeners">Global listeners of T.</param>
        /// <param name="singleListeners">Single listeners of T.</param>
        /// <param name="listeners">Active Listeners of T.</param>
        /// <param name="thing">IReadModifyWriteObject that has been added.</param>
        /// <param name="uniqueId">Unique identifier of `thing`.</param>
        /// <param name="permanentId">Permanent identifier persisting after removal and readding of `thing`.</param>
        /// <typeparam name="T">Type of IReadModifyWriteObject that has been added to tho server.</typeparam>
        private async Task OnThingAdded<T>(ConcurrentBag<GlobalListener<T>> globalListeners,
                                           ConcurrentDictionary<int, ImmutableList<GlobalListener<T>>> singleListeners,
                                           ConcurrentDictionary<int, ImmutableList<ActiveListener<T>>> listeners,
                                           T thing, int uniqueId, int permanentId) where T : IReadModifyWriteObject
        {
            // whenever a thing (e.g. user) is added, all registered listeners for this thing must be added
            // first get all registered users by getting all global listeners combined with registered single listeners
            foreach (var gl in globalListeners.Concat(singleListeners[permanentId]))
            {
                // create ActiveListener (mapper, listener, mapper(thing))
                var al = new ActiveListener<T>(gl);
                // add each listener to the ActiveListeners list
                // if it is the first one, a new List must be created and the object be added
                // otherwise add it to the list
                listeners.AddOrUpdate(uniqueId,
                    // add new list
                    i => ImmutableList<ActiveListener<T>>.Empty.Add(al),
                    // update list
                    (i, l) => l.Add(al)
                );
            }

            // execute all listeners asynchrounusly
            await Task.WhenAll(listeners[uniqueId].Select(t => t.Listener(thing)));
            // save changes
            await thing.SaveChanges();
        }

        public sealed override Task OnUserDisconnected(IUser user)
        {
            var t0 = OnThingRemoved(UserListeners, user.SessionId);
            var t1 = UserDisconnected(user);
            return Task.WhenAll(t0, t1);
        }

        /// <summary>
        /// Stub method to be overridden to receive OnUserDisconnected-Events.
        /// </summary>
        /// <param name="user">User that disconnected.</param>
        public virtual Task UserDisconnected(IUser user)
        {
            return Task.FromResult<object>(null);
        }

        public override Task OnChannelDeleted(IChannel channel)
        {
            var t0 = OnThingRemoved(ChannelListeners, channel.ChannelId);
            var t1 = ChannelDeleted(channel);
            return Task.WhenAll(t0, t1);
        }

        /// <summary>
        /// Stub method to be overridden to receive OnChannelDeleted-Events.
        /// </summary>
        /// <param name="channel">Deleted Channel.</param>
        public virtual Task ChannelDeleted(IChannel channel)
        {
            return Task.FromResult<object>(null);
        }

        private Task OnThingRemoved<T>(ConcurrentDictionary<int, ImmutableList<ActiveListener<T>>> listeners, int uniqueId) where T : IReadModifyWriteObject
        {
            ImmutableList<ActiveListener<T>> dontCare;
            if (!listeners.TryRemove(uniqueId, out dontCare))
            {
                throw new Exception("If this happens we are even more screwed than if one is thrown in Init()");
            }
            return Task.FromResult<object>(null);
        }

        public sealed override Task OnUserModified(IUser user)
        {
            return OnThingModified(UserListeners, user, user.SessionId);
        }

        public sealed override Task OnChannelModified(IChannel channel)
        {
            return OnThingModified(ChannelListeners, channel, channel.ChannelId);
        }

        private async Task OnThingModified<T>(ConcurrentDictionary<int, ImmutableList<ActiveListener<T>>> listeners, T thing, int uniqueId) where T : IReadModifyWriteObject
        {
            // every time a user is modified, all listening functions with changed objects need to be called
            // get all listening functions of the user
            await Task.WhenAll(listeners[uniqueId]
                // check if function has recently been added (Obj == null)
                // or if the old saved object differs from the newly calculated (and directly saved) one
                .Where(t =>
                    {
                        var prev = t.Obj;
                        t.Obj = t.Mapper(thing);
                        return null == prev || !prev.Equals(t.Obj);
                    })
                // get all listener functions
                .Select(t => t.Listener(thing))
            );

            // save the user after all Tasks have been finished
            await thing.SaveChanges();
        }
    }
}


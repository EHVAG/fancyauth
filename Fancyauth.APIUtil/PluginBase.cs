using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.API.Commands;
using Fancyauth.API.ContextCallbacks;

namespace Fancyauth.APIUtil
{
    /// <summary>
    /// Fancyauth plugin base class.
    /// </summary>
    /// <remarks>
    /// This provides a nice interface for plugins, removing most boilerplate code.
    /// </remarks>
    [InheritedExport(typeof(IFancyPlugin))]
    public class PluginBase : IFancyPlugin
    {
        [Import]
        protected IServer Server { get; private set; }
        [Import]
        protected IContextCallbackManager ContextCallbackManager { get; private set; }
        [Import]
        protected ICommandManager CommandManager { get; private set; }

        private List<ProxyCC.Server> ServerCallbacks = new List<ProxyCC.Server>();
        private List<ProxyCC.Channel> ChannelCallbacks = new List<ProxyCC.Channel>();
        private List<ProxyCC.User> UserCallbacks = new List<ProxyCC.User>();

        void IFancyPlugin.Init()
        {
            // TODO: improve these exceptions. Like, SIGNIFICANTLY.
            foreach (var method in GetType().GetMethods()) {
                var cmdattrs = method.GetCustomAttributes<CommandAttribute>();
                foreach (var cmdattr in cmdattrs) {
                    var autoName = method.Name.Substring(0, 1).ToLowerInvariant() + method.Name.Substring(1);
                    var name = cmdattr.Name ?? autoName;
                    var target = typeof(CommandFunc).GetMethod("Invoke");
                    if (method.ReturnType != target.ReturnType)
                        throw new InvalidOperationException("Return types don't match!");

                    CommandFunc cf;
                    var mparams = method.GetParameters().AsEnumerable();
                    var tparams = target.GetParameters().Select(x => x.ParameterType);
                    if (mparams.Select(x => x.ParameterType).SequenceEqual(tparams))
                        // straight up template. great.
                        cf = (CommandFunc)Delegate.CreateDelegate(typeof(CommandFunc), this, method);
                    else
                        cf = CommandBinder.CompileBinding(method, name, mparams, this);

                    CommandManager.RegisterCommand(name, cf);
                }

                var ccattrs = method.GetCustomAttributes<ContextCallbackAttribute>();
                foreach (var ccattr in ccattrs) {
                    if (method.ReturnType != typeof(Task))
                        throw new InvalidOperationException("must return task");

                    var mparams = method.GetParameters().Select(x => x.ParameterType);
                    if (mparams.SequenceEqual(new Type[] { typeof(IUser) }))
                        ServerCallbacks.Add(new ProxyCC.Server {
                            Name = Guid.NewGuid().ToString(),
                            Text = ccattr.Text,
                            Callback = (Func<IUser, Task>)Delegate.CreateDelegate(typeof(Func<IUser, Task>), this, method)
                        });
                    else if (mparams.SequenceEqual(new Type[] { typeof(IUser), typeof(IChannelShim) }))
                        ChannelCallbacks.Add(new ProxyCC.Channel {
                            Name = Guid.NewGuid().ToString(),
                            Text = ccattr.Text,
                            Callback = (Func<IUser, IChannelShim, Task>)Delegate.CreateDelegate(typeof(Func<IUser, IChannelShim, Task>), this, method)
                        });
                    else if (mparams.SequenceEqual(new Type[] { typeof(IUser), typeof(IUserShim) }))
                        UserCallbacks.Add(new ProxyCC.User {
                            Name = Guid.NewGuid().ToString(),
                            Text = ccattr.Text,
                            Callback = (Func<IUser, IUserShim, Task>)Delegate.CreateDelegate(typeof(Func<IUser, IUserShim, Task>), this, method)
                        });
                    else
                        throw new InvalidOperationException("invalid params");
                }
            }

            this.Init();
        }

        async Task IFancyPlugin.OnUserConnected(IUser user)
        {
            foreach (var cb in ServerCallbacks)
                await this.ContextCallbackManager.Register(user, cb);
            foreach (var cb in ChannelCallbacks)
                await this.ContextCallbackManager.Register(user, cb);
            foreach (var cb in UserCallbacks)
                await this.ContextCallbackManager.Register(user, cb);

            await this.OnUserConnected(user);
        }

        // Here, we just pass through IFancyPlugin's methods.
        // But with stub impls, so you can just override what you need.
        public virtual void Init()
        {
        }
        
        public virtual Task OnUserConnected(IUser user)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnUserModified(IUser user)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnUserDisconnected(IUser user)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnChannelCreated(IChannel channel)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnChannelModified(IChannel channel)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnChannelDeleted(IChannel channel)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnChatMessage(IUser sender, IEnumerable<IChannelShim> channels, string message)
        {
            return Task.FromResult<object>(null);
        }
    }
}


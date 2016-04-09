using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.API.ContextCallbacks;
using Fancyauth.APIUtil;
using Fancyauth.Model;
using Fancyauth.Model.UserAttribute;

namespace Fancyauth.Plugins.Builtin
{
    public class Godfather : PluginBase
    {
        public override async Task OnUserConnected(IUser user)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (context.Users.Where(u => u.Id == user.UserId).select(u => u.Membership) != null)
                {
                    await ContextCallbackManager.Register(user, new BecomeGodfather());
                    await ContextCallbackManager.Register(user, new RevokeGodfather());
                }
                transact.Commit();
            }
        }

        private class BecomeGodfather : IUserContextCallback
        {
            public string Name { get; }
            public string Text { get; }

            public BecomeGodfather()
            {
                Name = "becomeGodfather";
                Text = "Become Godfather";
            }

            public async Task Run(IUser actor, IUserShim targetShim)
            {
                var target = await targetShim.Load();
                using (var context = await FancyContext.Connect())
                using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var godfather = context.Users.Include(u => u.Membership).Where(u => u.Id == actor.UserId);

                    // if godfather is not a real member, this method call was super duper obsolete
                    if (godfather.Membership == null)
                        return;

                    var guest = context.Users.Include(u => u.Membership)
                        .Include(u => u.GuestInvite)
                        .Include(u => u.PersistentGuest)
                        .Where(u => u.Id == target.UserId);

                    // if guest is already a real member, same applies
                    if (guest.Membership != null)
                        return;

                    // if user is already a persistent guest, add new godfather
                    if (guest.PersistentGuest != null)
                    {
                        guest.PersistentGuest.Godfathers.add(godfather.Membership);
                    }
                    // otherwise add new PersistentGuest property
                    else
                    {
                        guest.PersistentGuest = new PersistentGuest
                        {
                            Godfathers = new List<Membership>() {godfather.Membership},
                            OriginalInvite = guest.GuestInvite,
                        };
                        guest.GuestInvite = null;
                    }
                    await context.SaveChangesAsync();
                    transact.Commit();
                }
            }
        }

        private class RevokeGodfather : IUserContextCallback
        {
            public string Name { get; }
            public string Text { get; }

            public RevokeGodfather()
            {
                Name = "revokeGodfather";
                Text = "Revoke Godfather";
            }

            public async Task Run(IUser actor, IUserShim targetShim)
            {
                var target = await targetShim.Load();
                using (var context = await FancyContext.Connect())
                using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var guest = context.Users.Include(u => u.Membership)
                        .Include(u => u.GuestInvite)
                        .Include(u => u.PersistentGuest)
                        .Where(u => u.Id == target.UserId);

                    // if guest is already a real member or guest not a persistent guest, why the heck did someone call this method
                    if (guest.Membership != null || guest.PersistentGuest == null)
                        return;

                    var membership = guest.PersistentGuest.Godfathers.Where(m => m.UserId == actor.UserId).Single();
                    if (membership != null)
                    {
                        guest.PersistentGuest.Godfathers.Remove(membership);
                        // no godfather anymore :(
                        if (guest.PersistentGuest.Godfathers.Count == 0)
                        {
                            guest.GuestInvite = guest.PersistentGuest.OriginalInvite;
                            guest.PersistentGuest = null;
                        }
                    }
                    await context.SaveChangesAsync();
                    transact.Commit();
                }
            }
        }
    }
}

using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Fancyauth.API;
using Fancyauth.API.DB;
using Fancyauth.APIUtil;
using Fancyauth.Model;
using Fancyauth.API.ContextCallbacks;
using System.Data;
using System.Data.Entity;
using Fancyauth.Model.UserAttribute;
using System.Collections.Generic;

namespace Fancyauth.Plugins.Builtin
{
    public class Invites : PluginBase
    {
        private const string PwdChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789äö";
        private const int PwdIndexMask = 0x3f;
        private readonly RandomNumberGenerator InviteRng = RandomNumberGenerator.Create();

        static Invites()
        {
            if (PwdChars.Length != (PwdIndexMask + 1))
                throw new Exception("mismatch between PwdChars and PwdIndexMask");

            for (var i = PwdIndexMask; i != 0; i >>= 1)
                if ((i & 1) == 0)
                    throw new Exception("bit gap in PwdIndexMask");
        }

        [Import]
        public IFancyContextProvider ContextProvider { get; set; }

        [Import]
        public Fancyauth Fancyauth { get; set; }

        [Command]
        public async Task Invite(IUser usr)
        {
            var rand = new byte[42];
            InviteRng.GetBytes(rand);
            var codeBuilder = new StringBuilder(rand.Length);
            for (int i = 0; i < rand.Length; i++)
                codeBuilder.Append(PwdChars[rand[i] & PwdIndexMask]);

            var code = codeBuilder.ToString();
            using (var context = await ContextProvider.Connect())
            {
                context.Invites.Add(new Invite
                {
                    Code = code,
                    Inviter = context.Users.Attach(new User { Id = usr.UserId }),
                    ExpirationDate = DateTimeOffset.Now.AddHours(1),
                });

                await context.SaveChangesAsync();
            }

            await usr.SendMessage("Invite code: " + code);
        }

        // happens not often enough to actually be a ContextCallback
        [Command]
        public async Task RevokeGodfather(IUser actor, string targetName)
        {
            using (var context = await ContextProvider.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var guest = await context.Users.Include(u => u.GuestInvite).Include(u => u.PersistentGuest.Godfathers)
                    .SingleAsync(u => u.Name == targetName);

                // if guest is not a persistent guest, why the heck did someone call this method
                if (guest.PersistentGuest == null)
                {
                    await actor.SendMessage("How about no.");
                    transact.Commit();
                    return;
                }

                var membership = guest.PersistentGuest.Godfathers.Single(m => m.UserId == actor.UserId);
                if (membership != null)
                {
                    guest.PersistentGuest.Godfathers.Remove(membership);

                    // no godfather anymore --> downgrade to regular guest
                    if (guest.PersistentGuest.Godfathers.Count == 0)
                    {
                        guest.GuestInvite = guest.PersistentGuest.OriginalInvite;
                        context.PersistentGuests.Remove(guest.PersistentGuest);
                        context.CertificateCredentials.Remove(guest.CertCredentials);
                        guest.PersistentGuest = null;
                        guest.CertCredentials = null;

                        await actor.SendMessage("Removed. Downgraded to a normal guest.");
                    }
                    else
                        await actor.SendMessage("Removed. User is still a persistent guest though.");
                }
                else
                    await actor.SendMessage("He was never yours anyways.");


                await context.SaveChangesAsync();
                transact.Commit();
            }
        }

        [ContextCallback("Ein Angebot, das er nicht ablehnen kann.")]
        public async Task BecomeGodfather(IUser actor, IUserShim targetShim)
        {
            var target = await targetShim.Load();
            using (var context = await ContextProvider.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var godfather = await context.Users.Include(u => u.Membership).SingleAsync(u => u.Id == actor.UserId);

                // if godfather is not a real member, this method call was super duper obsolete
                if (godfather.Membership == null)
                    return;

                var guest = await context.Users.Include(u => u.Membership).Include(u => u.GuestInvite)
                    .Include(u => u.PersistentGuest.Godfathers).SingleAsync(u => u.Id == target.UserId);

                if (guest.PersistentGuest == null)
                {
                    // only guests can be promoted to persistentguests
                    if (guest.GuestInvite == null)
                        return;

                    guest.PersistentGuest = new PersistentGuest
                    {
                        OriginalInvite = guest.GuestInvite,
                    };
                    guest.GuestInvite = null;

                    CertificateCredentials creds;
                    if (Fancyauth.GuestCredentials.TryGetValue(target.UserId, out creds))
                        guest.CertCredentials = creds;
                    else
                    {
                        await actor.SendMessage("No certificate found.");
                        return;
                    }
                }

                // ignore the nop case
                if (guest.PersistentGuest.Godfathers.Contains(godfather.Membership))
                    return;

                guest.PersistentGuest.Godfathers.Add(godfather.Membership);

                await context.SaveChangesAsync();
                transact.Commit();
            }

            await target.SendMessage("Irgendwann - möglicherweise aber auch nie - werde ich dich bitten, mir eine kleine Gefälligkeit zu erweisen.");
        }
    }
}


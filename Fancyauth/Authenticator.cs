using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fancyauth.Model;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Fancyauth.Model.UserAttribute;

namespace Fancyauth
{
    public class Authenticator : Wrapped.Authenticator
    {
        private readonly Fancyauth Fancyauth;

        public Authenticator(Fancyauth fancyauth)
        {
            Fancyauth = fancyauth;
        }

        public override async Task<Wrapped.AuthenticationResult> Authenticate(string name, string pw, byte[][] certificates, string certhash, bool certstrong)
        {
            string fingerprint = null;
            long? certSerial = null;
            string subCN = null;
            Org.BouncyCastle.X509.X509Certificate bouncyCert = null;
            if (certificates.Length > 0) {
                var certs = certificates.Select(x => new X509Certificate2(x)).ToArray();
                var usercert = certs.Last();
                var chain = new X509Chain();
                foreach (var cert in certs)
                    chain.ChainPolicy.ExtraStore.Add(cert);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain.Build(usercert);

                bouncyCert = new X509CertificateParser().ReadCertificate(certificates.Last());
                var subDN = bouncyCert.SubjectDN;
                subCN = subDN.GetValueList()[subDN.GetOidList().IndexOf(X509ObjectIdentifiers.CommonName)] as string;
                certSerial = bouncyCert.SerialNumber.LongValue;

                fingerprint = usercert.Thumbprint;
            } else {
                // oh @moritzuehling y u do dis to me Q_Q
                // (no certs at all)
            }

            CertificateCredentials creds = null;
            if (certSerial.HasValue)
                creds = new CertificateCredentials
                {
                    Fingerprint = fingerprint,
                    CertSerial = certSerial.Value,
                };


            User user;
            bool isGuest = false;
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable)) {
                user = await context.Users.Include(x => x.Membership).Include(x => x.PersistentGuest)
                                    .Include(x => x.PersistentGuest.Godfathers).SingleOrDefaultAsync(x => x.CertCredentials.Any(y => y.Fingerprint == fingerprint));

                if (user != null)
                {
                    // Known user. Why?
                    // * member
                    // * persistent guest
                    //
                    // As this is the /authenticator/, we can't query online users because that would deadlock murmur's main thread.
                    // As someone with CertificateCredentials is definitely allowed to connect, we just let them pass here and
                    // kick them in OnUserConnected if they're missing a godfather.
                }
                else
                {
                    // Unknown user. Why?
                    // * temporary guest
                    // * new cert for existing user
                    // * new user
                    // * random person on the internet

                    // Let's first check for guest invites
                    pw = pw.Trim();
                    var invite = await context.Invites.SingleOrDefaultAsync(x => (x.Code == pw) && (x.ExpirationDate > DateTimeOffset.Now));
                    if (invite != null)
                    {
                        // Try to match by name.
                        user = await context.Users.Include(x => x.PersistentGuest).SingleOrDefaultAsync(x => x.Name == name);

                        if (user != null)
                        {
                            if (user.GuestInvite == null)
                            {
                                // In case the name is already taken by a non-guest, we force them to a
                                // random but different name so everyone can see they're a guest.
                                name += "-guest-" + Guid.NewGuid().ToString();
                                user = null;
                            }
                            else
                            {
                                // The account once belonged to a guest? Nice.
                                // But adjust to the new guest invite.
                                user.GuestInvite = invite;
                                // (Note that we don't care about the edge case where guests get ghost-kicked
                                //  by other guests because we simply don't care about guests.)
                            }
                        }

                        if (user == null)
                        {
                            // Create a new user for this name if we need to.
                            user = context.Users.Add(new User
                            {
                                Name = name,
                                GuestInvite = invite,
                            });
                        }

                        isGuest = true;
                    }
                    else
                    {
                        // random person on the internet; has no signed or valid certificate
                        if (!certstrong)
                            return Wrapped.AuthenticationResult.Forbidden();

                        // New cert for existing user?
                        /*
                         * not longer supported
                        foreach (System.Collections.ICollection sans in bouncyCert.GetSubjectAlternativeNames())
                        {
                            var enm = sans.GetEnumerator();
                            enm.MoveNext();
                            enm.MoveNext();
                            var val = enm.Current as string;
                            var match = Regex.Match(val ?? String.Empty, "^([^@]*)@user.mumble.ehvag.de$");
                            if (match.Success)
                            {
                                var oldName = match.Groups[1].Captures[0].Value;
                                var existingUser = await context.Users.Include(x => x.CertCredentials).SingleOrDefaultAsync(x => x.Name == oldName && x.CertCredentials.CertSerial < certSerial);
                                if (existingUser != null)
                                {
                                    existingUser.Name = subCN;
                                    existingUser.CertCredentials.CertSerial = certSerial.Value;
                                    existingUser.CertCredentials.Fingerprint = fingerprint;
                                    user = existingUser;
                                    break;
                                }
                            }
                        }
                        */

                        if (user == null)
                        {
                            // no existing user found, so create new user
                            user = context.Users.Add(new User
                            {
                                Name = subCN,
                                CertCredentials = new List<CertificateCredentials> { creds },
                                Membership = new Membership()
                            });
                        }
                    }
                }

                // As stated above, we can't query mumble for connected users to reject persistent guests.
                // However, we can use the logs in the database as a heuristic to get currently connected users.
                if (user.PersistentGuest != null)
                {
                    var godfathers = user.PersistentGuest.Godfathers.Select(u => u.UserId).ToArray();
                    var godfathersQuery = from usr in context.Users
                        from godfathership in usr.Membership.Godfatherships
                        where godfathership.UserId == user.Id
                        join e in context.Logs.OfType<LogEntry.Connected>() on usr.Id equals e.Who.Id into connectEvents
                        join e in context.Logs.OfType<LogEntry.Disconnected>() on usr.Id equals e.Who.Id into disconnectEvents
                        select new {Con = connectEvents.Max(x => x.When), Dis = disconnectEvents.Max(x => x.When)};
                    var godfatherConnected = await godfathersQuery.AnyAsync(l => l.Con > l.Dis);
                    if (!godfatherConnected)
                    {
                        return Wrapped.AuthenticationResult.Forbidden();
                    }
                }

                await context.SaveChangesAsync();

                transact.Commit();
            }

            if (isGuest && (creds != null))
                Fancyauth.GuestCredentials.AddOrUpdate(user.Id, creds, (k, c) => creds);

            string[] groups = null;
            if (user.PersistentGuest != null)
                groups = PersistentGuestGroups;

            return Wrapped.AuthenticationResult.Success(user.Id, user.Name, groups);
        }
        private static readonly string[] PersistentGuestGroups = { "guys" };

        public override async Task<string> IdToName(int id)
        {
            using (var context = new FancyContext())
                return await context.Users.Where(x => x.Id == id).Select(x => x.Name).SingleOrDefaultAsync();
        }

        public override async Task<int?> NameToId(string name)
        {
            using (var context = new FancyContext())
                return await context.Users.Where(x => x.Name == name).Select(x => x.Id).SingleOrDefaultAsync();
        }

        public override async Task<byte[]> IdToTexture(int id)
        {
            using (var context = new FancyContext())
                return await context.Memberships.Where(x => x.UserId == id).Select(x => x.Texture).SingleOrDefaultAsync();
        }

        public override async Task<Wrapped.AuthenticatorUpdateResult> SetTexture(int id, byte[] texture)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable)) {
                var entity = new Membership { UserId = id, Texture = texture };
                context.Memberships.Attach(entity);
                context.Entry(entity).Property(x => x.Texture).IsModified = true;
                context.Configuration.ValidateOnSaveEnabled = false;
                try {
                    await context.SaveChangesAsync();
                } catch (Exception) {
                    return Wrapped.AuthenticatorUpdateResult.Failure;
                }

                transact.Commit();
                return Wrapped.AuthenticatorUpdateResult.Success;
            }
        }

        public override async Task<Dictionary<Murmur.UserInfo, string>> GetInfo(int id)
        {
            using (var context = new FancyContext()) {
                var user = await context.Users.Include(x => x.Membership).SingleAsync(x => x.Id == id);
                return new Dictionary<Murmur.UserInfo, string> {
                    { Murmur.UserInfo.UserName, user.Name },
                    { Murmur.UserInfo.UserComment, user.Membership?.Comment },
                };
            }
        }

        public override async Task<Wrapped.AuthenticatorUpdateResult> SetInfo(int id, Dictionary<Murmur.UserInfo, string> info)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable)) {
                var user = await context.Users.Include(x => x.Membership).SingleAsync(x => x.Id == id);
                foreach (var kv in info) {
                    switch (kv.Key) {
                    case Murmur.UserInfo.UserComment:
                        user.Membership.Comment = kv.Value;
                        break;
                    default:
                        System.Diagnostics.Trace.WriteLine(kv.Key, "Unhandled thing in SetInfo");
                        transact.Rollback();
                        return Wrapped.AuthenticatorUpdateResult.Failure;
                    }
                }

                await context.SaveChangesAsync();
                transact.Commit();
                return Wrapped.AuthenticatorUpdateResult.Success;
            }
        }

        public override async Task<Dictionary<int, string>> GetRegisteredUsers(string filter)
        {
            using (var context = new FancyContext()) {
                var users = context.Users.Select(x => new { x.Id, x.Name });
                if (!String.IsNullOrEmpty(filter))
                    users = users.Where(x => x.Name.Contains(filter));
                return await users.ToDictionaryAsync(x => x.Id, x => x.Name);
            }
        }
    }
}


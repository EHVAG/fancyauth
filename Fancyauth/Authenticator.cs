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

namespace Fancyauth
{
    public class Authenticator : Wrapped.Authenticator
    {
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


            User user;
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable)) {
                user = await context.Users.Where(x => x.Fingerprint == fingerprint).SingleOrDefaultAsync();

                if (user == null) {
                    // check for user migration
                    // TODO: remove this once we got fingerprints everywhere
                    user = await context.Users.Where(x => x.Fingerprint.StartsWith("imported") && x.CertSerial == certSerial).SingleOrDefaultAsync();
                    if (user != null)
                        user.Fingerprint = fingerprint;
                }

                if (user == null) {
                    // Unknown user. Why?
                    // * guest
                    // * new cert for existing user
                    // * new user
                    // * random person on the internet
                    var invite = await context.Invites.Where(x => (x.Code == pw.Trim()) && (x.ExpirationDate > DateTime.UtcNow)).Select(x => new { x.Id, x.UseCount }).SingleOrDefaultAsync();
                    if (invite != null) {
                        var tmpinv = context.Invites.Attach(new Invite { Id = invite.Id, UseCount = invite.UseCount + 1 });
                        context.GuestAssociations.Add(new GuestAssociation { Name = name, Invite = tmpinv });
                        context.Entry(tmpinv).Property(x => x.UseCount).IsModified = true;
                        context.Configuration.ValidateOnSaveEnabled = false;
                        try {
                            await context.SaveChangesAsync();
                            transact.Commit();
                            return Wrapped.AuthenticationResult.Fallthrough();
                        } catch (System.Data.Entity.Validation.DbEntityValidationException ex) {
                            foreach (var err in ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors))
                                Console.WriteLine("{0} - {1}", err.PropertyName, err.ErrorMessage);
                            return Wrapped.AuthenticationResult.TryAgainLater();
                        }
                    } else {
                        if (!certstrong)
                            return Wrapped.AuthenticationResult.Forbidden();

                        foreach (System.Collections.ICollection sans in bouncyCert.GetSubjectAlternativeNames()) {
                            var enm = sans.GetEnumerator();
                            enm.MoveNext();
                            enm.MoveNext();
                            var val = enm.Current as string;
                            var match = Regex.Match(val ?? String.Empty, "^([^@]*)@user.mumble.ehvag.de$");
                            if (match.Success) {
                                var oldName = match.Groups[1].Captures[0].Value;
                                var existingUser = await context.Users.Where(x => x.Name == oldName && x.CertSerial < certSerial).SingleOrDefaultAsync();
                                if (existingUser != null) {
                                    existingUser.Name = subCN;
                                    existingUser.CertSerial = certSerial.Value;
                                    existingUser.Fingerprint = fingerprint;
                                    user = existingUser;
                                    break;
                                }
                            }
                        }

                        if (user == null) {
                            // no existing user found, so create new user
                            user = context.Users.Add(new User {
                                Name = subCN,
                                Fingerprint = fingerprint,
                                CertSerial = certSerial.Value,
                            });
                        }
                    }
                }

                await context.SaveChangesAsync();
                transact.Commit();
            }

            return Wrapped.AuthenticationResult.Success(user.Id, user.Name);
        }

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
                return await context.Users.Where(x => x.Id == id).Select(x => x.Texture).SingleOrDefaultAsync();
        }

        public override async Task<Wrapped.AuthenticatorUpdateResult> SetTexture(int id, byte[] texture)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable)) {
                var entity = new User { Id = id, Texture = texture };
                context.Users.Attach(entity);
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
                var user = await context.Users.FindAsync(id);
                return new Dictionary<Murmur.UserInfo, string> {
                    { Murmur.UserInfo.UserName, user.Name },
                    { Murmur.UserInfo.UserComment, user.Comment },
                };
            }
        }

        public override async Task<Wrapped.AuthenticatorUpdateResult> SetInfo(int id, Dictionary<Murmur.UserInfo, string> info)
        {
            using (var context = await FancyContext.Connect())
            using (var transact = context.Database.BeginTransaction(IsolationLevel.Serializable)) {
                var user = await context.Users.FindAsync(id);
                foreach (var kv in info) {
                    switch (kv.Key) {
                    case Murmur.UserInfo.UserComment:
                        user.Comment = kv.Value;
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


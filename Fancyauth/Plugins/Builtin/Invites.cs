using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.APIUtil;

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

        [Command]
        public async Task Invite(IUser usr)
        {
            var rand = new byte[42];
            InviteRng.GetBytes(rand);
            var codeBuilder = new StringBuilder(rand.Length);
            for (int i = 0; i < rand.Length; i++)
                codeBuilder.Append(PwdChars[rand[i] & PwdIndexMask]);

            var code = codeBuilder.ToString();
            using (var context = await FancyContext.Connect()) {
                context.Invites.Add(new Model.Invite {
                    Code = code,
                    Inviter = context.Users.Attach(new Model.User { Id = usr.UserId }),
                    ExpirationDate = DateTimeOffset.Now.AddHours(1),
                });

                await context.SaveChangesAsync();
            }

            await usr.SendMessage("Invite code: " + code);
        }
    }
}


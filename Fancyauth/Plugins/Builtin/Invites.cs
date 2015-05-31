using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.API.DB;
using Fancyauth.APIUtil;
using Fancyauth.Model;

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

        [Command]
        public async Task Invite(IUser usr)
        {
            var rand = new byte[42];
            InviteRng.GetBytes(rand);
            var codeBuilder = new StringBuilder(rand.Length);
            for (int i = 0; i < rand.Length; i++)
                codeBuilder.Append(PwdChars[rand[i] & PwdIndexMask]);

            var code = codeBuilder.ToString();
            using (var context = await ContextProvider.Connect()) {
                context.Invites.Add(new Invite {
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


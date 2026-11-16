using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using Fancyauth.Model;

namespace Fancyauth
{
    /// <summary>
    /// Messages that integrations (Telegram, Steam) relay on a user's behalf: Mumble can only show them as server messages, so
    /// the chat log records them here, with the integration, instead of in ServerCallback.UserTextMessage.
    /// </summary>
    internal static class ChatRelay
    {
        /// <param name="message">HTML, as Mumble messages are.</param>
        public static async Task Post(Wrapped.Server server, int userId, string name, int channel, string message, string via)
        {
            await server.SendMessageChannel(channel, false,
                WebUtility.HtmlEncode(name) + " (" + WebUtility.HtmlEncode(via) + "): " + message);

            using (var context = await FancyContext.Connect())
            {
                context.Logs.Add(new LogEntry.ChatMessage
                {
                    When = DateTimeOffset.Now,
                    Who = await context.Users.SingleAsync(x => x.Id == userId),
                    Where = await context.Channels.SingleAsync(x => x.ServerId == channel),
                    Message = message,
                    Via = via,
                });
                await context.SaveChangesAsync();
            }
        }
    }
}

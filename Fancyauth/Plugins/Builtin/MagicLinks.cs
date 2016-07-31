using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.APIUtil;

namespace Fancyauth.Plugins.Builtin
{
    public class MagicLinks : PluginBase
    {
        private static readonly Regex LinkPattern = new Regex(@"^<a href=""([^""]*)"">([^<]*)<\/a>$", RegexOptions.Compiled);
        private static readonly Regex ImgurPattern = new Regex(@"^https?:\/\/(i\.)?imgur\.com\/([^./]*)(\.(png|jpg))?$", RegexOptions.Compiled);
        private static readonly Regex ImgurAlbumPattern = new Regex(@"^https?:\/\/imgur\.com\/a/(.*)$", RegexOptions.Compiled);
        private static readonly Regex ImgurExtractAlbumPattern = new Regex(@"<img src=""\/\/i.imgur\.com\/(.*?)""", RegexOptions.Compiled);
        private static readonly Regex GagPattern = new Regex(@"^https?:\/\/9gag\.com\/gag\/([^.]*)$", RegexOptions.Compiled);
        private const string ResizeImg = @"<a href=""{0}""><img src=""https://i.embed.ly/1/display/resize?key=412444a1782b460cada0ba48eb988d12&url={1}&width=200&grow=false"" alt=""9gag post"" /></a>";

        public override async Task OnChatMessage(API.IUser sender, IEnumerable<IChannelShim> channels, string message)
        {
            var linkMatch = LinkPattern.Match(message);
            if (linkMatch.Success && (linkMatch.Groups[1].Captures[0].Value == linkMatch.Groups[2].Captures[0].Value)) {
                var link = linkMatch.Groups[1].Captures[0].Value;
                var imgurMatch = ImgurPattern.Match(link);
                var imgurAlbumMatch = ImgurAlbumPattern.Match(link);
                var gagMatch = GagPattern.Match(link);
                string img;
                if (imgurMatch.Success)
                    img = String.Format("http://i.imgur.com/{0}.png", imgurMatch.Groups[2].Captures[0].Value);
                else if (imgurAlbumMatch.Success)
                {
                    using (var client = new HttpClient())
                        img = ImgurExtractAlbumPattern.Match(await client.GetStringAsync(link)).Groups[1].Captures[0].Value;
                }

                else if (gagMatch.Success)
                    img = String.Format("http://img-9gag-lol.9cache.com/photo/{0}_460s.jpg", gagMatch.Groups[1].Captures[0].Value);
                else
                    img = null;

                if (img != null)
                    foreach (var chan in channels)
                        await chan.SendMessage(String.Format(ResizeImg, link, img));
            }
        }
    }
}


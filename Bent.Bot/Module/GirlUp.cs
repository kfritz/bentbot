using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bent.Bot.Common;
using Bent.Bot.Configuration;
using Newtonsoft.Json.Linq;

namespace Bent.Bot.Module
{
    public class GirlUp : IModule
    {
        private static Regex regex = new Regex(@"girl\s+up\s+the\s+chat", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        private Random random = new Random();

        private IConfiguration config;
        private IBackend backend;

        public void OnStart(IConfiguration config, IBackend backend)
        {
            this.config = config;
            this.backend = backend;
        }

        public async void OnMessage(IMessage message)
        {
            if (!message.IsFromMyself && !message.IsHistorical)
            {
                var match = regex.Match(message.FullBody);

                if (match.Success)
                {
                    await backend.SendMessageAsync(message.ReplyTo, await GetRandomProgrammerGoslingUrlAsync() + " " + new String('~', 3 + random.Next(11)));
                }
            }
        }

        private async Task<string> GetRandomProgrammerGoslingUrlAsync()
        {
            var apiKey = this.config[Constants.ConfigKey.TumblrApiKey];

            var infoUrl = "http://api.tumblr.com/v2/blog/programmerryangosling.tumblr.com/info?api_key=" + HttpUtility.UrlEncode(apiKey);

            var infoResponse = await new HttpClient().GetAsync(infoUrl);
            infoResponse.EnsureSuccessStatusCode();

            int posts = (JObject.Parse(await infoResponse.Content.ReadAsStringAsync()) as dynamic).response.blog.posts;

            var imageUrl = "http://api.tumblr.com/v2/blog/programmerryangosling.tumblr.com/posts?limit=1&api_key=" + HttpUtility.UrlEncode(apiKey) + "&offset=" + random.Next(posts);
            var imageResponse = await new HttpClient().GetAsync(imageUrl);
            imageResponse.EnsureSuccessStatusCode();

            var image = (JObject.Parse(await imageResponse.Content.ReadAsStringAsync()) as dynamic);

            return image.response.posts[0].photos[0].original_size.url;
        }
    }
}

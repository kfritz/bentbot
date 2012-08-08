using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bent.Bot.Configuration;
using Bent.Bot.Module;

namespace Bent.Bot.Module
{
    public class Reddit : IModule
    {
        private static Regex regex = new Regex(@"^\s*reddit", RegexOptions.IgnoreCase);

        private HashSet<string> seenLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // TODO: persist

        private IBackend backend;

        public void OnStart(IConfiguration config, IBackend backend)
        {
            this.backend = backend;
        }

        public async void OnMessage(IMessage message)
        {
            if(message.IsRelevant)
            {
                if(regex.IsMatch(message.Body))
                {
                    var response = await new HttpClient().GetAsync("http://www.reddit.com/.rss");
                    response.EnsureSuccessStatusCode();

                    var body = await response.Content.ReadAsStringAsync();
                    var xml = XDocument.Parse(body);

                    var messages = new List<string>();

                    foreach(var item in xml.Descendants("item"))
                    {
                        var titleEl = item.Elements("title").FirstOrDefault();
                        var linkEl = item.Elements("link").FirstOrDefault();

                        if(titleEl != null && linkEl != null)
                        {
                            var title = titleEl.Value;
                            var link = linkEl.Value;

                            if (!this.seenLinks.Contains(link))
                            {
                                this.seenLinks.Add(link);

                                messages.Add(String.Format("{0} <{1}>", title, link));
                            }
                        }
                    }

                    if(messages.Any())
                    {
                        if(messages.Count > 3)
                        {
                            await this.backend.SendMessageAsync(message.ReplyTo, String.Format("There were {0} new stories, just going to give you the top 3.", messages.Count));
                        }

                        foreach (var m in messages.Take(3))
                        {
                            await this.backend.SendMessageAsync(message.ReplyTo, m);
                        }
                    }
                    else
                    {
                        await this.backend.SendMessageAsync(message.ReplyTo, "Sorry, nothing new. :-/");
                    }
                }
            }
        }
    }
}

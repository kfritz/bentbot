using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bent.Bot.Apis.LastFm;
using Bent.Bot.Configuration;

namespace Bent.Bot.Module
{
    [Export(typeof(IModule))]
    public class LastFm : IModule
    {
        private static Regex regex = new Regex(@"^\s*music\s+(.+?)\s*\.?\s*$", RegexOptions.IgnoreCase);
        private static Regex similarRegex = new Regex(@"^\s*similar\s+to\s+(.+)\s*$", RegexOptions.IgnoreCase);
        private static Regex helpRegex = new Regex(@"^\s*help\s*$");

        private IBackend backend;
        private IConfiguration config;

        public void OnStart(IConfiguration config, IBackend backend)
        {
            this.config = config;
            this.backend = backend;
        }

        public void OnMessage(IMessage message)
        {
            TestMusic(message);
        }

        private async void TestMusic(IMessage message)
        {
            try
            {
                if (message.IsRelevant && !message.IsHistorical)
                {
                    string artist;
                    var match = regex.Match(message.Body);
                    var musicBody = match.Groups[1].Value;
                    if (match.Success)
                    {
                        var similarMatch = similarRegex.Match(musicBody);
                        if (similarMatch.Success)
                        {
                            artist = similarMatch.Groups[1].Value;
                            XDocument xml = await new LastFmClient(this.config[Common.Constants.ConfigKey.LastFmApiKey]).GetSimilarArtistsAsync(artist);
                            await backend.SendMessageAsync(message.ReplyTo, LastFmResponse.CreateSimilarArtistResponse(xml));
                            return;
                        }

                        var helpMatch = helpRegex.Match(musicBody);
                        if (helpMatch.Success)
                        {
                           await backend.SendMessageAsync(message.ReplyTo, LastFmResponse.CreateHelpResponse(config.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static class LastFmResponse
        {
            public static string CreateSimilarArtistResponse(XDocument xml, bool isRandomized = true, int limit = 10)
            {
                StringBuilder response = new StringBuilder();

                var artistName = xml
                    .Descendants("similarartists").First()
                    .Attribute("artist").Value;

                response
                    .Append("Similar artists to ")
                    .Append(artistName)
                    .Append(": ");

                var names = new List<string>();
                var r = new Random();
                foreach (var item in xml.Descendants("artist").OrderBy(x => isRandomized ? r.Next() : 0).Take(limit))
                {
                    names.Add(item.Element("name").Value);
                }

                response
                    .Append(String.Join(", ", names))
                    .Append(".");

                return response.ToString();
            }

            public static string CreateHelpResponse(string botName)
            {
                var response = new StringBuilder();

                response.AppendLine();
                response.AppendLine(botName + " music help");
                response.AppendLine("    The help text you are currently viewing.");
                response.AppendLine(botName + " music similar to Rebecca Black");
                response.AppendLine("    Returns a randomized list of artists that are similar to Rebecca Black.");

                response.AppendLine();
                response.AppendLine("More cool features coming soon!");

                return response.ToString();
            }
        }
    }
}

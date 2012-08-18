using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bent.Bot.Apis.LastFm;
using Bent.Bot.Configuration;

namespace Bent.Bot.Module
{
    // TODO: break out classes

    [Export(typeof(IModule))]
    public class LastFm : IModule
    {
        private static Regex regex = new Regex(@"^\s*music\s+(.+?)\s*\.?\s*$", RegexOptions.IgnoreCase);
        private static Regex similarRegex = new Regex(@"^\s*similar\s+to\s+(.+)\s*$", RegexOptions.IgnoreCase);
        private static Regex discoveryChainRegex = new Regex(@"^\s*discovery\s+(.+)\s*$", RegexOptions.IgnoreCase);
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

        // TODO: prevent cycles
        private async Task<List<string>> DiscoveryChainLoop(string artist, int iterations)
        {
            Debug.Assert(iterations <= 10);

            var discovered = new List<string>();

            string originalArtistName = artist;
            for (int i = 0; i < iterations; i++)
            {
                XDocument xml = await new LastFmClient(this.config[Common.Constants.ConfigKey.LastFmApiKey]).GetSimilarArtistsAsync(artist);
                List<string> similar = LastFmXmlParser.GetSimilarArtistNames(xml, out originalArtistName, true, 1);

                if (i == 0)
                {
                    discovered.Add(originalArtistName);
                }

                if (similar.Any())
                {
                    discovered.Add(similar.First());
                }
                else
                {
                    break;
                }
            }

            return discovered;
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

                        var discoveryChainMatch = discoveryChainRegex.Match(musicBody);
                        if (discoveryChainMatch.Success)
                        {
                            artist = discoveryChainMatch.Groups[1].Value;
                            await backend.SendMessageAsync(message.ReplyTo, "Looking for cool stuff. Please be patient.");
                            List<string> discovered = await DiscoveryChainLoop(artist, 10);
                            await backend.SendMessageAsync(message.ReplyTo, "Discovery chain: " + String.Join(" -> ", discovered));
                            return;
                        }

                        var helpMatch = helpRegex.Match(musicBody);
                        if (helpMatch.Success)
                        {
                           await backend.SendMessageAsync(message.ReplyTo, LastFmResponse.CreateHelpResponse(config.Name));
                           return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static class LastFmXmlParser
        {
            public static List<string> GetSimilarArtistNames(XDocument xml, out string originalArtistName, bool isRandomized = true, int limit = 10)
            {
                Debug.Assert(limit > 0);

                originalArtistName = xml
                    .Descendants("similarartists").First()
                    .Attribute("artist").Value;
                
                var r = new Random();
                var names = new List<string>();
                foreach (var item in xml.Descendants("artist").OrderBy(x => isRandomized ? r.Next() : 0).Take(limit))
                {
                    names.Add(item.Element("name").Value);
                }

                return names;
            }
        }

        private static class LastFmResponse
        {
            public static string CreateSimilarArtistResponse(XDocument xml, bool isRandomized = true, int limit = 10)
            {
                StringBuilder response = new StringBuilder();

                string originalArtistName;
                List<string> similarArtistNames = LastFmXmlParser.GetSimilarArtistNames(xml, out originalArtistName, isRandomized, limit);
                
                response
                    .Append("Similar artists to ")
                    .Append(originalArtistName)
                    .Append(": ");

                response
                    .Append(String.Join(", ", similarArtistNames))
                    .Append(".");

                return response.ToString();
            }

            public static string CreateHelpResponse(string botName)
            {
                var response = new StringBuilder();

                response.AppendLine();
                response.AppendLine(botName + " music help");
                response.AppendLine("    The help text you are currently viewing.");
                response.AppendLine();
                response.AppendLine(botName + " music similar to Rebecca Black");
                response.AppendLine("    Returns a randomized list of artists that are similar to Rebecca Black.");
                response.AppendLine();
                response.AppendLine(botName + " music discovery Miley Cyrus");
                response.AppendLine("    Returns a discovery chain of artists, beginning with Miley Cyrus.");
                response.AppendLine();
                response.AppendLine();
                response.AppendLine("More cool features coming soon!");

                return response.ToString();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bent.Bot.Configuration;

namespace Bent.Bot.Module
{
    [Export(typeof(IModule))]
    internal class Extend : IModule
    {
        private static IList<string> confirmations = new List<string> { "OK!", "Will do!", "Roger that!", "Sure!", "Okey dokey!" };
        private IConfiguration configuration;
        private IBackend backend;
        private Regex regexGetDll;
        private Regex regexEnableModule;
        private Regex regexDisableModule;
        private Random random;

        public void OnStart(IConfiguration config, IBackend backend)
        {
            configuration = config;
            this.backend = backend;
            random = new Random();
            regexGetDll = new Regex(@"^\s*load library\s+(.+?)\s*$", RegexOptions.IgnoreCase);
            regexEnableModule = new Regex(@"^\s*enable module\s+(.+?)\s*$", RegexOptions.IgnoreCase);
            regexDisableModule = new Regex(@"^\s*disable module\s+(.+?)\s*$", RegexOptions.IgnoreCase);
        }

        public async void OnMessage(IMessage message)
        {
            if (message.IsRelevant)
            {
                var match = regexGetDll.Match(message.Body);
                if (match.Success)
                {
                    bool failed = false;
                    try
                    {
                        Uri remoteUri = new Uri(match.Groups[1].Value, UriKind.Absolute);
                        await DownloadDll(remoteUri, message.SenderName);
                        await backend.SendMessageAsync(message.ReplyTo, GetRandomConfirmation());
                    }
                    catch (Exception)
                    {
                        failed = true;
                    }
                    if (failed)
                    {
                        await backend.SendMessageAsync(message.ReplyTo, "Can't do that, sorry.");
                    }
                    return;
                }
                
                match = regexEnableModule.Match(message.Body);
                if (match.Success)
                {
                    configuration.EnableModule(match.Groups[1].Value);
                    await backend.SendMessageAsync(message.ReplyTo, GetRandomConfirmation());
                    return;
                }
                
                match = regexDisableModule.Match(message.Body);
                if (match.Success)
                {
                    configuration.DisableModule(match.Groups[1].Value);
                    await backend.SendMessageAsync(message.ReplyTo, GetRandomConfirmation());
                    return;
                }
            }
        }

        private async Task DownloadDll(Uri remoteUri, string from)
        {
            var response = await new HttpClient().GetAsync(remoteUri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsByteArrayAsync();
            string path = Path.Combine(configuration.ModulesDirectoryPath, from.Replace(' ', '_') + Guid.NewGuid() + ".dll");
            File.WriteAllBytes(path, content);
            //await File.WriteAllBytesAsync(path, content);
        }

        private string GetRandomConfirmation()
        {
            return confirmations[random.Next(confirmations.Count)];
        }
    }
}

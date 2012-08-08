using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bent.Bot.Backend;
using Bent.Bot.Backend.Xmpp;
using Bent.Bot.Backend.Xmpp.AgsXmpp;
using Bent.Bot.Configuration;

namespace Bent.Bot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // TODO: We never dispose of the file stream
            var config = new Configuration.Configuration(new FileStream(@"C:\BentBot\bentbot.config", FileMode.Open));

            var bot = new Bot(config, new AgsXmppBackend(config));

            bot.RunAsync().Wait();
        }
    }
}

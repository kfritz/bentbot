using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bent.Bot.Module.Seppuku
{
    [Export(typeof(IModule))]
    public class Seppuku : IModule
    {
        private static Regex regex = new Regex(@"^\s*kill\s+yourself", RegexOptions.IgnoreCase);
        private static Regex regexJenna = new Regex(@"^\s*ditch", RegexOptions.IgnoreCase);
        private static IList<string> lastWords = new List<string> {
            "Goodbye cruel world!",
            "I only ever loved you, {0}...",
            "I should never have switched from scotch to martinis.", 
            @"You fell victim to one of the classic blunders! The most famous is never get involved in a land war in Asia, but only slightly less well-known is this: never go in against a Sicilian when death is on the line!! Ha ha ha ha ha ha ha!! Ha ha ha ha ha ha ha!! Ha ha ha--",
            "Relax, what could possibly go wrong?",
            "{0} doesn't look so tough."
        };
        
        private IBackend backend;
        private Random random = new Random();

        public void OnStart(Configuration.IConfiguration config, IBackend backend)
        {
            this.backend = backend;
        }

        public async void OnMessage(IMessage message)
        {
            if (message.IsRelevant)
            {
                var match = regex.Match(message.Body).Success || regexJenna.Match(message.Body).Success;

                if (match)
                {
                    await backend.SendMessageAsync(message.ReplyTo, string.Format(lastWords[random.Next(lastWords.Count)], message.SenderName));
                    Environment.Exit(0);
                }
            }
        }
    }
}

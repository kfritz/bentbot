using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bent.Bot.Configuration;

namespace Bent.Bot.Module
{
    public class Google : IModule
    {
        private static Regex regex = new Regex(@"^\s*google\s+(.+?)\s*$", RegexOptions.IgnoreCase);
        private IBackend backend;

        public void OnStart(IConfiguration config, IBackend backend)
        {
            this.backend = backend;
        }

        public void OnMessage(IMessage message)
        {
            if(message.IsRelevant)
            {
                var match = regex.Match(message.Body);

                if (match.Success)
                {
                    this.backend.SendMessageAsync(message.ReplyTo, "http://lmgtfy.com/?q=" + HttpUtility.UrlEncode(match.Groups[1].Value));
                }
            }
        }
    }
}

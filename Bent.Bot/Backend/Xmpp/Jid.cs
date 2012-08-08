using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bent.Bot.Backend.Xmpp
{
    public class Jid : IAddress
    {
        private Regex jidPattern = new Regex(@"^(.+?)@(.+?)(?:/(.+?))?$");

        public string Local { get; private set; }
        public string Domain { get; private set; }
        public string Resource { get; private set; }
        public Jid Bare { get; private set; }

        public Jid(string jid)
        {
            var match = jidPattern.Match(jid.Trim());
            if (match.Success)
            {
                this.Local = match.Groups[1].Value;
                this.Domain = match.Groups[2].Value;
                this.Resource = String.IsNullOrWhiteSpace(match.Groups[3].Value) ? null : match.Groups[3].Value;
                this.Bare = this.Resource == null ? this : new Jid(this.Local, this.Domain);
            }
            else
            {
                throw new Exception(); // TODO: More specific exception
            }
        }

        public Jid(string local, string domain)
            : this(local, domain, null) { }

        public Jid(string local, string domain, string resource)
        {
            this.Local = local;
            this.Domain = domain;
            this.Resource = resource;
            this.Bare = resource == null ? this : new Jid(local, domain);
        }
    }
}

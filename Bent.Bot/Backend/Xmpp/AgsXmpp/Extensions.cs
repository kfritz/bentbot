using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using agsXMPP.protocol.client;
using Bent.Bot.Common;

namespace Bent.Bot.Backend.Xmpp.AgsXmpp
{
    internal static class Extensions
    {
        public static Jid ToJid(this agsXMPP.Jid jid)
        {
            return new Jid(jid.User, jid.Server, jid.Resource);
        }

        public static agsXMPP.Jid ToAgsJid(this Jid jid)
        {
            return new agsXMPP.Jid(jid.Local, jid.Domain, jid.Resource);
        }
    }
}

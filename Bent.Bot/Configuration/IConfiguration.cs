using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bent.Bot.Backend.Xmpp;
using Bent.Bot.Module;

namespace Bent.Bot.Configuration
{
    public interface IConfiguration
    {
        string this[string key] { get; }

        string Name { get; }
        Jid Jid { get; }
        string Password { get; }
        IEnumerable<Jid> Rooms { get; }
        IEnumerable<IModule> Modules { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bent.Bot.Configuration;

namespace Bent.Bot.Module
{
    public interface IModule // TODO: change this to IMessageHandler
    {
        void OnStart(IConfiguration config, IBackend backend);
        void OnMessage(IMessage message);
    }
}

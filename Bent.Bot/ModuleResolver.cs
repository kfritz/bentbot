using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bent.Bot.Module;

namespace Bent.Bot
{
    internal class ModuleResolver
    {
        public IModule Resolve(string name)
        {
            return this.Resolve(Enumerable.Repeat(name, 1)).FirstOrDefault();
        }

        public IEnumerable<IModule> Resolve(IEnumerable<string> names)
        {
            return names.Select(i => Activator.CreateInstance(null, "Bent.Bot.Module." + i).Unwrap()).Cast<IModule>().ToList();
        }
    }
}

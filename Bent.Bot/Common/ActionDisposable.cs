using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bent.Bot.Common
{
    public class ActionDisposable : IDisposable
    {
        private Action action;

        public ActionDisposable(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }
}

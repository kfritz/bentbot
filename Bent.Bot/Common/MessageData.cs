using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bent.Bot.Common
{
    public class MessageData
    {
        public IAddress ReplyTo { get; private set; }
        public string SenderName { get; private set; }

        public bool IsFromMyself { get; private set; }
        public bool IsHistorical { get; private set; }
        public bool IsPrivate { get; private set; }

        public string Body { get; private set; }

        public MessageData(IAddress replyTo, string senderName, string body, bool isFromMyself, bool isHistorical, bool isPrivate)
        {
            this.ReplyTo = replyTo;
            this.SenderName = senderName;

            this.IsFromMyself = isFromMyself;
            this.IsHistorical = isHistorical;
            this.IsPrivate = isPrivate;

            this.Body = body;
        }
    }
}

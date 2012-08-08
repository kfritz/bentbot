using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using Bent.Bot.Common;
using Bent.Bot.Configuration;

namespace Bent.Bot.Backend.Xmpp.AgsXmpp
{
    public class AgsXmppBackend : IBackend
    {
        private ISet<IObserver<MessageData>> observers = new HashSet<IObserver<MessageData>>();

        private IConfiguration config;

        private XmppClientConnection xmpp;
        private MucManager muc;
      
        public AgsXmppBackend(IConfiguration config)
        {
            this.config = config;

            this.xmpp = new XmppClientConnection(this.config.Jid.ToAgsJid(), this.config.Password);
            this.muc = new MucManager(xmpp);

            this.xmpp.OnLogin += OnLogin;
            this.xmpp.OnMessage +=OnMessage;
        }

        public void Dispose()
        {   // TODO: AgsXmppBackend: Implement Dispose()
            throw new NotImplementedException();
        }

        public async Task ConnectAsync()
        {
            await Task.Run(() => this.xmpp.Open());
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() => this.xmpp.Close());
        }

        public async Task SendMessageAsync(IAddress address, string body)
        {
            var jid = address as Jid;
            if(jid == null)
            {   // TODO: AgsXmppBackend: throw more specific exception
                throw new Exception();                
            }
            else
            {   // TODO: we need to determine if we should send a regular chat or a groupchat message
                await Task.Run(() => this.xmpp.Send(new Message(jid.ToAgsJid(), MessageType.groupchat, body)));
            }
        }

        public IDisposable Subscribe(IObserver<MessageData> observer)
        {
            this.observers.Add(observer);

            // TODO: AgsXmppBackend: Make Subscribe() thread-safe
            return new ActionDisposable(() => this.observers.Remove(observer));
        }

        private void OnLogin(object sender)
        {
            foreach (var room in this.config.Rooms)
            {
                muc.JoinRoom(room.ToAgsJid(), this.config.Name);
            }
        }

        private void OnMessage(object sender, Message msg)
        {
            Parallel.ForEach(this.observers, o =>
                {
                    try
                    {
                        if (msg.Type == MessageType.chat || msg.Type == MessageType.groupchat)
                        {
                            // TODO: XMPP can send all sorts of messages, gotta filter them out

                            if (msg.Body != null)
                            {
                                o.OnNext(
                                    new MessageData(
                                        // TODO: should we reply to the bare address all the time?
                                        replyTo: msg.Type == MessageType.groupchat ? msg.From.ToJid().Bare : msg.From.ToJid(),
                                        senderName: msg.Type == MessageType.groupchat ? msg.From.Resource : msg.From.User, // TODO: better way to do this...
                                        body: msg.Body,
                                        // TODO: is there a better way to check for this?
                                        isFromMyself: msg.Type == MessageType.groupchat ? String.Equals(msg.From.Resource, this.config.Name, StringComparison.OrdinalIgnoreCase) : string.Equals(msg.From.Bare, this.config.Jid.Bare),
                                        isHistorical: msg.XDelay != null,
                                        isPrivate: msg.Type != MessageType.groupchat
                                    )
                                );
                            }
                            else
                            {
                                // TODO: e.g. subject message
                                Console.Error.WriteLine("Message with null body");
                                Console.Error.WriteLine(msg.ToString());
                            }
                        }
                        else
                        {
                            // TODO: need to handle the other types
                        }
                    }
                    catch (Exception e)
                    {   // TODO: AgsXmppBackend: Handle observer errors
                        Console.Error.WriteLineAsync(e.ToString());
                    }
                });
        }
    }
}

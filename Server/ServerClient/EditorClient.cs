using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Communication.Messages;

using ProtocolLibrary.CSProtocol.CommonConfig;
using System.Collections;

namespace NIPlayRoomNetServer.ServerClient
{
    class EditorClient : BaseServerClient
    {
        public EditorClient(string token, IScsServerClient client, Server server) 
            : base(token, client, server)
        {
            client.MessageReceived += Client_MessageRecevied;
        }
        protected override void Client_MessageRecevied(object sender, Hik.Communication.Scs.Communication.Messages.MessageEventArgs e)
        {
            base.Client_MessageRecevied(sender, e);
        }
    }
}

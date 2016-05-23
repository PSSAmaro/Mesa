using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.Web.WebSockets;
namespace ServerMesa
{
    public class MicrosoftWebSockets : WebSocketHandler
    {
        private static WebSocketCollection mesas = new WebSocketCollection();
        private static WebSocketCollection jogadores = new WebSocketCollection();
        private int id;

        public override void OnOpen()
        {
            this.id = Convert.ToInt32(this.WebSocketContext.QueryString["id"]);
            if (id == 1)
                mesas.Add(this);
            else
                jogadores.Add(this);
        }

        public override void OnMessage(string message)
        {
            mesas.Broadcast(string.Format("{0} said: {1}", id, message));
        }

        public override void OnClose()
        {
            mesas.Remove(this);
            mesas.Broadcast(string.Format("{0} has gone away.", id));
        }

    }
}
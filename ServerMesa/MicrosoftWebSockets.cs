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
        private static WebSocketCollection clientes = new WebSocketCollection();
        private int _id;
        private string _tipo;

        public override void OnOpen()
        {
            _tipo = Convert.ToString(WebSocketContext.QueryString["tipo"]);
            if (_tipo == "MESA")
            {
                _id = 0;
                clientes.Add(this);
            }
            else
            {
                _tipo = "JOGADOR";
                _id = Convert.ToInt32(WebSocketContext.QueryString["id"]);
                clientes.Add(this);
                clientes.SingleOrDefault(r => ((MicrosoftWebSockets)r)._id == 0).Send(string.Format("J,NOVO,{0}", _id));
            }
        }

        public override void OnMessage(string message)
        {
            string[] argumentos = message.Split(',');
            switch(argumentos[0])
            {
                case "M":
                    switch(argumentos[1])
                    {
                        case "TURNO":
                            clientes.SingleOrDefault(r => ((MicrosoftWebSockets)r)._id == Convert.ToInt32(argumentos[2])).Send(message);
                            break;
                        default:
                            clientes.Broadcast(message);
                            break;
                    }
                    break;
                case "J":
                    clientes.SingleOrDefault(r => ((MicrosoftWebSockets)r)._id == 0).Send(message);
                    break;
            }
        }

        public override void OnClose()
        {
            clientes.Remove(this);
        }

    }
}
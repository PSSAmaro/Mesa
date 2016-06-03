using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ClientMesa.Jogos;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClientMesa
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Jogo j;
        private List<Jogador> jogadores;
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;
        private DispatcherTimer timer;
        private int atual;
        private bool ingame;

        public MainPage()
        {
            this.InitializeComponent();

            jogadores = new List<Jogador>();

            // Iniciar WebSocket
            messageWebSocket = new MessageWebSocket();
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
            messageWebSocket.MessageReceived += NovaMensagem;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            /*timer = new DispatcherTimer();
            timer.Tick += fazCenas;
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();*/
            await messageWebSocket.ConnectAsync(new Uri("ws://servermesa.azurewebsites.net/ws.ashx?tipo=MESA"));
            messageWriter = new DataWriter(messageWebSocket.OutputStream);
        }

        private void fazCenas(object sender, object e)
        {
            jogadores.Add(new Jogador("a", 2, 1, 1));
            jogadores.Add(new Jogador("a", 2, 2, 2));
            j.Turno(jogadores[0], 1);
            j.Turno(jogadores[0], 2);
            j.Turno(jogadores[0], 3);
            j.Turno(jogadores[0], 4);
            //j.Turno(jogadores[0], 8);
            j.Turno(jogadores[1], 0);
            j.Turno(jogadores[1], 5);
            j.Turno(jogadores[1], 6);
            j.Turno(jogadores[1], 7);
            if (j.VerificarFim())
                j.Fim();
        }

        private void NovaMensagem(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                string read = reader.ReadString(reader.UnconsumedBufferLength);
                string[] argumentos = read.Split(',');

                if (argumentos[0] == "J")
                {
                    switch (argumentos[1])
                    {
                        case "NOVO":
                            if (!ingame)
                            {
                                int nm = jogadores.Count + 1;
                                jogadores.Add(new Jogador("a", nm, jogadores.Count % 2 + 1, Convert.ToInt32(argumentos[2])));
                                EnviarMensagem("M,JGDRS," + nm);
                                VerificaInicio();
                            }
                            break;
                        case "JOGO":
                            if (j == null)
                            {
                                switch (Convert.ToInt32(argumentos[2]))
                                {
                                    case 1:
                                        j = new Galo(tabuleiro, tab, messageWriter);
                                        break;
                                    case 2:
                                        break;
                                }
                                VerificaInicio();
                            }
                            break;
                        case "TURNO":
                            if (Convert.ToInt32(argumentos[2]) == jogadores[atual].Id)
                            {
                                if (j.Turno(jogadores[atual], Convert.ToInt32(argumentos[3])))
                                {
                                    if (j.VerificarFim())
                                    {
                                        j.Fim();
                                        EnviarMensagem("M,FIM");
                                        ingame = false;
                                    }
                                    else
                                    {
                                        atual = (atual + 1) % j.NumJogadores;
                                        EnviarMensagem("M,TURNO," + jogadores[atual].Id);
                                    }
                                }
                                else
                                {
                                    EnviarMensagem("M,TURNO," + jogadores[atual].Id);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void VerificaInicio()
        {
            if (j != null && jogadores.Count >= j.NumJogadores)
            {
                j.Inicio();
                EnviarMensagem("M,COMECA," + j.Id);
                atual = 0;
                EnviarMensagem("M,TURNO," + jogadores[0].Id);
                ingame = true;
            }
        }

        private async void EnviarMensagem(string msg)
        {
            messageWriter.WriteString(msg);
            await messageWriter.StoreAsync();
        }
    }
}

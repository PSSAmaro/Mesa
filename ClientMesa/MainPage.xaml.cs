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

            /*j = new Bisca(tabuleiro, tab, messageWriter);
            j.Inicio(jogadores);*/
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
                    var ignore = tab.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
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
                                            j = new Bisca(tabuleiro, tab, messageWriter);
                                            break;
                                    }
                                    VerificaInicio();
                                }
                                break;
                            case "TURNO":
                                if (Convert.ToInt32(argumentos[2]) == jogadores[j.Proximo].Id)
                                {
                                    if (j.Turno(jogadores[j.Proximo].Numero - 1, Convert.ToInt32(argumentos[3])))
                                    {
                                        if (j.VerificarFim())
                                        {
                                            j.Fim();
                                            EnviarMensagem("M,FIM");
                                            ingame = false;
                                            j = null;
                                        }
                                        else
                                            EnviarMensagem("M,TURNO," + jogadores[j.Proximo].Id);
                                    }
                                    else
                                        EnviarMensagem("M,TURNO," + jogadores[j.Proximo].Id);
                                }
                                break;
                        }
                    });
                }
            }
        }

        private void VerificaInicio()
        {
            if (j != null && jogadores.Count >= j.NumJogadores)
            {
                EnviarMensagem("M,COMECA," + j.Id);
                j.Inicio(jogadores);
                EnviarMensagem("M,TURNO," + jogadores[j.Proximo].Id);
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClientW10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int id;
        private Image[] posPecas;
        private bool inGame;
        private int jogo;
        private bool turno;
        private List<Uri> pecas;
        private Thickness margemPecas;

        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;

        public MainPage()
        {
            this.InitializeComponent();

            posPecas = new Image[9];
            inGame = false;
            jogo = 0;
            turno = false;

            margemPecas = new Thickness(5, 5, 5, 5);

            // Gerar ID
            Random r = new Random();
            id = r.Next(0, 10000);

            // Iniciar WebSocket
            messageWebSocket = new MessageWebSocket();
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
            messageWebSocket.MessageReceived += NovaMensagem;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await messageWebSocket.ConnectAsync(new Uri("ws://servermesa.azurewebsites.net/ws.ashx?id=" + id));
            messageWriter = new DataWriter(messageWebSocket.OutputStream);

            TabelaDeJogos();
        }

        private void EscolherJogo(object sender, PointerRoutedEventArgs e)
        {
            int i = Array.IndexOf(posPecas, (Image)sender) + 1;
            EnviarMensagem("J,JOGO," + i);
        }

        private void Turno(object sender, PointerRoutedEventArgs e)
        {
            if (turno)
            {
                int i = Array.IndexOf(posPecas, (Image)sender);
                EnviarMensagem("J,TURNO," + id + "," + i);
                turno = false;
            }
        }

        private void TabelaDeJogos()
        {
            LimparImagens();
            tabuleiro.Source = new BitmapImage(new Uri("ms-appx:///Assets/branco.png"));
            ColocarImagem(new Uri("ms-appx:///Assets/galo.png"), 0, 0);
        }

        private void NovaMensagem(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                string read = reader.ReadString(reader.UnconsumedBufferLength);
                string[] argumentos = read.Split(',');

                if (argumentos[0] == "M")
                {
                    var ignore = tab.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        switch (argumentos[1])
                        {
                            case "COMECA":
                                inGame = true;
                                jogo = Convert.ToInt32(argumentos[2]) - 1;
                                tabuleiro.Source = posPecas[jogo].Source;
                                LimparImagens();
                                if (jogo == 0)
                                {
                                    // Outra maneira de fazer isto era fixe
                                    pecas = new List<Uri>
                                    {
                                        new Uri("ms-appx:///Assets/x.png"),
                                        new Uri("ms-appx:///Assets/o.png")
                                    };
                                    for (int b = 0; b < 9; b++)
                                        ColocarImagem(new Uri("ms-appx:///Assets/galo/" + b + ".png"), b, 1);
                                }
                                break;
                            case "TURNO":
                                turno = true;
                                break;
                            case "FIM":
                                inGame = false;
                                TabelaDeJogos();
                                break;
                            case "INF":
                                int i = Convert.ToInt32(argumentos[2]);
                                int j = Convert.ToInt32(argumentos[3]);
                                RetirarImagem(j);
                                if (i > 0)
                                    ColocarImagem(pecas[i - 1], j, 2);
                                break;
                        }
                    });
                }
            }
        }

        private async void EnviarMensagem(string msg)
        {
            messageWriter.WriteString(msg);
            await messageWriter.StoreAsync();
        }

        private void ColocarImagem(Uri url, int i, int t)
        {
            Image img;
            // T = Tipo: 0 = Tabela de jogos; 1 = Tabuleiro; 2 = Peça
            switch (t)
            {
                case 0:
                    img = new Image
                    {
                        Source = new BitmapImage(url),
                        Margin = margemPecas
                    };
                    img.PointerPressed += EscolherJogo;
                    break;
                case 1:
                    img = new Image
                    {
                        Source = new BitmapImage(url)
                    };
                    img.PointerPressed += Turno;
                    break;
                default:
                    img = new Image
                    {
                        Source = new BitmapImage(url),
                        Margin = margemPecas
                    };
                    break;
            }
            int x = i % 3;
            int y = i / 3;
            tab.Children.Add(img);
            Grid.SetRow(img, y);
            Grid.SetColumn(img, x);
            posPecas[i] = img;
        }

        private void RetirarImagem(int i)
        {
            if (posPecas[i] != null)
            {
                tab.Children.Remove(posPecas[i]);
                posPecas[i] = null;
            }
        }

        private void LimparImagens()
        {
            foreach (Image img in posPecas)
            {
                tab.Children.Remove(img);
            }
            Array.Clear(posPecas, 0, 9);
        }
    }
}

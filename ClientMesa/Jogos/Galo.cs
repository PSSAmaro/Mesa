using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientMesa.Jogos
{
    public class Galo : Jogo
    {
        private Jogador[,] usado;

        public Galo(Image t, Grid g, DataWriter m)
        {
            Id = 1;
            Nome = "Jogo do galo";
            NumJogadores = 2;

            Tabuleiro = t;

            Tabela = g;

            MargemPecas = new Thickness(5, 5, 5, 5);

            Pecas = new List<Peca>
            {
                new Peca("X", "Peça", 0, "x"),
                new Peca("O", "Peça", 0, "o")
            };

            TurnoAtual = 0;

            PecasColocadas = new List<Image>();

            usado = new Jogador[3,3];

            MessageWriter = m;
        }

        public override void Inicio()
        {
            var ignore = Tabuleiro.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Tabuleiro.Source = new BitmapImage(new Uri("ms-appx:///Assets/galo.png"));
            });
        }

        public override bool Turno(Jogador j, int i)
        {
            if (i >= 0 && i <= 8)
            {
                int x = i % 3;
                int y = i / 3;
                if (usado[x, y] == null)
                {
                    ColocarPeca(Pecas[j.Equipa - 1], x, y);
                    usado[x, y] = j;
                    Informar("M,INF," + j.Equipa + "," + i);
                    TurnoAtual++;
                    return true;
                }
            }
            return false;
        }

        private void ColocarPeca(Peca p, int x, int y)
        {
            var ignore = Tabela.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Image pc = new Image
                {
                    Source = new BitmapImage(p.Imagem),
                    Margin = MargemPecas
                };
                Tabela.Children.Add(pc);
                Grid.SetRow(pc, y);
                Grid.SetColumn(pc, x);
                PecasColocadas.Add(pc);
            });
        }

        public override bool VerificarFim()
        {
            // Diagonais
            if (usado[1, 1] != null && ((usado[0,0] == usado[1, 1] && usado[1, 1] == usado[2, 2])
            || (usado[0, 2] == usado[1, 1] && usado[1, 1] == usado[2, 0])))
            {
                Vencedor = usado[1, 1].Equipa;
                return true;
            }
            // Linhas e colunas
            for (int i = 0; i <= 2; i++)
            {
                if (usado[i, i] != null && ((usado[i, 0] == usado[i, 1] && usado[i, 1] == usado[i, 2])
                || (usado[0, i] == usado[1, i] && usado[1, i] == usado[2, i])))
                {
                    Vencedor = usado[i, i].Equipa;
                    return true;
                }
            }
            // Empate
            if (TurnoAtual >= 9)
            {
                Vencedor = 0;
                return true;
            }
            return false;
        }

        public override void Fim()
        {
            var ignore = Tabela.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PecasColocadas.ForEach(delegate (Image pc)
                {
                    Tabela.Children.Remove(pc);
                });
            });
            var ignore2 = Tabuleiro.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (Vencedor)
                {
                    case 0:
                        Tabuleiro.Source = new BitmapImage(new Uri("ms-appx:///Assets/empate.png"));
                        break;
                    case 1:
                        Tabuleiro.Source = new BitmapImage(new Uri("ms-appx:///Assets/vermelha.png"));
                        break;
                    case 2:
                        Tabuleiro.Source = new BitmapImage(new Uri("ms-appx:///Assets/preta.png"));
                        break;
                }
            });
        }

        public override async void Informar(string msg)
        {
            MessageWriter.WriteString(msg);
            await MessageWriter.StoreAsync();
        }
    }
}

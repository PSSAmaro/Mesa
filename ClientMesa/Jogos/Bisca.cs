using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientMesa.Jogos
{
    class Bisca : Jogo
    {
        private Random rng;
        private string[] naipes;
        private int[] pontuacao;
        private Peca[,] maos;
        private Peca mesa;
        private int proximaCarta;
        private int pontosJogada;
        private Peca cartaForte;
        private int jogadorForte;
        private int[] posJogadas;
        private List<Image> partesTab;

        public Bisca(Image t, Grid g, DataWriter m)
        {
            Id = 2;
            Nome = "Bisca";
            NumJogadores = 4;

            Tabuleiro = t;

            Tabela = g;

            MargemPecas = new Thickness(5, 5, 5, 5);

            naipes = new string[4]
            {
                "Copas",
                "Espadas",
                "Ouros",
                "Paus"
            };

            Pecas = new List<Peca>();

            for (int i = 0; i <= 3; i++)
            {
                for (int j = 2; j <= 6; j++)
                    Pecas.Add(new Peca(Convert.ToString(j), naipes[i], 0, "cartas/" + j + "_" + naipes[i], i * 10 + j - 1));
                Pecas.Add(new Peca("Q", naipes[i], 2, "cartas/Q_" + naipes[i], i * 10 + 6));
                Pecas.Add(new Peca("J", naipes[i], 3, "cartas/J_" + naipes[i], i * 10 + 7));
                Pecas.Add(new Peca("K", naipes[i], 4, "cartas/K_" + naipes[i], i * 10 + 8));
                Pecas.Add(new Peca("7", naipes[i], 10, "cartas/7_" + naipes[i], i * 10 + 9));
                Pecas.Add(new Peca("A", naipes[i], 11, "cartas/A_" + naipes[i], i * 10 + 10));
            }

            pontuacao = new int[2];

            maos = new Peca[4, 3];

            TurnoAtual = 0;

            proximaCarta = 0;

            pontosJogada = 0;

            posJogadas = new int[4];

            PecasColocadas = new List<Image>();

            partesTab = new List<Image>();

            MessageWriter = m;

            rng = new Random();
        }

        private void Baralhar()
        {
            int n = Pecas.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Peca value = Pecas[k];
                Pecas[k] = Pecas[n];
                Pecas[n] = value;
            }
        }

        public override void Inicio(List<Jogador> j)
        {
            Jogadores = j;
            Tabuleiro.Source = new BitmapImage(new Uri("ms-appx:///Assets/branco.png"));

            Baralhar();

            mesa = Pecas[Pecas.Count - 1];

            ColocarTab();

            for (int b = 0; b <= 3; b++)
            {
                var c = b + 42;
                Informar("M,PRIV," + Jogadores[b].Id + "," + c + ",6");
            }

            Informar("M,INF,41,7");
            Informar("M,INF," + mesa.Id + ",8");

            for (int i = 0; i <= 3; i++)
                for (int l = 0; l <= 2; l++)
                    DarCarta(i, l);
        }

        private void ColocarTab()
        {
            for (int i = 1; i <= 4; i++)
            {
                Image pc = new Image
                {
                    Source = new BitmapImage(new Uri("ms-appx:///Assets/" + i + ".png"))
                };
                Tabela.Children.Add(pc);
                switch (i)
                {
                    case 1:
                        Grid.SetRow(pc, 0);
                        Grid.SetColumn(pc, 2);
                        break;
                    case 2:
                        Grid.SetRow(pc, 0);
                        Grid.SetColumn(pc, 0);
                        break;
                    case 3:
                        Grid.SetRow(pc, 2);
                        Grid.SetColumn(pc, 0);
                        break;
                    case 4:
                        Grid.SetRow(pc, 2);
                        Grid.SetColumn(pc, 2);
                        break;
                }
                partesTab.Add(pc);
            }
        }

        public override bool Turno(int j, int i)
        {
            if (i >= 0 && i <= 2 && maos[j,i] != null)
            {
                TurnoAtual++;
                if (TurnoAtual%4 == 1)
                {
                    LimparMesa();
                    cartaForte = maos[j, i];
                    jogadorForte = j;

                    JogarCarta(maos[j, i], j, 1);
                }
                else if ((cartaForte.Classe == maos[j, i].Classe && (maos[j, i].Id-1)%10 > (cartaForte.Id-1)%10)
                    || cartaForte.Classe != mesa.Classe && maos[j, i].Classe == mesa.Classe)
                {
                    cartaForte = maos[j, i];
                    jogadorForte = j;

                    PecasColocadas.ForEach(delegate (Image pc)
                    {
                        pc.Opacity = 0.3;
                    });

                    JogarCarta(maos[j, i], j, 1);
                }
                else
                    JogarCarta(maos[j, i], j, 0.3);

                // Guardar a posição da carta que foi jogada para enviar a nova
                posJogadas[j] = i;
                pontosJogada += maos[j, i].Pontos;

                // Eliminar carta da mão
                maos[j, i] = null;
                Informar("M,PRIV," + Jogadores[j].Id + ",0," + i);

                if (TurnoAtual%4 == 0)
                {
                    pontuacao[Jogadores[jogadorForte].Equipa - 1] += pontosJogada;
                    pontosJogada = 0;

                    int g;
                    for (int f = 0; f <= 3; f++)
                    {
                        g = (jogadorForte + f)%4;
                        DarCarta(g, posJogadas[g]);
                    }
                    Proximo = jogadorForte;
                }
                else
                    Proximo = (Proximo + 1) % 4;
                return true;
            }
            return false;
        }

        private void DarCarta(int j, int p)
        {
            if (proximaCarta < 40)
            {
                maos[j, p] = Pecas[proximaCarta];
                proximaCarta++;
                Informar("M,PRIV," + Jogadores[j].Id + "," + maos[j,p].Id + "," + p);
            }
        }

        private void JogarCarta(Peca p, int j, double o)
        {
            Image pc = new Image
            {
                Source = new BitmapImage(p.Imagem),
                Opacity = o
            };
            Tabela.Children.Add(pc);
            switch (j)
            {
                case 0:
                    Grid.SetRow(pc, 0);
                    Grid.SetColumn(pc, 1);
                    break;
                case 1:
                    Grid.SetRow(pc, 1);
                    Grid.SetColumn(pc, 0);
                    break;
                case 2:
                    Grid.SetRow(pc, 2);
                    Grid.SetColumn(pc, 1);
                    break;
                case 3:
                    Grid.SetRow(pc, 1);
                    Grid.SetColumn(pc, 2);
                    break;
            }
            PecasColocadas.Add(pc);
        }

        private void LimparMesa()
        {
            PecasColocadas.ForEach(delegate (Image pc)
            {
                Tabela.Children.Remove(pc);
            });
        }

        public override bool VerificarFim()
        {
            if (TurnoAtual >= 40)
            {
                if(pontuacao[0] > 60)
                    Vencedor = 1;
                else if (pontuacao[0] < 60)
                    Vencedor = 2;
                else
                    Vencedor = 0;
                return true;
            }
            return false;
        }

        public override void Fim()
        {
            LimparMesa();
            partesTab.ForEach(delegate (Image pc)
            {
                Tabela.Children.Remove(pc);
            });
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
        }

        public override async void Informar(string msg)
        {
            MessageWriter.WriteString(msg);
            await MessageWriter.StoreAsync();
        }
    }
}

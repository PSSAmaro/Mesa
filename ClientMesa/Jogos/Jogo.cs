using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClientMesa.Jogos
{
    public abstract class Jogo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int NumJogadores { get; set; }
        public List<Peca> Pecas { get; set; }

        public Image Tabuleiro { get; set; }
        public Grid Tabela { get; set; }
        public Thickness MargemPecas { get; set; }
        public DataWriter MessageWriter { get; set; }

        public int TurnoAtual { get; set; }
        public int Vencedor { get; set; }

        // Esta lista guarda os elementos colocados para removê-los no fim do jogo ou de um turno
        public List<Image> PecasColocadas { get; set; }

        public abstract void Inicio();
        public abstract bool Turno(Jogador j, int i);
        public abstract bool VerificarFim();
        public abstract void Fim();

        public abstract void Informar(string msg);
    }
}

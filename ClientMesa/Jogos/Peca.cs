using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMesa.Jogos
{
    public class Peca
    {
        public string Nome { get; set; }
        public string Classe { get; set; }
        public int Pontos { get; set; }
        public Uri Imagem { get; set; }

        public Peca(string n, string c, int p, string i)
        {
            Nome = n;
            Classe = c;
            Pontos = p;
            Imagem = new Uri("ms-appx:///Assets/" + i + ".png");
        }
    }
}

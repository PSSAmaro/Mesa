using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMesa.Jogos
{
    public class Jogador
    {
        public string Nome { get; set; }
        public int Numero { get; set; }
        public int Equipa { get; set; }
        public int Id { get; set; }

        public Jogador()
        {
            Nome = "Empate";
            Numero = 0;
            Equipa = 0;
        }

        public Jogador(string n, int num, int e, int i)
        {
            Nome = n;
            Numero = num;
            Equipa = e;
            Id = i;
        }
    }
}

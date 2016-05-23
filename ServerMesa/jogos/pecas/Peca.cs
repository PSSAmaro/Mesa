using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerMesa.jogos.pecas
{
    public abstract class Peca
    {
        public string Nome { get; set; } // Nome da peça
        public int Valor { get; set; } // Valor pontual da peça
        public int Jogador { get; set; } // Mudar para tipo jogador, jogador que tem a peça atualmente
        public bool Jogada { get; set; } // Se já foi jogada ou não
    }
}